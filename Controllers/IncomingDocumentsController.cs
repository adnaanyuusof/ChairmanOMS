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
    public class IncomingDocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public IncomingDocumentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        public async Task<IActionResult> Index(string? status, string? priority, string? search)
        {
            var query = _context.IncomingDocuments
                .Include(d => d.AssignedToUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(d => d.Status == status);
            if (!string.IsNullOrEmpty(priority))
                query = query.Where(d => d.Priority == priority);
            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower();
                query = query.Where(d => d.Subject.ToLower().Contains(s) || 
                                         d.SourceInstitution.ToLower().Contains(s) || 
                                         d.ReferenceNumber.ToLower().Contains(s) || 
                                         (d.Purpose != null && d.Purpose.ToLower().Contains(s)));
            }

            ViewBag.Statuses = new SelectList(new[] { "Received", "UnderReview", "Approved", "Rejected", "Archived" });
            ViewBag.Priorities = new SelectList(new[] { "Low", "Medium", "High" });
            ViewBag.Search = search;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedPriority = priority;

            return View(await query.OrderByDescending(d => d.CreatedAt).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var doc = await _context.IncomingDocuments
                .Include(d => d.AssignedToUser)
                .Include(d => d.WorkflowActions).ThenInclude(w => w.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doc == null) return NotFound();

            var users = await _userManager.Users.ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "Email", doc.AssignedToUserId);
            return View(doc);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Secretary,Records Officer")]
        public async Task<IActionResult> Create()
        {
            var users = await _userManager.Users.ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "Email");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Secretary,Records Officer")]
        public async Task<IActionResult> Create(IncomingDocument model, IFormFile? attachment)
        {
            ModelState.Remove("AssignedToUser");
            ModelState.Remove("WorkflowActions");
            if (ModelState.IsValid)
            {
                if (attachment != null && attachment.Length > 0)
                {
                    var uploads = Path.Combine(_env.WebRootPath, "uploads", "incoming");
                    Directory.CreateDirectory(uploads);
                    var fileName = Guid.NewGuid() + Path.GetExtension(attachment.FileName);
                    var filePath = Path.Combine(uploads, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await attachment.CopyToAsync(stream);
                    model.AttachmentPath = "/uploads/incoming/" + fileName;
                }

                await _context.IncomingDocuments.AddAsync(model);
                await _context.SaveChangesAsync();

                await LogActivity("Create", $"Incoming document '{model.ReferenceNumber}' created.");
                return RedirectToAction(nameof(Index));
            }

            var users = await _userManager.Users.ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "Email");
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Secretary,Records Officer")]
        public async Task<IActionResult> Edit(int id)
        {
            var doc = await _context.IncomingDocuments.FindAsync(id);
            if (doc == null) return NotFound();

            var users = await _userManager.Users.ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "Email", doc.AssignedToUserId);
            ViewBag.Statuses = new SelectList(new[] { "Received", "UnderReview", "Approved", "Rejected", "Archived" }, doc.Status);
            ViewBag.Priorities = new SelectList(new[] { "Low", "Medium", "High" }, doc.Priority);
            return View(doc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Secretary,Records Officer")]
        public async Task<IActionResult> Edit(int id, IncomingDocument model, IFormFile? attachment)
        {
            if (id != model.Id) return NotFound();
            ModelState.Remove("AssignedToUser");
            ModelState.Remove("WorkflowActions");

            if (ModelState.IsValid)
            {
                var existing = await _context.IncomingDocuments.FindAsync(id);
                if (existing == null) return NotFound();

                if (attachment != null && attachment.Length > 0)
                {
                    var uploads = Path.Combine(_env.WebRootPath, "uploads", "incoming");
                    Directory.CreateDirectory(uploads);
                    var fileName = Guid.NewGuid() + Path.GetExtension(attachment.FileName);
                    using var stream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create);
                    await attachment.CopyToAsync(stream);
                    existing.AttachmentPath = "/uploads/incoming/" + fileName;
                }

                existing.ReferenceNumber = model.ReferenceNumber;
                existing.SourceInstitution = model.SourceInstitution;
                existing.Subject = model.Subject;
                existing.DateReceived = model.DateReceived;
                existing.Priority = model.Priority;
                existing.AssignedToUserId = model.AssignedToUserId;
                existing.Status = model.Status;
                existing.Purpose = model.Purpose;
                existing.ReceiverName = model.ReceiverName;
                existing.UnderProcessBy = model.UnderProcessBy;
                existing.HandedTo = model.HandedTo;
                existing.Other = model.Other;
                existing.DepartureDate = model.DepartureDate;
                existing.DateOfReturn = model.DateOfReturn;

                _context.Update(existing);
                await _context.SaveChangesAsync();
                await LogActivity("Update", $"Incoming document '{model.ReferenceNumber}' updated.");
                return RedirectToAction(nameof(Index));
            }

            var users = await _userManager.Users.ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "Email");
            ViewBag.Statuses = new SelectList(new[] { "Received", "UnderReview", "Approved", "Rejected", "Archived" });
            ViewBag.Priorities = new SelectList(new[] { "Low", "Medium", "High" });
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var doc = await _context.IncomingDocuments
                .Include(d => d.WorkflowActions)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (doc == null) return NotFound();

            // Remove related workflow actions first (FK constraint)
            _context.WorkflowActions.RemoveRange(doc.WorkflowActions);

            // Delete physical attachment if present
            if (!string.IsNullOrEmpty(doc.AttachmentPath))
            {
                var filePath = Path.Combine(_env.WebRootPath, doc.AttachmentPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.IncomingDocuments.Remove(doc);
            await _context.SaveChangesAsync();
            await LogActivity("Delete", $"Incoming document '{doc.ReferenceNumber}' deleted.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWorkflowAction(int documentId, string actionTaken, string? notes)
        {
            var user = await _userManager.GetUserAsync(User);
            var action = new WorkflowAction
            {
                IncomingDocumentId = documentId,
                DocumentType = "Incoming",
                UserId = user!.Id,
                ActionTaken = actionTaken,
                Notes = notes
            };
            _context.WorkflowActions.Add(action);

            var doc = await _context.IncomingDocuments.FindAsync(documentId);
            if (doc != null)
            {
                doc.Status = actionTaken == "Approved" ? "Approved"
                           : actionTaken == "Rejected" ? "Rejected"
                           : "UnderReview";
                _context.Update(doc);
            }

            await _context.SaveChangesAsync();
            await LogActivity("Workflow", $"Action '{actionTaken}' on incoming doc #{documentId}.");
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
