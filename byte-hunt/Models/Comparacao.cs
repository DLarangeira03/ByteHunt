using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace byte_hunt.Models;

public class Comparacao
{
    //Atributos da classe
    [Key]
    public int Id { get; set; } //identificador
    
    [Required]
    [Display(Name = "Data da Comparação")]
    public DateTime Data { get; set; } //Data da comparação
    
    [Required]
    [Display(Name = "Utilizador")]
    public string UtilizadorId { get; set; } = string.Empty; //chave estrangeira
    
    [ForeignKey("UtilizadorId")]
    [ValidateNever]
    public Utilizador Utilizador { get; set; } //utilizador que realiza a comparação
    
    public ICollection<Item> Itens { get; set; } = new List<Item>();//Relação com os itens comparados
}