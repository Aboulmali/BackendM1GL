using BackendM1GL.Data;
using BackendM1GL.Entities;
using BackendM1GL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendM1GL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductsController> _logger;
        private readonly ICacheService _cache;  // ✅ Un seul service

        public ProductsController(
            AppDbContext context,
            ILogger<ProductsController> logger,
            ICacheService cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        // ✅ GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            const string cacheKey = "all_products";

            // Chercher dans Redis d'abord
            var cachedProducts = await _cache.GetAsync<List<Product>>(cacheKey);

            if (cachedProducts != null)
            {
                _logger.LogInformation("⚡ Produits récupérés depuis REDIS");
                return cachedProducts;
            }

            // Si pas en cache → BD
            _logger.LogInformation("🔍 Récupération depuis la BD");
            var products = await _context.Products.ToListAsync();

            // Sauvegarder dans Redis (5 minutes)
            await _cache.SetAsync(cacheKey, products, 5);

            return products;
        }

        // ✅ GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var cacheKey = $"product_{id}";

            // Chercher dans Redis
            var cachedProduct = await _cache.GetAsync<Product>(cacheKey);  

            if (cachedProduct != null)
            {
                _logger.LogInformation("⚡ Produit {Id} depuis REDIS", id);
                return cachedProduct;
            }

            // Si pas en cache → BD
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                _logger.LogWarning("⚠️ Produit {Id} non trouvé", id);
                return NotFound();
            }

            // Sauvegarder dans Redis
            await _cache.SetAsync(cacheKey, product, 5);  

            return product;
        }

        // ✅ PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
                return BadRequest("L'ID dans l'URL ne correspond pas à l'ID du produit");

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Produit {Id} mis à jour", id);

                // 🗑️ Invalider le cache
                await _cache.RemoveAsync($"product_{id}");      // ← _cache.RemoveAsync
                await _cache.RemoveAsync("all_products");        // ← _cache.RemoveAsync
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // ✅ POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Nouveau produit créé : {Name}", product.Name);

            // 🗑️ Invalider le cache de la liste
            await _cache.RemoveAsync("all_products");  // ← _cache.RemoveAsync

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // ✅ DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("🗑️ Produit {Id} supprimé", id);

            // 🗑️ Invalider le cache
            await _cache.RemoveAsync($"product_{id}");      
            await _cache.RemoveAsync("all_products");        

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}