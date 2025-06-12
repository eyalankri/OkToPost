using Microsoft.Extensions.Caching.Distributed;
using OkToPost.DTOs;
using OkToPost.Models;
using OkToPost.Repositories;
using OkToPost.Services;
using OkToPost.Utils;

public class UrlShortenerService : IUrlShortenerService
{
    private readonly IUrlRepository _repository;
    private readonly ICodeGenerator _codeGenerator;
    private readonly IDistributedCache _cache;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;

    public UrlShortenerService(
        IUrlRepository repository,
        ICodeGenerator codeGenerator,
        IDistributedCache cache,
        IWebHostEnvironment env,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory)
    {
        _repository = repository;
        _codeGenerator = codeGenerator;
        _cache = cache;
        _env = env;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
    }

    public async Task<UrlResolutionResult> ShortenUrlAsync(string url)
    {
        try
        {
            UrlMapping? existing;
            try
            {
                existing = await _repository.GetByUrlAsync(url);
            }
            catch
            {
                return new UrlResolutionResult { Status = UrlResolutionStatus.DatabaseError };
            }

            if (existing != null)
            {
                return new UrlResolutionResult
                {
                    Url = GetFullShortUrl(existing.ShortCode),
                    Code = existing.ShortCode,
                    Status = UrlResolutionStatus.Success
                };
            }

            string code;
            try
            {
                do
                {
                    code = _codeGenerator.GenerateCode();
                } while (await _repository.GetByCodeAsync(code) != null);
            }
            catch
            {
                return new UrlResolutionResult { Status = UrlResolutionStatus.DatabaseError };
            }

            var mapping = new UrlMapping
            {
                OriginalUrl = url,
                ShortCode = code
            };

            try
            {
                await _repository.AddAsync(mapping);
            }
            catch
            {
                return new UrlResolutionResult { Status = UrlResolutionStatus.DatabaseError };
            }

            return new UrlResolutionResult
            {
                Url = GetFullShortUrl(code),
                Code = code,
                Status = UrlResolutionStatus.Success
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Unexpected error shortening URL '{url}': {ex.Message}");
            return new UrlResolutionResult { Status = UrlResolutionStatus.UnexpectedError };
        }
    }

    public async Task<UrlResolutionResult> GetOriginalUrlAsync(string code)
    {
        try
        {
            string? cachedUrl = await _cache.GetStringAsync(code);
            if (cachedUrl != null)
            {
                _ = Task.Run(async () =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scopedRepo = scope.ServiceProvider.GetRequiredService<IUrlRepository>();

                    try
                    {
                        await scopedRepo.IncrementClickCountAsync(code);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WARN] Failed to increment click count (background): {ex.Message}");
                    }
                });

                return new UrlResolutionResult
                {
                    Url = cachedUrl,
                    Status = UrlResolutionStatus.Success
                };
            }

            UrlMapping? mapping;
            try
            {
                mapping = await _repository.GetByCodeAsync(code);
            }
            catch
            {
                return new UrlResolutionResult { Status = UrlResolutionStatus.DatabaseError };
            }

            if (mapping == null)
            {
                return new UrlResolutionResult { Status = UrlResolutionStatus.NotFound };
            }

            await _cache.SetStringAsync(code, mapping.OriginalUrl, new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(10)
            });

            _ = Task.Run(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var scopedRepo = scope.ServiceProvider.GetRequiredService<IUrlRepository>();

                try
                {
                    await scopedRepo.IncrementClickCountAsync(code);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARN] Failed to increment click count (background): {ex.Message}");
                }
            });

            return new UrlResolutionResult
            {
                Url = mapping.OriginalUrl,
                Status = UrlResolutionStatus.Success
            };
        }
        catch
        {
            return new UrlResolutionResult { Status = UrlResolutionStatus.UnexpectedError };
        }
    }

    public async Task<UrlStatsResult> GetStatsAsync(string code)
    {
        UrlMapping? mapping;
        try
        {
            mapping = await _repository.GetByCodeAsync(code);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to get stats for code '{code}': {ex.Message}");
            return new UrlStatsResult { Status = UrlResolutionStatus.DatabaseError };
        }

        if (mapping == null)
        {
            return new UrlStatsResult { Status = UrlResolutionStatus.NotFound };
        }

        return new UrlStatsResult
        {
            Stats = new UrlStatsResponse
            {
                Code = mapping.ShortCode,
                Clicks = mapping.ClickCount,
                CreatedAt = mapping.CreatedAt
            },
            Status = UrlResolutionStatus.Success
        };
    }

    private string GetFullShortUrl(string code)
    {
        string? baseUrl = _env.IsDevelopment()
            ? _configuration["ShortUrl:Development"]
            : _configuration["ShortUrl:Production"];

        baseUrl ??= "http://sdf.co.il";
        return $"{baseUrl.TrimEnd('/')}/{code}";
    }
}