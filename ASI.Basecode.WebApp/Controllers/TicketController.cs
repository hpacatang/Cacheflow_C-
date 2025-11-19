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
    public class TicketController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;
        public TicketController(AsiBasecodeDBContext ctx) => _context = ctx;

        // GET https://localhost:56201/api/ticket
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

        // GET https://localhost:56201/api/ticket/{id}
        [HttpGet("{id:int}")]
        public IActionResult GetOne(int id)
        {
            var t = _context.Tickets.Find(id);
            return t is null ? NotFound() : Ok(t);
        }

        // POST https://localhost:56201/api/ticket
        // Fields: summary, name, assignee, type, description, dueDate, priority, category, status (optional), resolvedAt (optional)
        [HttpPost]
        public IActionResult Create([FromBody] Ticket ticket)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

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
            
            if (ticket.DueDate == null || ticket.DueDate == default || ticket.DueDate == DateTime.MinValue)
            {
                ticket.DueDate = new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);
            }

            _context.Tickets.Add(ticket);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetOne), new { id = ticket.Id }, ticket);
        }

        // PUT https://localhost:56201/api/ticket/{id}
        // Fields: summary, name, assignee, status, type, description, priority, category, dueDate, resolvedAt
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

        // DELETE https://localhost:56201/api/ticket/{id}
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