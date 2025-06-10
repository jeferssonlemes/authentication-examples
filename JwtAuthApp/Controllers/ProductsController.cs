using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        [HttpGet]
        [Authorize(Policy = "ViewProducts")]
        public IActionResult GetProducts()
        {
            var produtos = new[]
            {
                new
                {
                    id = 1,
                    nome = "Notebook Dell Inspiron",
                    preco = 2499.99m,
                    categoria = "Informática",
                    estoque = 15,
                    status = "Ativo"
                },
                new
                {
                    id = 2,
                    nome = "Smartphone Samsung Galaxy",
                    preco = 1299.99m,
                    categoria = "Telefonia",
                    estoque = 8,
                    status = "Ativo"
                },
                new
                {
                    id = 3,
                    nome = "Tênis Nike Air Max",
                    preco = 399.99m,
                    categoria = "Calçados",
                    estoque = 25,
                    status = "Ativo"
                },
                new
                {
                    id = 4,
                    nome = "Fones de Ouvido JBL",
                    preco = 299.99m,
                    categoria = "Áudio",
                    estoque = 0,
                    status = "Sem Estoque"
                },
                new
                {
                    id = 5,
                    nome = "Cadeira Gamer DXRacer",
                    preco = 899.99m,
                    categoria = "Móveis",
                    estoque = 12,
                    status = "Ativo"
                }
            };

            return Ok(new
            {
                titulo = "Catálogo de Produtos",
                totalProdutos = produtos.Length,
                produtos = produtos
            });
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "ViewProducts")]
        public IActionResult GetProduct(int id)
        {
            var produto = new
            {
                id = id,
                nome = "Produto Exemplo",
                descricao = "Esta é uma descrição detalhada do produto.",
                preco = 199.99m,
                categoria = "Categoria Exemplo",
                especificacoes = new[]
                {
                    "Especificação 1",
                    "Especificação 2",
                    "Especificação 3"
                }
            };

            return Ok(produto);
        }

        [HttpGet("categories")]
        [Authorize(Policy = "ViewProducts")]
        public IActionResult GetCategories()
        {
            var categorias = new[]
            {
                new { id = 1, nome = "Informática", total = 45 },
                new { id = 2, nome = "Telefonia", total = 23 },
                new { id = 3, nome = "Calçados", total = 67 },
                new { id = 4, nome = "Áudio", total = 18 },
                new { id = 5, nome = "Móveis", total = 12 }
            };

            return Ok(categorias);
        }

        [HttpPost]
        [Authorize(Policy = "EditProducts")]
        public IActionResult CreateProduct([FromBody] object productData)
        {
            // Simular criação de produto - apenas Moderators e Admins
            return Ok(new { message = "Produto criado com sucesso!", id = new Random().Next(100, 999) });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "EditProducts")]
        public IActionResult UpdateProduct(int id, [FromBody] object productData)
        {
            // Simular atualização de produto - apenas Moderators e Admins
            return Ok(new { message = $"Produto {id} atualizado com sucesso!" });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "DeleteProducts")]
        public IActionResult DeleteProduct(int id)
        {
            // Simular exclusão de produto - apenas Admins
            return Ok(new { message = $"Produto {id} excluído com sucesso!" });
        }

        [HttpGet("admin/stats")]
        [Authorize(Policy = "ManageProducts")]
        public IActionResult GetAdminStats()
        {
            // Estatísticas administrativas - apenas Admins
            return Ok(new
            {
                totalProducts = 156,
                lowStockProducts = 12,
                recentlyAdded = 8,
                topCategories = new[]
                {
                    new { category = "Informática", sales = 45000 },
                    new { category = "Telefonia", sales = 32000 }
                }
            });
        }
    }
} 