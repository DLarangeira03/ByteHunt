using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace byte_hunt.Models;

/// <summary>
///     Modelo que representa uma contribuição feita por um utilizador.
///     Inclui informações sobre o conteúdo, estado e revisão da contribuição.
/// </summary>
public class Contribuicao
{
    /// <summary>
    ///     Identificador único da contribuição.
    ///     Chave primária na base de dados.
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    ///     Descrição detalhada da contribuição.
    ///     Deve ter entre 10 e 1000 caracteres.
    /// </summary>
    [Required(ErrorMessage = "Os detalhes da contribuição são obrigatórios.")]
    [Display(Name = "Detalhes da Contribuição")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre 10 e 1000 caracteres.")]
    public string DetalhesContribuicao { get; set; }
    
    /// <summary>
    ///     Estado atual da contribuição.
    ///     Pode ser "Pendente", "Aprovado" ou "Rejeitado".
    /// </summary>
    [Required]
    [Display(Name = "Estado")]
    [RegularExpression("Pendente|Aprovado|Rejeitado", ErrorMessage = "Estado inválido.")]
    public string Status {get;set;}
    
    /// <summary>
    ///     Data e hora em que a contribuição foi submetida.
    /// </summary>
    [Required]
    [Display(Name = "Data da Contribuição")]
    public DateTime DataContribuicao { get; set; }
    
    /// <summary>
    ///     Data e hora em que a contribuição foi validada.
    ///     Nulo se ainda não foi validada.
    /// </summary>
    [Display(Name = "Data de Validação")]
    public DateTime? DataReview { get; set; }
    
    /// <summary>
    ///     Data e hora da última edição da contribuição.
    ///     Nulo se nunca foi editada após a submissão inicial.
    /// </summary>
    [Display(Name = "Data de Edição")]
    public DateTime? DataEditada { get; set; }
   
    /// <summary>
    ///     Referência ao utilizador que submeteu a contribuição.
    ///     Estabelece uma relação com a entidade Utilizador.
    /// </summary>
    [ForeignKey("UtilizadorId")]
    [Display(Name = "Utilizador")]
    [ValidateNever]
    public Utilizador Utilizador { get; set; }
    
    /// <summary>
    ///     Identificador do utilizador que submeteu a contribuição.
    ///     Chave estrangeira para a tabela de Utilizadores.
    /// </summary>
    [Required] 
    public string UtilizadorId { get; set; } = string.Empty;
    
    /// <summary>
    ///     Identificador do utilizador responsável pela validação da contribuição.
    ///     Nulo se a contribuição ainda não foi validada.
    /// </summary>
    [Display(Name = "Responsável pela Revisão")]
    public string? ResponsavelId { get; set; }
    
    /// <summary>
    ///     Referência ao utilizador responsável pela validação da contribuição.
    ///     Estabelece uma relação com a entidade Utilizador.
    /// </summary>
    [ForeignKey("ResponsavelId")]
    [ValidateNever]
    public Utilizador Responsavel { get; set; }
}