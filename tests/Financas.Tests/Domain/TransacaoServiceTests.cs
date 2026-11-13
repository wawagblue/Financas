using Financas.Common;
using Financas.Models;
using Financas.Repositories;
using Financas.Services;
using Moq;

namespace Financas.Tests.Domain;

public class TransacaoServiceTests
{
    private static readonly Guid UsuarioId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly DateTime Agora = new(2026, 11, 11, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ITransacaoRepository> _repo = new();
    private readonly TransacaoService _service;

    public TransacaoServiceTests()
    {
        _service = new TransacaoService(_repo.Object, new RelogioFixo(Agora));
    }

    private static DadosTransacao DadosValidos() =>
        new("Mercado", 150.75m, TipoTransacao.Despesa, "Alimentação", new DateTime(2026, 11, 10));

    // ---------- Criar ----------

    [Fact]
    public async Task Criar_com_dados_validos_salva_e_retorna_transacao()
    {
        var resultado = await _service.CriarAsync(UsuarioId, DadosValidos());

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Id.Should().NotBeEmpty();
        resultado.Valor.Descricao.Should().Be("Mercado");
        resultado.Valor.CriadoEm.Should().Be(Agora);
        _repo.Verify(r => r.AdicionarAsync(It.IsAny<Transacao>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Criar_remove_espacos_e_normaliza_categoria_vazia()
    {
        var dados = DadosValidos() with { Descricao = "  Mercado  ", Categoria = "   " };

        var resultado = await _service.CriarAsync(UsuarioId, dados);

        resultado.Valor!.Descricao.Should().Be("Mercado");
        resultado.Valor.Categoria.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Criar_sem_descricao_falha_com_validacao(string? descricao)
    {
        var resultado = await _service.CriarAsync(UsuarioId, DadosValidos() with { Descricao = descricao });

        resultado.Sucesso.Should().BeFalse();
        resultado.TipoErro.Should().Be(TipoErro.Validacao);
        resultado.Erros.Should().Contain(e => e.Contains("descrição"));
        _repo.Verify(r => r.AdicionarAsync(It.IsAny<Transacao>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Criar_com_valor_nao_positivo_falha(decimal valor)
    {
        var resultado = await _service.CriarAsync(UsuarioId, DadosValidos() with { Valor = valor });

        resultado.TipoErro.Should().Be(TipoErro.Validacao);
        resultado.Erros.Should().Contain(e => e.Contains("valor"));
    }

    [Fact]
    public async Task Criar_com_tipo_inexistente_falha()
    {
        var resultado = await _service.CriarAsync(UsuarioId, DadosValidos() with { Tipo = (TipoTransacao)99 });

        resultado.TipoErro.Should().Be(TipoErro.Validacao);
    }

    [Fact]
    public async Task Criar_com_descricao_longa_demais_falha()
    {
        var descricao = new string('x', TransacaoService.TamanhoMaximoDescricao + 1);

        var resultado = await _service.CriarAsync(UsuarioId, DadosValidos() with { Descricao = descricao });

        resultado.TipoErro.Should().Be(TipoErro.Validacao);
    }

    [Fact]
    public async Task Criar_sem_data_falha()
    {
        var resultado = await _service.CriarAsync(UsuarioId, DadosValidos() with { Data = default });

        resultado.Erros.Should().Contain(e => e.Contains("data"));
    }

    [Fact]
    public async Task Criar_com_varios_erros_retorna_todos()
    {
        var resultado = await _service.CriarAsync(UsuarioId, new DadosTransacao(null, 0, TipoTransacao.Receita, null, default));

        resultado.Erros.Should().HaveCount(3);
    }

    // ---------- Obter ----------

    [Fact]
    public async Task Obter_existente_retorna_transacao()
    {
        var existente = new Transacao { Id = Guid.NewGuid(), Descricao = "X" };
        _repo.Setup(r => r.ObterPorIdAsync(existente.Id, UsuarioId, It.IsAny<CancellationToken>())).ReturnsAsync(existente);

        var resultado = await _service.ObterAsync(existente.Id, UsuarioId);

        resultado.Valor.Should().BeSameAs(existente);
    }

    [Fact]
    public async Task Obter_inexistente_retorna_nao_encontrado()
    {
        var resultado = await _service.ObterAsync(Guid.NewGuid(), UsuarioId);

        resultado.TipoErro.Should().Be(TipoErro.NaoEncontrado);
    }

    // ---------- Atualizar ----------

    [Fact]
    public async Task Atualizar_existente_altera_campos_e_data_de_atualizacao()
    {
        var existente = new Transacao { Id = Guid.NewGuid(), Descricao = "Antiga", Valor = 1, Tipo = TipoTransacao.Receita };
        _repo.Setup(r => r.ObterPorIdAsync(existente.Id, UsuarioId, It.IsAny<CancellationToken>())).ReturnsAsync(existente);

        var resultado = await _service.AtualizarAsync(existente.Id, UsuarioId, DadosValidos());

        resultado.Sucesso.Should().BeTrue();
        existente.Descricao.Should().Be("Mercado");
        existente.Valor.Should().Be(150.75m);
        existente.AtualizadoEm.Should().Be(Agora);
        _repo.Verify(r => r.AtualizarAsync(existente, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Atualizar_inexistente_retorna_nao_encontrado()
    {
        var resultado = await _service.AtualizarAsync(Guid.NewGuid(), UsuarioId, DadosValidos());

        resultado.TipoErro.Should().Be(TipoErro.NaoEncontrado);
        _repo.Verify(r => r.AtualizarAsync(It.IsAny<Transacao>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Atualizar_com_dados_invalidos_nao_altera_a_transacao()
    {
        var existente = new Transacao { Id = Guid.NewGuid(), Descricao = "Antiga", Valor = 1 };
        _repo.Setup(r => r.ObterPorIdAsync(existente.Id, UsuarioId, It.IsAny<CancellationToken>())).ReturnsAsync(existente);

        var resultado = await _service.AtualizarAsync(existente.Id, UsuarioId, DadosValidos() with { Valor = -1 });

        resultado.TipoErro.Should().Be(TipoErro.Validacao);
        existente.Descricao.Should().Be("Antiga");
        _repo.Verify(r => r.AtualizarAsync(It.IsAny<Transacao>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------- Remover ----------

    [Fact]
    public async Task Remover_existente_remove()
    {
        var existente = new Transacao { Id = Guid.NewGuid() };
        _repo.Setup(r => r.ObterPorIdAsync(existente.Id, UsuarioId, It.IsAny<CancellationToken>())).ReturnsAsync(existente);

        var resultado = await _service.RemoverAsync(existente.Id, UsuarioId);

        resultado.Sucesso.Should().BeTrue();
        _repo.Verify(r => r.RemoverAsync(existente, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Remover_inexistente_retorna_nao_encontrado()
    {
        var resultado = await _service.RemoverAsync(Guid.NewGuid(), UsuarioId);

        resultado.TipoErro.Should().Be(TipoErro.NaoEncontrado);
        _repo.Verify(r => r.RemoverAsync(It.IsAny<Transacao>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private sealed class RelogioFixo(DateTime agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(agora);
    }
}
