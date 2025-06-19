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
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;

        public DeletePersonalDataModel(
            UserManager<Utilizador> userManager,
            SignInManager<Utilizador> signInManager,
            ILogger<DeletePersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        ///     Modelo para receber a palavra-passe do utilizador necessária para confirmação da eliminação.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     Classe que define os campos necessários para confirmação de eliminação da conta.
        ///     Contém apenas o campo de palavra-passe.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     Palavra-passe do utilizador para confirmar a eliminação da conta.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        /// <summary>
        ///     Indica se o sistema deve solicitar a palavra-passe para confirmar a eliminação da conta.
        /// </summary>
        public bool RequirePassword { get; set; }

        /// <summary>
        ///     Carrega a página de eliminação de dados pessoais.
        /// </summary>
        /// <returns>
        ///     Retorna a página de eliminação de dados pessoais se o utilizador for encontrado.
        ///     Retorna NotFound se o utilizador não for encontrado.
        /// </returns>
        public async Task<IActionResult> OnGet()
        {
            // Obtém o utilizador atual
            var user = await _userManager.GetUserAsync(User);
            // Verifica se o utilizador existe
            if (user == null)
            {
                // Se não existir, retorna NotFound com uma mensagem de erro
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            
            // Verifica se o utilizador tem palavra-passe definida
            RequirePassword = await _userManager.HasPasswordAsync(user);
            // Retorna a página de eliminação de dados pessoais
            return Page();
        }
        
        /// <summary>
        ///     Processa o pedido de eliminação de dados pessoais.
        /// </summary>
        /// <returns>
        ///     Redireciona para a página inicial se a eliminação for bem-sucedida.
        ///     Retorna a página com erros de validação se a palavra-passe estiver incorreta.
        ///     Retorna NotFound se o utilizador não for encontrado.
        /// </returns>
        public async Task<IActionResult> OnPostAsync()
        {
            // Procura o utilizador atual
            var user = await _userManager.GetUserAsync(User);
            // Verifica se o utilizador existe
            if (user == null)
            {
                // Se não existir, retorna NotFound com uma mensagem de erro
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            
            // Verifica se o utilizador tem palavra-passe definida
            RequirePassword = await _userManager.HasPasswordAsync(user);
            // Se a palavra-passe for necessária, verifica se foi fornecida e é correta
            if (RequirePassword)
            {
                // Verifica se a palavra-passe foi fornecida
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    // Se a palavra-passe estiver incorreta, adiciona um erro ao modelo e retorna a página
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    // Retorna a página com o modelo atualizado
                    return Page();
                }
            }
            
            // Tenta eliminar o utilizador
            var result = await _userManager.DeleteAsync(user);
            // Obtém o ID do utilizador para registo
            var userId = await _userManager.GetUserIdAsync(user);
            // Verifica se a eliminação foi bem-sucedida
            if (!result.Succeeded)
            {
                // Se a eliminação falhar, adiciona erros ao modelo
                throw new InvalidOperationException($"Unexpected error occurred deleting user.");
            }
            
            // Termina a sessão do utilizador após a eliminação
            await _signInManager.SignOutAsync();
            
            // Regista a eliminação do utilizador no log
            _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);
            
            // Redireciona para a página inicial após a eliminação bem-sucedida
            return Redirect("~/");
        }
    }
}
