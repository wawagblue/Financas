namespace Financas.Models;

public class MetaEconomia
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal ValorAlvo { get; set; }
    public decimal ValorGuardado { get; set; }
    public DateTime? Prazo { get; set; }
    public DateTime CriadoEm { get; set; }
}
