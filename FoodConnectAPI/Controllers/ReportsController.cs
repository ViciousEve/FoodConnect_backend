using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodConnectAPI.Interfaces.Services;
using FoodConnectAPI.Models;
using System.Security.Claims;

namespace FoodConnectAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportDto reportDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _reportService.CreateReportAsync(userId, reportDto);
                return CreatedAtAction(nameof(GetReportsByPostId), new { postId = result.PostId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllReports()
        {
            try
            {
                var reports = await _reportService.GetAllReportsAsync();
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }

        [HttpGet("post/{postId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetReportsByPostId(int postId)
        {
            try
            {
                var reports = await _reportService.GetReportsByPostIdAsync(postId);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }

        [HttpDelete("{reportId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteReport(int reportId)
        {
            try
            {
                var deleted = await _reportService.DeleteReportAsync(reportId);
                if (!deleted)
                {
                    return NotFound(new { error = "Report not found." });
                }
                return Ok(new { message = "Report deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred. Error: " + ex.Message });
            }
        }
    }
}
