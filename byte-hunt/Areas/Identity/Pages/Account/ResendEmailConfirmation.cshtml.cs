// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using byte_hunt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace byte_hunt.Areas.Identity.Pages.Account
{
    /// <summary>
    ///     Modelo da página para reenvio de confirmação de email.
    ///     Permite aos utilizadores solicitar novo email de confirmação.
    /// </summary>
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly IEmailSender _emailSender;

        /// <summary>
        ///     Construtor do modelo de reenvio de confirmação de email.
        /// </summary>
        /// <param name="userManager">Gestor de utilizadores para operações relacionadas com contas.</param>
        /// <param name="emailSender">Serviço de envio de emails.</param>
        public ResendEmailConfirmationModel(UserManager<Utilizador> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     Modelo de dados para o formulário de reenvio de confirmação.
        ///     Contém o endereço de email para onde enviar a confirmação.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     Classe que define o campo necessário para reenvio de confirmação de email.
        ///     Inclui validação para garantir formato válido de endereço.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     Endereço de email para o qual reenviar a confirmação.
            ///     Deve ser um email válido e é obrigatório.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        /// <summary>
        ///     Processa o carregamento inicial da página.
        /// </summary>
        public void OnGet()
        {
        }

        /// <summary>
        ///     Processa o pedido de reenvio de confirmação de email.
        /// </summary>
        /// <returns>
        ///     Retorna a página com mensagem de sucesso se o email for enviado.
        ///     Retorna a página com erro se o utilizador não for encontrado ou se o formulário for inválido.
        /// </returns>
        public async Task<IActionResult> OnPostAsync()
        {
            // Verifica se o modelo de entrada é válido
            if (!ModelState.IsValid)
            {
                // Se o modelo não for válido, retorna a página com erros
                return Page();
            }
            
            // Procura o utilizador pelo email fornecido
            var user = await _userManager.FindByEmailAsync(Input.Email);
            // Verifica se o utilizador existe
            if (user == null)
            {
                // Se não encontrar, adiciona um erro ao modelo e retorna a página
                ModelState.AddModelError(string.Empty, "Não foi encontrado nenhum utilizador associado a este email");
                // Retorna a página com o erro
                return Page();
            }
            
            // Obtém o ID do utilizador e o token de confirmação de email
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // Codifica o token para URL
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            // Cria a URL de confirmação com o ID do utilizador e o código
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            
            // Define o assunto e o corpo do email de confirmação
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
            
            // Envia o email de confirmação
            await _emailSender.SendEmailAsync(
                Input.Email,
                subject,
                body);
            
            // Adiciona uma mensagem de sucesso ao modelo
            ModelState.AddModelError(string.Empty, "Confirmação de conta reenviada, verifique seu email!");
            // Retorna a página com a mensagem de sucesso
            return Page();
        }
    }
}
