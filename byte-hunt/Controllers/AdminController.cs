using byte_hunt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace byte_hunt.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller {
    
    private readonly UserManager<Utilizador> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(UserManager<Utilizador> userManager, RoleManager<IdentityRole> roleManager) {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // listar todos os utilizadores e suas roles
    public async Task<IActionResult> ManageRoles() {
        var users = _userManager.Users.ToList();
        var model = new List<UserRolesViewModel>();

        foreach (var user in users) {
            var roles = await _userManager.GetRolesAsync(user);
            model.Add(new UserRolesViewModel {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = roles.ToList()
            });
        }
        //mandar todas as roles para a dropdown
        ViewBag.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
        return View(model);
    }
    
    //adicionar uma role nova
    [HttpPost]
    public async Task<IActionResult> AddRole(string userId, string role) {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null) {
            await _userManager.AddToRoleAsync(user, role);
        }
        return RedirectToAction("ManageRoles");
    }
    
    //remover uma role
    [HttpPost]
    public async Task<IActionResult> RemoveRole(string userId, string role) {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null && await _roleManager.RoleExistsAsync(role)) {
            await _userManager.RemoveFromRoleAsync(user, role);
        }
        return RedirectToAction("ManageRoles");
    }
}

public class UserRolesViewModel {
    public string UserId { get; set; }
    public string UserName { get; set; }
    public List<string> Roles { get; set; }
}