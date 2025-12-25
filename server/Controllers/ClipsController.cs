using Microsoft.AspNetCore.Mvc;
using Server.DTOs;
using Server.Models;
using Server.Services;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClipsController : ControllerBase
    {
        private readonly IClipService _clipService;
        private readonly ILogger<ClipsController> _logger;

        public ClipsController(
            IClipService clipService,
            ILogger<ClipsController> logger)
        {
            _clipService = clipService;
            _logger = logger;
        }

        // GET: api/clips
        [HttpGet]
        public async Task<ActionResult<ClipSearchResponse>> GetClips(
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? searchOperandAnd = null,
            [FromQuery] string? tags = null,
            [FromQuery] string? channelIds = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int? sortOption = null,
            [FromQuery] int limit = 100,
            [FromQuery] int skip = 0)
        {
            try
            {
                var request = new ClipSearchRequest
                {
                    SearchTerm = searchTerm,
                    SearchOperandAnd = searchOperandAnd ?? false,
                    Tags = tags?.Split(',', StringSplitOptions.RemoveEmptyEntries),
                    ChannelIds = channelIds?.Split(',', StringSplitOptions.RemoveEmptyEntries),
                    FromDate = fromDate,
                    ToDate = toDate,
                    SortOption = sortOption,
                    Limit = limit,
                    Skip = skip
                };

                var response = await _clipService.SearchClipsAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching clips");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult<ClipSearchResponse>> SearchClips([FromBody] ClipSearchRequest request)
        {
            var response = await _clipService.SearchClipsAsync(request);
            return Ok(response);
        }

        // GET: api/clips/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Clip>> GetClipById(string id)
        {
            var clip = await _clipService.GetByIdAsync(id);
            if (clip == null)
                return NotFound(new { message = "Clip not found" });

            return Ok(clip);
        }

        // POST: api/clips
        [HttpPost]
        public async Task<ActionResult<Clip>> CreateClip([FromBody] CreateClipsPayloadDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var clip = await _clipService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetClipById), new { id = clip.Id }, clip);
        }

        // PUT: api/clips/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<Clip>> UpdateClip(string id, [FromBody] UpdateClipDto dto)
        {
            var updated = await _clipService.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound(new { message = "Clip not found" });

            return Ok(updated);
        }

        // DELETE: api/clips/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClip(string id)
        {
            var success = await _clipService.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "Clip not found" });

            return NoContent();
        }

        // POST: api/clips/batch-delete
        [HttpPost("batch-delete")]
        public async Task<IActionResult> DeleteMultipleClips([FromBody] List<string> clipIds)
        {
            var count = await _clipService.DeleteMultipleAsync(clipIds);
            return Ok(new { deletedCount = count });
        }

        [HttpPut("{id}/addinsights")]
        public async Task<IActionResult> AddInsightsAsync(string id, [FromBody] List<InsightRequest> newInsights)
        {
            var dbClip = await _clipService.GetByIdAsync(id);

            if (dbClip == null)
                return NotFound(new { message = "Clip not found" });

            var updatedClip = await _clipService.AddInsightsToClipAsync(dbClip, newInsights);

            return Ok(updatedClip);
        }

        [HttpPut("{id}/removeinsights")]
        public async Task<IActionResult> RemoveInsightsAsync(string id, [FromBody] List<string> insightIdsToDelete)
        {
            var dbClip = await _clipService.GetByIdAsync(id);

            if (dbClip == null)
                return NotFound(new { message = "Clip not found" });

            var updatedClip = await _clipService.RemoveInsightsAsync(dbClip, insightIdsToDelete);

            return Ok(updatedClip);
        }
    }
}