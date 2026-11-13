namespace Financas.MicroServices;

/// <summary>
/// Base comum dos clientes HTTP dos outros microserviços.
/// Nunca lança exceção por indisponibilidade: devolve um <see cref="StatusServico"/> indisponível.
/// </summary>
public abstract class ServicoExternoClientBase(HttpClient http, string nome, string caminhoHealth)
{
    protected HttpClient Http { get; } = http;

    public async Task<StatusServico> VerificarDisponibilidadeAsync(CancellationToken ct = default)
    {
        if (Http.BaseAddress is null)
            return new StatusServico(nome, false, "URL do serviço não configurada.");

        try
        {
            using var resposta = await Http.GetAsync(caminhoHealth, ct);
            return resposta.IsSuccessStatusCode
                ? new StatusServico(nome, true)
                : new StatusServico(nome, false, $"Respondeu HTTP {(int)resposta.StatusCode}.");
        }
        catch (HttpRequestException ex)
        {
            return new StatusServico(nome, false, $"Falha de conexão: {ex.Message}");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return new StatusServico(nome, false, "Tempo limite excedido.");
        }
    }
}
