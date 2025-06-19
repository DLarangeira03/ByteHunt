using byte_hunt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace byte_hunt.Controllers;

[Authorize(Roles = "Moderator,Administrator")]
public class AdminController : Controller {
    
    private readonly UserManager<Utilizador> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(UserManager<Utilizador> userManager, RoleManager<IdentityRole> roleManager) {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    
    //===========================================\\
    //============ PAINEL PRINCIPAL =============\\
    //===========================================\\
    /// <summary>
    /// Exibe o painel principal de administração.
    /// </summary>
    /// <returns>View do dashboard de administração.</returns>
    public IActionResult Dashboard()
    {
        return View();
    }

    //===========================================\\
    //======== PAGINA DE GESTAO DE ROLES ========\\
    //===========================================\\
    /// <summary>
    /// Lista todos os utilizadores e as suas respetivas roles.
    /// Apenas acessível para administradores.
    /// </summary>
    /// <returns>View com a lista de utilizadores e suas roles.</returns>
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ManageRoles() {
        //obter todos os utilizadores
        var users = _userManager.Users.ToList();
        //criar o modelo para a view
        var model = new List<UserRolesViewModel>();
        
        // ordem de demonstração das roles
        var predefinedOrder = new List<string> {  "User", "Moderator", "Administrator" };
        
        //obter as roles de cada utilizador e adicionar ao modelo
        foreach (var user in users) {
            //obter as roles do utilizador
            var roles = await _userManager.GetRolesAsync(user);
            // Adiconar ao modelo, ordenando as roles de acordo com a ordem predefinida
            model.Add(new UserRolesViewModel {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = roles.OrderBy(r => predefinedOrder.IndexOf(r)).ToList()
            });
        }
        //mandar todas as roles para a dropdown
        ViewBag.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
        
        // Retornar a view com o modelo
        return View(model);
    }
    
    /// <summary>
    /// Adiciona uma role a um utilizador.
    /// Apenas acessível para administradores.
    /// </summary>
    /// <param name="userId">ID do utilizador.</param>
    /// <param name="role">Nome da role a adicionar.</param>
    /// <returns>Redireciona para a página de gestão de roles.</returns>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AddRole(string userId, string role) {
        // Procura o utilizador pelo ID
        var user = await _userManager.FindByIdAsync(userId);
        // Verifica se o utilizador existe 
        if (user != null) {
            // Adiciona a role ao utilizador se a role existir
            await _userManager.AddToRoleAsync(user, role);
        }
        // Redireciona para a página de gestão de roles 
        return RedirectToAction("ManageRoles");
    }
    
    /// <summary>
    /// Remove uma role de um utilizador.
    /// Apenas acessível por administradores.
    /// </summary>
    /// <param name="userId">ID do utilizador.</param>
    /// <param name="role">Nome da role a remover.</param>
    /// <returns>Redireciona para a página de gestão de roles.</returns>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> RemoveRole(string userId, string role) {
        // Procura o utilizador pelo ID 
        var user = await _userManager.FindByIdAsync(userId);
        // Verifica se o utilizador existe e se a role existe
        if (user != null && await _roleManager.RoleExistsAsync(role)) {
            // Remove a role do utilizador
            await _userManager.RemoveFromRoleAsync(user, role);
        }
        // Define uma mensagem de confirmação na TempData
        TempData["RoleRemovalConfirmation"] = $"Permissão \"{role}\" removida com sucesso do utilizador \"{user.UserName}\"";
        // Redireciona para a página de gestão de roles
        return RedirectToAction("ManageRoles");
    }
    
    //===========================================\\
    //============== PLACEHOLDER  ===============\\
    //===========================================\\
    
}

/// <summary>
/// ViewModel que representa um utilizador e as suas roles para a gestão de permissões no painel de administração.
/// </summary>
public class UserRolesViewModel {
    public string UserId { get; set; }
    public string UserName { get; set; }
    public List<string> Roles { get; set; }
}