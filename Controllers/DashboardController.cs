using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChairmanOMS.Data;
using ChairmanOMS.Models;

namespace ChairmanOMS.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var totalIncoming = await _context.IncomingDocuments.CountAsync();
            var totalOutgoing = await _context.OutgoingDocuments.CountAsync();
            var pendingApprovals = await _context.IncomingDocuments.CountAsync(d => d.Status == "Received" || d.Status == "UnderReview");
            var highPriority = await _context.IncomingDocuments.CountAsync(d => d.Priority == "High" && d.Status != "Archived");
            var pendingTasks = await _context.TaskItems.CountAsync(t => !t.IsCompleted);

            var recentIncoming = await _context.IncomingDocuments
                .Include(d => d.AssignedToUser)
                .OrderByDescending(d => d.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentOutgoing = await _context.OutgoingDocuments
                .OrderByDescending(d => d.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentLogs = await _context.ActivityLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .Take(10)
                .ToListAsync();

            // Stats by status for chart
            var incomingByStatus = await _context.IncomingDocuments
                .GroupBy(d => d.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.TotalIncoming = totalIncoming;
            ViewBag.TotalOutgoing = totalOutgoing;
            ViewBag.PendingApprovals = pendingApprovals;
            ViewBag.HighPriority = highPriority;
            ViewBag.PendingTasks = pendingTasks;
            ViewBag.RecentIncoming = recentIncoming;
            ViewBag.RecentOutgoing = recentOutgoing;
            ViewBag.RecentLogs = recentLogs;
            ViewBag.IncomingByStatus = incomingByStatus;

            return View();
        }
    }
}
