using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace byte_hunt.Models;

public class Contribuicao
{
    //Atributos da classe
    [Key]
    public int Id { get; set; } //identificador
    
    [Required]
    public string Descricao_Contribuicao { get; set; } //descrição da contribuição feita
    
    [Required]
    public DateTime Data { get; set; } //Data da contribuição
    
    [Required]
    public int UtilizadorId { get; set; } //chave estrangeira
    
    [ForeignKey("UtilizadorId")]
    [ValidateNever]
    public Utilizador Utilizador { get; set; } //utilizador que realiza a contribuição
    
    
    
}