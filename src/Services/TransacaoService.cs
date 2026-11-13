using Financas.Common;
using Financas.Models;
using Financas.Repositories;

namespace Financas.Services;

public record DadosTransacao(string? Descricao, decimal Valor, TipoTransacao Tipo, string? Categoria, DateTime Data);

public class TransacaoService(ITransacaoRepository repository, TimeProvider relogio)
{
    public const int TamanhoMaximoDescricao = 200;
    public const int TamanhoMaximoCategoria = 100;

    public Task<IReadOnlyList<Transacao>> ListarAsync(Guid usuarioId, CancellationToken ct = default) =>
        repository.ListarAsync(usuarioId, ct);

    public async Task<Resultado<Transacao>> ObterAsync(Guid id, Guid usuarioId, CancellationToken ct = default)
    {
        var transacao = await repository.ObterPorIdAsync(id, usuarioId, ct);
        return transacao is null
            ? Resultado<Transacao>.NaoEncontrado(MensagemNaoEncontrada(id))
            : Resultado<Transacao>.Ok(transacao);
    }

    public async Task<Resultado<Transacao>> CriarAsync(Guid usuarioId, DadosTransacao dados, CancellationToken ct = default)
    {
        var erros = Validar(dados);
        if (erros.Count > 0)
            return Resultado<Transacao>.Invalido(erros);

        var transacao = new Transacao
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            CriadoEm = relogio.GetUtcNow().UtcDateTime
        };
        Aplicar(transacao, dados);

        await repository.AdicionarAsync(transacao, ct);
        return Resultado<Transacao>.Ok(transacao);
    }

    public async Task<Resultado<Transacao>> AtualizarAsync(Guid id, Guid usuarioId, DadosTransacao dados, CancellationToken ct = default)
    {
        var transacao = await repository.ObterPorIdAsync(id, usuarioId, ct);
        if (transacao is null)
            return Resultado<Transacao>.NaoEncontrado(MensagemNaoEncontrada(id));

        var erros = Validar(dados);
        if (erros.Count > 0)
            return Resultado<Transacao>.Invalido(erros);

        Aplicar(transacao, dados);
        transacao.AtualizadoEm = relogio.GetUtcNow().UtcDateTime;

        await repository.AtualizarAsync(transacao, ct);
        return Resultado<Transacao>.Ok(transacao);
    }

    public async Task<Resultado<Transacao>> RemoverAsync(Guid id, Guid usuarioId, CancellationToken ct = default)
    {
        var transacao = await repository.ObterPorIdAsync(id, usuarioId, ct);
        if (transacao is null)
            return Resultado<Transacao>.NaoEncontrado(MensagemNaoEncontrada(id));

        await repository.RemoverAsync(transacao, ct);
        return Resultado<Transacao>.Ok(transacao);
    }

    private static List<string> Validar(DadosTransacao dados)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(dados.Descricao))
            erros.Add("A descrição é obrigatória.");
        else if (dados.Descricao.Trim().Length > TamanhoMaximoDescricao)
            erros.Add($"A descrição deve ter no máximo {TamanhoMaximoDescricao} caracteres.");

        if (dados.Valor <= 0)
            erros.Add("O valor deve ser maior que zero.");

        if (!Enum.IsDefined(dados.Tipo))
            erros.Add("O tipo deve ser Receita ou Despesa.");

        if (dados.Categoria is { Length: > TamanhoMaximoCategoria })
            erros.Add($"A categoria deve ter no máximo {TamanhoMaximoCategoria} caracteres.");

        if (dados.Data == default)
            erros.Add("A data é obrigatória.");

        return erros;
    }

    private static void Aplicar(Transacao transacao, DadosTransacao dados)
    {
        transacao.Descricao = dados.Descricao!.Trim();
        transacao.Valor = dados.Valor;
        transacao.Tipo = dados.Tipo;
        transacao.Categoria = string.IsNullOrWhiteSpace(dados.Categoria) ? null : dados.Categoria.Trim();
        transacao.Data = dados.Data;
    }

    private static string MensagemNaoEncontrada(Guid id) => $"Transação {id} não encontrada.";
}
