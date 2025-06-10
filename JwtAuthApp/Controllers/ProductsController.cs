using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        [HttpGet]
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
    }
} 