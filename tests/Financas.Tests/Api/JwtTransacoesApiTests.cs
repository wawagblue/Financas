using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Financas.Tests.Api;

public sealed class JwtTransacoesApiTests : IDisposable
{
    private const string Chave = "local_test_secret_value_at_least_32_bytes_long";
    private static readonly Guid UsuarioId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private readonly ApiFactory _factory = new() { UsarJwtReal = true };
    private readonly HttpClient _client;

    public JwtTransacoesApiTests() => _client = _factory.CreateClient();

    [Fact]
    public async Task Transacoes_sem_token_retornam_401()
    {
        var resposta = await _client.GetAsync("/api/transacoes");

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Jwt_valido_acessa_transacoes_e_dados_ficam_no_usuario_do_sub()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CriarToken(UsuarioId));
        var corpo = new { descricao = "Mercado", valor = 75.5m, tipo = "Despesa", data = "2026-11-11T00:00:00" };

        var criada = await _client.PostAsJsonAsync("/api/transacoes", corpo);
        var lista = await _client.GetAsync("/api/transacoes");

        criada.StatusCode.Should().Be(HttpStatusCode.Created);
        lista.StatusCode.Should().Be(HttpStatusCode.OK);
        (await lista.Content.ReadAsStringAsync()).Should().Contain("Mercado");
    }

    [Fact]
    public async Task Jwt_com_assinatura_invalida_retornam_401()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CriarToken(UsuarioId, "wrong_secret_that_is_long_enough_for_hmac"));

        var resposta = await _client.GetAsync("/api/transacoes");

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static string CriarToken(Guid usuarioId, string? chave = null)
    {
        var agora = DateTime.UtcNow;
        var credencial = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave ?? Chave)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "TccProway",
            audience: "TccProway.Client",
            claims: [new Claim("sub", usuarioId.ToString())],
            notBefore: agora.AddSeconds(-5),
            expires: agora.AddMinutes(5),
            signingCredentials: credencial);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
