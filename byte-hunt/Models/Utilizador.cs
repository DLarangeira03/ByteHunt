using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace byte_hunt.Models;

public class Utilizador:IdentityUser
{
    [Required]
    public string Nome { get; set; } //nome do utilizador
    
    [Required]
    public string Tipo { get; set; } //especifica a função do utilizador na app
    
    public string FotoPerfil { get; set; } // foto de perfil do utilizador

    public ICollection<Contribuicao> Contribuicoes { get; set; } = new List<Contribuicao>(); // Uma coleção de contribuições feitas pelo utilizador
    public ICollection<Comparacao> Comparacoes { get; set; } = new List<Comparacao>(); //Uma coleção de comparações feitas pelo utilizador

}