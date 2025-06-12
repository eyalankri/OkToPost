using Microsoft.AspNetCore.Mvc;
using OkToPost.DTOs;
using OkToPost.Services;
using OkToPost.Utils;
using static UrlShortenerService;

namespace OkToPost.Controllers
{
    [ApiController]
    public class RedirectController : ControllerBase
    {
        private readonly IUrlShortenerService _service;

        public RedirectController(IUrlShortenerService service)
        {
            _service = service;
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectToOriginal(string code)
        {
            if (!ShortCodeValidator.IsValid(code))
            {
                return BadRequest(new ApiErrorResponse
                {
                    Status = UrlResolutionStatus.InvalidRequest.ToString(),
                    Message = "Short code must be exactly 6 characters long."
                });
            }

            var result = await _service.GetOriginalUrlAsync(code);

            return result.Status switch
            {
                UrlResolutionStatus.Success => RedirectPermanent(result.Url!),
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
