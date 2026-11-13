using System.Net;
using Financas.MicroServices;
using Financas.MicroServices.Auth;
using Financas.MicroServices.IaWhatsApp;
using Financas.MicroServices.OpenFinance;
using Microsoft.Extensions.Options;

namespace Financas.Tests.Infra;

public class ServicoExternoClientTests
{
    private static readonly IOptions<ServicosExternosOptions> Opcoes = Options.Create(new ServicosExternosOptions());

    private static HttpClient CriarHttp(Func<HttpRequestMessage, HttpResponseMessage> responder, bool comBaseUrl = true) =>
        new(new HandlerFalso(responder)) { BaseAddress = comBaseUrl ? new Uri("http://servico-falso/") : null };

    public static TheoryData<string> Clientes => ["auth", "ia", "openfinance"];

    private static ServicoExternoClientBase Criar(string cliente, HttpClient http) => cliente switch
    {
        "auth" => new AuthClient(http, Opcoes),
        "ia" => new IaWhatsAppClient(http, Opcoes),
        _ => new OpenFinanceClient(http, Opcoes)
    };

    [Theory]
    [MemberData(nameof(Clientes))]
    public async Task Servico_respondendo_200_esta_disponivel(string cliente)
    {
        string? caminhoChamado = null;
        var http = CriarHttp(req =>
        {
            caminhoChamado = req.RequestUri!.AbsolutePath;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var status = await Criar(cliente, http).VerificarDisponibilidadeAsync();

        status.Disponivel.Should().BeTrue();
        caminhoChamado.Should().Be("/health");
    }

    [Theory]
    [MemberData(nameof(Clientes))]
    public async Task Servico_respondendo_erro_esta_indisponivel(string cliente)
    {
        var http = CriarHttp(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        var status = await Criar(cliente, http).VerificarDisponibilidadeAsync();

        status.Disponivel.Should().BeFalse();
        status.Detalhe.Should().Contain("503");
    }

    [Theory]
    [MemberData(nameof(Clientes))]
    public async Task Servico_fora_do_ar_esta_indisponivel_sem_lancar_excecao(string cliente)
    {
        var http = CriarHttp(_ => throw new HttpRequestException("Connection refused"));

        var status = await Criar(cliente, http).VerificarDisponibilidadeAsync();

        status.Disponivel.Should().BeFalse();
        status.Detalhe.Should().Contain("Falha de conexão");
    }

    [Fact]
    public async Task Servico_que_estoura_timeout_esta_indisponivel()
    {
        var http = CriarHttp(_ => throw new TaskCanceledException("timeout"));

        var status = await new AuthClient(http, Opcoes).VerificarDisponibilidadeAsync();

        status.Disponivel.Should().BeFalse();
        status.Detalhe.Should().Contain("Tempo limite");
    }

    [Fact]
    public async Task Servico_sem_url_configurada_esta_indisponivel()
    {
        var http = CriarHttp(_ => new HttpResponseMessage(HttpStatusCode.OK), comBaseUrl: false);

        var status = await new AuthClient(http, Opcoes).VerificarDisponibilidadeAsync();

        status.Disponivel.Should().BeFalse();
        status.Detalhe.Should().Contain("não configurada");
    }

    private sealed class HandlerFalso(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            Task.FromResult(responder(request));
    }
}
