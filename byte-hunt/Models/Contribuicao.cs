using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace byte_hunt.Models;

public class Contribuicao
{
    //Atributos da classe
    [Key]
    public int Id { get; set; } //identificador
    
    [Required(ErrorMessage = "Os detalhes da contribuição são obrigatórios.")]
    [Display(Name = "Detalhes da Contribuição")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre 10 e 1000 caracteres.")]
    public string DetalhesContribuicao { get; set; } //descrição da contribuição feita
    
    [Required]
    [Display(Name = "Estado")]
    [RegularExpression("Pendente|Aprovado|Rejeitado", ErrorMessage = "Estado inválido.")]
    public string Status {get;set;} //Estado da Contribuição
    
    [Required]
    [Display(Name = "Data da Contribuição")]
    public DateTime DataContribuicao { get; set; } //Data da contribuição
    
    [Display(Name = "Data de Validação")]
    public DateTime? DataReview { get; set; } //Data da validação
    
    [Display(Name = "Data de Edição")]
    public DateTime? DataEditada { get; set; } //Data de Edição
   
    [ForeignKey("UtilizadorId")]
    [Display(Name = "Utilizador")]
    [ValidateNever]
    public Utilizador Utilizador { get; set; } //utilizador que realiza a contribuição
    [Required] 
    public string UtilizadorId { get; set; } = string.Empty;
    
    [Display(Name = "Responsável pela Revisão")]
    public string? ResponsavelId { get; set; } //Responsável pela validação
    [ForeignKey("ResponsavelId")]
    [ValidateNever]
    public Utilizador Responsavel { get; set; } //moderador responsável 
    
    
    
    
}