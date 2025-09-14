using Microsoft.EntityFrameworkCore;
using prjetax.Models;
using PrjEtax.Models;
using PrjEtax.Services;    // ← Thêm dòng này

var builder = WebApplication.CreateBuilder(args);

// 1) MVC + Session
builder.Services.AddControllersWithViews();
builder.Services.AddSession(opt =>
{
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
    opt.IdleTimeout = TimeSpan.FromMinutes(30);
});

// Background worker nhắc việc
builder.Services.AddHostedService<ReminderWorker>();

// 2) EF Core + SQL Server
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3) SMTP & EmailService
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<EmailService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Bật Session trước khi vào MVC
app.UseSession();

// Không dùng [Authorize] thì KHÔNG cần UseAuthentication.
// Có thể để UseAuthorization, nhưng không bắt buộc.
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();