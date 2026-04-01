using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChairmanOMS.Data;
using ChairmanOMS.Models;

namespace ChairmanOMS.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public AppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // GET: Appointments/Index
        public async Task<IActionResult> Index(string? status, string? priority, string? search, string? date)
        {
            var query = _context.Appointments
                .Include(a => a.CreatedBy)
                .AsQueryable();

            // Staff cannot see appointments
            if (!User.IsInRole("Admin") && !User.IsInRole("Chairman") && !User.IsInRole("Secretary") && !User.IsInRole("Records Officer"))
                return Forbid();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);
            if (!string.IsNullOrEmpty(priority))
                query = query.Where(a => a.Priority == priority);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(a => a.VisitorName.Contains(search) || a.Organization.Contains(search) || a.Purpose.Contains(search));
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsedDate))
                query = query.Where(a => a.AppointmentDate.Date == parsedDate.Date);

            ViewBag.Search = search;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedPriority = priority;
            ViewBag.SelectedDate = date;

            // Today's summary stats
            var today = DateTime.Today;
            ViewBag.TodayCount = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date == today);
            ViewBag.PendingCount = await _context.Appointments.CountAsync(a => a.Status == "Pending");
            ViewBag.UpcomingCount = await _context.Appointments.CountAsync(a => a.AppointmentDate.Date > today && a.Status == "Approved");

            return View(await query.OrderByDescending(a => a.AppointmentDate).ThenBy(a => a.StartTime).ToListAsync());
        }

        // GET: Appointments/Calendar
        public async Task<IActionResult> Calendar()
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Chairman") && !User.IsInRole("Secretary") && !User.IsInRole("Records Officer"))
                return Forbid();

            var appointments = await _context.Appointments
                .Where(a => a.Status != "Cancelled" && a.Status != "Rejected")
                .ToListAsync();

            var events = appointments.Select(a => new
            {
                id = a.Id,
                title = $"{a.VisitorName} - {a.Purpose}",
                start = a.AppointmentDate.ToString("yyyy-MM-dd") + "T" + a.StartTime.ToString(@"hh\:mm"),
                end = a.AppointmentDate.ToString("yyyy-MM-dd") + "T" + a.EndTime.ToString(@"hh\:mm"),
                color = a.Status == "Approved" ? "#22c55e"
                      : a.Status == "Pending" ? "#f59e0b"
                      : a.Status == "Completed" ? "#3b82f6"
                      : "#e11d48",
                extendedProps = new { status = a.Status, priority = a.Priority, organization = a.Organization }
            });

            ViewBag.EventsJson = System.Text.Json.JsonSerializer.Serialize(events);
            return View();
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!User.IsInRole("Admin") && !User.IsInRole("Chairman") && !User.IsInRole("Secretary") && !User.IsInRole("Records Officer"))
                return Forbid();

            var appointment = await _context.Appointments
                .Include(a => a.CreatedBy)
                .Include(a => a.Logs).ThenInclude(l => l.ChangedBy)
                .Include(a => a.MeetingNotes).ThenInclude(n => n.CreatedBy)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();
            return View(appointment);
        }

        // GET: Appointments/Create
        [Authorize(Roles = "Admin,Secretary")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> Create(Appointment model, IFormFile? attachment)
        {
            ModelState.Remove("CreatedBy");
            ModelState.Remove("Logs");
            ModelState.Remove("MeetingNotes");

            if (ModelState.IsValid)
            {
                // Time conflict check
                var conflict = await _context.Appointments.AnyAsync(a =>
                    a.AppointmentDate.Date == model.AppointmentDate.Date &&
                    a.Status != "Cancelled" && a.Status != "Rejected" &&
                    a.StartTime < model.EndTime && a.EndTime > model.StartTime);

                if (conflict)
                {
                    ModelState.AddModelError("", "⚠️ A time conflict exists with another appointment on that day. Please choose a different time slot.");
                    return View(model);
                }

                if (attachment != null && attachment.Length > 0)
                {
                    var uploads = Path.Combine(_env.WebRootPath, "uploads", "appointments");
                    Directory.CreateDirectory(uploads);
                    var fileName = Guid.NewGuid() + Path.GetExtension(attachment.FileName);
                    using var stream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create);
                    await attachment.CopyToAsync(stream);
                    model.AttachmentPath = "/uploads/appointments/" + fileName;
                }

                var user = await _userManager.GetUserAsync(User);
                model.CreatedById = user?.Id;

                await _context.Appointments.AddAsync(model);
                await _context.SaveChangesAsync();

                // Log creation
                _context.AppointmentLogs.Add(new AppointmentLog
                {
                    AppointmentId = model.Id,
                    Status = "Pending",
                    ChangedById = user?.Id,
                    Notes = "Appointment created."
                });
                await _context.SaveChangesAsync();

                await LogActivity("Create", $"Appointment for '{model.VisitorName}' created.");
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Appointments/Edit/5
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> Edit(int id, Appointment model, IFormFile? attachment)
        {
            if (id != model.Id) return NotFound();
            ModelState.Remove("CreatedBy");
            ModelState.Remove("Logs");
            ModelState.Remove("MeetingNotes");

            if (ModelState.IsValid)
            {
                var existing = await _context.Appointments.FindAsync(id);
                if (existing == null) return NotFound();

                if (attachment != null && attachment.Length > 0)
                {
                    var uploads = Path.Combine(_env.WebRootPath, "uploads", "appointments");
                    Directory.CreateDirectory(uploads);
                    var fileName = Guid.NewGuid() + Path.GetExtension(attachment.FileName);
                    using var stream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create);
                    await attachment.CopyToAsync(stream);
                    existing.AttachmentPath = "/uploads/appointments/" + fileName;
                }

                existing.VisitorName = model.VisitorName;
                existing.Organization = model.Organization;
                existing.Phone = model.Phone;
                existing.Email = model.Email;
                existing.Purpose = model.Purpose;
                existing.AppointmentDate = model.AppointmentDate;
                existing.StartTime = model.StartTime;
                existing.EndTime = model.EndTime;
                existing.Masuulka = model.Masuulka;
                existing.Priority = model.Priority;

                _context.Update(existing);
                await _context.SaveChangesAsync();
                await LogActivity("Update", $"Appointment for '{model.VisitorName}' updated.");
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // POST: Appointments/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Chairman")]
        public async Task<IActionResult> UpdateStatus(int appointmentId, string newStatus, string? notes)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return NotFound();

            appointment.Status = newStatus;
            _context.Update(appointment);

            var user = await _userManager.GetUserAsync(User);
            _context.AppointmentLogs.Add(new AppointmentLog
            {
                AppointmentId = appointmentId,
                Status = newStatus,
                ChangedById = user?.Id,
                Notes = notes
            });

            await _context.SaveChangesAsync();
            await LogActivity("Workflow", $"Appointment #{appointmentId} status changed to '{newStatus}'.");
            return RedirectToAction(nameof(Details), new { id = appointmentId });
        }

        // POST: Appointments/CheckIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> CheckIn(int appointmentId, string visitorStatus)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return NotFound();

            appointment.VisitorStatus = visitorStatus;
            if (visitorStatus == "Arrived")
                appointment.CheckInTime = DateTime.UtcNow;

            _context.Update(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = appointmentId });
        }

        // POST: Appointments/CheckOut
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Secretary")]
        public async Task<IActionResult> CheckOut(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return NotFound();

            appointment.CheckOutTime = DateTime.UtcNow;
            appointment.Status = "Completed";
            _context.Update(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = appointmentId });
        }

        // POST: Appointments/AddNote
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Chairman,Secretary")]
        public async Task<IActionResult> AddNote(int appointmentId, string notes, string? decision)
        {
            var user = await _userManager.GetUserAsync(User);
            _context.MeetingNotes.Add(new MeetingNote
            {
                AppointmentId = appointmentId,
                Notes = notes,
                Decision = decision,
                CreatedById = user?.Id
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = appointmentId });
        }

        // POST: Appointments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Logs)
                .Include(a => a.MeetingNotes)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null) return NotFound();

            _context.AppointmentLogs.RemoveRange(appointment.Logs);
            _context.MeetingNotes.RemoveRange(appointment.MeetingNotes);

            if (!string.IsNullOrEmpty(appointment.AttachmentPath))
            {
                var filePath = Path.Combine(_env.WebRootPath, appointment.AttachmentPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            await LogActivity("Delete", $"Appointment for '{appointment.VisitorName}' deleted.");
            return RedirectToAction(nameof(Index));
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
