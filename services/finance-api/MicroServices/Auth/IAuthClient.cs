namespace Financas.MicroServices.Auth;

/// <summary>
/// Contrato com o microserviço de Autenticação (outro dev).
/// Adicione aqui os métodos conforme a API dele for definida (ex.: validar token, obter usuário).
/// </summary>
public interface IAuthClient
{
    Task<StatusServico> VerificarDisponibilidadeAsync(CancellationToken ct = default);
}
