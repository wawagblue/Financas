namespace Financas.MicroServices.IaWhatsApp;

/// <summary>
/// Contrato com o microserviço de IA + WhatsApp (outro dev).
/// Adicione aqui os métodos conforme a API dele for definida (ex.: enviar mensagem, classificar transação).
/// </summary>
public interface IIaWhatsAppClient
{
    Task<StatusServico> VerificarDisponibilidadeAsync(CancellationToken ct = default);
}
