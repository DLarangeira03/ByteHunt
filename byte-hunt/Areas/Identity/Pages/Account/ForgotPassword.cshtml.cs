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
    ///     Modelo da página de recuperação de palavra-passe.
    ///     Permite aos utilizadores solicitar alteração da palavra-passe.
    /// </summary>
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly IEmailSender _emailSender;

        /// <summary>
        ///     Construtor do modelo de recuperação de palavra-passe.
        /// </summary>
        /// <param name="userManager">Gestor de utilizadores para operações relacionadas com contas.</param>
        /// <param name="emailSender">Serviço de envio de emails.</param>
        public ForgotPasswordModel(UserManager<Utilizador> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     Modelo de dados para o formulário de recuperação de palavra-passe.
        ///     Contém o endereço de email para onde enviar as instruções.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     Classe que define o campo necessário para recuperação de palavra-passe.
        ///     Inclui validação para garantir formato válido de endereço.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     Email do utilizador que solicita recuperação de palavra-passe.
            ///     Usado para identificar a conta e enviar instruções.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        /// <summary>
        ///     Processa o pedido de recuperação de palavra-passe.
        /// </summary>
        /// <returns>
        ///     Redireciona para a página de confirmação após enviar o email de alteração.
        ///     Retorna a página com erros de validação se o formulário for inválido.
        /// </returns>
        public async Task<IActionResult> OnPostAsync()
        {
            // Verifica se o modelo de entrada é válido
            if (ModelState.IsValid)
            {
                // Procura o utilizador pelo email fornecido
                var user = await _userManager.FindByEmailAsync(Input.Email);
                // Verifica se o utilizador existe e se o email está confirmado
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Se o utilizador não existir ou o email não estiver confirmado, redireciona para a confirmação
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }
                
                // Gera o token de alteração de palavra-passe e cria a URL de callback
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                // Codifica o token para ser usado na URL
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                // Cria a URL de callback para alteração de palavra-passe
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                // Define o assunto e o corpo do email a ser enviado
                var subject = "Alteração de Palavra-Passe - ByteHunt";
                
                var body = $@"<div style='font-family:Arial; font-size:14px;'>
                <p>Olá <strong>{user.UserName}</strong>,</p>
                <p>Recebemos um pedido para alterar a sua Palavra-Passe na ByteHunt.</p>
                <p>Clique no botão abaixo para continuar:</p>
                <p style='margin-top:20px;'>
                    <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'
                       style='display:inline-block;padding:10px 20px;background-color:#dc3545;color:#fff;
                              text-decoration:none;border-radius:4px;'>
                        Alterar Palavra-Passe
                    </a>
                </p>
                <p>Se  não solicitou esta ação, ignore este email. Sua Palavra-Passe permanecerá segura.</p>
                <br/>
                <p>— Equipa ByteHunt</p>
              </div>";
                
                // Envia o email com as instruções para a alteração de palavra-passe
                await _emailSender.SendEmailAsync(Input.Email, subject, body);
                
                // Redireciona para a página de confirmação após enviar o email
                return RedirectToPage("./ForgotPasswordConfirmation");
            }
            
            // Se o modelo não for válido, retorna a página atual com os erros de validação
            return Page();
        }
    }
}
