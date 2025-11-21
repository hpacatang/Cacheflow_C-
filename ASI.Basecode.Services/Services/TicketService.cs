using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ASI.Basecode.Data;
using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;

namespace ASI.Basecode.Services.Services
{
    public class TicketService : ITicketService
    {
        private readonly AsiBasecodeDBContext _context;
        private readonly IWebHostEnvironment _environment;
        private const string ATTACHMENT_FOLDER = "TicketAttachments";

        public TicketService(AsiBasecodeDBContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
            EnsureAttachmentFolderExists();
        }

        public Ticket Create(Ticket ticket, IFormFile? attachment)
        {
            ticket.Status ??= "open";
            ticket.ResolvedAt ??= null;
            ticket.CreatedTime = DateTime.UtcNow;
            ticket.UpdatedTime = DateTime.UtcNow;

            if (attachment != null && attachment.Length > 0)
                ticket.AttachmentPath = SaveAttachment(attachment);

            _context.Tickets.Add(ticket);
            _context.SaveChanges();
            return ticket;
        }

        public IEnumerable<Ticket> GetAll() => _context.Tickets.ToList();

        public Ticket? Get(int id) => _context.Tickets.Find(id);

        public bool Update(int id, string? jsonData, IFormFile? attachment)
        {
            var ticket = _context.Tickets.Find(id);
            if (ticket == null) return false;

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

            if (attachment != null && attachment.Length > 0)
            {
                if (!string.IsNullOrEmpty(ticket.AttachmentPath))
                    DeleteAttachment(ticket.AttachmentPath);
                ticket.AttachmentPath = SaveAttachment(attachment);
            }

            ticket.UpdatedTime = DateTime.UtcNow;
            _context.SaveChanges();
            return true;
        }

        public bool Delete(int id)
        {
            var ticket = _context.Tickets.Find(id);
            if (ticket == null) return false;

            if (!string.IsNullOrEmpty(ticket.AttachmentPath))
                DeleteAttachment(ticket.AttachmentPath);

            _context.Tickets.Remove(ticket);
            _context.SaveChanges();
            return true;
        }

        public bool DeleteAttachment(int id)
        {
            var ticket = _context.Tickets.Find(id);
            if (ticket == null) return false;

            if (!string.IsNullOrEmpty(ticket.AttachmentPath))
            {
                DeleteAttachment(ticket.AttachmentPath);
                ticket.AttachmentPath = null;
                ticket.UpdatedTime = DateTime.UtcNow;
                _context.SaveChanges();
            }
            return true;
        }

        private void EnsureAttachmentFolderExists()
        {
            var attachmentPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, ATTACHMENT_FOLDER);
            if (!Directory.Exists(attachmentPath))
                Directory.CreateDirectory(attachmentPath);
        }

        private string SaveAttachment(IFormFile file)
        {
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png", ".zip" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException($"Invalid file type. Allowed: {string.Join(", ", allowedExtensions)}");
            if (file.Length > 10 * 1024 * 1024)
                throw new InvalidOperationException("File size must not exceed 10MB");

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, ATTACHMENT_FOLDER);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
                file.CopyTo(fileStream);
            return $"/{ATTACHMENT_FOLDER}/{uniqueFileName}";
        }

        private void DeleteAttachment(string attachmentPath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, attachmentPath.TrimStart('/'));
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
        }
            catch { }
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
    }
}