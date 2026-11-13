using Financas.Models;
using Financas.Data;
using Microsoft.EntityFrameworkCore;

namespace Financas.Repositories;

public class TransacaoRepository(FinancasDbContext db) : ITransacaoRepository
{
    public async Task<IReadOnlyList<Transacao>> ListarAsync(Guid usuarioId, CancellationToken ct = default) =>
        await db.Transacoes.AsNoTracking().Where(t => t.UsuarioId == usuarioId)
            .OrderByDescending(t => t.Data).ToListAsync(ct);

    public Task<Transacao?> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default) =>
        db.Transacoes.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == usuarioId, ct);

    public async Task AdicionarAsync(Transacao transacao, CancellationToken ct = default)
    {
        db.Transacoes.Add(transacao);
        await db.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(Transacao transacao, CancellationToken ct = default)
    {
        db.Transacoes.Update(transacao);
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoverAsync(Transacao transacao, CancellationToken ct = default)
    {
        db.Transacoes.Remove(transacao);
        await db.SaveChangesAsync(ct);
    }
}
