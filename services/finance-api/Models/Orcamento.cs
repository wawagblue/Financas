namespace Financas.Models;

public class Orcamento
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public decimal LimiteMensal { get; set; }
    public DateTime CriadoEm { get; set; }
}
