// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace byte_hunt.Areas.Identity.Pages.Account
{
    /// <summary>
    ///     Modelo da página de acesso negado.
    ///     Exibida quando um utilizador tenta aceder a um recurso para o qual não tem permissão.
    /// </summary>
    public class AccessDeniedModel : PageModel
    {
        /// <summary>
        ///     Carrega a página de acesso negado.
        /// </summary>
        public void OnGet()
        {
        }
    }
}
