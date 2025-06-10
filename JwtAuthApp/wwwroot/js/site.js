// Funções utilitárias para autenticação JWT

// Verificar se o usuário está autenticado
function checkAuthentication() {
    const token = localStorage.getItem('jwt_token');
    const userData = localStorage.getItem('user_data');
    
    if (!token || !userData) {
        // Redirecionar para login se não estiver autenticado
        window.location.href = '/Home/Login';
        return false;
    }
    
    // Verificar se o token não expirou (simples)
    try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const currentTime = Math.floor(Date.now() / 1000);
        
        if (payload.exp < currentTime) {
            // Token expirado
            logout();
            return false;
        }
        
        // Atualizar nome do usuário na navbar
        const user = JSON.parse(userData);
        const userNameElement = document.getElementById('userNameNav');
        if (userNameElement) {
            userNameElement.textContent = user.username;
        }
        
        return true;
    } catch (error) {
        // Token inválido
        logout();
        return false;
    }
}

// Função de logout
async function logout() {
    try {
        const token = localStorage.getItem('jwt_token');
        
        if (token) {
            // Tentar chamar o endpoint de logout da API
            await fetch('/api/auth/logout', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
        }
    } catch (error) {
        console.log('Erro ao fazer logout na API:', error);
    } finally {
        // Limpar dados locais
        localStorage.removeItem('jwt_token');
        localStorage.removeItem('user_data');
        
        // Remover cookie
        document.cookie = 'jwt_token=; path=/; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
        
        // Redirecionar para login
        window.location.href = '/Home/Login';
    }
}

// Função para fazer requisições autenticadas
async function authenticatedFetch(url, options = {}) {
    const token = localStorage.getItem('jwt_token');
    
    if (!token) {
        throw new Error('Token não encontrado');
    }
    
    // Adicionar Authorization header
    const headers = {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
        ...(options.headers || {})
    };
    
    const response = await fetch(url, {
        ...options,
        headers
    });
    
    // Se não autorizado, fazer logout
    if (response.status === 401) {
        logout();
        throw new Error('Não autorizado');
    }
    
    return response;
}

// Formatação de números para moeda brasileira
function formatCurrency(value) {
    return new Intl.NumberFormat('pt-BR', {
        style: 'currency',
        currency: 'BRL'
    }).format(value);
}

// Formatação de números
function formatNumber(value) {
    return new Intl.NumberFormat('pt-BR').format(value);
}

// Formatação de datas
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR');
}

// Formatação de data e hora
function formatDateTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleString('pt-BR');
}

// Função para mostrar notificações (usando Bootstrap alerts)
function showNotification(message, type = 'info') {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show position-fixed" 
             style="top: 20px; right: 20px; z-index: 9999; min-width: 300px;" role="alert">
            <i class="fas fa-${getIconForType(type)} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    document.body.insertAdjacentHTML('beforeend', alertHtml);
    
    // Remover automaticamente após 5 segundos
    setTimeout(() => {
        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(alert => {
            if (alert.textContent.includes(message)) {
                alert.remove();
            }
        });
    }, 5000);
}

// Função auxiliar para ícones de notificação
function getIconForType(type) {
    switch (type) {
        case 'success': return 'check-circle';
        case 'danger': return 'exclamation-triangle';
        case 'warning': return 'exclamation-triangle';
        case 'info': return 'info-circle';
        default: return 'info-circle';
    }
}

// Função para mostrar/ocultar loading
function setLoading(elementId, isLoading) {
    const element = document.getElementById(elementId);
    if (element) {
        if (isLoading) {
            element.classList.add('loading');
        } else {
            element.classList.remove('loading');
        }
    }
}

// Inicialização quando a página carrega
document.addEventListener('DOMContentLoaded', function() {
    // Verificar se estamos na página de login
    const currentPath = window.location.pathname;
    
    if (currentPath === '/Home/Login' || currentPath === '/') {
        // Se já está logado, redirecionar para dashboard
        const token = localStorage.getItem('jwt_token');
        if (token) {
            try {
                const payload = JSON.parse(atob(token.split('.')[1]));
                const currentTime = Math.floor(Date.now() / 1000);
                
                if (payload.exp >= currentTime) {
                    window.location.href = '/Home/Dashboard';
                    return;
                }
            } catch (error) {
                // Token inválido, continuar na tela de login
            }
        }
        
        // Ocultar navbar na tela de login
        const navbar = document.getElementById('mainNav');
        if (navbar) {
            navbar.style.display = 'none';
        }
    } else {
        // Nas outras páginas, verificar autenticação
        const navbar = document.getElementById('mainNav');
        if (navbar) {
            navbar.style.display = 'block';
        }
        
        checkAuthentication();
    }
}); 