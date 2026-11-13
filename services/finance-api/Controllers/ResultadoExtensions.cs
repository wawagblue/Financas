using Financas.Common;
using Microsoft.AspNetCore.Mvc;

namespace Financas.Controllers;

internal static class ResultadoExtensions
{
    /// <summary>Converte uma falha de domínio em 400 (validação) ou 404 (não encontrado).</summary>
    public static ActionResult ParaErro<T>(this ControllerBase controller, Resultado<T> resultado)
    {
        if (resultado.TipoErro == TipoErro.NaoEncontrado)
            return controller.Problem(
                title: "Recurso não encontrado",
                detail: string.Join(" ", resultado.Erros),
                statusCode: StatusCodes.Status404NotFound);

        var problema = new ValidationProblemDetails(
            new Dictionary<string, string[]> { ["Transacao"] = resultado.Erros.ToArray() })
        {
            Title = "Dados inválidos",
            Status = StatusCodes.Status400BadRequest
        };
        return controller.BadRequest(problema);
    }
}
