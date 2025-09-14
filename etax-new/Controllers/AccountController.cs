using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using prjetax.Models;
using System.Threading.Tasks;

namespace prjetax.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        public AccountController(AppDbContext context) => _context = context;

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost, ValidateAntiForgeryToken]
         
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Managers
                .FirstOrDefaultAsync(m => m.Username == username && m.PasswordHash == password);

            if (user != null)
            {
                HttpContext.Session.SetInt32("ManagerId", user.Id);
                HttpContext.Session.SetInt32("IsAdmin", user.IsAdmin ? 1 : 0);
                HttpContext.Session.SetString("ManagerName", user.Name ?? user.Username);   // <-- thêm
                HttpContext.Session.SetString("UnitCode", "TXU-KV1");                       // <-- nếu có đơn vị, tạm set
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu";
            return View();
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        
    }
}
