# Configuração de Debug - VS Code

Este diretório contém as configurações necessárias para debugar a aplicação JwtAuthApp no Visual Studio Code usando o debugger `netcoredbg`.

## Arquivos de Configuração

### launch.json
Contém 3 configurações de debug:

1. **JwtAuthApp .NET Core** (Padrão - CoreCLR)
   - Ambiente: Development
   - URLs: http://localhost:5000 e https://localhost:5001
   - Usa o debugger padrão do VS Code
   - Abre automaticamente o navegador quando a aplicação estiver pronta

2. **JwtAuthApp .NET Core (HTTPS Only)** (CoreCLR)
   - Ambiente: Development
   - URL: https://localhost:5001 apenas
   - Para testes específicos de HTTPS

3. **JwtAuthApp .NET Core (netcoredbg)** (NetCoreDbg via pipe)
   - Ambiente: Development
   - URLs: http://localhost:5000 e https://localhost:5001
   - Usa netcoredbg através de pipe transport
   - Requer variável de ambiente `NETCOREDBG_PATH`

### tasks.json
Define as seguintes tarefas:

- **build-jwtauthapp**: Compila o projeto (tarefa padrão)
- **publish-jwtauthapp**: Publica o projeto
- **watch-jwtauthapp**: Executa com hot reload
- **clean-jwtauthapp**: Limpa os arquivos de build
- **restore-jwtauthapp**: Restaura os pacotes NuGet

### settings.json
Configurações específicas do workspace:
- Formatação automática ao salvar
- Exclusão de pastas bin/obj da busca
- Configurações do OmniSharp/C# Dev Kit

### extensions.json
Recomenda extensões úteis:
- C# Dev Kit
- PowerShell
- REST Client
- Path IntelliSense

## Como Usar

### Para Debugar:
1. Abra o VS Code na pasta JwtAuthApp
2. Pressione `F5` ou vá em "Run and Debug" (Ctrl+Shift+D)
3. Selecione uma das configurações disponíveis
4. A aplicação será compilada e executada automaticamente
5. O navegador abrirá automaticamente quando estiver pronto

### Para Executar Tarefas:
1. Pressione `Ctrl+Shift+P`
2. Digite "Tasks: Run Task"
3. Selecione a tarefa desejada

### Breakpoints:
- Clique na margem esquerda do editor para adicionar breakpoints
- Os breakpoints funcionarão em Controllers, Services, Middlewares, etc.

## URLs da Aplicação

- **Home/Login**: http://localhost:5000 ou https://localhost:5001
- **API Swagger**: http://localhost:5000/swagger (se habilitado)
- **API Base**: http://localhost:5000/api

## Credenciais de Teste

- **Admin**: admin / admin123
- **Moderador**: user1 / user123  
- **Usuário**: user2 / user456

## Sobre os Debuggers Disponíveis

### CoreCLR (Padrão)
- Debugger padrão do VS Code para .NET
- Funciona out-of-the-box com a extensão C#
- Recomendado para uso geral

### NetCoreDbg (Avançado)
- Debugger alternativo de alta performance
- Requer instalação separada e configuração da variável `NETCOREDBG_PATH`
- Melhor para debugging avançado e cenários específicos
- Para usar: defina `NETCOREDBG_PATH` apontando para o diretório do netcoredbg.exe

## Dicas de Debug

1. Use o console integrado para ver logs da aplicação
2. Configure breakpoints nos Controllers para interceptar requests
3. Use o "Watch" para monitorar variáveis específicas
4. O hot reload está disponível com a tarefa "watch-jwtauthapp"
5. O `vsdbg` oferece melhor suporte a debugging de código assíncrono
6. Use "Step Into" (F11) para debugar métodos internos do framework 