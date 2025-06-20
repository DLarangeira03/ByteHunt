using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace byte_hunt.Models;

/// <summary>
///     Modelo que representa um item/produto disponível para comparação.
///     Contém informações detalhadas sobre as características e especificações do produto.
/// </summary>
public class Item
{ 
    /// <summary>
    ///     Identificador único do item.
    ///     Chave primária na base de dados.
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    ///     Nome do item.
    ///     Campo obrigatório para identificação do produto.
    /// </summary>
    [Required(ErrorMessage = "O nome do item é obrigatório.")]
    [Display(Name = "Nome do Item")]
    public string Nome { get; set; }
    
    /// <summary>
    ///     Marca do item.
    ///     Campo obrigatório para categorização do produto.
    /// </summary>
    [Required(ErrorMessage = "A marca é obrigatória.")]
    [Display(Name = "Marca")]
    public string Marca { get; set; }
    
    /// <summary>
    ///     Descrição detalhada do item.
    /// </summary>
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }
    
    /// <summary>
    ///     Preço do item em euros.
    ///     Deve ser um valor positivo e pode ser nulo se o preço não estiver disponível.
    /// </summary>
    [Display(Name = "Preço (€)")]
    [DataType(DataType.Currency)]
    [Range(0, double.MaxValue, ErrorMessage = "O preço deve ser positivo.")]
    public decimal? Preco { get; set; }

    /// <summary>
    ///     Caminho para a imagem do item.
    /// </summary>
    [Display(Name = "Imagem")]
    public string? FotoItem { get; set; }
    
    /// <summary>
    ///     Especificações técnicas do item em formato JSON.
    /// </summary>
    [Display(Name = "Especificações")]
    public string? AttrsJson { get; set; }
    
    /// <summary>
    ///     ID da categoria à qual o item pertence.
    ///     Chave estrangeira para a tabela de Categorias.
    /// </summary>
    [Display(Name = "Categoria")]
    public int CategoriaId { get; set; }
    
    /// <summary>
    ///     Referência à categoria do item.
    ///     Estabelece uma relação com a entidade Categoria.
    /// </summary>
    [ForeignKey("CategoriaId")]
    [ValidateNever]
    public Categoria Categoria { get; set; }
    
    /// <summary>
    ///     Coleção de comparações que incluem este item.
    ///     Representa uma relação muitos-para-muitos com a entidade Comparacao.
    /// </summary>
    public ICollection<Comparacao> Comparacoes { get; set; } = new List<Comparacao>();
}