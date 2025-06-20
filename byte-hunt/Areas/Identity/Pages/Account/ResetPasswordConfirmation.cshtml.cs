// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace byte_hunt.Areas.Identity.Pages.Account
{
    /// <summary>
    ///     Modelo da página de confirmação de redefinição de palavra-passe.
    ///     Exibida após o utilizador alterar a sua palavra-passe com sucesso.
    /// </summary>
    [AllowAnonymous]
    public class ResetPasswordConfirmationModel : PageModel
    {
        /// <summary>
        ///     Processa o carregamento da página de confirmação de alteração de palavra-passe.
        /// </summary>
        public void OnGet()
        {
        }
    }
}
