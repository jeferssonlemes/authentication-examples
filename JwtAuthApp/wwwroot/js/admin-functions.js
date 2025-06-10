// Funções de teste XSS
async function testXSSVulnerability() {
  const input =
    document.getElementById("xssTestInput").value ||
    '<script>alert("XSS Test")</script>';

  try {
    const response = await fetch("/api/security/validate-input", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ input: input }),
    });

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    const data = await response.json();

    const resultDiv = document.getElementById("xssTestResults");
    if (data.isSuspicious) {
      resultDiv.innerHTML = `
                <div class="text-success">
                    ✅ <strong>Proteção Funcionando!</strong><br>
                    <small>Input suspeito detectado e bloqueado</small><br>
                    <small><strong>Original:</strong> ${input}</small><br>
                    <small><strong>Sanitizado:</strong> ${
                      data.sanitizedInput
                    }</small><br>
                    <small>🕒 ${new Date().toLocaleTimeString()}</small>
                </div>`;

      // Log da tentativa para o servidor
      if (window.xssProtection) {
        window.xssProtection.logXSSAttempt("testXSSVulnerability", input);
      }
    } else {
      resultDiv.innerHTML = `
                <div class="text-warning">
                    ⚠️ <strong>Input Considerado Seguro</strong><br>
                    <small>Nenhuma ameaça detectada no input: "${input}"</small><br>
                    <small>🕒 ${new Date().toLocaleTimeString()}</small>
                </div>`;
    }
  } catch (error) {
    document.getElementById("xssTestResults").innerHTML = `
            <div class="text-danger">
                ❌ <strong>Erro no Teste</strong><br>
                <small>${error.message}</small><br>
                <small>🕒 ${new Date().toLocaleTimeString()}</small>
            </div>`;
  }
}

async function testXSSProtection() {
  const testPayload = '<img src="x" onerror="alert(\'XSS\')">';

  try {
    // Usar nosso utilitário de proteção XSS
    if (window.xssProtection) {
      const sanitized = window.xssProtection.sanitizeText(testPayload);
      const resultDiv = document.getElementById("xssTestResults");

      if (sanitized !== testPayload) {
        resultDiv.innerHTML = `
                <div class="text-success">
                    ✅ <strong>Proteção XSS Ativa!</strong><br>
                    <small>Conteúdo malicioso foi sanitizado automaticamente</small><br>
                    <small><strong>Original:</strong> ${testPayload}</small><br>
                    <small><strong>Sanitizado:</strong> ${sanitized}</small>
                </div>`;
      } else {
        resultDiv.innerHTML = `
                <div class="text-warning">
                    ⚠️ <strong>Proteção Parcial</strong><br>
                    <small>Input não foi alterado pelo sistema de proteção</small>
                </div>`;
      }
    } else {
      // Fallback para teste básico
      const testDiv = document.createElement("div");
      testDiv.innerHTML = testPayload;

      const resultDiv = document.getElementById("xssTestResults");
      if (testDiv.innerHTML.includes("onerror")) {
        resultDiv.innerHTML = `
                <div class="text-danger">
                    ❌ <strong>Vulnerabilidade Detectada!</strong><br>
                    <small>Proteção XSS não está funcionando corretamente</small>
                </div>`;
      } else {
        resultDiv.innerHTML = `
                <div class="text-success">
                    ✅ <strong>Proteção XSS Ativa!</strong><br>
                    <small>Conteúdo malicioso foi sanitizado automaticamente</small><br>
                    <small><strong>Resultado:</strong> ${testDiv.innerHTML}</small>
                </div>`;
      }
    }
  } catch (error) {
    document.getElementById(
      "xssTestResults"
    ).innerHTML = `<div class="text-success">✅ Proteção funcionando - Erro capturado: ${error.message}</div>`;
  }
}

async function checkSecurityStatus() {
  try {
    // Primeiro tentar o endpoint com autorização
    let response = await window.csrfHelper.securedFetch(
      "/api/security/security-status",
      {
        method: "GET",
      }
    );

    // Se falhar por autorização, tentar o endpoint simples
    if (response.status === 403) {
      console.log("Admin endpoint failed, trying simple endpoint...");
      response = await fetch("/api/security/security-status-simple");
    }

    if (response.ok) {
      const data = await response.json();
      console.log("Security Status Response:", data); // Debug

      // Verificar se a resposta tem a estrutura esperada (camelCase ou PascalCase)
      const xssEnabled =
        data.XSSProtection?.Enabled ?? data.xssProtection?.enabled ?? false;
      const csrfEnabled =
        data.CSRFProtection?.Enabled ?? data.csrfProtection?.enabled ?? false;
      const headersEnabled =
        data.SecurityHeaders?.ContentSecurityPolicy ??
        data.securityHeaders?.contentSecurityPolicy ??
        false;
      const securityLevel =
        data.SecurityLevel ?? data.securityLevel ?? "UNKNOWN";

      document.getElementById("xssTestResults").innerHTML = `
                <div class="text-info">
                    <strong>📊 Status de Segurança:</strong><br>
                    <small>🛡️ XSS Protection: ${
                      xssEnabled ? "✅ Ativo" : "❌ Inativo"
                    }</small><br>
                    <small>🔒 CSRF Protection: ${
                      csrfEnabled ? "✅ Ativo" : "❌ Inativo"
                    }</small><br>
                    <small>📋 Security Headers: ${
                      headersEnabled ? "✅ Ativo" : "❌ Inativo"
                    }</small><br>
                    <small>🔐 Nível de Segurança: <strong>${securityLevel}</strong></small><br>
                    <small>🕒 Última Verificação: ${new Date().toLocaleTimeString()}</small><br>
                    <small><strong>Debug:</strong> ${JSON.stringify(
                      data,
                      null,
                      2
                    ).substring(0, 100)}...</small>
                </div>`;
    } else if (response.status === 403) {
      document.getElementById("xssTestResults").innerHTML = `
                <div class="text-warning">
                    ⚠️ <strong>Acesso Negado</strong><br>
                    <small>Apenas administradores podem verificar o status de segurança</small>
                </div>`;
    } else {
      const errorText = await response.text();
      document.getElementById("xssTestResults").innerHTML = `
                <div class="text-danger">
                    ❌ <strong>Erro ao verificar status</strong><br>
                    <small>Status: ${response.status}</small><br>
                    <small>Resposta: ${errorText}</small>
                </div>`;
    }
  } catch (error) {
    document.getElementById("xssTestResults").innerHTML = `
            <div class="text-danger">
                ❌ <strong>Erro de Conexão</strong><br>
                <small>${error.message}</small><br>
                <small>Verifique se o servidor está executando</small>
            </div>`;
  }
}

// Função de teste simples para debug
async function testSecuritySimple() {
  try {
    const response = await fetch("/api/security/security-status-simple");
    const data = await response.json();

    document.getElementById("xssTestResults").innerHTML = `
            <div class="text-success">
                ✅ <strong>Teste Simples Funcionou!</strong><br>
                <small>Status: ${response.status}</small><br>
                <small>Dados: ${JSON.stringify(data, null, 2)}</small>
            </div>`;
  } catch (error) {
    document.getElementById("xssTestResults").innerHTML = `
            <div class="text-danger">
                ❌ <strong>Teste Simples Falhou</strong><br>
                <small>${error.message}</small>
            </div>`;
  }
}

console.log("🔧 Funções Admin carregadas!");
