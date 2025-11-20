using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using System.Text.Json;

namespace ASI.Basecode.WebApp.Controllers
{
    [Route("api/[controller]")]  
    [ApiController]
    [AllowAnonymous]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly AsiBasecodeDBContext _context;

        public TicketController(ITicketService ticketService, AsiBasecodeDBContext ctx)
        {
            _ticketService = ticketService;
            _context = ctx;
        }

        /*
         * GET https://localhost:56201/api/ticket
         * Returns all tickets as JSON array
         */
        [HttpGet]
        public IActionResult GetAll() 
        {
            try 
            {
                return Ok(_context.Tickets.ToList());
            } 
            catch (System.Exception ex) 
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
        }

        /*
         * GET https://localhost:56201/api/ticket/{id}
         * Returns single ticket by id
         * Example: GET https://localhost:56201/api/ticket/5
         */
        [HttpGet("{id:int}")]
        public IActionResult GetOne(int id)
        {
            var t = _context.Tickets.Find(id);
            return t is null ? NotFound() : Ok(t);
        }

        
        /*
         * POST https://localhost:56201/api/ticket
         * Creates a new ticket
         * 
         * JSON Body (required fields):
         * {
         *   "summary": "Issue title",
         *   "name": "creator_username",
         *   "assignee": "assignee_username",
         *   "type": "hardware",
         *   "description": "Detailed description",
         *   "dueDate": "2025-10-31T17:00:00Z",
         *   "priority": "high",
         *   "category": "Hardware"
         * }
         * 
         * Optional fields:
         * - id (int) : for testing only, server assigns if omitted
         * - status (string) : defaults to "open"
         * - resolvedAt (datetime or null)
         */
        [HttpPost]
        public IActionResult Create([FromBody] Ticket ticket)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // If client supplied an id for testing, accept it if not already used
            if (ticket.Id != 0)
            {
                if (_context.Tickets.Any(x => x.Id == ticket.Id))
                {
                    return Conflict(new { success = false, message = "Ticket Id already exists" });
                }
            }
            else
            {
                ticket.Id = (_context.Tickets.Max(x => (int?)x.Id) ?? 0) + 1;
            }

            ticket.Status ??= "open";
            ticket.ResolvedAt ??= null;

