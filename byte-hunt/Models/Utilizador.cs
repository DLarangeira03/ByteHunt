using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace byte_hunt.Models;

/// <summary>
///     Modelo de utilizador da aplicação.
///     Estende da classe Identity do EF.
/// </summary>
public class Utilizador:IdentityUser {
    
    /// <summary>
    ///     Nome completo do utilizador.
    ///     Deve ter entre 3 e 50 caracteres.
    /// </summary>
    [Required(ErrorMessage = "O nome é obrigatório.")] 
    [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 50 caracteres.")]
    [Display(Name = "Nome Completo")]
    public string Nome { get; set; } = "";//nome do utilizador

    /// <summary>
    ///     Tipo de utilizador no sistema.
    ///     Define o papel e as permissões do utilizador na aplicação.
    /// </summary>
    [Required(ErrorMessage = "O tipo de utilizador é obrigatório.")] 
    [Display(Name = "Tipo de Utilizador")]
    public string Tipo { get; set; } = ""; //especifica a função do utilizador na app
    
    /// <summary>
    ///     Foto de perfil do utilizador.
    /// </summary>
    [Display(Name = "Foto de Perfil")]
    public string FotoPerfil { get; set; } = string.Empty; //foto do perfil do utilizador
    
    /// <summary>
    ///     Coleção de contribuições feitas pelo utilizador.
    /// </summary>
    public ICollection<Contribuicao> Contribuicoes { get; set; } = new List<Contribuicao>(); // Uma coleção de contribuições feitas pelo utilizador
    
    /// <summary>
    ///        Coleção de comparações feitas pelo utilizador.
    /// </summary>
    public ICollection<Comparacao> Comparacoes { get; set; } = new List<Comparacao>(); //Uma coleção de comparações feitas pelo utilizador

}