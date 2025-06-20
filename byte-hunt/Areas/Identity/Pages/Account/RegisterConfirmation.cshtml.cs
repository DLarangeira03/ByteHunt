// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Text;
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
    ///     Modelo da página de confirmação de registo.
    ///     Exibe informações sobre a necessidade de confirmação de email.
    /// </summary>
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly IEmailSender _sender;

        /// <summary>
        ///     Construtor do modelo de confirmação de registo.
        /// </summary>
        /// <param name="userManager">Gestor de utilizadores para operações relacionadas com contas.</param>
        /// <param name="sender">Serviço de envio de emails.</param>
        public RegisterConfirmationModel(UserManager<Utilizador> userManager, IEmailSender sender)
        {
            _userManager = userManager;
            _sender = sender;
        }

        /// <summary>
        ///     Email do utilizador que se registou.
        ///     Utilizado para exibir informações na página.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     Indica se deve ser exibido um link de confirmação direto na página.
        ///     Normalmente desativado em ambientes de produção.
        /// </summary>
        public bool DisplayConfirmAccountLink { get; set; }

        /// <summary>
        ///     URL para confirmação de email quando DisplayConfirmAccountLink é verdadeiro.
        ///     Utilizado para testes ou ambientes de desenvolvimento.
        /// </summary>
        public string EmailConfirmationUrl { get; set; }

        /// <summary>
        ///     Processa o carregamento da página de confirmação de registo.
        /// </summary>
        /// <param name="email">Email do utilizador que se registou.</param>
        /// <param name="returnUrl">URL para redirecionar após a confirmação.</param>
        /// <returns>
        ///     Retorna a página de confirmação se o utilizador for encontrado.
        ///     Redireciona para a página inicial se o email for nulo.
        ///     Retorna NotFound se o utilizador não for encontrado.
        /// </returns>
        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            // Se o email for nulo, redireciona para a página inicial
            if (email == null)
            {
                // Loga o evento de registo
                return RedirectToPage("/Index");
            }
            // Define o URL de retorno padrão se não for fornecido
            returnUrl = returnUrl ?? Url.Content("~/");
            
            // Tenta encontrar o utilizador pelo email fornecido
            var user = await _userManager.FindByEmailAsync(email);
            // Se o utilizador não for encontrado, retorna NotFound
            if (user == null)
            {
                // Não revela que o utilizador não existe
                return NotFound($"Unable to load user with email '{email}'.");
            }
            
            // Define o email do utilizador na propriedade Email
            Email = email;
            // Define se deve exibir o link de confirmação
            DisplayConfirmAccountLink = false;
            // Verifica se o utilizador precisa de confirmação de email
            if (DisplayConfirmAccountLink)
            {
                // Obtem o ID do utilizador e gera um token de confirmação de email
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // Codifica o token para URL
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                // Cria a URL de confirmação de email
                EmailConfirmationUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);
            }
            
            // Se não for para exibir o link de confirmação, apenas retorna a página
            return Page();
        }
    }
}
