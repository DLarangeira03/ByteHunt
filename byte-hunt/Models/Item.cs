using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace byte_hunt.Models;

public class Item
{ 
    //Atributos da classe
    [Key]
    public int Id { get; set; } //identificador
    
    [Required(ErrorMessage = "O nome do item é obrigatório.")]
    [Display(Name = "Nome do Item")]
    public string Nome { get; set; } //nome do item
    
    [Required(ErrorMessage = "A marca é obrigatória.")]
    [Display(Name = "Marca")]
    public string Marca { get; set; } //marca do item
    
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; } //descrição do item
    
    [Display(Name = "Preço (€)")]
    [DataType(DataType.Currency)]
    [Range(0, double.MaxValue, ErrorMessage = "O preço deve ser positivo.")]
    public decimal? Preco { get; set; } //preço do item

    [Display(Name = "Imagem")]
    public string? FotoItem { get; set; } //foto do item
    
    [Display(Name = "Especificações")]
    public string? AttrsJson { get; set; } // JSON de atributos do item
    
    [Display(Name = "Categoria")]
    public int CategoriaId { get; set; } //chave estrangeira
    
    [ForeignKey("CategoriaId")]
    [ValidateNever]
    public Categoria Categoria { get; set; } //categoria do item
    
    public ICollection<Comparacao> Comparacoes { get; set; } = new List<Comparacao>(); //Relação muitos-para-muitos com Comparacao   

        
    
}