# Guia para continuar em outro computador

Este pacote contém o código-fonte do Finanças, o frontend Angular, a API .NET, as migrações do banco e a configuração do Docker. Ele não inclui credenciais, bancos locais, caches ou dependências já compiladas.

## O que instalar

- Docker Desktop com suporte a containers Linux.
- Git é opcional se você recebeu o projeto pelo ZIP.
- Para desenvolver o Angular fora do Docker: Node.js 22.22 ou superior.
- Para desenvolver/compilar a API fora do Docker: .NET SDK 10.

Com Docker Desktop, o backend, MySQL e frontend podem ser compilados sem instalar Node.js ou .NET no computador.

## Preparar o ambiente

1. Extraia `Financas-portable.zip` em uma pasta sem sincronização em andamento, por exemplo `C:\Projetos\Financas`.
2. Abra essa pasta no terminal e crie seu arquivo de ambiente:

   ```powershell
   Copy-Item .env.example .env
   ```

3. Edite `.env` e substitua todos os placeholders de senha. Configure `JWT_SECRET` com uma chave aleatória de pelo menos 32 bytes. No PowerShell, você pode gerar uma com:

   ```powershell
   [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
   ```

4. Configure **o mesmo** `JWT_SECRET`, `JWT_ISSUER` e `JWT_AUDIENCE` no serviço de autenticação. Defina `AUTH_URL`, `IA_WHATSAPP_URL` e `OPEN_FINANCE_URL` com os endereços HTTPS reais dos serviços; fora de `Development`, a API rejeita URLs HTTP ou ausentes. Configure `CORS_ORIGIN` com a origem exata do frontend.
5. As portas padrão do frontend e da API são 4200 e 8081. A API publica 8081 apenas em loopback; o MySQL não publica porta no host.
6. Inicie a plataforma:

   ```powershell
   docker compose up --build -d
   ```

O Docker inicia o MySQL, API e frontend. A API aplica as migrações ao iniciar. As redes `frontend-api` e `api-database` limitam a comunicação: Nginx alcança a API, e o banco só é alcançado pela API. Swagger permanece desligado fora de Development. O Nginx deste Compose serve HTTP; TLS e HSTS exigem uma configuração de TLS de borda antes de publicar na internet.

## Endereços

- Aplicação: <http://localhost:4200>
- Status público dos serviços: <http://localhost:4200/status>
- API financeira, apenas para desenvolvimento local: <http://127.0.0.1:8081>
- Swagger é disponibilizado somente ao executar a API em ambiente Development.

Faça seu cadastro no app e entre com a conta criada. Não há usuário de demonstração embutido.

## Dados e segurança

- As transações, os orçamentos, as despesas recorrentes e as metas são filtrados pelo usuário autenticado.
- O serviço Auth emite os tokens. O gateway valida o JWT nas rotas privadas e deixa públicas apenas as rotas de cadastro, login, renovação e logout; a API financeira também valida o token antes de acessar dados.
- Configure `JWT_SECRET`, `JWT_ISSUER` e `JWT_AUDIENCE` iguais no Auth e na API Finanças. Só o serviço Auth deve emitir tokens; mantenha essa chave protegida.
- O volume Docker `mysql-data` persiste o banco entre reinicializações.
- `docker compose down` para os containers sem apagar o banco.
- **Não use `docker compose down -v` se quiser preservar os dados**: a opção `-v` apaga o volume do banco.
- O arquivo `.env` é local e não deve ser enviado ou compartilhado. O ZIP contém apenas `.env.example`.

## Desenvolvimento do Angular fora do Docker

Com a API e os serviços necessários rodando:

```powershell
cd frontend
npm ci
npm start
```

Abra <http://localhost:4200>. O proxy de desenvolvimento encaminha `/api` e `/gateway` à API principal em `http://localhost:8081`.

## Se algo não iniciar

- Confira se o Docker Desktop está aberto e usando Linux containers.
- Se 4200 ou 8081 estiver ocupada, ajuste `FRONTEND_PORTA` ou `API_PORTA` no `.env`.
- Confira os logs com `docker compose logs -f api mysql frontend`.
- Se a autenticação não responder, verifique `AUTH_URL` e confirme que as três configurações JWT são iguais nos dois serviços.
- Acesse `/status` para ver a disponibilidade reportada pelo gateway.
