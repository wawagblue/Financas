using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Financas.Contracts;
using Financas.Models;

namespace Financas.Tests.Api;

public class TransacoesApiTests : IDisposable
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ApiFactory _factory = new();
    private readonly HttpClient _client;

    public TransacoesApiTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose() => _factory.Dispose();

    private static object RequestValido(string descricao = "Salário") => new
    {
        descricao,
        valor = 5000m,
        tipo = "Receita",
        categoria = "Trabalho",
        data = "2026-11-05T00:00:00"
    };

    private async Task<TransacaoResponse> CriarAsync(string descricao = "Salário")
    {
        var resposta = await _client.PostAsJsonAsync("/api/transacoes", RequestValido(descricao));
        resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await resposta.Content.ReadFromJsonAsync<TransacaoResponse>(Json))!;
    }

    // ---------- Sucesso ----------

    [Fact]
    public async Task Post_cria_transacao_e_retorna_201_com_location()
    {
        var resposta = await _client.PostAsJsonAsync("/api/transacoes", RequestValido());

        resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        var criada = await resposta.Content.ReadFromJsonAsync<TransacaoResponse>(Json);
        criada!.Descricao.Should().Be("Salário");
        criada.Tipo.Should().Be(TipoTransacao.Receita);
        resposta.Headers.Location!.ToString().Should().EndWith($"/api/transacoes/{criada.Id}");
    }

    [Fact]
    public async Task Get_lista_transacoes_criadas()
    {
        await CriarAsync("A");
        await CriarAsync("B");

        var lista = await _client.GetFromJsonAsync<List<TransacaoResponse>>("/api/transacoes", Json);

        lista.Should().HaveCount(2);
    }

    [Fact]
    public async Task Get_por_id_retorna_transacao()
    {
        var criada = await CriarAsync();

        var obtida = await _client.GetFromJsonAsync<TransacaoResponse>($"/api/transacoes/{criada.Id}", Json);

        obtida!.Id.Should().Be(criada.Id);
    }

    [Fact]
    public async Task Put_atualiza_transacao()
    {
        var criada = await CriarAsync();

        var resposta = await _client.PutAsJsonAsync($"/api/transacoes/{criada.Id}", new
        {
            descricao = "Aluguel",
            valor = 1200m,
            tipo = "Despesa",
            data = "2026-11-06T00:00:00"
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        var atualizada = await resposta.Content.ReadFromJsonAsync<TransacaoResponse>(Json);
        atualizada!.Descricao.Should().Be("Aluguel");
        atualizada.Tipo.Should().Be(TipoTransacao.Despesa);
        atualizada.AtualizadoEm.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_remove_transacao()
    {
        var criada = await CriarAsync();

        var resposta = await _client.DeleteAsync($"/api/transacoes/{criada.Id}");

        resposta.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _client.GetAsync($"/api/transacoes/{criada.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---------- Falha ----------

    [Fact]
    public async Task Post_com_dados_invalidos_retorna_400_com_erros()
    {
        var resposta = await _client.PostAsJsonAsync("/api/transacoes", new
        {
            descricao = "",
            valor = -5m,
            tipo = "Receita",
            data = "2026-11-05T00:00:00"
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var corpo = await resposta.Content.ReadAsStringAsync();
        corpo.Should().Contain("descrição").And.Contain("valor");
    }

    [Fact]
    public async Task Post_com_tipo_desconhecido_retorna_400()
    {
        var resposta = await _client.PostAsJsonAsync("/api/transacoes", new
        {
            descricao = "X",
            valor = 10m,
            tipo = "Investimento",
            data = "2026-11-05T00:00:00"
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_sem_tipo_retorna_400()
    {
        var resposta = await _client.PostAsJsonAsync("/api/transacoes", new
        {
            descricao = "X",
            valor = 10m,
            data = "2026-11-05T00:00:00"
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_com_json_malformado_retorna_400()
    {
        var conteudo = new StringContent("{ isso não é json", Encoding.UTF8, "application/json");

        var resposta = await _client.PostAsync("/api/transacoes", conteudo);

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Get_por_id_inexistente_retorna_404()
    {
        var resposta = await _client.GetAsync($"/api/transacoes/{Guid.NewGuid()}");

        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_com_id_que_nao_e_guid_retorna_404()
    {
        var resposta = await _client.GetAsync("/api/transacoes/abc");

        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_inexistente_retorna_404()
    {
        var resposta = await _client.PutAsJsonAsync($"/api/transacoes/{Guid.NewGuid()}", RequestValido());

        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_com_dados_invalidos_retorna_400()
    {
        var criada = await CriarAsync();

        var resposta = await _client.PutAsJsonAsync($"/api/transacoes/{criada.Id}", new
        {
            descricao = "X",
            valor = 0m,
            tipo = "Despesa",
            data = "2026-11-06T00:00:00"
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_inexistente_retorna_404()
    {
        var resposta = await _client.DeleteAsync($"/api/transacoes/{Guid.NewGuid()}");

        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
