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
    public string DetalhesContribuicao { get; set; } //descrição da contribuição feita
    
    [Required]
    [RegularExpression("Pending|Approved|Rejected", ErrorMessage = "Invalid status")]
    public string Status {get;set;} //Estado da Contribuição
    
    [Required]
    public DateTime DataContribuicao { get; set; } //Data da contribuição
    
    public DateTime? DataReview { get; set; } //Data da validação
    
   
    [ForeignKey("UtilizadorId")]
    [ValidateNever]
    public Utilizador Utilizador { get; set; } //utilizador que realiza a contribuição
    [Required] 
    public string UtilizadorId { get; set; } = string.Empty;
    
    public string? ResponsavelId { get; set; } //Responsável pela validação
    [ForeignKey("ResponsavelId")]
    [ValidateNever]
    public Utilizador Responsavel { get; set; } //moderador responsável 
    

    
    
    
}