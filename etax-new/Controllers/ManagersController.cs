using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using prjetax.Models;
using System.Threading.Tasks;

namespace prjetax.Controllers
{
    public class ManagersController : Controller
    {
        private readonly AppDbContext _context;
        public ManagersController(AppDbContext context) => _context = context;

        // Chỉ Admin được truy cập
        private bool IsAdmin => HttpContext.Session.GetInt32("IsAdmin") == 1;

        // GET: /Managers
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin) return StatusCode(403);
            return View(await _context.Managers.ToListAsync());
        }

        // GET: /Managers/Create
        public IActionResult Create()
        {
            if (!IsAdmin) return StatusCode(403);
            return View();
        }

        // POST: /Managers/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Manager manager)
        {
            try
            {
                if (!IsAdmin) return StatusCode(403);
                if (!ModelState.IsValid) return View(manager);

                _context.Managers.Add(manager);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                return BadRequest(new
                {
                    message = "Lỗi khi tạo nhân viên mới",
                    ex.Message
                });
            }
        }

        // GET: /Managers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin) return StatusCode(403);
            var manager = await _context.Managers.FindAsync(id);
            if (manager == null) return NotFound();
            return View(manager);
        }

        // POST: /Managers/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Manager model)
        {
            if (!IsAdmin) return StatusCode(403);
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var m = await _context.Managers.FindAsync(id);
            if (m == null) return NotFound();

            m.Username = model.Username;
            m.PasswordHash = model.PasswordHash;
            m.Name = model.Name;
            m.IsAdmin = model.IsAdmin;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Managers/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin) return StatusCode(403);
            var manager = await _context.Managers.FindAsync(id);
            if (manager == null) return NotFound();
            return View(manager);
        }

        // POST: /Managers/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin) return StatusCode(403);
            var manager = await _context.Managers.FindAsync(id);
            _context.Managers.Remove(manager);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
