using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/Image")]
    public class ImageController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ImageController()
        {
            _httpClient = new HttpClient();
        }

        [HttpPost("predict")]
        public async Task<IActionResult> Predict([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No image file uploaded.");

            try
            {
                // Save the uploaded file temporarily
                var filePath = Path.Combine(Path.GetTempPath(), file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Forward image to ML model
                var mlApiUrl = "http://128.199.250.106:5001/predict";
                using (var fileStream = new FileStream(filePath, FileMode.Open))
                {
                    var content = new MultipartFormDataContent();
                    var streamContent = new StreamContent(fileStream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpg");
                    content.Add(streamContent, "file", file.FileName);

                    var response = await _httpClient.PostAsync(mlApiUrl, content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        return StatusCode((int)response.StatusCode, errorMessage);
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    return Ok(result);  // Return ML model response to mobile app
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error communicating with ML model: {ex.Message}");
            }
        }
    }
}