// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using byte_hunt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace byte_hunt.Areas.Identity.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;

        public ChangePasswordModel(
            UserManager<Utilizador> userManager,
            SignInManager<Utilizador> signInManager,
            ILogger<ChangePasswordModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        ///     Modelo de entrada de dados para o formulário de alteração de palavra-passe.
        ///     Contém os campos para palavra-passe atual, nova palavra-passe e confirmação.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     Mensagem de estado exibida ao utilizador após tentativa de alteração de palavra-passe.
        ///     Persiste entre pedidos através do TempData.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     Classe que define os campos necessários para alteração de palavra-passe.
        ///     Inclui validações de requisitos de segurança para palavras-passe.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     Palavra-passe atual do utilizador, necessária para confirmar a identidade.
            ///     Campo obrigatório para realizar alterações.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Palavra-Passe atual")]
            public string OldPassword { get; set; }

            /// <summary>
            ///     Nova palavra-passe que o utilizador deseja configurar.
            ///     Deve cumprir os requisitos mínimos de segurança (pelo menos 6 caracteres).
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Nova Palavra-Passe")]
            public string NewPassword { get; set; }

            /// <summary>
            ///     Confirmação da nova palavra-passe 
            ///     Deve ser idêntica ao campo de nova palavra-passe.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar nova Palavra-Passe")]
            [Compare("NewPassword", ErrorMessage = "A nova Palavra-Passe e sua confirmação devem ser iguais")]
            public string ConfirmPassword { get; set; }
        }

        /// <summary>
        ///     Carrega a página de alteração de palavra-passe.
        /// </summary>
        /// <returns>
        ///     Retorna a página de alteração de palavra-passe se o utilizador tiver palavra-passe definida.
        ///     Redireciona para a página de definição de palavra-passe caso contrário.
        ///     Retorna NotFound se o utilizador não for encontrado.
        /// </returns>
        public async Task<IActionResult> OnGetAsync()
        {
            // Obtém o utilizador atual
            var user = await _userManager.GetUserAsync(User);
            // Verifica se o utilizador existe
            if (user == null)
            {
                // Se não for encontrado, retorna NotFound com mensagem de erro
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            
            // Verifica se o utilizador tem uma palavra-passe definida
            var hasPassword = await _userManager.HasPasswordAsync(user);
            // Se não tiver palavra-passe, redireciona para a página de definição de palavra-passe
            if (!hasPassword)
            {
                // Redireciona para a página de definição de palavra-passe
                return RedirectToPage("./SetPassword");
            }
            
            // Retorna a página de alteração de palavra-passe com o modelo de entrada vazio
            return Page();
        }
        
        /// <summary>
        ///     Processa o pedido de alteração de palavra-passe.
        /// </summary>
        /// <returns>
        ///     Retorna a mesma página com erros de validação se o formulário for inválido ou se a alteração falhar.
        ///     Redireciona para a mesma página com mensagem de sucesso se a palavra-passe for alterada corretamente.
        ///     Retorna NotFound se o utilizador não for encontrado.
        /// </returns>
        public async Task<IActionResult> OnPostAsync()
        {   
            // Verifica se o modelo de entrada é válido
            if (!ModelState.IsValid)
            {
                // Se o modelo não for válido, retorna a mesma página para exibir erros
                return Page();
            }
            
            // Obtém o utilizador atual
            var user = await _userManager.GetUserAsync(User);
            // Verifica se o utilizador existe
            if (user == null)
            {
                // Se não for encontrado, retorna NotFound com mensagem de erro
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            
            // Tenta alterar a palavra-passe do utilizador
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            // Verifica se a alteração da palavra-passe foi bem-sucedida
            if (!changePasswordResult.Succeeded)
            {
                // Se a alteração falhar, adiciona erros ao modelo de estado
                foreach (var error in changePasswordResult.Errors)
                {
                    // Verifica se o erro é de palavra-passe incorreta
                    if (error.Code == "PasswordMismatch")
                    {
                        // Adiciona erro específico para palavra-passe incorreta
                        ModelState.AddModelError(string.Empty, "A Palavra-Passe atual está incorreta.");
                    }
                    else
                    {
                        // Adiciona erro genérico ao modelo de estado
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                 // Retorna a mesma página com erros de validação
                return Page();
            }
            
            // Se a alteração for bem-sucedida, regista o evento e atualiza a sessão do utilizador
            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");
            // Define a mensagem de estado para ser exibida ao utilizador
            StatusMessage = "Palavra-Passe foi alterada com sucesso.";
            
            // Redireciona para a mesma página para evitar reenvio do formulário
            return RedirectToPage();
        }
    }
}
