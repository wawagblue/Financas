using System.Net;
using System.Net.Http.Json;
using Financas.MicroServices;
using Financas.MicroServices.Auth;
using Financas.MicroServices.IaWhatsApp;
using Financas.MicroServices.OpenFinance;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Financas.Tests.Api;

public class ServicosApiTests
{
    private static ApiFactory CriarFactory(bool authNoAr, bool iaNoAr, bool openFinanceNoAr)
    {
        var auth = new Mock<IAuthClient>();
        auth.Setup(c => c.VerificarDisponibilidadeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusServico("Autenticação", authNoAr));
        var ia = new Mock<IIaWhatsAppClient>();
        ia.Setup(c => c.VerificarDisponibilidadeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusServico("IA + WhatsApp", iaNoAr, iaNoAr ? null : "Falha de conexão: ia-whatsapp.interno:8080; stack trace"));
        var openFinance = new Mock<IOpenFinanceClient>();
        openFinance.Setup(c => c.VerificarDisponibilidadeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusServico("Open Finance", openFinanceNoAr));

        return new ApiFactory
        {
            ConfigurarServicos = services =>
            {
                services.AddSingleton(auth.Object);
                services.AddSingleton(ia.Object);
                services.AddSingleton(openFinance.Object);
            }
        };
    }

    [Fact]
    public async Task Status_com_todos_servicos_no_ar()
    {
        using var factory = CriarFactory(true, true, true);

        var status = await factory.CreateClient().GetFromJsonAsync<List<StatusServico>>("/api/servicos/status");

        status.Should().HaveCount(3).And.OnlyContain(s => s.Disponivel);
    }

    [Fact]
    public async Task Status_com_servico_fora_do_ar_continua_respondendo_200()
    {
        using var factory = CriarFactory(true, false, true);

        var resposta = await factory.CreateClient().GetAsync("/api/servicos/status");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        var status = await resposta.Content.ReadFromJsonAsync<List<StatusServico>>();
        status.Should().ContainSingle(s => !s.Disponivel).Which.Nome.Should().Be("IA + WhatsApp");
    }

    [Fact]
    public async Task Status_publico_nao_expoe_detalhes_internos()
    {
        using var factory = CriarFactory(true, false, true);

        var status = await factory.CreateClient().GetFromJsonAsync<List<StatusServico>>("/api/servicos/status");

        status.Should().ContainSingle(s => !s.Disponivel)
            .Which.Detalhe.Should().BeNull();
    }

    [Fact]
    public async Task Swagger_nao_e_exposto_fora_de_desenvolvimento_por_padrao()
    {
        using var factory = CriarFactory(true, true, true);

        var resposta = await factory.CreateClient().GetAsync("/swagger/v1/swagger.json");

        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

}
