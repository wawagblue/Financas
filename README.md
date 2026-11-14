# Finanças

Aplicação de finanças pessoais com frontend Angular, API financeira e gateway YARP, autenticação JWT e MySQL.

## Estrutura

    frontend/                 Aplicação Angular servida pelo Nginx
    services/
      auth/                   Cadastro, login, perfil e tokens
      finance-api/            API financeira e gateway YARP
    tests/                    Testes automatizados da API financeira
    scripts/                  Dados fictícios para demonstração
    docker-compose.yml        Compose de produção
    docker-compose.development.yml
                              Override local que inicia o Auth na rede interna

## Pré-requisitos

- Docker Desktop com Docker Compose
- .NET SDK 10 para compilar e executar testes
- Node.js 22 ou superior apenas para desenvolver o Angular fora do Docker

## Rodar o ambiente local integrado

Abra um terminal na raiz do repositório.

1. Crie o arquivo local de configuração:

    Copy-Item .env.example .env

   Se já existir um .env, não o substitua: compare-o com .env.example e acrescente as variáveis que estiverem faltando.

2. Edite o .env. Defina senhas fortes para MYSQL_PASSWORD e MYSQL_ROOT_PASSWORD. Gere JWT_SECRET com pelo menos 32 bytes; use o mesmo segredo, issuer e audience em todos os serviços que validam esses tokens. Um segredo local pode ser gerado no PowerShell:

    [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))

   Copie o resultado para JWT_SECRET. O arquivo .env é ignorado pelo Git.

3. Inicie frontend, API financeira, Auth e MySQL:

    docker compose -f docker-compose.yml -f docker-compose.development.yml up --build -d

   O override inicia o Auth em Development, acessível somente pela rede interna do Compose. Também configura o gateway para encaminhar as rotas de autenticação para esse container. As migrations do Auth são aplicadas ao iniciar em Development; a API financeira aplica as suas migrations ao iniciar.

4. Confira os containers e abra a aplicação:

    docker compose -f docker-compose.yml -f docker-compose.development.yml ps

   Frontend: http://localhost:4200
   Status dos serviços: http://localhost:4200/api/servicos/status
   Swagger da API financeira: http://localhost:4200/swagger

O MySQL não publica porta no host. A porta da API financeira é limitada a 127.0.0.1:8081; o frontend é a entrada normal do navegador. O Auth também não publica porta no host. No modo local, o tráfego API–Auth usa HTTP apenas na rede privada do Compose e ambos os serviços ficam em Development.

As URLs de IA + WhatsApp e Open Finance no .env.example são exemplos reservados e esses serviços não iniciam neste Compose. O app funciona sem eles, mas o status os mostrará indisponíveis. Para integrá-los, substitua os valores por URLs HTTPS reais.

### Criar uma conta e testar o login

O cadastro exige senha com pelo menos 12 caracteres e até 72 bytes em UTF-8. Exemplo em PowerShell:

    $email = 'demo.local@financas.test'
    $senha = Read-Host 'Informe uma senha local com pelo menos 12 caracteres'
    $cadastro = @{ nome = 'Pessoa Demo'; email = $email; password = $senha } | ConvertTo-Json
    Invoke-RestMethod -Method Post -Uri 'http://localhost:4200/gateway/auth/api/v1/users' -ContentType 'application/json' -Body $cadastro

    $login = @{ email = $email; password = $senha } | ConvertTo-Json
    $tokens = Invoke-RestMethod -Method Post -Uri 'http://localhost:4200/gateway/auth/api/v1/auth/login' -ContentType 'application/json' -Body $login
    $tokens.accessToken

O frontend faz o login pela interface. Os tokens ficam em memória no navegador; atualizar a página encerra a sessão local. O Auth também tem um guia próprio em services/auth/README.md.

### Dados fictícios para demonstração

Para criar uma conta aleatória e preencher transações, orçamentos, recorrências e uma meta:

    .\scripts\popular-demo.ps1 -Preview
    .\scripts\popular-demo.ps1

