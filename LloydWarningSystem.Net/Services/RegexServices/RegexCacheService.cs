﻿using Microsoft.Extensions.Caching.Memory;
using System.Collections.Immutable;

namespace LloydWarningSystem.Net.Services.RegexServices;

public interface IRegexCache
{
    void Add(ulong guildId, string regexName);
    void Remove(ulong guildId, string regexName);
    void Set(ulong guildId, IEnumerable<string> tags);
    ImmutableArray<string> Search(ulong guildId, string partialName, int? maxResults = null);
}

public class RegexCache : IRegexCache
{
    private readonly IMemoryCache _cache;

    private static readonly ReaderWriterLockSlim _cacheLock = new();

    public RegexCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void Add(ulong guildId, string regexName)
    {
        _cacheLock.EnterWriteLock();

        try
        {
            var cachedTags = GetFromCache(guildId);
            cachedTags.Add(regexName);
        }
        finally
        {
            _cacheLock.ExitWriteLock();
        }
    }

    public void Remove(ulong guildId, string regexName)
    {
        _cacheLock.EnterWriteLock();

        try
        {
            var cachedTags = GetFromCache(guildId);
            cachedTags.Remove(regexName);
        }
        finally
        {
            _cacheLock.ExitWriteLock();
        }
    }

    public void Set(ulong guildId, IEnumerable<string> tags)
    {
        _cacheLock.EnterWriteLock();

        try
        {
            _cache.Set(GetCacheKey(guildId), new SortedSet<string>(tags), 
                new MemoryCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10) });
        }
        finally
        {
            _cacheLock.ExitWriteLock();
        }
    }

    public ImmutableArray<string> Search(ulong guildId, string partialName, int? maxResults = null)
    {
        if (string.IsNullOrWhiteSpace(partialName))
            return ImmutableArray<string>.Empty;

        _cacheLock.EnterReadLock();

        try
        {
            var cachedTags = GetFromCache(guildId);

            var results = cachedTags
                .Where(x => x.Contains(partialName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x);

            return maxResults.HasValue 
                ? results.Take(maxResults.Value).ToImmutableArray() 
                : results.ToImmutableArray();
        }
        finally
        {
            _cacheLock.ExitReadLock();
        }
    }

    private SortedSet<string> GetFromCache(ulong guildId)
        => _cache.Get<SortedSet<string>>(GetCacheKey(guildId)) ?? [];

    private static object GetCacheKey(ulong guildId)
        => new { guildId, Target = "Regex" };
}