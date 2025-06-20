// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using byte_hunt.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace byte_hunt.Areas.Identity.Pages.Account {
    /// <summary>
    ///     Modelo da página de início de sessão.
    ///     Gere a autenticação dos utilizadores na aplicação.
    /// </summary>
    public class LoginModel : PageModel {
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        /// <summary>
        ///     Construtor do modelo de início de sessão.
        /// </summary>
        /// <param name="signInManager">Gestor de início de sessão para operações de autenticação.</param>
        /// <param name="logger">Serviço de registo para eventos de início de sessão.</param>
        public LoginModel(SignInManager<Utilizador> signInManager, ILogger<LoginModel> logger) {
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        ///     Modelo de dados para o formulário de início de sessão.
        /// </summary>
        [BindProperty] public InputModel Input { get; set; }

        /// <summary>
        ///     Lista de esquemas de autenticação externos disponíveis.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     URL para redirecionamento após início de sessão bem-sucedido.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     Mensagem de erro a ser exibida ao utilizador.
        /// </summary>
        [TempData] public string ErrorMessage { get; set; }

        /// <summary>
        ///     Classe que define os campos necessários para início de sessão.
        ///     Inclui email, palavra-passe e opção de lembrar sessão.
        /// </summary>
        public class InputModel {
            /// <summary>
            ///     Email do utilizador para início de sessão.
            /// </summary>
            [Required] [EmailAddress] public string Email { get; set; }

            /// <summary>
            ///     Palavra-passe do utilizador para autenticação.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     Opção para manter o utilizador autenticado por mais tempo.
            /// </summary>
            [Display(Name = "Lembrar de mim.")] public bool RememberMe { get; set; }
        }

        /// <summary>
        ///     Processa o carregamento da página de início de sessão.
        /// </summary>
        /// <param name="returnUrl">URL para redirecionamento após início de sessão bem-sucedido.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public async Task OnGetAsync(string returnUrl = null) {
            // Verifica se há uma mensagem de erro a ser exibida
            if (!string.IsNullOrEmpty(ErrorMessage)) {
                // Adiciona a mensagem de erro ao estado do modelo para exibição na página
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }
            
            // Define a URL de retorno padrão se não for fornecida
            returnUrl ??= Url.Content("~/");
            
            // Limpa o cookie de autenticação externa existente para garantir um processo de início de sessão limpo
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            
            // Obtém os esquemas de autenticação externos disponíveis
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            
            // Define a URL de retorno para redirecionamento após início de sessão
            ReturnUrl = returnUrl;
        }

        /// <summary>
        ///     Processa o pedido de início de sessão.
        /// </summary>
        /// <param name="returnUrl">URL para redirecionamento após início de sessão bem-sucedido.</param>
        /// <returns>
        ///     Redireciona para a URL de retorno se o início de sessão for bem-sucedido.
        ///     Redireciona para 2FA se necessário.
        ///     Redireciona para bloqueio se a conta estiver bloqueada.
        ///     Retorna a página com erros se o início de sessão falhar.
        /// </returns>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null) {
            // Verifica se o modelo de estado é válido
            returnUrl ??= Url.Content("~/");
            
            // Prepara a lista de esquemas de autenticação externos
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            
            // Verifica se o modelo de entrada é válido
            if (ModelState.IsValid) {
                // Tenta encontrar o utilizador pelo email fornecido
                var user = await _signInManager.UserManager.FindByEmailAsync(Input.Email);
                // Verifica se o utilizador foi encontrado
                if (user == null) {
                    // Adiciona um erro ao modelo de estado se o utilizador não for encontrado
                    ModelState.AddModelError(string.Empty, "Utilizador não encontrado!");
                }
                else {
                    // Tenta iniciar sessão com o nome de utilizador e palavra-passe fornecidos
                    var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password,
                        Input.RememberMe, lockoutOnFailure: false);
                    // Verifica o resultado do início de sessão
                    if (result.Succeeded) {
                        // Regista o evento de início de sessão bem-sucedido
                        _logger.LogInformation("User logged in.");
                        // Redireciona para a URL de retorno após início de sessão bem-sucedido
                        return LocalRedirect(returnUrl);
                    }
                    
                    // Verifica se o início de sessão requer autenticação de dois fatores
                    if (result.RequiresTwoFactor) {
                        return RedirectToPage("./LoginWith2fa",
                            new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                    }
                    
                    // Verifica se a conta do utilizador está bloqueada
                    if (result.IsLockedOut) {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }
                    else {
                        // Adiciona um erro ao modelo de estado se o início de sessão falhar
                        ModelState.AddModelError(string.Empty, "Erro ao entrar. Verifique seus dados.");
                        return Page();
                    }
                }
            }

            // Se o modelo de estado não for válido, retorna a página atual com os erros
            return Page();
        }
    }
}