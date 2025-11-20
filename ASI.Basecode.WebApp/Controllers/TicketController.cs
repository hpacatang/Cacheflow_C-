using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
        private readonly AsiBasecodeDBContext _context;
        public TicketController(AsiBasecodeDBContext ctx) => _context = ctx;

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
         *   "userID": 1,
         *   "agentID": 2,
         *   "type": "hardware",
         *   "description": "Detailed description",
         *   "dueDate": "2025-10-31T17:00:00Z",
         *   "priority": "high",
         *   "category": "Hardware"
         * }
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

            // Set timestamps
            ticket.CreatedTime = DateTime.UtcNow;
            ticket.UpdatedTime = DateTime.UtcNow;

            _context.Tickets.Add(ticket);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetOne), new { id = ticket.Id }, ticket);
        }

        /*
         * PUT https://localhost:56201/api/ticket/{id}
         * Updates ticket with any fields present in JSON body (partial or full update)
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

            static bool TryGetInt(JsonElement el, string[] keys, out int? value)
            {
                foreach (var k in keys)
                {
                    if (el.TryGetProperty(k, out var prop))
                    {
                        if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var i))
                        {
                            value = i;
                            return true;
                        }
                        if (prop.ValueKind == JsonValueKind.String)
                        {
                            var s = prop.GetString();
                            if (!string.IsNullOrEmpty(s) && int.TryParse(s, out var pi))
                            {
                                value = pi;
                                return true;
                            }
                        }
                        if (prop.ValueKind == JsonValueKind.Null)
                        {
                            value = null;
                            return true;
                        }
                    }
                }
                value = null;
                return false;
            }

            // Update fields if present in body
            if (TryGetString(body, new[] { "summary", "Summary" }, out var summary)) t.Summary = summary ?? t.Summary;
            if (TryGetInt(body, new[] { "userId", "UserID", "userID", "UserId" }, out var userId)) t.UserID = userId ?? t.UserID;
            if (TryGetInt(body, new[] { "agentId", "AgentID", "agentID", "AgentId" }, out var agentId)) t.AgentID = agentId ?? t.AgentID;
            if (TryGetString(body, new[] { "status", "Status" }, out var status)) t.Status = status ?? t.Status;
            if (TryGetString(body, new[] { "type", "Type" }, out var type)) t.Type = type ?? t.Type;
            if (TryGetString(body, new[] { "description", "Description" }, out var desc)) t.Description = desc ?? t.Description;
            if (TryGetString(body, new[] { "priority", "Priority" }, out var priority)) t.Priority = priority ?? t.Priority;
            if (TryGetString(body, new[] { "category", "Category" }, out var category)) t.Category = category ?? t.Category;

            if (TryGetDate(body, new[] { "dueDate", "DueDate" }, out var due)) t.DueDate = due ?? t.DueDate;
            if (TryGetDate(body, new[] { "resolvedAt", "ResolvedAt" }, out var resolved)) t.ResolvedAt = resolved;

            // update timestamp
            t.UpdatedTime = DateTime.UtcNow;

            _context.SaveChanges();
            return NoContent();
        }

        /*
         * PATCH https://localhost:56201/api/ticket/{id}
         * Partial update - only updates the fields you send
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

            if (body.TryGetProperty("userId", out var uid))
            {
                if (uid.ValueKind == JsonValueKind.Number && uid.TryGetInt32(out var i)) t.UserID = i;
                else if (uid.ValueKind == JsonValueKind.String && int.TryParse(uid.GetString(), out var pi)) t.UserID = pi;
            }

            if (body.TryGetProperty("agentId", out var aid))
            {
                if (aid.ValueKind == JsonValueKind.Number && aid.TryGetInt32(out var i)) t.AgentID = i;
                else if (aid.ValueKind == JsonValueKind.String && int.TryParse(aid.GetString(), out var pi)) t.AgentID = pi;
            }

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

            // update timestamp
            t.UpdatedTime = DateTime.UtcNow;

            _context.SaveChanges();
            return NoContent();
        }

        /*
         * DELETE https://localhost:56201/api/ticket/{id}
         * Deletes a ticket
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
    }
}