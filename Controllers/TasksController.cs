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
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            IQueryable<TaskItem> query = _context.TaskItems
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedBy);

            if (!User.IsInRole("Admin") && !User.IsInRole("Chairman"))
            {
                query = query.Where(t => t.AssignedToUserId == user!.Id || t.CreatedById == user!.Id);
            }

            return View(await query.OrderByDescending(t => t.CreatedAt).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var users = await _userManager.Users.ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "Email");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem model)
        {
            ModelState.Remove("CreatedBy");
            ModelState.Remove("AssignedToUser");
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                model.CreatedById = user!.Id;
                await _context.TaskItems.AddAsync(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var users = await _userManager.Users.ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "Email");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleComplete(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task != null)
            {
                task.IsCompleted = !task.IsCompleted;
                _context.Update(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
