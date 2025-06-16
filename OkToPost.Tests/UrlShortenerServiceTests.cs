using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OkToPost.DTOs;
using OkToPost.Models;
using OkToPost.Repositories;
using OkToPost.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;
using OkToPost.Utils;

namespace OkToPost.Tests.Services
{
    public class UrlShortenerServiceTests
    {
        // Mock dependencies
        private readonly Mock<IUrlRepository> _mockRepo = new();
        private readonly Mock<IDistributedCache> _mockCache = new();
        private readonly Mock<IWebHostEnvironment> _mockEnv = new();
        private readonly Mock<IConfiguration> _mockConfig = new();
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory = new();

        // Helper to create the service with mocked dependencies
        private UrlShortenerService CreateService(ICodeGenerator? codeGen = null)
        {
            return new UrlShortenerService(
                _mockRepo.Object,
                codeGen ?? Mock.Of<ICodeGenerator>(),
                _mockCache.Object,
                _mockEnv.Object,
                _mockConfig.Object,
                _mockScopeFactory.Object);
        }

        [Fact]
        public async Task ShortenUrlAsync_ReturnsExistingShortCode_WhenUrlAlreadyExists()
        {
            // Arrange
            string inputUrl = "https://test.com";
            string existingCode = "exist1";

            _mockRepo.Setup(r => r.GetByUrlAsync(inputUrl)).ReturnsAsync((UrlMapping?)new UrlMapping
            {
                ShortCode = existingCode,
                OriginalUrl = inputUrl
            });

            var service = CreateService();

            // Act
            var result = await service.ShortenUrlAsync(inputUrl);

            // Assert
            Assert.Equal(UrlResolutionStatus.Success, result.Status);
            Assert.Equal(existingCode, result.Code);
            Assert.Contains(existingCode, result.Url);
        }

        [Fact]
        public async Task ShortenUrlAsync_CreatesNewShortCode_WhenUrlDoesNotExist()
        {
            // Arrange
            string inputUrl = "https://newsite.com";
            string generatedCode = "abc123";

            var mockCodeGen = new Mock<ICodeGenerator>();
            mockCodeGen.Setup(g => g.GenerateCode()).Returns(generatedCode);

            _mockRepo.Setup(r => r.GetByUrlAsync(inputUrl)).ReturnsAsync((UrlMapping?)null);
            _mockRepo.Setup(r => r.GetByCodeAsync(generatedCode)).ReturnsAsync((UrlMapping?)null);
            _mockRepo.Setup(r => r.AddAsync(It.Is<UrlMapping>(m => m.OriginalUrl == inputUrl && m.ShortCode == generatedCode)))
                     .Returns(Task.CompletedTask);

            _mockCache.Setup(c =>
                c.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService(mockCodeGen.Object);

            // Act
            var result = await service.ShortenUrlAsync(inputUrl);

            // Assert
            Assert.Equal(UrlResolutionStatus.Success, result.Status);
            Assert.Equal(generatedCode, result.Code);
            Assert.Contains(generatedCode, result.Url);
        }

        [Fact]
        public async Task GetOriginalUrlAsync_ReturnsFromCache_WhenAvailable()
        {
            // Arrange
            string code = "abc123";
            string originalUrl = "https://cached.com";

            byte[] cachedBytes = Encoding.UTF8.GetBytes(originalUrl);

            _mockCache.Setup(c =>
                c.GetAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedBytes);

            var service = CreateService();

            // Act
            var result = await service.GetOriginalUrlAsync(code);

            // Assert
            Assert.Equal(UrlResolutionStatus.Success, result.Status);
            Assert.Equal(originalUrl, result.Url);
        }

        [Fact]
        public async Task GetOriginalUrlAsync_ReturnsFromDatabase_WhenCacheMisses()
        {
            // Arrange
            string code = "abc123";
            string originalUrl = "https://from-db.com";

            _mockCache.Setup(c =>
                c.GetAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null); // Cache miss

            _mockRepo.Setup(r => r.GetByCodeAsync(code)).ReturnsAsync((UrlMapping?)new UrlMapping
            {
                ShortCode = code,
                OriginalUrl = originalUrl,
                ClickCount = 0,
                CreatedAt = DateTime.UtcNow
            });

            _mockCache.Setup(c =>
                c.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            var result = await service.GetOriginalUrlAsync(code);

            // Assert
            Assert.Equal(UrlResolutionStatus.Success, result.Status);
            Assert.Equal(originalUrl, result.Url);
        }
    }
}
