using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchTool_ServerSide.Authentication;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Controllers
{
    [ApiController]
    [Route("feedback"), Authorize(Policy = "Pharmacist")]
    public class FeedbackController(FeedbackService feedbackService, UserAccessToken userAccessToken) : ControllerBase
    {

        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] FeedbackFormResponse feedback)
        {
            var userData = userAccessToken.tokenData();
            if (userData == null || string.IsNullOrEmpty(userData.UserId))
            {
                return Unauthorized("Invalid or missing token data");
            }

            if (!int.TryParse(userData.UserId, out int userId))
            {
                return BadRequest("Invalid user ID format");
            }
            if (feedback == null || feedback.Sections == null)
                return BadRequest("Invalid structure");
            feedback.UserEmail = userData.Email; // Set the email from token data
            await feedbackService.SaveFeedbackAsync(feedback);
            return Ok(new { message = "Feedback submitted to DB" });
        }
        [HttpGet("all"),Authorize(Policy = "SuperAdmin")]
        public async Task<IActionResult> GetAllFeedbacks()
        {
            var feedbackList = await feedbackService.GetAllFeedbackAsync();
            return Ok(feedbackList);
        }
        [HttpGet("export"),AllowAnonymous]
        public async Task<IActionResult> ExportCsv()
        {
            var csvBytes = await feedbackService.ExportFeedbackAsCsvAsync();
            return File(csvBytes, "text/csv", $"MedSearchFeedback_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }

    }

}