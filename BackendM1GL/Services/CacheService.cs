using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BackendM1GL.Services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, int minutes = 10);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }

    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(
            IDistributedCache cache,
            ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        // ✅ Récupérer
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await _cache.GetStringAsync(key);

                if (string.IsNullOrEmpty(value))
                    return default;

                return JsonSerializer.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lecture cache: {Key}", key);
                return default;
            }
        }

        // ✅ Sauvegarder
        public async Task SetAsync<T>(string key, T value, int minutes = 10)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);

                await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(minutes)
                });

                _logger.LogInformation("✅ Cache sauvegardé: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur écriture cache: {Key}", key);
            }
        }

        // ✅ Supprimer
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogInformation("🗑️ Cache supprimé: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur suppression cache: {Key}", key);
            }
        }

        // ✅ Vérifier existence
        public async Task<bool> ExistsAsync(string key)
        {
            var value = await _cache.GetStringAsync(key);
            return !string.IsNullOrEmpty(value);
        }
    }
}