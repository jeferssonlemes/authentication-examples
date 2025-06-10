using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetDashboard()
        {
            var username = User.Identity?.Name;
            
            var dashboardData = new
            {
                titulo = "Dashboard Principal",
                mensagem = $"Bem-vindo ao dashboard, {username}!",
                dados = new
                {
                    totalVendas = 125000.50m,
                    numeroClientes = 1250,
                    pedidosPendentes = 45,
                    crescimentoMensal = 12.5
                },
                graficos = new[]
                {
                    new { nome = "Vendas por Mês", valores = new[] { 10000, 15000, 12000, 18000, 22000 } },
                    new { nome = "Clientes Ativos", valores = new[] { 800, 950, 1100, 1200, 1250 } }
                }
            };

            return Ok(dashboardData);
        }

        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            return Ok(new
            {
                estatisticas = new
                {
                    visitasHoje = 1847,
                    visitasOntem = 1654,
                    novosUsuarios = 23,
                    taxaConversao = 3.4
                }
            });
        }
    }
} 