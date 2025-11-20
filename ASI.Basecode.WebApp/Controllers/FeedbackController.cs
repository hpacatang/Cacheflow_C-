using System.Linq;
using System;
using Microsoft.AspNetCore.Authorization;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        // GET: api/feedback
        [HttpGet]
        public IActionResult GetAll()
        {
            var feedbacks = _feedbackService.GetAllFeedbacks();
            return Ok(feedbacks);
        }

        // GET: api/feedback/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var feedback = _feedbackService.GetFeedbackById(id);
            if (feedback == null)
                return NotFound();
            return Ok(feedback);
        }

        // POST: api/feedback
        [HttpPost]
        public IActionResult SubmitFeedback([FromBody] FeedbackViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _feedbackService.SubmitFeedback(model);
            return Ok(new { message = "Feedback submitted successfully!" });
        }
    }
}