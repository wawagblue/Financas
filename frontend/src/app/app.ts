import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UpperCasePipe } from '@angular/common';
import { AuthService } from './core/auth.service';
import { MetaEconomia, Orcamento, Recorrencia, Relatorio, StatusServico, TipoTransacao, Transacao, TransacaoInput } from './core/api.models';
import { TransacoesService } from './core/transacoes.service';
import { ServicosService } from './core/servicos.service';
import { PlanejamentoService } from './core/planejamento.service';

type Filtro = 'Todas' | 'Receitas' | 'Despesas';
type FormTransacao = { descricao: string; valor: number | null; tipo: TipoTransacao; categoria: string; data: string };
type Pagina = 'inicio' | 'transacoes' | 'planejamento' | 'relatorios';

const NOVA_TRANSACAO: FormTransacao = {
  descricao: '', valor: null, tipo: 'Despesa', categoria: '', data: new Date().toISOString().slice(0, 10),
};

@Component({
  selector: 'app-root',
  imports: [FormsModule, UpperCasePipe],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly Math = Math;
  protected readonly isStatusPage = window.location.pathname.replace(/\/+$/, '') === '/status';
  protected readonly hoje = new Intl.DateTimeFormat('pt-BR', { weekday: 'long', day: '2-digit', month: 'long', year: 'numeric' }).format(new Date());
  protected readonly ano = new Date().getFullYear();
  protected readonly auth = inject(AuthService);
  private readonly transacoesApi = inject(TransacoesService);
  private readonly servicosApi = inject(ServicosService);
  private readonly planejamentoApi = inject(PlanejamentoService);

  protected readonly modoAcesso = signal<'entrar' | 'criar'>('entrar');
  protected readonly pagina = signal<Pagina>('inicio');
  protected readonly filtro = signal<Filtro>('Todas');
  protected readonly transacoes = signal<Transacao[]>([]);
  protected readonly statusServicos = signal<StatusServico[]>([]);
  protected readonly consultandoServicos = signal(false);
  protected readonly atualizadoEm = signal<string | null>(null);
  protected readonly mesSelecionado = signal(new Date().toISOString().slice(0, 7));
  protected readonly orcamentos = signal<Orcamento[]>([]);
  protected readonly recorrencias = signal<Recorrencia[]>([]);
  protected readonly metas = signal<MetaEconomia[]>([]);
  protected readonly relatorio = signal<Relatorio | null>(null);
  protected readonly categoriaOrcamento = signal('');
  protected readonly limiteOrcamento = signal<number | null>(null);
  protected readonly formularioRecorrencia = { descricao: '', valor: null as number | null, categoria: '', proximoVencimento: new Date().toISOString().slice(0, 10) };
  protected readonly formularioMeta = { nome: '', valorAlvo: null as number | null, prazo: '' };
  protected readonly deposito = signal<number | null>(null);
  protected readonly metaDoDeposito = signal('');
  protected readonly carregando = signal(false);
  protected readonly enviando = signal(false);
  protected readonly modalAberto = signal(false);
  protected readonly editandoId = signal<string | null>(null);
  protected readonly mensagemErro = signal('');
  protected readonly mensagemSucesso = signal('');
  protected readonly email = signal('');
  protected readonly senha = signal('');
  protected readonly nome = signal('');
  protected readonly busca = signal('');
  protected readonly servicosMonitorados = ['Autenticação', 'IA + WhatsApp', 'Open Finance'];
  protected readonly statusPorNome = computed(() => Object.fromEntries(this.statusServicos().map((item) => [item.nome, item])));
  protected formulario: FormTransacao = { ...NOVA_TRANSACAO };

  constructor() {
    if (this.isStatusPage) this.carregarStatusServicos();
  }

  protected readonly receitas = computed(() => this.transacoes()
    .filter((item) => item.tipo === 'Receita')
    .reduce((total, item) => total + item.valor, 0));
  protected readonly despesas = computed(() => this.transacoes()
    .filter((item) => item.tipo === 'Despesa')
    .reduce((total, item) => total + item.valor, 0));
  protected readonly saldo = computed(() => this.receitas() - this.despesas());
  protected readonly visiveis = computed(() => {
    const filtro = this.filtro();
    const busca = this.busca().trim().toLocaleLowerCase('pt-BR');
    return this.transacoes().filter((item) => {
      const correspondeTipo = filtro === 'Todas' || (filtro === 'Receitas' ? item.tipo === 'Receita' : item.tipo === 'Despesa');
      const correspondeBusca = !busca || `${item.descricao} ${item.categoria ?? ''}`.toLocaleLowerCase('pt-BR').includes(busca);
      return correspondeTipo && correspondeBusca;
    });
  });

  protected entrar(): void {
    this.limparMensagens();
    this.enviando.set(true);
    this.auth.login(this.email().trim(), this.senha()).subscribe({
      next: () => {
        this.enviando.set(false);
        this.senha.set('');
        this.auth.loadProfile().subscribe({ error: () => undefined });
        this.carregarTransacoes();
        this.carregarPlanejamento();
      },
      error: (erro: unknown) => {
        this.enviando.set(false);
        this.mensagemErro.set(this.mensagemDoErro(erro, 'Não foi possível entrar. Confira seu e-mail e senha.'));
      },
    });
  }

  protected cadastrar(): void {
    this.limparMensagens();
    this.enviando.set(true);
    this.auth.register(this.nome().trim(), this.email().trim(), this.senha()).subscribe({
      next: () => {
        this.enviando.set(false);
        this.modoAcesso.set('entrar');
        this.senha.set('');
        this.mensagemSucesso.set('Conta criada. Entre com seu e-mail e senha.');
      },
      error: (erro: unknown) => {
        this.enviando.set(false);
        this.mensagemErro.set(this.mensagemDoErro(erro, 'Não foi possível criar sua conta.'));
      },
    });
  }

  protected sair(): void {
    this.auth.logout().subscribe({
      next: () => this.limparSessao(),
      error: () => {
        this.auth.clearSession();
        this.limparSessao();
      },
    });
  }

  protected abrirNovaTransacao(): void {
    this.editandoId.set(null);
    this.formulario = { ...NOVA_TRANSACAO, data: new Date().toISOString().slice(0, 10) };
    this.modalAberto.set(true);
    this.limparMensagens();
  }

  protected editar(item: Transacao): void {
    this.editandoId.set(item.id);
    this.formulario = {
      descricao: item.descricao,
      valor: item.valor,
      tipo: item.tipo,
      categoria: item.categoria ?? '',
      data: item.data.slice(0, 10),
    };
    this.modalAberto.set(true);
    this.limparMensagens();
  }

  protected salvarTransacao(): void {
    if (this.enviando() || !this.formulario.descricao.trim() || !this.formulario.valor || this.formulario.valor <= 0 || !this.formulario.data) {
      this.mensagemErro.set('Preencha a descrição, um valor maior que zero e a data.');
      return;
    }

    const valor = this.formulario;
    const dados: TransacaoInput = {
      descricao: valor.descricao.trim(),
      valor: Number(valor.valor),
      tipo: valor.tipo,
      categoria: valor.categoria.trim() || null,
      data: `${valor.data}T12:00:00`,
    };
    const id = this.editandoId();
    this.enviando.set(true);
    const request = id ? this.transacoesApi.atualizar(id, dados) : this.transacoesApi.criar(dados);
    request.subscribe({
      next: () => {
        this.enviando.set(false);
        this.modalAberto.set(false);
        this.mensagemSucesso.set(id ? 'Transação atualizada.' : 'Transação adicionada.');
        this.carregarTransacoes();
        this.carregarPlanejamento();
      },
      error: (erro: unknown) => {
        this.enviando.set(false);
        this.mensagemErro.set(this.mensagemDoErro(erro, 'Não foi possível salvar a transação.'));
      },
    });
  }

  protected excluir(item: Transacao): void {
    if (!window.confirm(`Excluir a transação “${item.descricao}”?`)) return;
    this.transacoesApi.remover(item.id).subscribe({
      next: () => {
        this.mensagemSucesso.set('Transação excluída.');
        this.carregarTransacoes();
        this.carregarPlanejamento();
      },
      error: (erro: unknown) => this.mensagemErro.set(this.mensagemDoErro(erro, 'Não foi possível excluir a transação.')),
    });
  }

  protected trocarModo(): void {
    this.modoAcesso.update((modo) => modo === 'entrar' ? 'criar' : 'entrar');
    this.limparMensagens();
  }

  protected formatarMoeda(valor: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor);
  }

  protected formatarData(valor: string): string {
    return new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: 'short', year: 'numeric' })
      .format(new Date(`${valor.slice(0, 10)}T12:00:00`));
  }

  protected fecharModal(): void {
    if (!this.enviando()) this.modalAberto.set(false);
  }

  protected atualizarStatusServicos(): void {
    this.carregarStatusServicos();
  }

  protected selecionarPagina(pagina: Pagina): void {
    this.pagina.set(pagina);
    if (pagina === 'planejamento' || pagina === 'relatorios') this.carregarPlanejamento();
  }

  protected mudarMes(mes: string): void { this.mesSelecionado.set(mes); this.carregarPlanejamento(); }

  protected criarOrcamento(): void {
    const categoria = this.categoriaOrcamento().trim(); const limite = this.limiteOrcamento();
    if (!categoria || !limite || limite <= 0) { this.mensagemErro.set('Informe uma categoria e um limite maior que zero.'); return; }
    this.planejamentoApi.criarOrcamento(categoria, limite).subscribe({ next: () => { this.categoriaOrcamento.set(''); this.limiteOrcamento.set(null); this.mensagemSucesso.set('Orçamento criado.'); this.carregarPlanejamento(); }, error: (e: unknown) => this.mensagemErro.set(this.mensagemDoErro(e, 'Não foi possível criar o orçamento.')) });
  }

  protected excluirOrcamento(item: Orcamento): void { this.planejamentoApi.removerOrcamento(item.id).subscribe({ next: () => this.carregarPlanejamento(), error: (e: unknown) => this.mensagemErro.set(this.mensagemDoErro(e, 'Não foi possível excluir o orçamento.')) }); }

  protected criarRecorrencia(): void {
    const f = this.formularioRecorrencia;
    if (!f.descricao.trim() || !f.categoria.trim() || !f.valor || f.valor <= 0 || !f.proximoVencimento) { this.mensagemErro.set('Preencha todos os campos da conta recorrente.'); return; }
    this.planejamentoApi.criarRecorrencia({ descricao: f.descricao.trim(), categoria: f.categoria.trim(), valor: f.valor, proximoVencimento: `${f.proximoVencimento}T12:00:00` }).subscribe({ next: () => { f.descricao = ''; f.categoria = ''; f.valor = null; this.mensagemSucesso.set('Conta recorrente cadastrada.'); this.carregarPlanejamento(); }, error: (e: unknown) => this.mensagemErro.set(this.mensagemDoErro(e, 'Não foi possível cadastrar a conta.')) });
  }

  protected pagarRecorrencia(item: Recorrencia): void { this.planejamentoApi.pagarRecorrencia(item.id).subscribe({ next: () => { this.mensagemSucesso.set('Pagamento registrado como transação.'); this.carregarPlanejamento(); this.carregarTransacoes(); }, error: (e: unknown) => this.mensagemErro.set(this.mensagemDoErro(e, 'Não foi possível registrar o pagamento.')) }); }
  protected excluirRecorrencia(item: Recorrencia): void { this.planejamentoApi.removerRecorrencia(item.id).subscribe({ next: () => this.carregarPlanejamento(), error: (e: unknown) => this.mensagemErro.set(this.mensagemDoErro(e, 'Não foi possível excluir a conta.')) }); }

  protected criarMeta(): void {
    const f = this.formularioMeta;
    if (!f.nome.trim() || !f.valorAlvo || f.valorAlvo <= 0) { this.mensagemErro.set('Informe o nome e o valor-alvo da meta.'); return; }
    this.planejamentoApi.criarMeta({ nome: f.nome.trim(), valorAlvo: f.valorAlvo, prazo: f.prazo ? `${f.prazo}T12:00:00` : null }).subscribe({ next: () => { f.nome = ''; f.valorAlvo = null; f.prazo = ''; this.mensagemSucesso.set('Meta criada.'); this.carregarPlanejamento(); }, error: (e: unknown) => this.mensagemErro.set(this.mensagemDoErro(e, 'Não foi possível criar a meta.')) });
  }

  protected iniciarDeposito(meta: MetaEconomia): void { this.metaDoDeposito.set(meta.id); this.deposito.set(null); }
  protected cancelarDeposito(): void { this.metaDoDeposito.set(''); this.deposito.set(null); }
  protected salvarDeposito(): void { const valor = this.deposito(); const id = this.metaDoDeposito(); if (!valor || valor <= 0) { this.mensagemErro.set('Informe um valor maior que zero.'); return; } this.planejamentoApi.depositar(id, valor).subscribe({ next: () => { this.cancelarDeposito(); this.mensagemSucesso.set('Depósito adicionado à meta.'); this.carregarPlanejamento(); }, error: (e: unknown) => this.mensagemErro.set(this.mensagemDoErro(e, 'Não foi possível adicionar o depósito.')) }); }
  protected excluirMeta(meta: MetaEconomia): void { this.planejamentoApi.removerMeta(meta.id).subscribe({ next: () => this.carregarPlanejamento(), error: (e: unknown) => this.mensagemErro.set(this.mensagemDoErro(e, 'Não foi possível excluir a meta.')) }); }

  protected exportarCsv(): void {
    const r = this.relatorio(); if (!r) return;
    const linhas = [['Categoria', 'Receitas', 'Despesas'], ...r.categorias.map(x => [x.categoria, x.receitas.toFixed(2), x.despesas.toFixed(2)])];
    const csv = '\uFEFF' + linhas.map(l => l.map(v => `"${String(v).replaceAll('"', '""')}"`).join(';')).join('\r\n');
    const link = document.createElement('a'); link.href = URL.createObjectURL(new Blob([csv], { type: 'text/csv;charset=utf-8' })); link.download = `relatorio-financeiro-${r.mes}.csv`; link.click(); URL.revokeObjectURL(link.href);
  }
  protected imprimirRelatorio(): void { window.print(); }

  protected percentualMeta(meta: MetaEconomia): number { return Math.min(100, Math.round(meta.valorGuardado / meta.valorAlvo * 100)); }
  protected mesLegivel(mes: string): string { const [ano, numero] = mes.split('-').map(Number); return new Intl.DateTimeFormat('pt-BR', { month: 'short' }).format(new Date(ano, numero - 1, 1)); }

  private carregarTransacoes(): void {
    this.carregando.set(true);
    this.transacoesApi.listar().subscribe({
      next: (dados) => {
        this.transacoes.set(dados);
        this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.carregando.set(false);
        if (!this.auth.accessToken) return;
        this.mensagemErro.set(this.mensagemDoErro(erro, 'Não foi possível carregar suas transações.'));
      },
    });
  }

  private carregarPlanejamento(): void {
    if (!this.auth.accessToken) return;
    const mes = this.mesSelecionado();
    this.planejamentoApi.orcamentos(mes).subscribe({ next: x => this.orcamentos.set(x), error: () => undefined });
    this.planejamentoApi.recorrencias().subscribe({ next: x => this.recorrencias.set(x), error: () => undefined });
    this.planejamentoApi.metas().subscribe({ next: x => this.metas.set(x), error: () => undefined });
    this.planejamentoApi.relatorio(mes).subscribe({ next: x => this.relatorio.set(x), error: () => undefined });
  }

  private carregarStatusServicos(): void {
    this.consultandoServicos.set(true);
    this.servicosApi.status().subscribe({
      next: (status) => {
        this.statusServicos.set(status);
        this.atualizadoEm.set(new Intl.DateTimeFormat('pt-BR', { hour: '2-digit', minute: '2-digit' }).format(new Date()));
        this.consultandoServicos.set(false);
      },
      error: () => {
        this.statusServicos.set([]);
        this.atualizadoEm.set(null);
        this.consultandoServicos.set(false);
      },
    });
  }

  private limparSessao(): void {
    this.transacoes.set([]);
    this.orcamentos.set([]);
    this.recorrencias.set([]);
    this.metas.set([]);
    this.relatorio.set(null);
    this.mensagemErro.set('');
    this.mensagemSucesso.set('');
  }

  private limparMensagens(): void {
    this.mensagemErro.set('');
    this.mensagemSucesso.set('');
  }

  private mensagemDoErro(erro: unknown, padrao: string): string {
    if (!(erro instanceof HttpErrorResponse)) return padrao;
    const corpo = erro.error as { message?: string; title?: string; errors?: Record<string, string[]> } | null;
    const validacoes = corpo?.errors ? Object.values(corpo.errors).flat().join(' ') : '';
    return validacoes || corpo?.message || corpo?.title || padrao;
  }
}
