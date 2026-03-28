using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TH3.Models;
using TH3.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<IdentityUser, IdentityRole>()
        .AddDefaultTokenProviders()
        .AddDefaultUI()
        .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Product}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Tạo roles nếu chưa có
    string[] roleNames = { "Admin", "Employee", "Customer" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Hàm helper để tạo user và gán role
    async Task EnsureUser(string email, string password, string role)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser == null)
        {
            var newUser = new IdentityUser { UserName = email, Email = email };
            var result = await userManager.CreateAsync(newUser, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newUser, role);
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(existingUser, role))
            {
                await userManager.AddToRoleAsync(existingUser, role);
            }
        }
    }

    // Tạo các user mẫu
    await EnsureUser("admin@gmail.com", "Admin@123", "Admin");
    await EnsureUser("employee@gmail.com", "Employee@123", "Employee");
    await EnsureUser("customer@gmail.com", "Customer@123", "Customer");

    // Seeding Categories
    if (!await dbContext.Categories.AnyAsync())
    {
        dbContext.Categories.AddRange(
            new Category { Name = "SUV" },
            new Category { Name = "Sedan" },
            new Category { Name = "Coupe" },
            new Category { Name = "Electric" }
        );
        await dbContext.SaveChangesAsync();
    }
}
app.Run();
