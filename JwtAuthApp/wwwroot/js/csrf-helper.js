// Utilitário para gerenciar tokens CSRF
class CSRFHelper {
  constructor() {
    this.token = null;
    this.cookieName = "__RequestVerificationToken";
    this.headerName = "X-CSRF-TOKEN";
  }

  // Obter token CSRF do servidor
  async getToken() {
    try {
      const response = await fetch("/api/antiforgery/token");
      if (response.ok) {
        const data = await response.json();
        this.token = data.token;
        console.log(
          "🛡️ Token CSRF obtido:",
          this.token?.substring(0, 20) + "..."
        );
        alert(
          "✅ Token CSRF obtido com sucesso!\n" +
            "Preview: " +
            this.token?.substring(0, 20) +
            "..."
        );
        return this.token;
      }
    } catch (error) {
      console.error("❌ Erro ao obter token CSRF:", error);
      alert("❌ Erro ao obter token CSRF: " + error.message);
    }
    return null;
  }

  // Obter token CSRF para usuário autenticado
  async getAuthToken() {
    try {
      const jwtToken = localStorage.getItem("jwt_token");
      const response = await fetch("/api/antiforgery/auth-token", {
        headers: {
          Authorization: `Bearer ${jwtToken}`,
        },
      });
      if (response.ok) {
        const data = await response.json();
        this.token = data.token;
        console.log(
          "🛡️ Token CSRF autenticado obtido:",
          this.token?.substring(0, 20) + "..."
        );
        return this.token;
      }
    } catch (error) {
      console.error("❌ Erro ao obter token CSRF autenticado:", error);
    }
    return null;
  }

  // Obter token do cookie
  getTokenFromCookie() {
    const cookies = document.cookie.split(";");
    for (let cookie of cookies) {
      const [name, value] = cookie.trim().split("=");
      if (name === this.cookieName) {
        return decodeURIComponent(value);
      }
    }
    return null;
  }

  // Fazer requisição com proteção CSRF
  async securedFetch(url, options = {}) {
    // Garantir que temos um token
    if (!this.token) {
      await this.getAuthToken();
    }

    // Se ainda não temos token, tentar obter token básico
    if (!this.token) {
      await this.getToken();
    }

    // Configurar headers
    const headers = {
      "Content-Type": "application/json",
      ...options.headers,
    };

    // Adicionar token JWT se disponível
    const jwtToken = localStorage.getItem("jwt_token");
    if (jwtToken) {
      headers["Authorization"] = `Bearer ${jwtToken}`;
    }

    // Adicionar token CSRF
    if (this.token) {
      headers[this.headerName] = this.token;
    }

    // Fazer a requisição
    const response = await fetch(url, {
      ...options,
      headers,
    });

    // Se recebemos 400 (token inválido), tentar renovar
    if (response.status === 400) {
      console.log("🔄 Token CSRF pode estar inválido, renovando...");
      await this.getAuthToken();

      if (this.token) {
        headers[this.headerName] = this.token;
        return fetch(url, { ...options, headers });
      }
    }

    return response;
  }

  // Testar validação CSRF
  async testValidation() {
    try {
      const response = await this.securedFetch("/api/antiforgery/validate", {
        method: "POST",
        body: JSON.stringify({ test: "CSRF validation test" }),
      });

      if (response.ok) {
        const data = await response.json();
        console.log("✅ Validação CSRF bem-sucedida:", data);
        return true;
      } else {
        console.error("❌ Falha na validação CSRF:", response.status);
        return false;
      }
    } catch (error) {
      console.error("❌ Erro no teste de validação CSRF:", error);
      return false;
    }
  }

  // Renovar token CSRF
  async refreshToken() {
    this.token = null;
    return await this.getAuthToken();
  }

  // Verificar se o token existe e é válido
  hasValidToken() {
    return this.token !== null && this.token.length > 0;
  }

  // Obter informações do token atual
  getTokenInfo() {
    return {
      hasToken: this.hasValidToken(),
      tokenPreview: this.token ? this.token.substring(0, 20) + "..." : "N/A",
      cookieName: this.cookieName,
      headerName: this.headerName,
    };
  }

  // Renovar token (alias para refreshToken)
  async renewToken() {
    const result = await this.refreshToken();
    if (result) {
      console.log("🔄 Token CSRF renovado com sucesso!");
      alert("✅ Token CSRF renovado com sucesso!");
    } else {
      console.error("❌ Falha ao renovar token CSRF");
      alert("❌ Falha ao renovar token CSRF");
    }
    return result;
  }

  // Limpar token atual
  clearToken() {
    this.token = null;
    console.log("🗑️ Token CSRF limpo");
    alert(
      "🗑️ Token CSRF foi limpo. Clique em 'Obter Token' para criar um novo."
    );
  }
}

// Instância global
window.csrfHelper = new CSRFHelper();

// Inicializar token quando a página carregar
document.addEventListener("DOMContentLoaded", async function () {
  const jwtToken = localStorage.getItem("jwt_token");
  if (jwtToken) {
    await window.csrfHelper.getAuthToken();
  } else {
    await window.csrfHelper.getToken();
  }
});
