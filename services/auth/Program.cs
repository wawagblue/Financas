using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TccProway.Api.Application.Abstractions;
using TccProway.Api.Application.Services;
using TccProway.Api.Application.Validation;
using TccProway.Api.Infrastructure.Persistence;
using TccProway.Api.Infrastructure.Security;
using TccProway.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.IsDevelopment();

// Aceita tanto ConnectionStrings__MySql quanto MYSQL_CONNECTION_STRING.
var connectionString = builder.Configuration.GetConnectionString("MySql")
    ?? builder.Configuration["MYSQL_CONNECTION_STRING"];
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "MYSQL_CONNECTION_STRING não configurada. Defina a variável de ambiente MYSQL_CONNECTION_STRING " +
        "ou ConnectionStrings__MySql com uma connection string válida para o MySQL.");
}

// Em Development, usa uma chave local conhecida apenas para desbloquear testes sem configuração JWT.
// Em qualquer outro ambiente, JWT_SECRET (ou Jwt__Secret) é obrigatório.
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? builder.Configuration["JWT_SECRET"]
    ?? (isDevelopment ? "DEV_ONLY_CHANGE_ME_Use_a_real_random_secret_1234567890" : null);
if (string.IsNullOrWhiteSpace(jwtSecret))
    throw new InvalidOperationException("JWT_SECRET não configurado. Defina JWT_SECRET ou Jwt__Secret.");
if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
    throw new InvalidOperationException("JWT_SECRET deve ter pelo menos 32 bytes UTF-8.");

// JwtTokenService também lê esta chave pelo nome hierárquico da configuração .NET.
builder.Configuration["Jwt:Secret"] = jwtSecret;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(connectionString));
builder.Services.AddControllers();
builder.Services.AddApiDocumentation();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();

var issuer = builder.Configuration["Jwt:Issuer"] ?? "TccProway";
var audience = builder.Configuration["Jwt:Audience"] ?? "TccProway.Client";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = issuer,
        ValidateAudience = true, ValidAudience = audience,
        ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30),
        NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
    };
});
builder.Services.AddAuthorization();
var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
{
    if (origins.Length > 0) policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
}));

var app = builder.Build();
if (isDevelopment)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
app.UseApiDocumentation();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
