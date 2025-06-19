using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace byte_hunt.Models;

public class Utilizador:IdentityUser {
    
    [Required(ErrorMessage = "O nome é obrigatório.")] 
    [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 50 caracteres.")]
    [Display(Name = "Nome Completo")]
    public string Nome { get; set; } = "";//nome do utilizador

    [Required(ErrorMessage = "O tipo de utilizador é obrigatório.")] 
    [Display(Name = "Tipo de Utilizador")]
    public string Tipo { get; set; } = ""; //especifica a função do utilizador na app
    
    [Display(Name = "Foto de Perfil")]
    public string FotoPerfil { get; set; } = string.Empty; //foto do perfil do utilizador

    public ICollection<Contribuicao> Contribuicoes { get; set; } = new List<Contribuicao>(); // Uma coleção de contribuições feitas pelo utilizador
    public ICollection<Comparacao> Comparacoes { get; set; } = new List<Comparacao>(); //Uma coleção de comparações feitas pelo utilizador

}