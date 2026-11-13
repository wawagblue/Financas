namespace Financas.Models;

public class DespesaRecorrente
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public int DiaVencimento { get; set; }
    public DateTime ProximoVencimento { get; set; }
    public DateTime CriadoEm { get; set; }
}
