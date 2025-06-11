// 🚦 Rate Limiting Test Suite
class RateLimitTester {
  constructor() {
    this.stats = {
      total: 0,
      successful: 0,
      rateLimited: 0,
      failed: 0,
      responseTimes: [],
    };
    this.baseUrl = "/api/ratelimit";
  }

  // Fazer requisição com medição de tempo
  async makeRequest(url, options = {}) {
    const startTime = performance.now();

    try {
      const response = await fetch(url, {
        ...options,
        headers: {
          "Content-Type": "application/json",
          ...options.headers,
        },
      });

      const endTime = performance.now();
      const responseTime = Math.round(endTime - startTime);

      this.stats.total++;
      this.stats.responseTimes.push(responseTime);

      const data = await response.json();

      if (response.status === 200) {
        this.stats.successful++;
        return { success: true, data, responseTime, status: response.status };
      } else if (response.status === 429) {
        this.stats.rateLimited++;
        return {
          success: false,
          data,
          responseTime,
          status: response.status,
          rateLimited: true,
        };
      } else {
        this.stats.failed++;
        return { success: false, data, responseTime, status: response.status };
      }
    } catch (error) {
      const endTime = performance.now();
      const responseTime = Math.round(endTime - startTime);

      this.stats.total++;
      this.stats.failed++;
      this.stats.responseTimes.push(responseTime);

      return {
        success: false,
        error: error.message,
        responseTime,
        status: "ERROR",
      };
    }
  }

  // Atualizar estatísticas na UI
  updateStats() {
    document.getElementById("totalRequests").textContent = this.stats.total;
    document.getElementById("successfulRequests").textContent =
      this.stats.successful;
    document.getElementById("rateLimitedRequests").textContent =
      this.stats.rateLimited;
    document.getElementById("failedRequests").textContent = this.stats.failed;

    const successRate =
      this.stats.total > 0
        ? Math.round((this.stats.successful / this.stats.total) * 100)
        : 0;
    document.getElementById("successRate").textContent = `${successRate}%`;

    const avgResponseTime =
      this.stats.responseTimes.length > 0
        ? Math.round(
            this.stats.responseTimes.reduce((a, b) => a + b, 0) /
              this.stats.responseTimes.length
          )
        : 0;
    document.getElementById(
      "avgResponseTime"
    ).textContent = `${avgResponseTime}ms`;
  }