A senha é gerada e salva somente em demo-credentials.local.json, ignorado pelo Git. Consulte esse arquivo localmente; não publique nem envie a senha.

## Compose de produção

O Compose principal, sem override, inicia frontend, API financeira e MySQL. Ele não inicia Auth, IA + WhatsApp ou Open Finance: configure AUTH_URL, IA_WHATSAPP_URL e OPEN_FINANCE_URL com endereços HTTPS reais e acessíveis antes de iniciar:

    docker compose up --build -d

Fora de Development, a API rejeita URLs de serviços externos que não usem HTTPS. Configure CORS_ORIGIN com a origem exata do frontend. O Compose não configura certificados TLS no Nginx; em uma implantação pública, termine TLS em um proxy/load balancer configurado e proteja o tráfego até o Nginx.

Nunca use docker-compose.development.yml em produção. Ele habilita HTTP interno para facilitar desenvolvimento e aplica migrations do Auth ao iniciar.

## Parar e preservar os dados

Para parar os containers sem apagar os dados:

    docker compose -f docker-compose.yml -f docker-compose.development.yml down

Para apagar também o banco e os dados locais:

    docker compose -f docker-compose.yml -f docker-compose.development.yml down -v

Atenção: down -v remove permanentemente o volume MySQL local.

## Desenvolvimento do Angular fora do Docker

Com a API e os serviços necessários em execução:

    cd frontend
    npm ci
    npm start

O Angular abre em http://localhost:4200 e encaminha /api e /gateway para a API em localhost:8081.

## Testes e build

Na raiz do repositório:

    dotnet build Financas.slnx
    dotnet test Financas.slnx

A suíte atual cobre a API financeira. Ainda não há uma suíte automatizada própria para o serviço Auth.

## Endpoints principais

- Auth: POST /gateway/auth/api/v1/users
- Login: POST /gateway/auth/api/v1/auth/login
- Refresh: POST /gateway/auth/api/v1/auth/refresh-token
- Logout: POST /gateway/auth/api/v1/auth/logout
- Perfil: GET e PUT /gateway/auth/api/v1/users/me
- Transações: /api/transacoes
- Orçamentos, recorrências, metas e relatórios: /api/orcamentos, /api/recorrencias, /api/metas e /api/relatorios
- Status público mínimo dos serviços: GET /api/servicos/status

As rotas financeiras exigem Bearer JWT e filtram os registros pelo identificador do usuário autenticado. Consulte o Swagger da API financeira para os schemas e respostas.

### Contrato de integração para os outros módulos

O frontend deve chamar o gateway em `/gateway/auth/...`; o YARP remove o prefixo `/gateway/auth` antes de encaminhar a requisição ao Auth. Assim, por exemplo, `/gateway/auth/api/v1/auth/login` chega ao Auth como `/api/v1/auth/login`. O frontend não deve chamar o container Auth diretamente.

| Ação | Método e rota pelo gateway | Corpo e resultado normal |
|---|---|---|
| Cadastro | `POST /gateway/auth/api/v1/users` | `{"nome":"Ana Demo","email":"ana@example.test","password":"senha-com-12-caracteres"}` → `201`; email duplicado → `409` |
| Login | `POST /gateway/auth/api/v1/auth/login` | `{"email":"ana@example.test","password":"senha-com-12-caracteres"}` → `200`; credencial inválida → `401` |
| Renovar sessão | `POST /gateway/auth/api/v1/auth/refresh-token` | `{"refreshToken":"<refresh-token-atual>"}` → `200`; token inválido → `401` |
| Logout | `POST /gateway/auth/api/v1/auth/logout` | `{"refreshToken":"<refresh-token-atual>"}` → `204` |
| Consultar perfil | `GET /gateway/auth/api/v1/users/me` | Sem corpo; requer Bearer; responde `200` ou `401` |
| Atualizar perfil | `PUT /gateway/auth/api/v1/users/me` | `{"nome":"Ana","email":"ana@example.test"}`; requer Bearer |

