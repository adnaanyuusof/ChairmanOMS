using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChairmanOMS.Data;
using ChairmanOMS.Models;

namespace ChairmanOMS.Controllers
{
    [Authorize]
    public class OutgoingDocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public OutgoingDocumentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        public async Task<IActionResult> Index(string? status, string? search)
        {
            var query = _context.OutgoingDocuments
                .Include(d => d.LinkedIncomingDocument)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(d => d.Status == status);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(d => d.Subject.Contains(search) || d.DestinationInstitution.Contains(search) || d.ReferenceNumber.Contains(search));

            ViewBag.Statuses = new SelectList(new[] { "Draft", "Approved", "Sent", "Delivered" });
            ViewBag.Search = search;
            ViewBag.SelectedStatus = status;

            return View(await query.OrderByDescending(d => d.CreatedAt).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var doc = await _context.OutgoingDocuments
                .Include(d => d.LinkedIncomingDocument)
                .Include(d => d.WorkflowActions).ThenInclude(w => w.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null) return NotFound();
            return View(doc);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Chairman,Secretary")]
        public async Task<IActionResult> Create()
        {
            ViewBag.IncomingDocs = new SelectList(
                await _context.IncomingDocuments.ToListAsync(), "Id", "Subject");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Chairman,Secretary")]
        public async Task<IActionResult> Create(OutgoingDocument model, IFormFile? attachment)
        {
            ModelState.Remove("LinkedIncomingDocument");
            ModelState.Remove("WorkflowActions");
            if (ModelState.IsValid)
            {
                if (attachment != null && attachment.Length > 0)
                {
                    var uploads = Path.Combine(_env.WebRootPath, "uploads", "outgoing");
                    Directory.CreateDirectory(uploads);
                    var fileName = Guid.NewGuid() + Path.GetExtension(attachment.FileName);
                    using var stream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create);
                    await attachment.CopyToAsync(stream);
                    model.AttachmentPath = "/uploads/outgoing/" + fileName;
                }

                await _context.OutgoingDocuments.AddAsync(model);
                await _context.SaveChangesAsync();
                await LogActivity("Create", $"Outgoing document '{model.ReferenceNumber}' created.");
                return RedirectToAction(nameof(Index));
            }

            ViewBag.IncomingDocs = new SelectList(await _context.IncomingDocuments.ToListAsync(), "Id", "Subject");
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Chairman,Secretary")]
        public async Task<IActionResult> Edit(int id)
        {
            var doc = await _context.OutgoingDocuments.FindAsync(id);
            if (doc == null) return NotFound();

            ViewBag.Statuses = new SelectList(new[] { "Draft", "Approved", "Sent", "Delivered" }, doc.Status);
            ViewBag.IncomingDocs = new SelectList(await _context.IncomingDocuments.ToListAsync(), "Id", "Subject", doc.LinkedIncomingDocumentId);
            return View(doc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Chairman,Secretary")]
        public async Task<IActionResult> Edit(int id, OutgoingDocument model, IFormFile? attachment)
        {
            if (id != model.Id) return NotFound();
            ModelState.Remove("LinkedIncomingDocument");
            ModelState.Remove("WorkflowActions");

            if (ModelState.IsValid)
            {
                var existing = await _context.OutgoingDocuments.FindAsync(id);
                if (existing == null) return NotFound();

                if (attachment != null && attachment.Length > 0)
                {
                    var uploads = Path.Combine(_env.WebRootPath, "uploads", "outgoing");
                    Directory.CreateDirectory(uploads);
                    var fileName = Guid.NewGuid() + Path.GetExtension(attachment.FileName);
                    using var stream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create);
                    await attachment.CopyToAsync(stream);
                    existing.AttachmentPath = "/uploads/outgoing/" + fileName;
                }

                existing.ReferenceNumber = model.ReferenceNumber;
                existing.DestinationInstitution = model.DestinationInstitution;
                existing.Subject = model.Subject;
                existing.DateSent = model.DateSent;
                existing.Status = model.Status;
                existing.LinkedIncomingDocumentId = model.LinkedIncomingDocumentId;

                _context.Update(existing);
                await _context.SaveChangesAsync();
                await LogActivity("Update", $"Outgoing document '{model.ReferenceNumber}' updated.");
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Statuses = new SelectList(new[] { "Draft", "Approved", "Sent", "Delivered" });
            ViewBag.IncomingDocs = new SelectList(await _context.IncomingDocuments.ToListAsync(), "Id", "Subject");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWorkflowAction(int documentId, string actionTaken, string? notes)
        {
            var user = await _userManager.GetUserAsync(User);
            _context.WorkflowActions.Add(new WorkflowAction
            {
                OutgoingDocumentId = documentId,
                DocumentType = "Outgoing",
                UserId = user!.Id,
                ActionTaken = actionTaken,
                Notes = notes
            });

            var doc = await _context.OutgoingDocuments.FindAsync(documentId);
            if (doc != null)
            {
                doc.Status = actionTaken == "Approved" ? "Approved"
                           : actionTaken == "Sent" ? "Sent"
                           : actionTaken == "Delivered" ? "Delivered"
                           : doc.Status;
                _context.Update(doc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = documentId });
        }

        private async Task LogActivity(string actionType, string description)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserId = user.Id,
                    ActionType = actionType,
                    Description = description,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });
                await _context.SaveChangesAsync();
            }
        }
    }
}
