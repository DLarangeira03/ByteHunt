// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using byte_hunt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace byte_hunt.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly IEmailSender _emailSender;

        public EmailModel(
            UserManager<Utilizador> userManager,
            SignInManager<Utilizador> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     Email atual do utilizador registado no sistema.
        ///     Exibido na página para referência ao alterar o endereço.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     Indica se o email do utilizador já foi confirmado.
        /// </summary>
        public bool IsEmailConfirmed { get; set; }

        /// <summary>
        ///     Mensagem de estado exibida ao utilizador após operações de alteração de email.
        ///     Persiste entre pedidos através do TempData.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     Modelo de entrada de dados para o formulário de alteração de email.
        ///     Contém o campo para o novo endereço de email.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     Classe que define os campos necessários para alteração de email.
        ///     Inclui validações para garantir formato válido de endereço.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     Novo endereço de email que o utilizador deseja configurar.
            ///     Precisa ser confirmado via link enviado para este endereço.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Novo Email")]
            public string NewEmail { get; set; }
        }

        /// <summary>
        ///     Carrega os dados de email do utilizador para exibição na página.
        /// </summary>
        /// <param name="user">O utilizador para o qual carregar os dados de email.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        private async Task LoadAsync(Utilizador user)
        {
            // Obtém o email atual do utilizador e o armazena na propriedade Email.
            var email = await _userManager.GetEmailAsync(user);
            // Define a propriedade Email para ser usada na página.
            Email = email;
            
            // Inicializa o modelo de entrada com o novo email.
            Input = new InputModel
            {
            // Define o novo email para o modelo de entrada, que será usado no formulário.
                NewEmail = email,
            };
            
            // Verifica se o email do utilizador já foi confirmado.
            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }
        
        /// <summary>
        ///     Carrega a página de gestão de email com os dados do utilizador atual.
        /// </summary>
        /// <returns>
        ///     Retorna a página de gestão de email se o utilizador for encontrado.
        ///     Retorna NotFound se o utilizador não for encontrado.
        /// </returns>
        public async Task<IActionResult> OnGetAsync()
        {
            // Obtém o utilizador atual a partir do contexto de autenticação.
            var user = await _userManager.GetUserAsync(User);
            // Verifica se o utilizador foi encontrado.
            if (user == null)
            {
                // Se não for encontrado, retorna uma mensagem de erro NotFound.
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            
            // Carrega os dados do utilizador para a página.
            await LoadAsync(user);
            // Retorna a página de gestão de email com os dados carregados.
            return Page();
        }
        
        /// <summary>
        ///     Processa o pedido de alteração de email enviando um email de confirmação.
        /// </summary>
        /// <returns>
        ///     Redireciona para a mesma página com uma mensagem de confirmação se o pedido for bem-sucedido.
        ///     Retorna a página com erros de validação se o formulário for inválido.
        ///     Retorna NotFound se o utilizador não for encontrado.
        /// </returns>
        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            // Obtém o utilizador atual a partir do contexto de autenticação.
            var user = await _userManager.GetUserAsync(User);
            // Verifica se o utilizador foi encontrado.
            if (user == null)
            {
                // Se não for encontrado, retorna uma mensagem de erro NotFound.
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            
            // Verifica se o modelo de entrada é válido.
            if (!ModelState.IsValid)
            {
                // Se o modelo não for válido, recarrega os dados do utilizador 
                await LoadAsync(user);
                // Retorna a página 
                return Page();
            }
            
            // Obtém o email atual do utilizador.
            var email = await _userManager.GetEmailAsync(user);
            // Verifica se o novo email é diferente do email atual.
            if (Input.NewEmail != email)
            {
                // Procura o ID do utilizador atual.
                var userId = await _userManager.GetUserIdAsync(user);
                // Gera um token de alteração de email para o novo endereço.
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                // Codifica o token para ser usado na URL.
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                // Cria a URL de confirmação de alteração de email.
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = code },
                    protocol: Request.Scheme);
                
                // Define o assunto e o corpo do email de confirmação.
                var subject = "Confirmação de Conta - ByteHunt";
                var body = $@"<div style='font-family:Arial; font-size:14px;'>
                                      <p>Olá <strong>{user.UserName}</strong>,</p>
                                      <p>Obrigado por se registar na ByteHunt!</p>
                                      <p>Por favor confirma o email clicando no botão abaixo:</p>
                                      <p style='margin-top:20px;'>
                                          <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'
                                             style='display:inline-block;padding:10px 20px;background-color:#007bff;color:#fff;
                                                    text-decoration:none;border-radius:4px;'>
                                              Confirmar Conta
                                          </a>
                                      </p>
                                      <p>Se não solicitou a confirmação, ignore este email.</p>
                                      <br/>
                                      <p>— a ByteHunt</p>
                                  </div>";
                
                // Envia o email de confirmação para o novo endereço.
                await _emailSender.SendEmailAsync(
                    Input.NewEmail,
                    subject,
                    body);
                
                // Define a mensagem de estado para ser exibida na página.
                StatusMessage = "Enviámos para o seu email a verificação de conta. Verifique a sua caixa de entrada por favor";
                
                // Redireciona para a mesma página para exibir a mensagem de estado.
                return RedirectToPage();
            }
            
            // Se o novo email for igual ao atual, define uma mensagem de estado indicando que não houve alteração.
            StatusMessage = "Seu email não foi alterado.";
            // Redireciona para a mesma página para exibir a mensagem de estado.
            return RedirectToPage();
        }
        
        /// <summary>
        ///     Reenvia o email de verificação para o email atual do utilizador.
        /// </summary>
        /// <returns>
        ///     Redireciona para a mesma página com uma mensagem de confirmação se o email for enviado com sucesso.
        ///     Retorna a página com erros de validação se o formulário for inválido.
        ///     Retorna NotFound se o utilizador não for encontrado.
        /// </returns>
        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            // Obtém o utilizador atual a partir do contexto de autenticação.
            var user = await _userManager.GetUserAsync(User);
            // Verifica se o utilizador foi encontrado.
            if (user == null)
            {
                // Se não for encontrado, retorna uma mensagem de erro NotFound.
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            
            // Verifica se o modelo de entrada é válido.
            if (!ModelState.IsValid)
            {
                // Se o modelo não for válido, recarrega os dados do utilizador
                await LoadAsync(user);
                // Retorna a página com os dados carregados.
                return Page();
            }
            
            // Obtém o ID do utilizador, email e gera um token de confirmação de email.
            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            
            // Cria a URL de confirmação de email com o ID do utilizador e o token.
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);
            
            // Define o assunto e o corpo do email de confirmação.
            var subject = "Confirmação de Conta - ByteHunt";
            var body = $@"<div style='font-family:Arial; font-size:14px;'>
                                      <p>Olá <strong>{user.UserName}</strong>,</p>
                                      <p>Obrigado por se registar na ByteHunt!</p>
                                      <p>Por favor confirma o email clicando no botão abaixo:</p>
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
            
            // Envia o email de confirmação para o email atual do utilizador.
            await _emailSender.SendEmailAsync(
                email,
                subject,
                body);
            
            // Define a mensagem de estado para ser exibida na página.
            StatusMessage = "Enviámos para o seu email a verificação de conta. Verifique a sua caixa de entrada por favor";
            // Redireciona para a mesma página para exibir a mensagem de estado.
            return RedirectToPage();
        }
    }
}
