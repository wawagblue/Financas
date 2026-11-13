# Finanças – Serviço Principal

Backend principal de um site de finanças com integração ao WhatsApp. Este serviço é dono do banco de dados e do frontend, e funciona como **gateway** para os outros microserviços da equipe.

## Arquitetura

O sistema é dividido em 4 microserviços:

| Serviço | Responsável | Acesso pelo gateway |
|---|---|---|
| Autenticação | Wagner | `/gateway/auth/*` |
| IA + WhatsApp | Gabriel Sá | `/gateway/ia-whatsapp/*` |
| Open Finance | João P. C. | `/gateway/open-finance/*` |
| **Serviço principal** (este repositório) | Pedro Demmer | `/api/*` |

```
Frontend (Angular) ──► Serviço principal ──► MySQL
                           │
                           ├──► /gateway/auth/*          ──► Autenticação
                           ├──► /gateway/ia-whatsapp/*   ──► IA + WhatsApp
                           └──► /gateway/open-finance/*  ──► Open Finance
```

Quando um dos outros serviços está fora do ar, a API não quebra: `/api/servicos/status` informa que ele está indisponível, e o gateway responde `502`.

## Tecnologias

- **Backend:** C# / ASP.NET Core Web API (.NET 10)
- **Banco:** MySQL 8.4 com EF Core 9 + Pomelo
- **Gateway:** YARP (Reverse Proxy)
- **Documentação da API:** Swagger
- **Testes:** xUnit + FluentAssertions + Moq
- **Ambiente:** Docker + Docker Compose
- **Frontend:** Angular 22 (standalone, TypeScript, SCSS)

## Estrutura do projeto

```
src/
├── Controllers/       Endpoints da API
├── Contracts/         DTOs de request/response
├── Models/            Entidades e enums
├── Services/          Regras de negócio
├── Repositories/      Interfaces e implementações de acesso a dados
├── Data/              DbContext, configurações do EF e Migrations
├── MicroServices/     Clientes HTTP dos outros microserviços
│   ├── Auth/
│   ├── IaWhatsApp/
│   └── OpenFinance/
├── Common/            Tipos compartilhados
├── DependencyInjection.cs
├── Program.cs
└── Dockerfile
tests/
└── Financas.Tests/    Testes automatizados
frontend/              Aplicação Angular
├── src/app/core/       Autenticação, interceptor JWT e serviços HTTP
├── Dockerfile          Build Angular + Nginx
└── nginx.conf          Proxy de /api e /gateway para a API
scripts/popular-demo.ps1  Cria uma conta local e dados fictícios para demonstração
docker-compose.yml
.env.example
```

## Como rodar

### Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET SDK 10](https://dotnet.microsoft.com/download) (para rodar fora do Docker e rodar os testes)
- [Node.js 22.22 ou superior](https://nodejs.org/) (para desenvolver o frontend localmente)

### Com Docker (recomendado)

```bash
# 1. Crie o arquivo de variáveis de ambiente
cp .env.example .env

# 2. Substitua as senhas de exemplo e configure a mesma JWT_SECRET do serviço de autenticação
# 3. Suba frontend, API e MySQL
docker compose up --build
```

Inicie também os três serviços integrados com URLs HTTPS em `AUTH_URL`, `IA_WHATSAPP_URL` e `OPEN_FINANCE_URL`; fora de `Development`, a API recusa iniciar se qualquer URL estiver ausente ou usar HTTP. Configure `CORS_ORIGIN` com a origem exata do frontend. O frontend/Nginx fica em **http://localhost:4200** neste Compose (TLS de borda não está configurado). A API financeira fica limitada a **127.0.0.1:8081** para desenvolvimento local; o acesso publicado para clientes passa pelo Nginx. O Swagger só é habilitado em `Development`. O MySQL não publica porta no host e só recebe conexões da API pela rede interna.

O Nginx aplica CSP sem `unsafe-eval`, proteção contra MIME sniffing e framing, política de referrer/permissões, limite de corpo de 1 MiB, timeouts e rate limits separados para login, cadastro e refresh. `unsafe-inline` é permitido somente em estilos porque o Angular injeta estilos de componentes em elementos `<style>` durante a execução. HSTS depende de TLS e não é enviado enquanto o Nginx servir apenas HTTP.

As migrações do banco são aplicadas automaticamente quando a API sobe.

### Frontend em desenvolvimento

Com a autenticação e a API financeira em execução, inicie o Angular:

```bash
cd frontend
npm install
npm start
```

Abra **http://localhost:4200**. O proxy de desenvolvimento encaminha `/api` e `/gateway` para `http://localhost:8081`. Access e refresh tokens ficam em memória; a renovação é tentada automaticamente após uma resposta `401`, e atualizar a página encerra a sessão local.

### Dados de demonstração

Depois de iniciar o frontend, a API, o banco e o serviço Auth configurado no gateway, confira primeiro a prévia:

```powershell
.\scripts\popular-demo.ps1 -Preview
```

Para criar a conta de demonstração e preencher o app com dados fictícios:

```powershell
.\scripts\popular-demo.ps1
```

O script registra `demo@financas.test` no Auth, gera uma senha aleatória e guarda as credenciais em `demo-credentials.local.json`, na raiz do repositório. O arquivo está no `.gitignore`; use-o para entrar no app e **não o publique nem compartilhe**. Nenhuma conta ou senha vem pronta no repositório público. Se o e-mail já estiver cadastrado, use uma conta sua com `-UsarContaExistente -Email seu@email` e informe a senha no prompt.

Os exemplos cobrem seis meses de receitas e despesas, quatro orçamentos, três despesas recorrentes e a meta “Reserva de emergência”. O script evita duplicar itens se for executado novamente. Para usar outro endereço do frontend, passe `-BaseUrl`, por exemplo `-BaseUrl http://localhost:4300`.

### Fora do Docker (Visual Studio / `dotnet run`)

O `.env` é lido só pelo Docker Compose. O Compose não publica a porta do MySQL; por isso, `dotnet run` diretamente no host requer um banco de desenvolvimento separado e uma connection string em *user-secrets*. O fluxo recomendado é executar API, banco e frontend pelo Compose.

```bash
dotnet user-secrets set "ConnectionStrings:Financas" "<connection string do banco de desenvolvimento>" --project src
dotnet run --project src
```

Acesse o Swagger em **http://localhost:5030/swagger**.

> No Visual Studio, use o perfil de execução **`http`**.

### Parar

```bash
docker compose down        # para os containers
docker compose down -v     # para e APAGA os dados do banco
```

## Variáveis de ambiente

Definidas no arquivo `.env` (copiado de `.env.example`). **Nunca commite o `.env`.**

| Variável | Descrição | Padrão |
|---|---|---|
| `MYSQL_DATABASE` | Nome do banco | `financas` |
| `MYSQL_USER` / `MYSQL_PASSWORD` | Usuário e senha da aplicação | — |
| `MYSQL_ROOT_PASSWORD` | Senha do root do MySQL | — |
| `API_PORTA` | Porta loopback da API para desenvolvimento local | `8081` |
| `FRONTEND_PORTA` | Porta do frontend na sua máquina | `4200` |
| `AUTH_URL` | URL HTTPS do serviço de Autenticação | — |
| `IA_WHATSAPP_URL` | URL HTTPS do serviço de IA + WhatsApp | — |
| `OPEN_FINANCE_URL` | URL HTTPS do serviço de Open Finance | — |
| `CORS_ORIGIN` | Origem exata autorizada para CORS | `http://localhost:4200` |

> Mudar a senha no `.env` depois que o banco já foi criado **não** altera a senha no MySQL. Para recriar o banco, use `docker compose down -v`.

### Acessar o MySQL localmente

O MySQL não tem porta publicada. Para abrir o cliente dentro do container, use:

```powershell
docker compose exec mysql mysql -u financas_app -p financas
```

Informe `MYSQL_PASSWORD` do seu `.env` quando solicitado. Ajuste usuário e banco no comando se tiver alterado `MYSQL_USER` ou `MYSQL_DATABASE`.

## Endpoints

### Transações

| Método | Rota | Descrição | Respostas |
|---|---|---|---|
| `GET` | `/api/transacoes` | Lista as transações (mais recentes primeiro) | `200` |
| `GET` | `/api/transacoes/{id}` | Busca uma transação | `200`, `404` |
| `POST` | `/api/transacoes` | Cria uma transação | `201`, `400` |
| `PUT` | `/api/transacoes/{id}` | Atualiza uma transação | `200`, `400`, `404` |
| `DELETE` | `/api/transacoes/{id}` | Remove uma transação | `204`, `404` |

Exemplo de corpo para `POST` / `PUT`:

```json
{
  "descricao": "Salário de novembro",
  "valor": 5000.00,
  "tipo": "Receita",
  "categoria": "Trabalho",
  "data": "2026-11-05T00:00:00"
}
```

- `tipo`: `Receita` ou `Despesa`
- `valor` precisa ser maior que zero
- `descricao` é obrigatória
- `categoria` tem no máximo 100 caracteres

### Serviços externos

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/servicos/status` | Mostra se Autenticação, IA + WhatsApp e Open Finance estão disponíveis |

### Planejamento financeiro

| Método | Rota | Descrição |
|---|---|---|
| `GET`, `POST`, `DELETE` | `/api/orcamentos` | Limites mensais por categoria e gasto atual |
| `GET`, `POST`, `DELETE` | `/api/recorrencias` | Contas mensais e próximo vencimento |
| `POST` | `/api/recorrencias/{id}/pagar` | Registra a despesa e avança o próximo vencimento em um mês |
| `GET`, `POST`, `DELETE` | `/api/metas` | Metas de economia |
| `POST` | `/api/metas/{id}/depositos` | Adiciona valor guardado a uma meta |
| `GET` | `/api/relatorios?mes=AAAA-MM` | Resumo mensal por categoria e histórico de seis meses |

Todas essas rotas exigem o mesmo JWT e limitam os dados ao usuário autenticado. A tela Angular oferece exportação do relatório em CSV e impressão/salvamento em PDF pelo navegador.

### Gateway

As chamadas em `/gateway/{servico}/...` são encaminhadas ao microserviço correspondente, removendo o prefixo do serviço. As rotas de IA + WhatsApp e Open Finance, além das rotas privadas de Auth, exigem um JWT válido no gateway. Cadastro, login, renovação e logout ficam públicos para permitir iniciar e encerrar uma sessão. Exemplo: `POST /gateway/auth/api/v1/auth/login` chega ao serviço Auth como `POST /api/v1/auth/login`.

### Autenticação das transações

Cadastro, login e emissão/renovação de tokens continuam sob responsabilidade do microserviço de autenticação, que é a autoridade de identidade. O gateway valida o JWT antes de encaminhar chamadas protegidas. A API financeira também valida assinatura, emissor, público e validade do JWT e filtra os registros pelo `sub` do usuário. Configure a mesma `JWT_SECRET`, `JWT_ISSUER` e `JWT_AUDIENCE` nos serviços. Assim, autenticação no gateway não substitui a autorização feita por cada serviço que protege seus próprios dados.

O identificador do usuário vem da claim `sub` emitida pelo serviço de autenticação e é salvo como `UsuarioId` em cada transação. Listagem, leitura, edição e exclusão filtram por esse identificador. A API de finanças não mantém uma segunda tabela de usuários: o cadastro continua pertencendo ao serviço de autenticação.

## Testes

```bash
dotnet test
```

Os testes cobrem cenários de **sucesso** e de **falha**: dado inválido, recurso inexistente e serviço externo fora do ar. Eles usam banco em memória, então não precisam do Docker rodando.

## Migrações do banco

```bash
# uma vez, após clonar
dotnet tool restore

# criar uma nova migração
dotnet ef migrations add <NomeDaMigracao> -p src -s src -o Data/Migrations
```

## Padrões da equipe

- Toda funcionalidade nova tem testes de sucesso **e** de falha.
- Rode `dotnet test` antes de abrir um PR.
- A comunicação com os outros microserviços passa só pelas interfaces em `src/MicroServices` (ex.: `IAuthClient`), para poder mockar nos testes.
- Segredos só em variáveis de ambiente / `.env`.
- Commits pequenos, com mensagens em português.
