using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChairmanOMS.Data;
using ChairmanOMS.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ChairmanOMS.Controllers
{
    [Authorize]
    public class ApprovedDocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApprovedDocumentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ApprovedDocuments
        public async Task<IActionResult> Index(string searchString, string docType, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.ApprovedDocuments
                .Include(a => a.ApprovedBy)
                .Include(a => a.CreatedBy)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.ReferenceNumber.Contains(searchString) || s.Title.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(docType))
            {
                query = query.Where(s => s.DocumentType == docType);
            }

            if (startDate.HasValue)
            {
                query = query.Where(s => s.ApprovedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(s => s.ApprovedDate <= endDate.Value);
            }

            return View(await query.OrderByDescending(a => a.ApprovedDate).ToListAsync());
        }

        // GET: ApprovedDocuments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var approvedDocument = await _context.ApprovedDocuments
                .Include(a => a.ApprovedBy)
                .Include(a => a.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (approvedDocument == null)
            {
                return NotFound();
            }

            return View(approvedDocument);
        }

        // GET: ApprovedDocuments/Create
        [Authorize(Roles = "Admin,Chairman")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ApprovedDocuments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Chairman")]
        public async Task<IActionResult> Create([Bind("Id,ReferenceNumber,Title,DocumentType,SourceOrDestination,Description,ApprovedDate,ApprovedByUserId,RelatedDocumentId,FilePath,Status,Remarks")] ApprovedDocument approvedDocument)
        {
            if (ModelState.IsValid)
            {
                approvedDocument.CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
                approvedDocument.CreatedAt = DateTime.UtcNow;
                _context.Add(approvedDocument);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(approvedDocument);
        }

        // GET: ApprovedDocuments/Edit/5
        [Authorize(Roles = "Admin,Chairman")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var approvedDocument = await _context.ApprovedDocuments.FindAsync(id);
            if (approvedDocument == null)
            {
                return NotFound();
            }
            return View(approvedDocument);
        }

        // POST: ApprovedDocuments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Chairman")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ReferenceNumber,Title,DocumentType,SourceOrDestination,Description,ApprovedDate,ApprovedByUserId,RelatedDocumentId,FilePath,Status,Remarks,CreatedAt,CreatedById")] ApprovedDocument approvedDocument)
        {
            if (id != approvedDocument.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(approvedDocument);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApprovedDocumentExists(approvedDocument.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(approvedDocument);
        }

        // GET: ApprovedDocuments/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var approvedDocument = await _context.ApprovedDocuments
                .Include(a => a.ApprovedBy)
                .Include(a => a.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (approvedDocument == null)
            {
                return NotFound();
            }

            return View(approvedDocument);
        }

        // POST: ApprovedDocuments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var approvedDocument = await _context.ApprovedDocuments.FindAsync(id);
            if (approvedDocument != null)
            {
                _context.ApprovedDocuments.Remove(approvedDocument);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApprovedDocumentExists(int id)
        {
            return _context.ApprovedDocuments.Any(e => e.Id == id);
        }
    }
}
