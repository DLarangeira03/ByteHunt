// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace  byte_hunt.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    ///     Classe que gere a navegação na área de configurações da conta.
    ///     Responsável por definir nomes de rotas e classes CSS para o menu de navegação.
    /// </summary>
    public static class ManageNavPages
    {
        /// <summary>
        ///     Identificador para a página principal de gestão do perfil.
        /// </summary>
        public static string Index => "Index";

        /// <summary>
        ///     Identificador para a página de gestão de email.
        /// </summary>
        public static string Email => "Email";

        /// <summary>
        ///     Identificador para a página de alteração de palavra-passe.
        /// </summary>
        public static string ChangePassword => "ChangePassword";

        /// <summary>
        ///     Identificador para a página de download dos dados pessoais.
        /// </summary>
        public static string DownloadPersonalData => "DownloadPersonalData";

        /// <summary>
        ///     Identificador para a página de eliminação de dados pessoais.
        /// </summary>
        public static string DeletePersonalData => "DeletePersonalData";

        /// <summary>
        ///     Identificador para a página de dados pessoais.
        /// </summary>
        public static string PersonalData => "PersonalData";
        
        /// <summary>
        ///     Classe de CSS quando o item da dropdown escolhido é a Página Inical.
        ///     Usado para destacar o item de menu selecionado.
        /// </summary>
        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

        /// <summary>
        ///     Classe de CSS quando o item da dropdown escolhido é a Página Email.
        ///     Usado para destacar o item de menu selecionado.
        /// </summary>
        public static string EmailNavClass(ViewContext viewContext) => PageNavClass(viewContext, Email);

        /// <summary>
        ///     Classe de CSS quando o item da dropdown escolhido é a Página Alterar de Password.
        ///     Usado para destacar o item de menu selecionado.
        /// </summary>
        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

        /// <summary>
        ///     Classe de CSS quando o item da dropdown escolhido é a Página Download dos Dados Pessoais.
        ///     Usado para destacar o item de menu selecionado.
        /// </summary>
        public static string DownloadPersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DownloadPersonalData);

        /// <summary>
        ///     Classe de CSS quando o item da dropdown escolhido é a Página Eliminar os Dados Pessoais.
        ///     Usado para destacar o item de menu selecionado.
        /// </summary>
        public static string DeletePersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DeletePersonalData);
        
    
        /// <summary>
        ///     Classe de CSS quando o item da dropdown escolhido é a Página Dados Pessoais.
        ///     Usado para destacar o item de menu selecionado.
        /// </summary>
        public static string PersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, PersonalData);
        
    
        /// <summary>
        ///     Método que determina se uma página está ativa com base no contexto atual.
        ///     Devolve a classe CSS apropriada para o estilo de navegação.
        /// </summary>
        /// <param name="viewContext">O contexto da vista atual que contém informações sobre a página.</param>
        /// <param name="page">O identificador da página para verificar se está ativa.</param>
        /// <returns>
        ///     A classe CSS "active" se a página estiver ativa, ou null caso contrário.
        /// </returns>
        public static string PageNavClass(ViewContext viewContext, string page)
        {
            // Verifica se o contexto da vista e o identificador da página são válidos.
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            // Se não houver um identificador de página ativo, usa o nome do arquivo da ação atual.
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}
