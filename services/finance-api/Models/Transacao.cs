
namespace Financas.Models;

public class Transacao
{
    public Guid Id { get; set; }
    /// <summary>Identificador do usuário dono da transação no serviço de autenticação.</summary>
    public Guid UsuarioId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public TipoTransacao Tipo { get; set; }
    public string? Categoria { get; set; }
    public DateTime Data { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}
