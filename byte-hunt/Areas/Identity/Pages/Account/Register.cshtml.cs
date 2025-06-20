// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using byte_hunt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace byte_hunt.Areas.Identity.Pages.Account {
    /// <summary>
    ///     Modelo da página de registo de novos utilizadores.
    ///     Permite a criação de contas na aplicação.
    /// </summary>
    public class RegisterModel : PageModel {
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly UserManager<Utilizador> _userManager;
        private readonly IUserStore<Utilizador> _userStore;
        private readonly IUserEmailStore<Utilizador> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        /// <summary>
        ///     Construtor do modelo de registo.
        /// </summary>
        /// <param name="userManager">Gestor de utilizadores para operações relacionadas com contas.</param>
        /// <param name="userStore">Armazenamento de utilizadores para guardar dados de conta.</param>
        /// <param name="signInManager">Gestor de início de sessão para autenticação.</param>
        /// <param name="logger">Serviço de registo para eventos relacionados com o registo.</param>
        /// <param name="emailSender">Serviço de envio de emails para confirmação de conta.</param>
        public RegisterModel(
            UserManager<Utilizador> userManager,
            IUserStore<Utilizador> userStore,
            SignInManager<Utilizador> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender) {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     Modelo de dados para o formulário de registo.
        /// </summary>
        [BindProperty] public InputModel Input { get; set; }

        /// <summary>
        ///     URL para redirecionamento após registo bem-sucedido.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     Lista de esquemas de autenticação externos disponíveis.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     Classe que define os campos necessários para o registo de utilizador.
        ///     Inclui validações para garantir dados corretos.
        /// </summary>
        public class InputModel {
            /// <summary>
            ///     Nome de utilizador para acesso à conta.
            ///     Deve ser único no sistema.
            /// </summary>
            [Required]
            [Display(Name = "Nome de Utilizador")]
            public string UserName { get; set; }

            /// <summary>
            ///     Nome completo do utilizador.
            /// </summary>
            [Required] [Display(Name = "Nome")] public string Nome { get; set; }

            /// <summary>
            ///     Endereço de email do utilizador.
            ///     Usado para confirmação de conta e comunicações.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     Palavra-passe para a conta.
            ///     Deve cumprir os requisitos mínimos de segurança.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "A {0} deve ter entre {2} e {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Palavra-Passe")]
            public string Password { get; set; }

            /// <summary>
            ///     Confirmação da palavra-passe.
            ///     Deve ser idêntica ao campo de palavra-passe.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirme a Palavra-Passe")]
            [Compare("Password", ErrorMessage = "A Palavra-Passe e a confirmação devem ser iguais!")]
            public string ConfirmPassword { get; set; }
        }

        /// <summary>
        ///     Processa o carregamento da página de registo.
        /// </summary>
        /// <param name="returnUrl">URL para redirecionamento após registo bem-sucedido.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        public async Task OnGetAsync(string returnUrl = null) {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        /// <summary>
        ///     Processa o pedido de registo de novo utilizador.
        /// </summary>
        /// <param name="returnUrl">URL para redirecionamento após registo bem-sucedido.</param>
        /// <returns>
        ///     Redireciona para a confirmação de registo se o registo for bem-sucedido e requerer confirmação.
        ///     Inicia sessão automaticamente se o registo for bem-sucedido e não requerer confirmação.
        ///     Retorna a página com erros se o registo falhar.
        /// </returns>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null) {
            // Verifica o estado do modelo e define o URL de retorno padrão se não for fornecido.
            returnUrl ??= Url.Content("~/");
            // Obtém os esquemas de autenticação externos disponíveis.
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            // Verifica se o modelo de entrada é válido.
            if (ModelState.IsValid) {
                // Cria uma nova instância do utilizador.
                var user = CreateUser();
            
                //campos extras
                user.Nome = Input.Nome;
                user.Tipo = "User";
                
                // Define o nome de utilizador e email no armazenamento de utilizadores.
                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);
                
                // Verifica se a criação do utilizador foi bem-sucedida.
                if (result.Succeeded) {
                    // Regista o evento de criação de utilizador.
                    _logger.LogInformation("User created a new account with password.");

                    // Adiciona o utilizador ao papel "User".
                    await _userManager.AddToRoleAsync(user, "User");
                    
                    // Gera o ID do utilizador e o token de confirmação de email.
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    // Codifica o token para URL.
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    // Cria a URL de confirmação de email com os parâmetros necessários.
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    // Envia o email com o assunto e corpo formatado.
                    var subject = "Confirmação de Conta - ByteHunt";

                    var body = $@"<div style='font-family:Arial; font-size:14px;'>
                                      <p>Olá <strong>{user.UserName}</strong>,</p>
                                      <p>Obrigado por se registar na ByteHunt!</p>
                                      <p>Por favor confirme o email clicando no botão abaixo:</p>
                                      <p style='margin-top:20px;'>
                                          <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'
                                             style='display:inline-block;padding:10px 20px;background-color:#007bff;color:#fff;
                                                    text-decoration:none;border-radius:4px;'>
                                              Confirmar Conta
                                          </a>
                                      </p>
                                      <p>Se não solicitou a confirmação, ignore este email.</p>
                                      <br/>
                                      <p>— Equipa ByteHunt</p>
                                  </div>";
                    
                    // Envia o email de confirmação.
                    await _emailSender.SendEmailAsync(Input.Email, subject, body);
                    
                    // Se a confirmação de conta é necessária, redireciona para a página de confirmação.
                    if (_userManager.Options.SignIn.RequireConfirmedAccount) {
                        return RedirectToPage("RegisterConfirmation",
                            new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else {
                        // Caso contrário, inicia sessão automaticamente e redireciona para a URL de retorno.
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                
                // Se a criação do utilizador falhar, adiciona os erros ao estado do modelo.
                foreach (var error in result.Errors) {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Se o modelo não for válido ou se ocorrer algum erro, retorna a página atual com os erros.
            return Page();
        }

        /// <summary>
        ///     Cria uma nova instância do modelo de utilizador.
        /// </summary>
        /// <returns>Uma nova instância do tipo Utilizador.</returns>
        /// <exception cref="InvalidOperationException">
        ///     Lançada quando não é possível criar uma instância do tipo Utilizador.
        /// </exception>
        private Utilizador CreateUser() {
            // Tenta criar uma nova instância do modelo de utilizador.
            try {
                return Activator.CreateInstance<Utilizador>();
            }
            catch {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Utilizador)}'. " +
                                                    $"Ensure that '{nameof(Utilizador)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                                                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        /// <summary>
        ///     Obtém o armazenamento de email do utilizador.
        /// </summary>
        /// <returns>A interface IUserEmailStore para operações relacionadas com email.</returns>
        /// <exception cref="NotSupportedException">
        ///     Lançada quando o gestor de utilizadores não suporta email.
        /// </exception>
        private IUserEmailStore<Utilizador> GetEmailStore() {
            // Verifica se o gestor de utilizadores suporta email.
            if (!_userManager.SupportsUserEmail) {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }

            return (IUserEmailStore<Utilizador>)_userStore;
        }
    }
}