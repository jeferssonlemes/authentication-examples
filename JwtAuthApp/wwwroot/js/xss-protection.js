// 🛡️ Utilitário de Proteção XSS
class XSSProtection {
  constructor() {
    this.allowedTags = ["b", "i", "strong", "em", "br"];
    this.allowedAttributes = [];
  }

  // Escapar HTML para prevenir XSS
  escapeHtml(unsafe) {
    if (typeof unsafe !== "string") {
      return String(unsafe);
    }

    return unsafe
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#039;")
      .replace(/\//g, "&#x2F;");
  }

  // Sanitizar texto removendo tags HTML perigosas
  sanitizeText(text) {
    if (typeof text !== "string") {
      return String(text);
    }

    // Remover scripts e outros elementos perigosos
    const dangerous = /<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi;
    const cleaned = text.replace(dangerous, "");

    // Escapar o resto
    return this.escapeHtml(cleaned);
  }

  // Criar elemento DOM de forma segura
  createSafeElement(tagName, textContent = "", className = "") {
    const element = document.createElement(tagName);

    if (textContent) {
      element.textContent = textContent; // textContent é seguro contra XSS
    }

    if (className) {
      element.className = className;
    }

    return element;
  }

  // Criar HTML seguro para alertas/notificações
  createSafeAlert(message, type = "info", includeIcon = true) {
    const alertDiv = this.createSafeElement(
      "div",
      "",
      `alert alert-${this.sanitizeText(
        type
      )} alert-dismissible fade show position-fixed`
    );
    alertDiv.style.cssText =
      "top: 20px; right: 20px; z-index: 9999; min-width: 300px;";
    alertDiv.setAttribute("role", "alert");

    if (includeIcon) {
      const icon = this.createSafeElement(
        "i",
        "",
        `fas fa-${this.getIconForType(type)} me-2`
      );
      alertDiv.appendChild(icon);
    }

    const messageSpan = this.createSafeElement(
      "span",
      this.sanitizeText(message)
    );
    alertDiv.appendChild(messageSpan);

    const closeButton = this.createSafeElement("button", "", "btn-close");
    closeButton.type = "button";
    closeButton.setAttribute("data-bs-dismiss", "alert");
    alertDiv.appendChild(closeButton);

    return alertDiv;
  }

  // Atualizar conteúdo de forma segura
  safeUpdateContent(elementId, content, isHtml = false) {
    const element = document.getElementById(elementId);
    if (!element) {
      console.warn(`Elemento ${elementId} não encontrado`);
      return;
    }

    if (isHtml) {
      // Se precisar de HTML, sanitizar primeiro
      element.innerHTML = this.sanitizeHtml(content);
    } else {
      // Usar textContent para texto puro (mais seguro)
      element.textContent = content;
    }
  }

  // Sanitizar HTML permitindo apenas tags seguras
  sanitizeHtml(html) {
    if (typeof html !== "string") {
      return String(html);
    }

    // Método mais simples que não causa loops
    return html
      .replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, "")
      .replace(/<iframe\b[^<]*(?:(?!<\/iframe>)<[^<]*)*<\/iframe>/gi, "")
      .replace(/<object\b[^<]*(?:(?!<\/object>)<[^<]*)*<\/object>/gi, "")
      .replace(/<embed[^>]*>/gi, "")
      .replace(/javascript:/gi, "")
      .replace(/on\w+\s*=\s*["'][^"']*["']/gi, "")
      .replace(/on\w+\s*=\s*[^>\s]+/gi, "");
  }

  // Validar e sanitizar dados de API
  sanitizeApiResponse(data) {
    if (typeof data === "string") {
      return this.sanitizeText(data);
    }

    if (Array.isArray(data)) {
      return data.map((item) => this.sanitizeApiResponse(item));
    }

    if (typeof data === "object" && data !== null) {
      const sanitized = {};
      for (const [key, value] of Object.entries(data)) {
        sanitized[key] = this.sanitizeApiResponse(value);
      }
      return sanitized;
    }

    return data;
  }

  // Função auxiliar para ícones
  getIconForType(type) {
    const iconMap = {
      success: "check-circle",
      danger: "exclamation-triangle",
      warning: "exclamation-triangle",
      info: "info-circle",
    };
    return iconMap[type] || "info-circle";
  }

  // Criar estrutura HTML complexa de forma segura
  createSafeStructure(template, data) {
    const container = document.createElement("div");

    // Processar template de forma segura
    const processedTemplate = template.replace(
      /\{\{(\w+)\}\}/g,
      (match, key) => {
        return this.escapeHtml(data[key] || "");
      }
    );

    container.innerHTML = this.sanitizeHtml(processedTemplate);
    return container;
  }

  // Validar URLs para prevenir javascript: e data: URLs maliciosos
  sanitizeUrl(url) {
    if (typeof url !== "string") {
      return "#";
    }

    // Bloquear URLs perigosos
    const dangerous = /^(javascript:|data:|vbscript:|file:|about:)/i;
    if (dangerous.test(url.trim())) {
      console.warn("URL perigoso bloqueado:", url);
      return "#";
    }

    return url;
  }

  // Log de tentativas de XSS para monitoramento
  logXSSAttempt(context, payload) {
    console.warn("🚨 Tentativa de XSS detectada:", {
      context: context,
      payload: payload,
      timestamp: new Date().toISOString(),
      userAgent: navigator.userAgent,
    });

    // Em produção, enviar para sistema de monitoramento
    if (window.location.hostname !== "localhost") {
      fetch("/api/security/xss-attempt", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          context,
          payload: payload.substring(0, 500), // Limitar tamanho
          timestamp: new Date().toISOString(),
        }),
      }).catch(() => {}); // Falhar silenciosamente
    }
  }
}

// Instância global
window.xssProtection = new XSSProtection();

// Proteção mais segura sem sobrescrever innerHTML globalmente
// (removido para evitar loops infinitos - a proteção será feita manualmente onde necessário)

console.log("🛡️ Proteção XSS ativada!");
