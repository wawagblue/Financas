namespace Financas.MicroServices.OpenFinance;

/// <summary>
/// Contrato com o microserviço de Open Finance (outro dev).
/// Adicione aqui os métodos conforme a API dele for definida (ex.: listar contas, importar extrato).
/// </summary>
public interface IOpenFinanceClient
{
    Task<StatusServico> VerificarDisponibilidadeAsync(CancellationToken ct = default);
}
