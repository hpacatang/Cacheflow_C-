using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using System.Text.Json;
using ASI.Basecode.Services.Interfaces;

namespace ASI.Basecode.WebApp.Controllers
{
    [Route("api/[controller]")]  
    [ApiController]
    [AllowAnonymous]
    public class TicketController : ControllerBase
    {
        private readonly AsiBasecodeDBContext _context;
        private readonly ITicketService _ticketService;
        private readonly IWebHostEnvironment _environment;
        private const string ATTACHMENT_FOLDER = "TicketAttachments";

        public TicketController(AsiBasecodeDBContext ctx, ITicketService ticketService, IWebHostEnvironment environment) 
        { 
            _context = ctx;
            _ticketService = ticketService;
            _environment = environment;
            EnsureAttachmentFolderExists();
        }

        private void EnsureAttachmentFolderExists()
        {
            var attachmentPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, ATTACHMENT_FOLDER);
            if (!Directory.Exists(attachmentPath))
            {
                Directory.CreateDirectory(attachmentPath);
            }
        }

        // GET /api/ticket
        [HttpGet]
        public IActionResult GetAll() 
        {
            return Ok(_context.Tickets.ToList());
        }

        // GET /api/ticket/{id}
        [HttpGet("{id:int}")]
        public IActionResult GetOne(int id)
        {
            var ticket = _context.Tickets.Find(id);
            return ticket is null ? NotFound() : Ok(ticket);
        }

        // POST /api/ticket
        [HttpPost]
        public IActionResult Create([FromForm] Ticket ticket, IFormFile? attachment)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (ticket.Id != 0)
            {
                if (_context.Tickets.Any(x => x.Id == ticket.Id))
                    return Conflict(new { success = false, message = "Ticket Id already exists" });
            }
            else
            {
                ticket.Id = (_context.Tickets.Max(x => (int?)x.Id) ?? 0) + 1;
            }

            ticket.Status ??= "open";
            ticket.ResolvedAt ??= null;
            ticket.CreatedTime = DateTime.UtcNow;
            ticket.UpdatedTime = DateTime.UtcNow;

            // Handle file attachment
            if (attachment != null && attachment.Length > 0)
            {
                ticket.AttachmentPath = SaveAttachment(attachment);
            }

            _context.Tickets.Add(ticket);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetOne), new { id = ticket.Id }, ticket);
        }

        // PUT /api/ticket/{id}
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromForm] string? jsonData, IFormFile? attachment)
        {
            var ticket = _context.Tickets.Find(id);
            if (ticket is null) return NotFound();

            // Parse JSON data if provided
            if (!string.IsNullOrEmpty(jsonData))
            {
                var body = JsonDocument.Parse(jsonData).RootElement;

                if (TryGetString(body, new[] { "summary" }, out var summary)) ticket.Summary = summary ?? ticket.Summary;
                if (TryGetInt(body, new[] { "userId", "userID" }, out var userId)) ticket.UserID = userId ?? ticket.UserID;
                if (TryGetInt(body, new[] { "agentId", "agentID" }, out var agentId)) ticket.AgentID = agentId ?? ticket.AgentID;
                if (TryGetString(body, new[] { "status" }, out var status)) ticket.Status = status ?? ticket.Status;
                if (TryGetString(body, new[] { "type" }, out var type)) ticket.Type = type ?? ticket.Type;
                if (TryGetString(body, new[] { "description" }, out var desc)) ticket.Description = desc ?? ticket.Description;
                if (TryGetString(body, new[] { "priority" }, out var priority)) ticket.Priority = priority ?? ticket.Priority;
                if (TryGetString(body, new[] { "category" }, out var category)) ticket.Category = category ?? ticket.Category;
                if (TryGetDate(body, new[] { "dueDate" }, out var due)) ticket.DueDate = due ?? ticket.DueDate;
                if (TryGetDate(body, new[] { "resolvedAt" }, out var resolved)) ticket.ResolvedAt = resolved;
            }

            // Handle new attachment
            if (attachment != null && attachment.Length > 0)
            {
                // Delete old attachment if exists
                if (!string.IsNullOrEmpty(ticket.AttachmentPath))
                {
                    DeleteAttachment(ticket.AttachmentPath);
                }
                ticket.AttachmentPath = SaveAttachment(attachment);
            }

            ticket.UpdatedTime = DateTime.UtcNow;
            _context.SaveChanges();
            
            return NoContent();
        }

        // DELETE /api/ticket/{id}
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var ticket = _context.Tickets.Find(id);
            if (ticket is null) return NotFound();

            // Delete attachment if exists
            if (!string.IsNullOrEmpty(ticket.AttachmentPath))
            {
                DeleteAttachment(ticket.AttachmentPath);
            }

            _context.Tickets.Remove(ticket);
            _context.SaveChanges();
            
            return NoContent();
        }

        // DELETE /api/ticket/{id}/attachment
        [HttpDelete("{id:int}/attachment")]
        public IActionResult DeleteAttachmentOnly(int id)
        {
            var ticket = _context.Tickets.Find(id);
            if (ticket is null) return NotFound();

            if (!string.IsNullOrEmpty(ticket.AttachmentPath))
            {
                DeleteAttachment(ticket.AttachmentPath);
                ticket.AttachmentPath = null;
                ticket.UpdatedTime = DateTime.UtcNow;
                _context.SaveChanges();
            }

            return NoContent();
        }

        private string SaveAttachment(IFormFile file)
        {
            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png", ".zip" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException($"Invalid file type. Allowed: {string.Join(", ", allowedExtensions)}");

            // Validate file size (10MB limit)
            if (file.Length > 10 * 1024 * 1024)
                throw new InvalidOperationException("File size must not exceed 10MB");

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, ATTACHMENT_FOLDER);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return $"/{ATTACHMENT_FOLDER}/{uniqueFileName}";
        }

        private void DeleteAttachment(string attachmentPath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, attachmentPath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch
            {
                // Ignore deletion errors
            }
        }

        private static bool TryGetString(JsonElement el, string[] keys, out string? value)
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

        private static bool TryGetDate(JsonElement el, string[] keys, out DateTime? value)
        {
            foreach (var k in keys)
            {
                if (el.TryGetProperty(k, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.String && DateTime.TryParse(prop.GetString(), out var dt))
                    {
                        value = dt;
                        return true;
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

        private static bool TryGetInt(JsonElement el, string[] keys, out int? value)
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
                    if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out var pi))
                    {
                        value = pi;
                        return true;
                    }
                }
            }
            value = null;
            return false;
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