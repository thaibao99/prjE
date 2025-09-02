using PrjEtax.Models;
using PrjEtax.Services;    // ← Thêm dòng này
using Microsoft.EntityFrameworkCore;
using prjetax.Models;

var builder = WebApplication.CreateBuilder(args);

// 1) MVC + Session
builder.Services.AddControllersWithViews();
builder.Services.AddSession(opt => {
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
    opt.IdleTimeout = TimeSpan.FromMinutes(30);
});

// 2) EF Core + SQL Server
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3) SMTP & EmailService
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<EmailService>();  // bây giờ VS sẽ tìm thấy loại EmailService

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Bật Session
app.UseSession();
app.UseAuthorization();

// Route mặc định: vào Login của AccountController
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
