[CmdletBinding()]
param(
    [string]$BaseUrl = 'http://localhost:4200',
    [string]$Email = 'demo@financas.test',
    [switch]$UsarContaExistente,
    [switch]$Preview
)

$ErrorActionPreference = 'Stop'
$BaseUrl = $BaseUrl.TrimEnd('/')
$credentialPath = Join-Path (Split-Path -Parent $PSScriptRoot) 'demo-credentials.local.json'

function Formatar-DataApi([datetime]$Data) {
    return $Data.ToString("yyyy-MM-dd'T'HH:mm:ss", [Globalization.CultureInfo]::InvariantCulture)
}

function Converter-Senha([securestring]$SenhaSegura) {
    $ponteiro = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($SenhaSegura)
    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($ponteiro)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ponteiro)
    }
}

$hoje = (Get-Date).Date
$primeiroDiaMes = $hoje.AddDays(1 - $hoje.Day)
$transacoesDemo = [System.Collections.Generic.List[object]]::new()
for ($mesesAtras = 5; $mesesAtras -ge 0; $mesesAtras--) {
    $mes = $primeiroDiaMes.AddMonths(-$mesesAtras)
    $rotulo = $mes.ToString('yyyy-MM')
    $transacoesDoMes = @(
        @{ descricao = "Salário - $rotulo"; valor = 5200.00; tipo = 'Receita'; categoria = 'Salário'; data = $mes.AddDays(4) },
        @{ descricao = "Aluguel - $rotulo"; valor = 1650.00; tipo = 'Despesa'; categoria = 'Moradia'; data = $mes.AddDays(7) },
        @{ descricao = "Mercado - $rotulo"; valor = 720.00; tipo = 'Despesa'; categoria = 'Alimentação'; data = $mes.AddDays(10) },
        @{ descricao = "Transporte - $rotulo"; valor = 240.00; tipo = 'Despesa'; categoria = 'Transporte'; data = $mes.AddDays(12) },
        @{ descricao = "Internet residencial - $rotulo"; valor = 119.90; tipo = 'Despesa'; categoria = 'Moradia'; data = $mes.AddDays(14) }
    )
    foreach ($item in $transacoesDoMes) {
        $data = $item.data
        if ($mes.Year -eq $hoje.Year -and $mes.Month -eq $hoje.Month -and $data.Date -gt $hoje) {
            $data = $hoje
        }
        $transacoesDemo.Add([pscustomobject]@{
            descricao = $item.descricao
            valor = $item.valor
            tipo = $item.tipo
            categoria = $item.categoria
            data = Formatar-DataApi $data
        })
    }
    if ($mes.Month % 2 -eq 0) {
        $transacoesDemo.Add([pscustomobject]@{
            descricao = "Projeto freelance - $rotulo"
            valor = 850.00
            tipo = 'Receita'
            categoria = 'Renda extra'
            data = Formatar-DataApi $mes.AddDays(19)
        })
    }
}

$orcamentosDemo = @(
    @{ categoria = 'Alimentação'; limiteMensal = 1200.00 },
    @{ categoria = 'Moradia'; limiteMensal = 2000.00 },
    @{ categoria = 'Transporte'; limiteMensal = 500.00 },
    @{ categoria = 'Saúde'; limiteMensal = 400.00 }
)
$recorrenciasDemo = @(
    @{ descricao = 'Internet residencial'; categoria = 'Moradia'; valor = 119.90; proximoVencimento = (Formatar-DataApi $primeiroDiaMes.AddMonths(1).AddDays(9)) },
    @{ descricao = 'Streaming'; categoria = 'Lazer'; valor = 39.90; proximoVencimento = (Formatar-DataApi $primeiroDiaMes.AddMonths(1).AddDays(14)) },
    @{ descricao = 'Plano de celular'; categoria = 'Comunicação'; valor = 59.90; proximoVencimento = (Formatar-DataApi $primeiroDiaMes.AddMonths(1).AddDays(19)) }
)

if ($Preview) {
    Write-Output 'Prévia: nenhuma conta ou transação foi criada.'
    $transacoesDemo | Select-Object descricao, valor, tipo, categoria, data | Format-Table -AutoSize
    Write-Output "Orçamentos: $($orcamentosDemo.Count); despesas recorrentes: $($recorrenciasDemo.Count); meta: Reserva de emergência (R$ 10.000,00)."
    return
}