            _context.Tickets.Add(ticket);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetOne), new { id = ticket.Id }, ticket);
        }

        /*
         * PUT https://localhost:56201/api/ticket/{id}
         * Updates ticket with any fields present in JSON body (partial or full update)
         * 
         * Example - update single field:
         * PUT https://localhost:56201/api/ticket/5
         * { "status": "resolved" }
         * 
         * Example - update multiple fields:
         * {
         *   "status": "inProgress",
         *   "assignee": "tech_support",
         *   "priority": "high"
         * }
         */
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] JsonElement body)
        {
            var t = _context.Tickets.Find(id);
            if (t is null) return NotFound();

            static bool TryGetString(JsonElement el, string[] keys, out string? value)
            {
                foreach (var k in keys)
                {
                    if (el.TryGetProperty(k, out var prop) && prop.ValueKind != JsonValueKind.Null)
                    {
                        value = prop.GetString();
                        return true;
                    }
                }
                value = null;
                return false;
            }

            static bool TryGetDate(JsonElement el, string[] keys, out DateTime? value)
            {
                foreach (var k in keys)
                {
                    if (el.TryGetProperty(k, out var prop) && prop.ValueKind == JsonValueKind.String)
                    {
                        var s = prop.GetString();
                        if (!string.IsNullOrEmpty(s) && DateTime.TryParse(s, out var dt))
                        {
                            value = dt;
                            return true;
                        }
                        if (string.IsNullOrEmpty(s))
                        {
                            value = null;
                            return true;
                        }
                    }
                    else if (el.TryGetProperty(k, out prop) && prop.ValueKind == JsonValueKind.Null)
                    {
                        value = null;
                        return true;
                    }
                }
                value = null;
                return false;
            }

            // Update fields if present in body
            if (TryGetString(body, new[] { "summary", "Summary" }, out var summary)) t.Summary = summary ?? t.Summary;
            if (TryGetString(body, new[] { "name", "Name" }, out var name)) t.Name = name ?? t.Name;
            if (TryGetString(body, new[] { "assignee", "Assignee" }, out var assignee)) t.Assignee = assignee ?? t.Assignee;
            if (TryGetString(body, new[] { "status", "Status" }, out var status)) t.Status = status ?? t.Status;
            if (TryGetString(body, new[] { "type", "Type" }, out var type)) t.Type = type ?? t.Type;
            if (TryGetString(body, new[] { "description", "Description" }, out var desc)) t.Description = desc ?? t.Description;
            if (TryGetString(body, new[] { "priority", "Priority" }, out var priority)) t.Priority = priority ?? t.Priority;
            if (TryGetString(body, new[] { "category", "Category" }, out var category)) t.Category = category ?? t.Category;

            if (TryGetDate(body, new[] { "dueDate", "DueDate" }, out var due)) t.DueDate = due ?? t.DueDate;
            if (TryGetDate(body, new[] { "resolvedAt", "ResolvedAt" }, out var resolved)) t.ResolvedAt = resolved;

            _context.SaveChanges();
            return NoContent();
        }

        /*
         * PATCH https://localhost:56201/api/ticket/{id}
         * Partial update - only updates the fields you send
         * 
         * Example:
         * PATCH https://localhost:56201/api/ticket/5
         * { "priority": "low" }
         */
        [HttpPatch("{id:int}")]
        public IActionResult Patch(int id, [FromBody] JsonElement body)
        {
            var t = _context.Tickets.Find(id);
            if (t is null) return NotFound();

            if (body.TryGetProperty("summary", out var s) && s.ValueKind != JsonValueKind.Null) t.Summary = s.GetString();
            if (body.TryGetProperty("description", out var d) && d.ValueKind != JsonValueKind.Null) t.Description = d.GetString();
            if (body.TryGetProperty("priority", out var p) && p.ValueKind != JsonValueKind.Null) t.Priority = p.GetString();
            if (body.TryGetProperty("category", out var c) && c.ValueKind != JsonValueKind.Null) t.Category = c.GetString();

            if (body.TryGetProperty("dueDate", out var dd))
            {
                if (dd.ValueKind == JsonValueKind.String && DateTime.TryParse(dd.GetString(), out var dt)) t.DueDate = dt;
                else if (dd.ValueKind == JsonValueKind.Null) t.DueDate = default;
            }
            if (body.TryGetProperty("resolvedAt", out var ra))
            {
                if (ra.ValueKind == JsonValueKind.String && DateTime.TryParse(ra.GetString(), out var rt)) t.ResolvedAt = rt;
                else if (ra.ValueKind == JsonValueKind.Null) t.ResolvedAt = null;
            }

            _context.SaveChanges();
            return NoContent();
        }

        /*
         * DELETE https://localhost:56201/api/ticket/{id}
         * Deletes a ticket
         * Example: DELETE https://localhost:56201/api/ticket/5
         */
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var t = _context.Tickets.Find(id);
            if (t is null) return NotFound();
            _context.Tickets.Remove(t);
            _context.SaveChanges();
            return NoContent();
        }

        /*
         * GET https://localhost:56201/api/ticket/{id}/with-feedback
         * Returns single ticket with its feedback by id
         * Example: GET https://localhost:56201/api/ticket/5/with-feedback
         */
        [HttpGet("{id:int}/with-feedback")]
        public IActionResult GetTicketWithFeedback(int id)
        {
            var ticketWithFeedback = _ticketService.GetTicketWithFeedback(id);
            if (ticketWithFeedback == null)
                return NotFound();
            return Ok(ticketWithFeedback);
        }

        /*
         * GET https://localhost:51811/api/ticket/with-feedback/all
         * Returns only tickets that have feedback
         */
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
                        t.Name,
                        t.Assignee,
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