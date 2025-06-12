using Microsoft.AspNetCore.Mvc;
using OkToPost.DTOs;
using OkToPost.Services;
using OkToPost.Utils;

namespace OkToPost.Controllers
{
    [ApiController]
    [Route("api/")]
    public class ShortenUrlController : ControllerBase
    {
        private readonly IUrlShortenerService _service;

        public ShortenUrlController(IUrlShortenerService service)
        {
            _service = service;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> Shorten([FromBody] ShortenUrlRequest request)
        {
            if (!Uri.IsWellFormedUriString(request.Url, UriKind.Absolute))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Status = UrlResolutionStatus.InvalidRequest.ToString(),
                    Message = "Invalid URL"
                });
            }

            UrlResolutionResult result = await _service.ShortenUrlAsync(request.Url);

            return result.Status switch
            {
                UrlResolutionStatus.Success => Ok(result),

                UrlResolutionStatus.DatabaseError => StatusCode(503, new ApiErrorResponse
                {
                    Status = result.Status.ToString(),
                    Message = "Database unavailable."
                }),
                UrlResolutionStatus.UnexpectedError => StatusCode(500, new ApiErrorResponse
                {
                    Status = result.Status.ToString(),
                    Message = "Unexpected error."
                }),
                _ => StatusCode(500, new ApiErrorResponse
                {
                    Status = "Unknown",
                    Message = "Unknown error."
                })
            };

        }





        [HttpGet("/api/stats/{code}")]
        public async Task<IActionResult> Stats(string code)
        {
            if (!ShortCodeValidator.IsValid(code))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Status = UrlResolutionStatus.InvalidRequest.ToString(),
                    Message = "Short code must be exactly 6 characters long."
                });
            }

            UrlStatsResult result = await _service.GetStatsAsync(code);

            return result.Status switch
            {
                UrlResolutionStatus.Success => Ok(result),
                UrlResolutionStatus.NotFound => NotFound(new ApiErrorResponse
                {
                    Status = result.Status.ToString(),
                    Message = "Short code not found."
                }),
                UrlResolutionStatus.DatabaseError => StatusCode(503, new ApiErrorResponse
                {
                    Status = result.Status.ToString(),
                    Message = "Database unavailable."
                }),
                UrlResolutionStatus.UnexpectedError => StatusCode(500, new ApiErrorResponse
                {
                    Status = result.Status.ToString(),
                    Message = "Unexpected error."
                }),
                _ => StatusCode(500, new ApiErrorResponse
                {
                    Status = "Unknown",
                    Message = "Unknown error."
                })
            };
        }
    }
}
