using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using byte_hunt.Models;

namespace byte_hunt.Controllers;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger) {
        _logger = logger;
    }
    
    /// <summary>
    /// Exibe a página inicial.
    /// </summary>
    /// <returns>View da página inicial.</returns>
    public IActionResult Index() {
        return View();
    }
    
    /// <summary>
    /// Exibe a página de privacidade.
    /// </summary>
    /// <returns>View da política de privacidade.</returns>
    public IActionResult Privacy() {
        return View();
    }
    
    /// <summary>
    /// Exibe as informações dos desenvolvedores.
    /// </summary>
    /// <returns>View da página pessoal.</returns>
    public IActionResult PaginaPessoal() {
        return View();
    }
    
    /// <summary>
    /// Exibe a página de erro.
    /// </summary>
    /// <returns>View com detalhes do erro.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}