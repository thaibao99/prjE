using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prjetax.Models;         // chứa EnterpriseDemo, SendEmailViewModel
using PrjEtax.Services;
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
        public async Task<IActionResult> SendEmail(int id)
        {
            var ent = await _context.Enterprises.FindAsync(id);
            if (ent == null) return NotFound();

            var vm = new SendEmailViewModel
            {
                ToEmail = ent.Email,
                Subject = $"Nhắc nhở doanh nghiệp {ent.TaxPayerName}",
                Body = $"Kính gửi {ent.TaxPayerName},\n\nĐây là email nhắc việc..."
            };
            ViewBag.EnterpriseId = id;
            return View(vm);
        }

        // POST: /Mail/SendEmail/5
        [HttpPost, ValidateAntiForgeryToken]
        
        public async Task<IActionResult> SendEmail(int id, SendEmailViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.EnterpriseId = id;
                return View(vm);
            }

            // Chuyển attachment thành byte[]
            byte[] fileBytes = null;
            string fileName = null;
            if (vm.Attachment != null && vm.Attachment.Length > 0)
            {
                fileName = vm.Attachment.FileName;
                using var ms = new MemoryStream();
                await vm.Attachment.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            // Gọi EmailService với mảng byte
            await _emailService.SendEmailAsync(
                vm.ToEmail,
                vm.Subject,
                vm.Body,
                fileBytes,        // đây là byte[]
                fileName          // và tên file
            );

            // Lưu vào lịch sử...
            var hist = new EnterpriseHistory
            {
                EnterpriseId = id,
                Date = DateTime.Now,
                Content = $"Gửi mail: “{vm.Subject}”",
                Reminder = vm.Deadline,
                Notes = vm.Notes,
                  Result       = "Đã gửi mail thành công", 
                Rating = vm.Rating,
                DocumentName = fileName,
                Document = fileBytes
            };
            _context.EnterpriseHistories.Add(hist);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Gửi mail và ghi lịch sử thành công!";
            return RedirectToAction("Index", "Enterprises");
        }

    }
}
