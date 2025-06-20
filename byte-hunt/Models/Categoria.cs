using System.ComponentModel.DataAnnotations;

namespace byte_hunt.Models;

/// <summary>
///     Modelo que representa uma categoria de itens.
///     Permite classificar e organizar os itens em grupos lógicos.
/// </summary>
public class Categoria
{
    /// <summary>
    ///     Identificador único da categoria.
    ///     Chave primária na base de dados.
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    ///     Nome da categoria.
    ///     Campo obrigatório que deve ter entre 2 e 50 caracteres.
    /// </summary>
    [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 50 caracteres.")]
    [Display(Name = "Nome da Categoria")]
    public string Nome { get; set; }
    
    /// <summary>
    ///     Descrição detalhada da categoria.
    ///     Pode ser nulo se não houver descrição disponível.
    /// </summary>
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }
    
    /// <summary>
    ///     Coleção de itens que pertencem a esta categoria.
    ///     Representa uma relação um-para-muitos com a entidade Item.
    /// </summary>
    public ICollection<Item> Itens { get; set; } = new List<Item>();
}