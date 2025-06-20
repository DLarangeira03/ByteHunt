// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using byte_hunt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace byte_hunt.Areas.Identity.Pages.Account
{
    /// <summary>
    ///     Modelo da página de alteração de palavra-passe.
    ///     Permite aos utilizadores definir uma nova palavra-passe após solicitação de recuperação.
    /// </summary>
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;

        /// <summary>
        ///     Construtor do modelo de alteração de palavra-passe.
        /// </summary>
        /// <param name="userManager">Gestor de utilizadores para operações relacionadas com contas.</param>
        public ResetPasswordModel(UserManager<Utilizador> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        ///     Modelo de dados para o formulário de alteração de palavra-passe.
        ///     Contém os campos necessários para a alteração.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     Classe que define os campos necessários para alteração de palavra-passe.
        ///     Inclui validações para garantir segurança da nova palavra-passe.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     Email do utilizador que solicita a alteração de palavra-passe.
            ///     Usado para identificar a conta a ser modificada.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     Nova palavra-passe que o utilizador deseja definir.
            ///     Deve cumprir os requisitos mínimos de segurança.
            /// </summary>
            [Required]
            [Display(Name = "Nova Palavra-Passe:")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     Confirmação da nova palavra-passe.
            ///     Deve ser idêntica ao campo de nova palavra-passe.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirme a Palavra-Passe:")]
            [Compare("Password", ErrorMessage = "As Palavras-Passe devem ser iguais!")]
            public string ConfirmPassword { get; set; }

            /// <summary>
            ///     Código de segurança enviado por email para autorizar a alteração.
            ///     Necessário para validar a operação.
            /// </summary>
            [Required]
            public string Code { get; set; }

        }

        /// <summary>
        ///     Processa o carregamento da página de alteração de palavra-passe.
        /// </summary>
        /// <param name="code">Código de segurança para autorizar a alteração.</param>
        /// <returns>
        ///     Retorna a página de alteração se o código for válido.
        ///     Retorna BadRequest se o código for nulo.
        /// </returns>
        public IActionResult OnGet(string code = null)
        {
            // Verifica se o código foi fornecido na solicitação
            if (code == null)
            {
                // Se não houver código, retorna um erro de solicitação inválida
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                // Inicializa o modelo de entrada 
                Input = new InputModel
                {
                    // Decodifica o código de segurança fornecido
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
                };
                // Retorna a página de alteração de palavra-passe com o modelo preenchido
                return Page();
            }
        }

        /// <summary>
        ///     Processa o pedido de alteração de palavra-passe.
        /// </summary>
        /// <returns>
        ///     Redireciona para a página de confirmação se a alteração for bem-sucedida.
        ///     Retorna a página com erros de validação se o formulário for inválido ou a alteração falhar.
        /// </returns>
        public async Task<IActionResult> OnPostAsync()
        {
            // Verifica se o modelo de entrada é válido
            if (!ModelState.IsValid)
            {
                // Se o modelo não for válido, retorna a página com os erros de validação
                return Page();
            }
            
            // Procura o utilizador pelo email fornecido
            var user = await _userManager.FindByEmailAsync(Input.Email);
            // Verifica se o utilizador existe
            if (user == null)
            {
                // Adiciona um erro ao modelo de estado se o utilizador não for encontrado
                return RedirectToPage("./ResetPasswordConfirmation");
            }
            
            // Tenta redefinir a palavra-passe do utilizador com o código e nova palavra-passe fornecidos
            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            // Verifica se a redefinição foi bem-sucedida
            if (result.Succeeded)
            {
                // Regista a informação de que a palavra-passe foi alterada com sucesso
                return RedirectToPage("./ResetPasswordConfirmation");
            }
            
            // Se a redefinição falhar, adiciona os erros ao modelo de estado
            foreach (var error in result.Errors)
            {
                // Adiciona cada erro ao modelo de estado para exibição na página
                ModelState.AddModelError(string.Empty, error.Description);
            }
            // Se houver erros, retorna a página com os erros de validação
            return Page();
        }
    }
}
