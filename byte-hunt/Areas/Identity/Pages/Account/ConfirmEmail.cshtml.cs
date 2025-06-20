// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Linq;
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
    ///     Modelo da página de confirmação de email.
    ///     Processa o link de confirmação enviado para o email do utilizador.
    /// </summary>
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;

        /// <summary>
        ///     Construtor do modelo de confirmação de email.
        /// </summary>
        /// <param name="userManager">Gestor de utilizadores para operações relacionadas com contas.</param>
        public ConfirmEmailModel(UserManager<Utilizador> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        ///     Mensagem de estado exibida ao utilizador após tentativa de confirmação.
        ///     Persiste entre pedidos através do TempData.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     Processa a confirmação de email através do link enviado ao utilizador.
        /// </summary>
        /// <param name="userId">ID do utilizador a confirmar.</param>
        /// <param name="code">Código de segurança que valida a operação.</param>
        /// <returns>
        ///     Retorna a página com mensagem de sucesso se a confirmação for bem-sucedida.
        ///     Retorna NotFound se o utilizador não for encontrado.
        ///     Redireciona para a página inicial se os parâmetros forem inválidos.
        /// </returns>
        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            // Verifica se os parâmetros necessários foram fornecidos
            if (userId == null || code == null)
            {
                // Se não foram fornecidos, redireciona para a página inicial
                return RedirectToPage("/Index");
            }
            
            // Tenta encontrar o utilizador pelo ID fornecido
            var user = await _userManager.FindByIdAsync(userId);
            // Verifica se o utilizador foi encontrado
            if (user == null)
            {
                // Se não foi encontrado, retorna uma mensagem de erro
                return NotFound($"Erro ao carregar ID: '{userId}'.");
            }
            
            // Decodifica o código de confirmação do email
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            // Atualiza o estado do utilizador para confirmar o email
            var result = await _userManager.ConfirmEmailAsync(user, code);
            // Define a mensagem de estado com base no resultado da confirmação
            StatusMessage = result.Succeeded ? "Obrigado por confirmar o email!." : "Ocorreu um erro ao confirmar o email.";
            // Retorna a página atual com a mensagem de estado
            return Page();
        }
    }
}
