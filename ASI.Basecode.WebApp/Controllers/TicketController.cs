using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;

namespace ASI.Basecode.WebApp.Controllers
{
    [Route("api/[controller]")]  
    [ApiController]
    [AllowAnonymous]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _tickets;

        public TicketController(ITicketService tickets) 
        { 
            _tickets = tickets; 
        }

        // CREATE 
        // POST /api/ticket
        [HttpPost]
        public IActionResult Create([FromForm] Ticket ticket, IFormFile? attachment)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = _tickets.Create(ticket, attachment);
            return CreatedAtAction(nameof(GetOne), new { id = created.Id }, created);
        }

        // READ 
        // GET /api/ticket (all)
        [HttpGet]
        public IActionResult GetAll() => Ok(_tickets.GetAll());

        // GET /api/ticket/{id}
        [HttpGet("{id:int}")]
        public IActionResult GetOne(int id)
        {
            var ticket = _tickets.Get(id);
            return ticket is null ? NotFound() : Ok(ticket);
        }

        // UPDATE 
        // PUT /api/ticket/{id} full / partial
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromForm] string? jsonData, IFormFile? attachment)
        {
            if (!_tickets.Update(id, jsonData, attachment)) return NotFound();
            return NoContent();
        }

        // DELETE ATTACHMENT ONLY 
        // DELETE /api/ticket/{id}/attachment
        [HttpDelete("{id:int}/attachment")]
        public IActionResult DeleteAttachmentOnly(int id)
        {
            if (!_tickets.DeleteAttachment(id)) return NotFound();
            return NoContent();
        }

        // DELETE TICKET
        // DELETE /api/ticket/{id}
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            if (!_tickets.Delete(id)) return NotFound();
            return NoContent();
        }

        // GET api/ticket/{id}/with-feedback
 
        [HttpGet("{id:int}/with-feedback")]
        public IActionResult GetTicketWithFeedback(int id)
        {
            var ticketWithFeedback = _ticketService.GetTicketWithFeedback(id);
            if (ticketWithFeedback == null)
                return NotFound();
            return Ok(ticketWithFeedback);
        }
        
        
         // GET /api/ticket/with-feedback/all
         
        [HttpGet("with-feedback/all")]
        public IActionResult GetAllTicketsWithFeedback()
        {
            try
            {
                var ticketsWithFeedback = _context.Tickets
                    .Where(t => t.Feedback.Any())
                    .Select(t => new
                    {
                        t.Id,
                        t.Summary,
                        t.UserID,
                        t.AgentID,
                        t.Status,
                        t.ResolvedAt,
                        t.DueDate,
                        t.Priority,
                        t.Category,
                        FeedbackCount = t.Feedback.Count()
                    })
                    .ToList();
        
                return Ok(ticketsWithFeedback);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
        }
    }
}
