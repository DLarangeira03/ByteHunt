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
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool IsEmailConfirmed { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Novo Email")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(Utilizador user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = code },
                    protocol: Request.Scheme);
                
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
                
                await _emailSender.SendEmailAsync(
                    Input.NewEmail,
                    subject,
                    body);

                StatusMessage = "Enviámos para o seu email a verificação de conta. Verifique a sua caixa de entrada por favor";
                return RedirectToPage();
            }

            StatusMessage = "Seu email não foi alterado.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);
            
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
            
            await _emailSender.SendEmailAsync(
                email,
                subject,
                body);

            StatusMessage = "Enviámos para o seu email a verificação de conta. Verifique a sua caixa de entrada por favor";
            return RedirectToPage();
        }
    }
}
