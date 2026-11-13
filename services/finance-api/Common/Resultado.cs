namespace Financas.Common;

public enum TipoErro
{
    Nenhum,
    Validacao,
    NaoEncontrado
}

/// <summary>
/// Resultado de uma operação de domínio: sucesso com valor, ou falha com tipo e mensagens de erro.
/// </summary>
public class Resultado<T>
{
    public bool Sucesso { get; }
    public T? Valor { get; }
    public TipoErro TipoErro { get; }
    public IReadOnlyList<string> Erros { get; }

    private Resultado(bool sucesso, T? valor, TipoErro tipoErro, IReadOnlyList<string> erros)
    {
        Sucesso = sucesso;
        Valor = valor;
        TipoErro = tipoErro;
        Erros = erros;
    }

    public static Resultado<T> Ok(T valor) => new(true, valor, TipoErro.Nenhum, []);

    public static Resultado<T> Invalido(IReadOnlyList<string> erros) => new(false, default, TipoErro.Validacao, erros);

    public static Resultado<T> NaoEncontrado(string mensagem) => new(false, default, TipoErro.NaoEncontrado, [mensagem]);
}