Login retorna `accessToken`, `refreshToken` e `accessTokenExpiresAtUtc`. Envie o access token nas chamadas protegidas como `Authorization: Bearer <accessToken>`. O refresh token é um valor aleatório de 64 bytes; somente seu hash SHA-256 é persistido. Ele é rotacionado: depois de renovar, substitua os dois tokens pelos valores novos; o refresh token anterior deixa de valer. Logout revoga o refresh token enviado e responde `204`. O logout não invalida um access token já emitido; ele continua válido por até 15 minutos, e o frontend deve limpar a sessão imediatamente.

O JWT usa HS256 e contém o identificador do usuário na claim `sub`. Todo serviço que guarda dados do usuário deve validar o JWT, extrair `sub` e usar esse valor como dono dos registros. Não use um `userId` fornecido pelo frontend nem confie em headers como `X-User-Id`. O gateway e a API financeira já validam o token; os módulos também devem validar a identidade ao proteger seus próprios endpoints.

Para validar tokens, os serviços precisam concordar em `JWT_SECRET`, issuer (`TccProway`) e audience (`TccProway.Client`). Na API financeira, configure `JWT_SECRET`, `JWT_ISSUER` e `JWT_AUDIENCE`; no Auth, `JWT_SECRET`, `Jwt__Issuer` e `Jwt__Audience`. O Compose de desenvolvimento já compartilha e mapeia esses valores. Como HS256 é simétrico, qualquer serviço que recebe o segredo também poderia emitir tokens; isso é aceitável para a demonstração controlada, mas deve ser revisto antes de uma implantação distribuída em produção.

## Estado da autenticação

A autenticação está implementada como MVP funcional: cadastro, login, perfil, senha com BCrypt, access token JWT, refresh token persistido como hash, rotação no refresh e revogação do refresh token no logout. A API financeira e o gateway validam assinatura, issuer, audience e expiração. O access token dura 15 minutos e o refresh token, 7 dias por padrão.

Ainda não considero o Auth pronto para produção. Antes disso, priorize:

1. Testes automatizados de cadastro, login, refresh, logout e concorrência.
2. Limite de tentativas aplicado em toda entrada pública do Auth, inclusive se alguém conseguir contornar o Nginx.
3. Detecção de reuso de refresh token e revogação da família de sessão.
4. Evitar enumeração de contas na resposta de cadastro.
5. Avaliar assinatura assimétrica e JWKS para não distribuir a chave HMAC compartilhada a todos os serviços.
6. Fluxos de verificação de e-mail, recuperação de senha e, conforme o produto, MFA.
7. Separar o banco/schema do Auth do banco da API financeira e definir migrações de produção como etapa de deploy.

O login e as rotas protegidas funcionam no ambiente local integrado descrito acima. A classificação de MVP não substitui uma auditoria de segurança antes de expor o serviço à internet.

### Checklist para a apresentação

1. Suba o ambiente com `docker-compose.development.yml` e confirme que o status mostra Autenticação disponível.
2. Cadastre uma conta de demonstração ou rode `scripts/popular-demo.ps1` para inserir dados fictícios.
3. Demonstre cadastro, login, carregamento do perfil e acesso às transações com sessão ativa.
4. Renove a sessão usando o refresh token e confirme que a aplicação substitui o token antigo.
5. Faça logout e confirme que uma chamada protegida sem access token recebe `401`.

Esses fluxos bastam para a demonstração acadêmica. Deixe os itens de produção acima como evolução documentada; priorize agora a integração visual no Angular e a repetição desta checklist no ambiente que será usado na banca.

## Configuração local

As variáveis e seus exemplos estão em .env.example. Nunca versione .env, senhas, tokens ou demo-credentials.local.json. Se alterar as credenciais do MySQL depois de inicializar o volume, a alteração no .env não modifica automaticamente a senha armazenada no banco.
