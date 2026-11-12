using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace ApiSDD.Data.NoSql;

/// <summary>
/// Documento TTL para caché distribuida y blacklist JWT compartida entre réplicas en MongoDB Atlas.
/// </summary>
public sealed class CacheDistribuidaDoc
{
    [BsonId]
    public string Id { get; set; } = default!;

    [BsonElement("valor")]
    public string ValorBase64 { get; set; } = string.Empty;

    [BsonElement("expiraEnUtc")]
    public DateTime? ExpiraEnUtc { get; set; }

    [BsonElement("slidingSeconds")]
    public long? SlidingSeconds { get; set; }
}

/// <summary>
/// Implementación <see cref="IDistributedCache"/> respaldada por MongoDB Atlas con expiración TTL automática.
/// Se activa en modo costo cero cuando <c>Redis:ConnectionString</c> no está configurado, garantizando
/// revocación inmediata de JWT y coherencia de caché entre múltiples réplicas de la API.
/// </summary>
internal sealed class MongoDistributedCache : IDistributedCache
{
    internal const string CollectionName = "cache_distribuida";
    private readonly MongoDbContext _mongo;
    private readonly TimeProvider _time;

    public MongoDistributedCache(MongoDbContext mongo, TimeProvider time)
    {
        _mongo = mongo ?? throw new ArgumentNullException(nameof(mongo));
        _time = time ?? throw new ArgumentNullException(nameof(time));
    }

    private IMongoCollection<CacheDistribuidaDoc> Collection =>
        _mongo.GetCollection<CacheDistribuidaDoc>(CollectionName);

    public byte[]? Get(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        var now = _time.GetUtcNow().UtcDateTime;
        var doc = Collection.Find(ActiveFilter(key, now)).FirstOrDefault();
        if (doc is null)
        {
            return null;
        }

        RefreshSlidingSync(doc, now);
        return Convert.FromBase64String(doc.ValorBase64);
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        var now = _time.GetUtcNow().UtcDateTime;
        var doc = await Collection.Find(ActiveFilter(key, now)).FirstOrDefaultAsync(token).ConfigureAwait(false);
        if (doc is null)
        {
            return null;
        }

        if (doc.SlidingSeconds is > 0)
        {
            var nextExp = now.AddSeconds(doc.SlidingSeconds.Value);
            await Collection.UpdateOneAsync(
                Builders<CacheDistribuidaDoc>.Filter.Eq(x => x.Id, key),
                Builders<CacheDistribuidaDoc>.Update.Set(x => x.ExpiraEnUtc, nextExp),
                cancellationToken: token).ConfigureAwait(false);
        }

        return Convert.FromBase64String(doc.ValorBase64);
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);
        var doc = BuildDocument(key, value, options, _time.GetUtcNow().UtcDateTime);
        Collection.ReplaceOne(
            Builders<CacheDistribuidaDoc>.Filter.Eq(x => x.Id, key),
            doc,
            new ReplaceOptions { IsUpsert = true });
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);
        var doc = BuildDocument(key, value, options, _time.GetUtcNow().UtcDateTime);
        return Collection.ReplaceOneAsync(
            Builders<CacheDistribuidaDoc>.Filter.Eq(x => x.Id, key),
            doc,
            new ReplaceOptions { IsUpsert = true },
            token);
    }

    public void Refresh(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        var now = _time.GetUtcNow().UtcDateTime;
        var doc = Collection.Find(ActiveFilter(key, now)).FirstOrDefault();
        if (doc is not null)
        {
            RefreshSlidingSync(doc, now);
        }
    }

    public async Task RefreshAsync(string key, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        var now = _time.GetUtcNow().UtcDateTime;
        var doc = await Collection.Find(ActiveFilter(key, now)).FirstOrDefaultAsync(token).ConfigureAwait(false);
        if (doc?.SlidingSeconds is > 0)
        {
            var nextExp = now.AddSeconds(doc.SlidingSeconds.Value);
            await Collection.UpdateOneAsync(
                Builders<CacheDistribuidaDoc>.Filter.Eq(x => x.Id, key),
                Builders<CacheDistribuidaDoc>.Update.Set(x => x.ExpiraEnUtc, nextExp),
                cancellationToken: token).ConfigureAwait(false);
        }
    }

    public void Remove(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        Collection.DeleteOne(Builders<CacheDistribuidaDoc>.Filter.Eq(x => x.Id, key));
    }

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return Collection.DeleteOneAsync(Builders<CacheDistribuidaDoc>.Filter.Eq(x => x.Id, key), token);
    }

    internal static CacheDistribuidaDoc BuildDocument(
        string key,
        byte[] value,
        DistributedCacheEntryOptions options,
        DateTime utcNow)
    {
        var expira = ComputeExpirationUtc(options, utcNow);
        var sliding = options.SlidingExpiration.HasValue
            ? (long?)Math.Max(1, Math.Ceiling(options.SlidingExpiration.Value.TotalSeconds))
            : null;
        return new CacheDistribuidaDoc
        {
            Id = key,
            ValorBase64 = Convert.ToBase64String(value),
            ExpiraEnUtc = expira,
            SlidingSeconds = sliding,
        };
    }

    internal static DateTime ComputeExpirationUtc(DistributedCacheEntryOptions options, DateTime utcNow)
    {
        if (options.AbsoluteExpiration.HasValue)
        {
            return options.AbsoluteExpiration.Value.UtcDateTime;
        }

        if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            return utcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);
        }

        if (options.SlidingExpiration.HasValue)
        {
            return utcNow.Add(options.SlidingExpiration.Value);
        }

        return utcNow.AddHours(1);
    }

    private static FilterDefinition<CacheDistribuidaDoc> ActiveFilter(string key, DateTime utcNow) =>
        Builders<CacheDistribuidaDoc>.Filter.And(
            Builders<CacheDistribuidaDoc>.Filter.Eq(x => x.Id, key),
            Builders<CacheDistribuidaDoc>.Filter.Or(
                Builders<CacheDistribuidaDoc>.Filter.Eq(x => x.ExpiraEnUtc, null),
                Builders<CacheDistribuidaDoc>.Filter.Gt(x => x.ExpiraEnUtc, utcNow)));

    private void RefreshSlidingSync(CacheDistribuidaDoc doc, DateTime utcNow)
    {
        if (doc.SlidingSeconds is > 0)
        {
            Collection.UpdateOne(
                Builders<CacheDistribuidaDoc>.Filter.Eq(x => x.Id, doc.Id),
                Builders<CacheDistribuidaDoc>.Update.Set(x => x.ExpiraEnUtc, utcNow.AddSeconds(doc.SlidingSeconds.Value)));
        }
    }
}