  // Adicionar resultado à lista
  addResult(result, testName, details = "") {
    const resultsDiv = document.getElementById("testResults");

    // Remover mensagem de "nenhum teste"
    if (resultsDiv.querySelector(".text-muted")) {
      resultsDiv.innerHTML = "";
    }

    const timestamp = new Date().toLocaleTimeString();
    const statusIcon = result.success ? "✅" : result.rateLimited ? "⚠️" : "❌";
    const statusClass = result.success
      ? "success"
      : result.rateLimited
      ? "warning"
      : "danger";

    const resultElement = document.createElement("div");
    resultElement.className = `alert alert-${statusClass} alert-dismissible fade show`;
    resultElement.innerHTML = `
            <div class="d-flex justify-content-between align-items-start">
                <div>
                    <strong>${statusIcon} ${testName}</strong><br>
                    <small>
                        <strong>Status:</strong> ${result.status} | 
                        <strong>Tempo:</strong> ${result.responseTime}ms | 
                        <strong>Horário:</strong> ${timestamp}
                        ${
                          details
                            ? `<br><strong>Detalhes:</strong> ${details}`
                            : ""
                        }
                        ${
                          result.data?.policy
                            ? `<br><strong>Política:</strong> ${result.data.policy}`
                            : ""
                        }
                        ${
                          result.data?.message
                            ? `<br><strong>Mensagem:</strong> ${result.data.message}`
                            : ""
                        }
                    </small>
                </div>
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;

    resultsDiv.insertBefore(resultElement, resultsDiv.firstChild);
    this.updateStats();
  }

  // Reset das estatísticas
  resetStats() {
    this.stats = {
      total: 0,
      successful: 0,
      rateLimited: 0,
      failed: 0,
      responseTimes: [],
    };
    this.updateStats();
  }
}

// Instância global do testador
const rateLimitTester = new RateLimitTester();

// Carregar informações das políticas
async function loadRateLimitPolicies() {
  try {
    const response = await fetch("/api/ratelimit/policies");
    if (response.ok) {
      const data = await response.json();
      displayPoliciesInfo(data);
    } else {
      document.getElementById("rateLimitStatus").innerHTML =
        '<div class="alert alert-danger">❌ Erro ao carregar políticas de rate limiting</div>';
    }
  } catch (error) {
    document.getElementById("rateLimitStatus").innerHTML =
      '<div class="alert alert-danger">❌ Erro de conexão ao carregar políticas</div>';
  }
}

// Exibir informações das políticas
function displayPoliciesInfo(data) {
  const statusDiv = document.getElementById("rateLimitStatus");
  statusDiv.innerHTML = `
        <div class="alert alert-success">
            <h6><i class="fas fa-check-circle"></i> Rate Limiting Ativo</h6>
            <p class="mb-2">Políticas configuradas: <strong>${
              data.policies.length
            }</strong></p>
            <div class="row">
                ${data.policies
                  .map(
                    (policy) => `
                    <div class="col-md-6 mb-2">
                        <small>
                            <strong>${policy.name}:</strong> ${policy.limit} (${policy.type})
                        </small>
                    </div>
                `
                  )
                  .join("")}
            </div>
            <small class="text-muted">
                <i class="fas fa-info-circle"></i> 
                Respostas 429 incluem headers: Retry-After, X-RateLimit-Policy
            </small>
        </div>
    `;
}

// Testes individuais das políticas
async function testGeneralPolicy() {
  const result = await rateLimitTester.makeRequest(
    `${rateLimitTester.baseUrl}/test-general`
  );
  rateLimitTester.addResult(
    result,
    "Política Geral",
    "100 req/min, Sliding Window"
  );
}

async function testAuthPolicy() {
  const result = await rateLimitTester.makeRequest(
    `${rateLimitTester.baseUrl}/test-auth`,
    {
      method: "POST",
      body: JSON.stringify({ test: "auth policy test" }),
    }
  );
  rateLimitTester.addResult(
    result,
    "Política Auth",
    "5 req/5min, Fixed Window"
  );
}

async function testStrictPolicy() {
  // Primeiro tentar obter token JWT
  const token = localStorage.getItem("jwt_token");
  if (!token) {
    rateLimitTester.addResult(
      { success: false, status: "AUTH_REQUIRED", responseTime: 0 },
      "Política Rigorosa",
      "Requer autenticação JWT"
    );
    return;
  }

  const result = await rateLimitTester.makeRequest(
    `${rateLimitTester.baseUrl}/test-strict`,
    {
      headers: { Authorization: `Bearer ${token}` },
    }
  );
  rateLimitTester.addResult(
    result,
    "Política Rigorosa",
    "10 tokens, reabastece 2/30s, Token Bucket"
  );
}

async function testConcurrencyPolicy() {
  const result = await rateLimitTester.makeRequest(
    `${rateLimitTester.baseUrl}/test-concurrency`
  );
  rateLimitTester.addResult(
    result,
    "Política Concorrência",
    "5 simultâneas, processamento 2s"
  );
}

async function testRapidPolicy() {
  const result = await rateLimitTester.makeRequest(
    `${rateLimitTester.baseUrl}/test-rapid`
  );
  rateLimitTester.addResult(
    result,
    "Política Rápida",
    "20 req/min, Sliding Window"
  );
}

async function testGlobalPolicy() {
  const result = await rateLimitTester.makeRequest(
    `${rateLimitTester.baseUrl}/test-global`
  );
  rateLimitTester.addResult(
    result,
    "Política Global",
    "200 req/min por IP, Global Limiter"
  );
}

// Teste de stress
async function runStressTest() {
  const requestCount = parseInt(document.getElementById("requestCount").value);
  const interval = parseInt(document.getElementById("requestInterval").value);
  const endpoint = document.getElementById("testEndpoint").value;

  if (requestCount > 100) {
    alert("Máximo de 100 requisições permitidas");
    return;
  }

  const button = event.target;
  button.disabled = true;
  button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Executando...';

  try {
    rateLimitTester.addResult(
      { success: true, status: "STARTED", responseTime: 0 },
      "Teste de Stress Iniciado",
      `${requestCount} requisições com intervalo de ${interval}ms`
    );

    for (let i = 0; i < requestCount; i++) {
      setTimeout(async () => {
        const result = await rateLimitTester.makeRequest(
          `${rateLimitTester.baseUrl}/${endpoint}`
        );
        rateLimitTester.addResult(
          result,
          `Stress Test ${i + 1}/${requestCount}`,
          `Endpoint: ${endpoint}`
        );

        // Se for a última requisição, reativar botão
        if (i === requestCount - 1) {
          setTimeout(() => {
            button.disabled = false;
            button.innerHTML =
              '<i class="fas fa-bolt"></i> Executar Teste de Stress';
          }, 1000);
        }
      }, i * interval);
    }
  } catch (error) {
    button.disabled = false;
    button.innerHTML = '<i class="fas fa-bolt"></i> Executar Teste de Stress';
    rateLimitTester.addResult(
      {
        success: false,
        status: "ERROR",
        responseTime: 0,
        error: error.message,
      },
      "Teste de Stress Falhou",
      error.message
    );
  }
}

// Simulação de login
async function simulateLogin() {
  const username = document.getElementById("loginUsername").value;
  const password = document.getElementById("loginPassword").value;

  const result = await rateLimitTester.makeRequest(
    `${rateLimitTester.baseUrl}/simulate-login`,
    {
      method: "POST",
      body: JSON.stringify({ username, password }),
    }
  );

  rateLimitTester.addResult(
    result,
    "Simulação Login",
    `User: ${username}, AuthPolicy aplicada`
  );
}

// Teste de requisições concorrentes
async function testConcurrentRequests() {
  const count = parseInt(document.getElementById("concurrentRequests").value);

  if (count > 20) {
    alert("Máximo de 20 requisições simultâneas permitidas");
    return;
  }

  const button = event.target;
  button.disabled = true;
  button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Testando...';

  rateLimitTester.addResult(
    { success: true, status: "STARTED", responseTime: 0 },
    "Teste Concorrência Iniciado",
    `${count} requisições simultâneas`
  );

  // Criar array de promises para requisições simultâneas
  const promises = [];
  for (let i = 0; i < count; i++) {
    promises.push(
      rateLimitTester
        .makeRequest(`${rateLimitTester.baseUrl}/test-concurrency`)
        .then((result) => {
          rateLimitTester.addResult(
            result,
            `Concorrente ${i + 1}/${count}`,
            "Teste simultâneo"
          );
          return result;
        })
    );
  }

  try {
    await Promise.all(promises);
  } catch (error) {
    console.error("Erro no teste de concorrência:", error);
  }

  button.disabled = false;
  button.innerHTML = '<i class="fas fa-rocket"></i> Testar Concorrência';
}

// Limpar resultados
function clearResults() {
  document.getElementById("testResults").innerHTML = `
        <div class="text-muted text-center py-4">
            <i class="fas fa-clipboard-list fa-2x mb-2"></i><br>
            Nenhum teste executado ainda. Use os botões acima para começar.
        </div>
    `;
  rateLimitTester.resetStats();
}

// Utilitários para demonstração
function showRateLimitInfo() {
  const modal = new bootstrap.Modal(
    document.getElementById("rateLimitInfoModal")
  );
  modal.show();
}

// Auto-refresh das estatísticas a cada 5 segundos
setInterval(() => {
  if (rateLimitTester.stats.total > 0) {
    rateLimitTester.updateStats();
  }
}, 5000);

console.log("🚦 Rate Limiting Test Suite carregado!");
console.log("Disponível globalmente: rateLimitTester");
console.log(
  "Políticas disponíveis: GeneralPolicy, AuthPolicy, StrictPolicy, ConcurrencyPolicy, TestPolicy, GlobalLimiter"
);