$senha = $null
if ($UsarContaExistente) {
    $Email = Read-Host 'E-mail da conta existente'
    $senhaSegura = Read-Host 'Senha da conta' -AsSecureString
    $senha = Converter-Senha $senhaSegura
    $senhaSegura.Dispose()
}
elseif (Test-Path -LiteralPath $credentialPath) {
    $credenciais = Get-Content -LiteralPath $credentialPath -Raw | ConvertFrom-Json
    $Email = $credenciais.email
    $senha = $credenciais.password
    Write-Output "Usando a conta local de demonstração $Email."
}
else {
    $bytesSenha = New-Object byte[] 30
    $gerador = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $gerador.GetBytes($bytesSenha) } finally { $gerador.Dispose() }
    $senha = [Convert]::ToBase64String($bytesSenha).TrimEnd('=').Replace('+', '-').Replace('/', '_') + 'F7!'
    [Array]::Clear($bytesSenha, 0, $bytesSenha.Length)

    $cadastro = @{ nome = 'Conta de demonstração'; email = $Email; password = $senha } | ConvertTo-Json -Compress
    try {
        Invoke-RestMethod -Method Post -Uri "$BaseUrl/gateway/auth/api/v1/users" -ContentType 'application/json' -Body $cadastro | Out-Null
    }
    catch {
        $status = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { 0 }
        if ($status -eq 409) {
            throw "O e-mail $Email já existe. Rode novamente com -UsarContaExistente para carregar os dados nessa conta."
        }
        throw "Não foi possível cadastrar a conta demo (HTTP $status). Confirme se Auth, gateway e banco estão ativos."
    }

    @{ email = $Email; password = $senha } | ConvertTo-Json | Set-Content -LiteralPath $credentialPath -Encoding utf8
    Write-Output "Conta de demonstração criada. Credenciais locais: $credentialPath"
}

$loginBody = @{ email = $Email; password = $senha } | ConvertTo-Json -Compress
$sessao = Invoke-RestMethod -Method Post -Uri "$BaseUrl/gateway/auth/api/v1/auth/login" -ContentType 'application/json' -Body $loginBody
$headers = @{ Authorization = "Bearer $($sessao.accessToken)" }
$senha = $null
$loginBody = $null

$transacoesExistentes = @(Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/transacoes" -Headers $headers)
$descricoesExistentes = @($transacoesExistentes | ForEach-Object { $_.descricao })
$transacoesCriadas = 0
foreach ($transacao in $transacoesDemo) {
    if ($descricoesExistentes -contains $transacao.descricao) { continue }
    $body = $transacao | ConvertTo-Json -Compress
    Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/transacoes" -Headers $headers -ContentType 'application/json' -Body $body | Out-Null
    $transacoesCriadas++
}

$mesAtual = $primeiroDiaMes.ToString('yyyy-MM')
$orcamentosExistentes = @(Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/orcamentos?mes=$mesAtual" -Headers $headers)
$orcamentosCriados = 0
foreach ($orcamento in $orcamentosDemo) {
    if (@($orcamentosExistentes | Where-Object categoria -eq $orcamento.categoria).Count -gt 0) { continue }
    $body = $orcamento | ConvertTo-Json -Compress
    Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/orcamentos" -Headers $headers -ContentType 'application/json' -Body $body | Out-Null
    $orcamentosCriados++
}

$recorrenciasExistentes = @(Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/recorrencias" -Headers $headers)
$recorrenciasCriadas = 0
foreach ($recorrencia in $recorrenciasDemo) {
    if (@($recorrenciasExistentes | Where-Object descricao -eq $recorrencia.descricao).Count -gt 0) { continue }
    $body = $recorrencia | ConvertTo-Json -Compress
    Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/recorrencias" -Headers $headers -ContentType 'application/json' -Body $body | Out-Null
    $recorrenciasCriadas++
}

$metasExistentes = @(Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/metas" -Headers $headers)
$metaReserva = $metasExistentes | Where-Object nome -eq 'Reserva de emergência' | Select-Object -First 1
if (-not $metaReserva) {
    $body = @{ nome = 'Reserva de emergência'; valorAlvo = 10000.00; prazo = (Formatar-DataApi $primeiroDiaMes.AddMonths(12)) } | ConvertTo-Json -Compress
    $metaReserva = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/metas" -Headers $headers -ContentType 'application/json' -Body $body
    Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/metas/$($metaReserva.id)/depositos" -Headers $headers -ContentType 'application/json' -Body (@{ valor = 1800.00 } | ConvertTo-Json -Compress) | Out-Null
    $metaCriada = 1
}
else { $metaCriada = 0 }

[pscustomobject]@{
    Conta = $Email
    TransacoesCriadas = $transacoesCriadas
    OrcamentosCriados = $orcamentosCriados
    RecorrenciasCriadas = $recorrenciasCriadas
    MetaCriada = $metaCriada
    CredenciaisLocais = if (Test-Path -LiteralPath $credentialPath) { $credentialPath } else { 'senha fornecida no prompt; não gravada pelo script' }
} | Format-List
