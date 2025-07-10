using Microsoft.AspNetCore.Mvc;
using prjetax.Models;
using prjetax.Models;   // for SendEmailViewModel
using PrjEtax.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace prjetax.Controllers
{
    public class MailController : Controller
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public MailController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: /Mail/SendEmail/{id?}
        public async Task<IActionResult> SendEmail(int? id)
        {
            var vm = new SendEmailViewModel();
            if (id.HasValue)
            {
                var ent = await _context.Enterprises.FindAsync(id.Value);
                if (ent != null)
                {
                    vm.ToList = new List<string> { ent.Email };
                    vm.Subject = $"Nhắc nhở doanh nghiệp {ent.TaxPayerName}";
                    vm.Body = $"Kính gửi {ent.TaxPayerName},\n\nĐây là email nhắc việc...";
                }
            }
            else
            {
                // gửi hàng loạt
                vm.ToList = await _context.Enterprises
                                   .Select(e => e.Email)
                                   .Where(email => !string.IsNullOrEmpty(email))
                                   .Distinct()
                                   .ToListAsync();
                vm.Subject = "Nhắc nhở chung";
                vm.Body = "Kính gửi quý doanh nghiệp,...";
            }

            return View(vm);
        }

        // POST: /Mail/SendEmail
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmail(SendEmailViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            foreach (var to in vm.ToList.Distinct())
            {
                await _emailService.SendEmailAsync(to, vm.Subject, vm.Body);
            }

            TempData["Success"] = "Gửi mail thành công!";
            return RedirectToAction("Index", "Enterprises");
        }
    }
}
