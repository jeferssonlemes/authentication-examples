using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetUsers()
        {
            var usuarios = new[]
            {
                new
                {
                    id = 1,
                    nome = "João Silva",
                    email = "joao.silva@email.com",
                    perfil = "Administrador",
                    ativo = true,
                    dataCadastro = "2024-01-15",
                    ultimoAcesso = "2024-06-09"
                },
                new
                {
                    id = 2,
                    nome = "Maria Santos",
                    email = "maria.santos@email.com",
                    perfil = "Usuário",
                    ativo = true,
                    dataCadastro = "2024-02-20",
                    ultimoAcesso = "2024-06-08"
                },
                new
                {
                    id = 3,
                    nome = "Pedro Oliveira",
                    email = "pedro.oliveira@email.com",
                    perfil = "Usuário",
                    ativo = false,
                    dataCadastro = "2024-03-10",
                    ultimoAcesso = "2024-05-15"
                },
                new
                {
                    id = 4,
                    nome = "Ana Costa",
                    email = "ana.costa@email.com",
                    perfil = "Moderador",
                    ativo = true,
                    dataCadastro = "2024-04-05",
                    ultimoAcesso = "2024-06-09"
                },
                new
                {
                    id = 5,
                    nome = "Carlos Pereira",
                    email = "carlos.pereira@email.com",
                    perfil = "Usuário",
                    ativo = true,
                    dataCadastro = "2024-05-12",
                    ultimoAcesso = "2024-06-07"
                }
            };

            return Ok(new
            {
                titulo = "Gerenciamento de Usuários",
                totalUsuarios = usuarios.Length,
                usuariosAtivos = usuarios.Count(u => u.ativo),
                usuarios = usuarios
            });
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            var usuario = new
            {
                id = id,
                nome = "Usuário Exemplo",
                email = "usuario@email.com",
                perfil = "Usuário",
                ativo = true,
                informacoesPessoais = new
                {
                    telefone = "(11) 99999-9999",
                    endereco = "Rua Exemplo, 123 - São Paulo, SP",
                    dataNascimento = "1990-05-15"
                },
                permissoes = new[]
                {
                    "Visualizar Dashboard",
                    "Gerenciar Produtos",
                    "Visualizar Relatórios"
                }
            };

            return Ok(usuario);
        }

        [HttpGet("stats")]
        public IActionResult GetUserStats()
        {
            return Ok(new
            {
                estatisticas = new
                {
                    totalUsuarios = 156,
                    usuariosAtivos = 142,
                    novosUsuariosHoje = 3,
                    novosUsuariosSemana = 18,
                    distribuicaoPorPerfil = new
                    {
                        administradores = 5,
                        moderadores = 12,
                        usuarios = 139
                    }
                }
            });
        }
    }
} 