# Sistema de Autenticação JWT - .NET Core 8.0

Este é um sistema completo de autenticação JWT implementado em .NET Core 8.0 com interface web e API. O projeto inclui telas de login, logout e 3 páginas principais que consomem dados da API através de autenticação JWT.

## 🚀 Como executar

```bash
dotnet run
```

A aplicação será executada em `https://localhost:7066` por padrão.

## 👥 Usuários de teste

O sistema possui 3 usuários pré-configurados:

- **Admin**: username: `admin`, password: `admin123`
- **User1**: username: `user1`, password: `user123`  
- **User2**: username: `user2`, password: `user456`

## 📱 Funcionalidades

### Interface Web (MVC)
- **Tela de Login** - Interface simplificada para autenticação
- **Dashboard** - Painel principal com estatísticas e gráficos
- **Produtos** - Catálogo de produtos com filtros e categorias
- **Usuários** - Gerenciamento de usuários do sistema
- **Logout** - Funcionalidade de sair do sistema

### API REST
- Autenticação JWT com tokens de 1 hora
- Endpoints protegidos para cada funcionalidade
- CORS configurado para integração

## 🔐 Como usar

1. **Acesse a aplicação**: Navegue para `https://localhost:7066`
2. **Faça login**: Use um dos usuários de teste na tela de login
3. **Navegue pelas telas**: Use o menu superior para acessar as diferentes funcionalidades
4. **Logout**: Clique no dropdown do usuário e selecione "Sair"

## 🏗️ Estrutura do projeto

```
JwtAuthApp/
├── Controllers/
│   ├── HomeController.cs      # Controller MVC para as views
│   ├── AuthController.cs      # API de autenticação
│   ├── DashboardController.cs # API do dashboard
│   ├── ProductsController.cs  # API de produtos
│   └── UsersController.cs     # API de usuários
├── Views/
│   ├── Shared/
│   │   └── _Layout.cshtml     # Layout principal
│   └── Home/
│       ├── Login.cshtml       # Tela de login
│       ├── Dashboard.cshtml   # Dashboard principal
│       ├── Products.cshtml    # Catálogo de produtos
│       └── Users.cshtml       # Gerenciamento de usuários
├── wwwroot/
│   ├── css/
│   │   └── site.css          # Estilos personalizados
│   └── js/
│       └── site.js           # JavaScript de autenticação
├── Models/                    # Modelos de dados
├── Services/                  # Serviços (AuthService)
└── Program.cs                # Configuração da aplicação
```

## 🎨 Tecnologias utilizadas

- **.NET Core 8.0** - Framework principal
- **ASP.NET Core MVC** - Interface web
- **ASP.NET Core Web API** - API REST
- **JWT Bearer Authentication** - Autenticação
- **Bootstrap 5** - Interface responsiva
- **Font Awesome** - Ícones
- **JavaScript ES6+** - Interatividade frontend

## 🔧 Endpoints da API

### Autenticação
- `POST /api/auth/login` - Fazer login
- `POST /api/auth/logout` - Fazer logout (requer token)
- `GET /api/auth/profile` - Obter perfil do usuário (requer token)

### Dashboard
- `GET /api/dashboard` - Dados do dashboard (requer token)
- `GET /api/dashboard/stats` - Estatísticas (requer token)

### Produtos
- `GET /api/products` - Lista de produtos (requer token)
- `GET /api/products/{id}` - Detalhes de um produto (requer token)
- `GET /api/products/categories` - Categorias (requer token)

### Usuários
- `GET /api/users` - Lista de usuários (requer token)
- `GET /api/users/{id}` - Detalhes de um usuário (requer token)
- `GET /api/users/stats` - Estatísticas de usuários (requer token)

## ⚙️ Configuração JWT

As configurações JWT estão no arquivo `appsettings.json`:

```json
{
  "JwtSettings": {
    "Secret": "MinhaChaveSecretaSuperSegura123456789",
    "Issuer": "JwtAuthApp",
    "Audience": "JwtAuthApp-Users",
    "ExpirationHours": 1
  }
}
```

## 🔒 Segurança

- Tokens JWT com expiração de 1 hora
- Validação automática de tokens no frontend
- Redirecionamento automático para login quando token expira
- Headers de autorização em todas as requisições protegidas
- Logout limpa tokens do localStorage e cookies

## 📱 Responsividade

A interface é totalmente responsiva e funciona em:
- Desktop
- Tablets
- Smartphones

## 🧪 Testando a API

Use o arquivo `test-api.http` para testar os endpoints da API diretamente, ou use as telas web que já consomem a API automaticamente.

## 📝 Exemplo de fluxo

1. **Login**: Usuário acessa `/` → é redirecionado para `/Home/Login`
2. **Autenticação**: Submete credenciais → API retorna JWT token
3. **Armazenamento**: Token é salvo no localStorage e cookie
4. **Navegação**: Usuário é redirecionado para `/Home/Dashboard`
5. **Consumo da API**: Cada tela faz requisições autenticadas para os endpoints
6. **Logout**: Usuário clica em sair → tokens são limpos → redirecionado para login

O sistema está pronto para uso em desenvolvimento e pode ser facilmente adaptado para produção! 