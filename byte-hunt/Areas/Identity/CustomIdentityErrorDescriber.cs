using Microsoft.AspNetCore.Identity;

namespace byte_hunt.Areas.Identity;

/// <summary>
/// Fornece descrições personalizadas (em português) para erros de identidade do ASP.NET Identity.
/// </summary>
public class CustomIdentityErrorDescriber: IdentityErrorDescriber
{
    /// <summary>
    /// Mensagem de erro quando a palavra-passe não contém uma letra maiúscula.
    /// </summary>
    public override IdentityError PasswordRequiresUpper()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresUpper),
            Description = "A Palavra-passe deve conter pelo menos uma letra maiúscula (A-Z)."
        };
    }
    
    /// <summary>
    /// Mensagem de erro quando a palavra-passe não contém uma letra minúscula.
    /// </summary>
    public override IdentityError PasswordRequiresLower()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresLower),
            Description = "A Palavra-passe deve conter pelo menos uma letra minuscula (a-z)."
        };
    }
    
    /// <summary>
    /// Mensagem de erro quando a palavra-passe não contém um carácter não alfanumérico.
    /// </summary>
    public override IdentityError PasswordRequiresNonAlphanumeric()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresNonAlphanumeric),
            Description = "A Palavra-passe deve conter pelo menos um carácter não alfanumérico (por exemplo, !, @, #, $, etc.)."
        };
    }
    
    /// <summary>
    /// Mensagem de erro quando a palavra-passe não contém um dígito.
    /// </summary>
    public override IdentityError PasswordRequiresDigit()
    {
        return new IdentityError
        {
            Code = nameof(PasswordRequiresDigit),
            Description = "A Palavra-passe deve conter pelo menos um dígito (0-9)."
        };
    }
    
    /// <summary>
    /// Mensagem de erro quando a palavra-passe é demasiado curta.
    /// </summary>
    /// <param name="length">Comprimento mínimo exigido.</param>
    public override IdentityError PasswordTooShort(int length)
    {
        return new IdentityError
        {
            Code = nameof(PasswordTooShort),
            Description = "A Palavra-passe deve ter pelo menos " + length + " caracteres."
        };
    }
    
    /// <summary>
    /// Mensagem de erro quando o nome de utilizador já está em uso.
    /// </summary>
    /// <param name="userName">Nome de utilizador duplicado.</param>
    public override IdentityError DuplicateUserName(string userName)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateUserName),
            Description = "O Nome de utilizador '" + userName + "' já está em uso. Por favor, escolha outro nome de utilizador."
        };
    }
    
    /// <summary>
    /// Mensagem de erro quando o email já está em uso.
    /// </summary>
    /// <param name="email">Email duplicado.</param>
    public override IdentityError DuplicateEmail(string email)
    {
        return new IdentityError
        {
            Code = nameof(DuplicateEmail),
            Description = "O Email '" + email + "' já está em uso. Por favor, escolha outro email."
        };
    }
    
    /// <summary>
    /// Mensagem de erro quando o nome de utilizador contém caracteres inválidos.
    /// </summary>
    /// <param name="userName">Nome de utilizador inválido.</param>
    public override IdentityError InvalidUserName(string userName)
    {
        return new IdentityError
        {
            Code = nameof(InvalidUserName),
            Description = "O Nome de utilizador '" + userName + "' contém caracteres inválidos. Por favor, use apenas letras, números e os seguintes caracteres: - . _ @"
        };
    }
    
    /// <summary>
    /// Mensagem de erro quando o email é inválido.
    /// </summary>
    /// <param name="email">Email inválido.</param>
    public override IdentityError InvalidEmail(string email)
    {
        return new IdentityError
        {
            Code = nameof(InvalidEmail),
            Description = "O Email '" + email + "' é inválido. Por favor, insira um endereço de email válido."
        };
    }
}