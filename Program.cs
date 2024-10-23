using FriendsCompatibilityGame.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FriendsCompatibilityGame
{
    public class Program
    {
        public static async Task Main(string[] args) // تغيير هنا إلى async Task
        {
            var builder = WebApplication.CreateBuilder(args);

            // إعداد قاعدة البيانات
            builder.Services.AddDbContext<FriendsGameContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // إعداد الهوية
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false; // لا تحتاج إلى تأكيد الحساب
                options.Password.RequiredLength = 6; // الحد الأدنى لطول كلمة المرور
                options.Password.RequireDigit = true; // يتطلب وجود رقم في كلمة المرور
                options.Password.RequireLowercase = true; // يتطلب وجود حرف صغير
                options.Password.RequireUppercase = true; // يتطلب وجود حرف كبير
                options.Password.RequireNonAlphanumeric = true; // يتطلب وجود حرف غير أبجدي
            })
            .AddRoles<IdentityRole>() // إضافة الأدوار (Roles)
            .AddEntityFrameworkStores<FriendsGameContext>();

            // إضافة خدمات MVC وRazor Pages
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // إنشاء مستخدم إداري عند بدء التطبيق
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                await CreateAdminUser(userManager, roleManager); // تأكد من استخدام await هنا
            }

            // إعداد Middleware الخاص بالمصادقة والتفويض
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles(); // للتأكد من تحميل الملفات الثابتة (CSS, JS)

            app.MapRazorPages();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        // دالة لإنشاء المستخدم الإداري
        private static async Task CreateAdminUser(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // التحقق من وجود دور "Admin"، إذا لم يكن موجودًا يتم إنشاؤه
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // التحقق من وجود المستخدم الإداري، إذا لم يكن موجودًا يتم إنشاؤه
            var adminEmail = "admin@example.com";  // البريد الإلكتروني للمدير
            var adminPassword = "Admin@123";       // كلمة المرور للمدير

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdmin = new IdentityUser { UserName = adminEmail, Email = adminEmail };
                var createAdminResult = await userManager.CreateAsync(newAdmin, adminPassword);

                // التأكد من نجاح عملية إنشاء المستخدم
                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");  // إضافة دور "Admin" للمستخدم
                }
            }
        }
    }
}
