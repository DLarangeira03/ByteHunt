using byte_hunt.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using byte_hunt.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);


// metodo para definir roles
// isso garante que as roles existam na base de dados
async Task SeedRoles(IServiceProvider serviceProvider) {
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roleNames = {"User", "Moderator", "Administrator"};
    foreach (var roleName in roleNames) {
        if(!await roleManager.RoleExistsAsync(roleName)) {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}


CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-PT");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-PT");

// Add services to the container.
builder.Services.AddControllersWithViews();

// DB context
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

builder.Services.AddDefaultIdentity<Utilizador>(options => {
    options.SignIn.RequireConfirmedAccount = true; // email deve ser confirmado para login
    options.Password.RequireUppercase = false; 
    options.Password.RequireNonAlphanumeric = false; 
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages(); // Identity UI

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider;
    await SeedRoles(services);
}

// run the app
app.Run();