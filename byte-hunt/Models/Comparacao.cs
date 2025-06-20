using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace byte_hunt.Models;

/// <summary>
///     Modelo que representa uma comparação entre múltiplos itens.
///     Regista quando um utilizador compara diferentes produtos.
/// </summary>
public class Comparacao
{
    /// <summary>
    ///     Identificador único da comparação.
    ///     Chave primária na base de dados.
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    ///     Data e hora em que a comparação foi realizada.
    ///     Campo obrigatório para registar quando ocorreu a comparação.
    /// </summary>
    [Required]
    [Display(Name = "Data da Comparação")]
    public DateTime Data { get; set; }
    
    /// <summary>
    ///     ID do utilizador que realizou a comparação.
    ///     Chave estrangeira para a tabela de Utilizadores.
    /// </summary>
    [Required]
    [Display(Name = "Utilizador")]
    public string UtilizadorId { get; set; } = string.Empty;
    
    /// <summary>
    ///     Referência ao utilizador que realizou a comparação.
    ///     Estabelece uma relação com a entidade Utilizador.
    /// </summary>
    [ForeignKey("UtilizadorId")]
    [ValidateNever]
    public Utilizador Utilizador { get; set; }
    
    /// <summary>
    ///     Coleção de itens incluídos nesta comparação.
    ///     Representa uma relação muitos-para-muitos com a entidade Item.
    /// </summary>
    public ICollection<Item> Itens { get; set; } = new List<Item>();
}