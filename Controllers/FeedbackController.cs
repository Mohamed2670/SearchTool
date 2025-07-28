using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchTool_ServerSide.Models;

namespace SearchTool_ServerSide.Controllers
{
    [ApiController]
    [Route("feedback"), AllowAnonymous]
    public class FeedbackController(FeedbackService feedbackService) : ControllerBase
    {

        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] FeedbackFormResponse feedback)
        {
            if (feedback == null || feedback.Sections == null)
                return BadRequest("Invalid structure");

            await feedbackService.SaveFeedbackAsync(feedback);
            return Ok(new { message = "Feedback submitted to DB" });
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllFeedbacks()
        {
            var feedbackList = await feedbackService.GetAllFeedbackAsync();
            return Ok(feedbackList);
        }
    }

}