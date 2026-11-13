namespace Financas.MicroServices;

public record StatusServico(string Nome, bool Disponivel, string? Detalhe = null);
