using Financas.Models;

namespace Financas.Repositories;

public interface ITransacaoRepository
{
    Task<IReadOnlyList<Transacao>> ListarAsync(Guid usuarioId, CancellationToken ct = default);
    Task<Transacao?> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
    Task AdicionarAsync(Transacao transacao, CancellationToken ct = default);
    Task AtualizarAsync(Transacao transacao, CancellationToken ct = default);
    Task RemoverAsync(Transacao transacao, CancellationToken ct = default);
}
