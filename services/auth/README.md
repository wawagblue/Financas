# Serviço de autenticação

API ASP.NET Core responsável por cadastro, login, refresh token e dados do usuário.

## Configuração

O serviço exige uma connection string MySQL (`MYSQL_CONNECTION_STRING` ou `ConnectionStrings__MySql`) e um segredo JWT (`JWT_SECRET` ou `Jwt__Secret`) com pelo menos 32 bytes. Em produção, configure também issuer e audience conforme a API financeira. O ASP.NET Core não carrega arquivos `.env` automaticamente.

## Executar localmente

Na raiz do repositório, configure as variáveis no terminal e execute:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:MYSQL_CONNECTION_STRING = 'Server=localhost;Port=3306;Database=financas;User=app;Password=SUA_SENHA;'
$env:JWT_SECRET = 'UMA_CHAVE_LOCAL_COM_PELO_MENOS_32_BYTES'
dotnet run --project services/auth/TccProway.Api.csproj
```

Em `Development`, a documentação Swagger fica em `/swagger`. As migrations do EF Core podem ser aplicadas com:

```powershell
dotnet ef database update --project services/auth/TccProway.Api.csproj
```

## Imagem Docker

Na raiz do repositório:

```powershell
docker build -f services/auth/Dockerfile -t financas-auth .
```

O container escuta na porta `8080`. Em produção, publique o serviço somente em rede interna e configure HTTPS na comunicação entre serviços conforme a implantação.
