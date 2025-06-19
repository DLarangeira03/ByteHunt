using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using byte_hunt.Models;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<Utilizador> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<Utilizador> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }
    
    // POST: api/Auth/login
    /// <summary>
    /// Permite realizar o login para receber o token que permite autenticar na API.
    /// Todos os utilizadores autenticados e não autenticados podem aceder a este endpoint.
    /// </summary>
    /// param name="dto">Dados de login contendo o email e a password do utilizador.</param>
    /// <returns>Token JWT</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login dto)
    {
        //procura o utilizador pelo email
        var user = await _userManager.FindByEmailAsync(dto.Email);
        //verifica se o utilizador existe e se a password está correta
        if (user != null && await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            // Se o utilizador existir e a password estiver correta, gera o token JWT
            
            // Obtém os roles do utilizador
            var roles = await _userManager.GetRolesAsync(user);
            
            // Cria uma lista de claims com o nome do utilizador e o ID único do token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            
            // Adiciona os roles como claims
            foreach (var role in roles)
            {
                // Adiciona cada role como uma claim
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            // Configura o segredo e as credenciais para assinar o token
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            // Cria o token JWT com o emissor, o público, a data de expiração, as claims e as credenciais de assinatura
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                expires: DateTime.UtcNow.AddHours(2),
                claims: claims,
                signingCredentials: creds);
            
            // Retorna o token JWT como uma string
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
        
        // Se o utilizador não existir ou a password estiver incorreta, retorna um erro de autenticação
        return Unauthorized("Credenciais inválidas");
    }
    
}

/// <summary>
/// Modelo de dados para o login.
/// </summary>
public class Login
{
    public string Email { get; set; }
    public string Password { get; set; }
}
