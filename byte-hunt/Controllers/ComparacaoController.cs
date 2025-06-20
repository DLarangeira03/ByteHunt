using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using byte_hunt.Data;
using byte_hunt.Models;
using byte_hunt.Models.Comparador;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace byte_hunt.Controllers
{
    public class ComparacaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;

        public ComparacaoController(ApplicationDbContext context, UserManager<Utilizador> userManager,
            SignInManager<Utilizador> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Exibe o formulário para selecionar itens a comparar.
        /// </summary>
        /// <returns>View com todos os itens disponíveis para comparação.</returns>
        public IActionResult CompareSelect()
        {
            // Carrega todos os itens com suas categorias para a seleção
            var items = _context.Itens.Include(i => i.Categoria).ToList();
            // Retorna a view com todos os itens disponíveis para comparação
            return View(new ItemCompareSelectViewModel { AllItems = items });
        }

        /// <summary>
        /// Executa a comparação entre os itens selecionados.
        /// </summary>
        /// <param name="itemNames">Lista dos nomes dos itens a comparar.</param>
        /// <returns>View com o resultado da comparação ou erro se inválido.</returns>
        [HttpPost]
        public async Task<IActionResult> RunCompare(List<string> itemNames)
        {
            // Query para obter os itens selecionados pelo seu nome
            var items = _context.Itens
                .Include(i => i.Categoria)
                .Where(i => itemNames.Contains(i.Nome))
                .ToList();

            // Verifica se a quantidade de itens está entre 2 e 4
            if (items.Count < 2 || items.Count > 4)
                // Retorna um erro se a quantidade de itens não for válida
                return BadRequest("Selecione de 2 a 4 itens por favor");

            // Tira o atributo CategoriaId dos itens selecionados
            var categoryId = items.First().CategoriaId;
            // Verifica se todos os itens pertencem à mesma categoria
            if (items.Any(i => i.CategoriaId != categoryId))
                // Retorna um erro se os itens não pertencerem à mesma categoria
                return BadRequest("Todos os itens devem pertencer à mesma categoria");

            // Se o utilizador estiver autenticado, salva a comparação no histórico
            if (_signInManager.IsSignedIn(User))
            {
                // Obtém o utilizador autenticado
                var user = await _userManager.GetUserAsync(User);
                // Cria uma nova comparação com a data atual, ID do utilizador e os itens selecionados
                var comparacao = new Comparacao
                {
                    Data = DateTime.Now,
                    UtilizadorId = user.Id,
                    Itens = items
                };
                // Adiciona a comparação ao contexto e salva as alterações no banco de dados
                _context.Comparacoes.Add(comparacao);
                // Salva as alterações no contexto
                await _context.SaveChangesAsync();
            }

            // Constrói o modelo de comparação com os itens selecionados
            var model = BuildComparisonViewModel(items);
            // Retorna a view de comparação com o modelo construído
            return View("Compare", model);
        }

        /// <summary>
        /// Constrói o modelo de comparação entre os itens.
        /// </summary>
        /// <param name="items">Lista de itens a comparar.</param>
        /// <returns>ViewModel com os dados organizados para comparação.</returns>
        private ItemComparisonViewModel BuildComparisonViewModel(List<Item> items)
        {
            // Cria um dicionário para mapear atributos e seus valores
            var attrMap = new Dictionary<string, List<string>>();

            // Itera sobre cada item para extrair seus atributos
            for (int i = 0; i < items.Count; i++)
            {
                // Deserializa o JSON de atributos do item para um dicionário
                var attrs =
                    System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(items[i].AttrsJson ?? "{}");

                // Para cada atributo do item atual, garante que existe uma lista para esse atributo em attrMap.
                // Se não existir, inicializa a lista com "---" para todos os itens.
                // Em seguida, atribui o valor real do atributo na posição correspondente ao item atual.
                foreach (var kv in attrs)
                {
                    var key = kv.Key.Trim();
                    if (!attrMap.ContainsKey(key))
                        attrMap[key] = Enumerable.Repeat("---", items.Count).ToList();
                    attrMap[key][i] = kv.Value;
                }
            }

            // Cria uma lista para armazenar as linhas de comparação de atributos
            var rows = new List<AttrComparisonRow>();

            // Itera sobre o dicionário de atributos para criar as linhas de comparação
            foreach (var kvp in attrMap)
            {
                // Obtém a chave e os valores correspondentes
                var key = kvp.Key;
                var values = kvp.Value;

                // Procura uma regra de comparação para o atributo `key` no registo de regras.
                // Se não encontrar, cria uma regra padrão assumindo que valores maiores são melhores.
                var rule = AttributeRulesRegistry.Rules
                               .FirstOrDefault(r => string.Equals(r.Key, key, StringComparison.OrdinalIgnoreCase)).Value
                           ?? new AttributeComparer.AttributeRule
                           {
                               Key = key,
                               Direction = AttributeComparer.ComparisonDirection.HigherIsBetter
                           };

                // Compara os valores dos atributos usando as regras definidas
                List<HighlightType> highlights;

                highlights = new List<HighlightType>(new HighlightType[values.Count]);
                
                // Filtra os índices dos valores válidos (diferentes de "---")
                var validIndices = values
                    .Select((v, idx) => (v, idx))
                    .Where(t => t.v != "---")
                    .Select(t => t.idx)
                    .ToList();
                
                // Se houver valores válidos, compara-os com a regra
                if (validIndices.Count > 0) {
                    var validValues = validIndices.Select(idx => values[idx]).ToList();
                    var partialHighlights = AttributeComparer.Compare(validValues, rule);

                    for (int i = 0; i < validIndices.Count; i++) {
                        highlights[validIndices[i]] = partialHighlights[i];
                    }
                }

                // Adiciona uma nova linha de comparação com a chave, valores e destaques
                rows.Add(new AttrComparisonRow
                {
                    Key = key,
                    Values = values,
                    Highlights = highlights
                });
            }

            // Retorna o modelo de comparação com os itens e as linhas de atributos
            return new ItemComparisonViewModel
            {
                Items = items,
                AttrRows = rows
            };
        }

        /// <summary>
        /// Mostra o histórico de comparações do utilizador autenticado.
        /// </summary>
        /// <returns>View com a lista de comparações realizadas pelo utilizador.</returns>
        [Authorize]
        public async Task<IActionResult> HistoricoComparacao()
        {
            // Verifica se o utilizador está autenticado
            // Se não estiver autenticado, redireciona para a página de login
            if (!_signInManager.IsSignedIn(User)) return RedirectToAction("Login", "Account");

            // Obtém o utilizador autenticado e suas comparações
            var user = await _userManager.GetUserAsync(User);
            // Busca as comparações do utilizador, incluindo os itens relacionados, ordenadas por data
            var comparacoes = await _context.Comparacoes
                .Where(c => c.UtilizadorId == user.Id)
                .Include(c => c.Itens)
                .OrderByDescending(c => c.Data)
                .ToListAsync();

            // Retorna a view com as comparações do utilizador
            return View(comparacoes);
        }

        /// <summary>
        /// Remove uma comparação específica do histórico do utilizador.
        /// </summary>
        /// <param name="id">ID da comparação a eliminar.</param>
        /// <returns>Redireciona para o histórico de comparações.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EliminarHistorico(int id)
        {
            // Procura as informações do utilizador autenticado
            var user = await _userManager.GetUserAsync(User);
            // Procura a comparação específica pelo ID e pelo ID do utilizador
            var comp = await _context.Comparacoes
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.Id == id && c.UtilizadorId == user.Id);

            // Se a comparação for encontrada, remove-a do contexto e salva as alterações
            if (comp != null)
            {
                // Remove a comparação do contexto
                _context.Comparacoes.Remove(comp);
                // Salva as alterações no contexto
                await _context.SaveChangesAsync();
            }

            // Redireciona para a ação de histórico de comparação
            return RedirectToAction("HistoricoComparacao");
        }

        /// <summary>
        /// Remove todas as comparações do histórico do utilizador autenticado.
        /// </summary>
        /// <returns>Redireciona para o histórico de comparações.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EliminarHistoricoAll()
        {
            // Procura as informações do utilizador autenticado
            var user = await _userManager.GetUserAsync(User);
            // Retira todas as comparações associadas ao utilizador
            var all = _context.Comparacoes.Where(c => c.UtilizadorId == user.Id);

            // Remove todas as comparações encontradas
            _context.Comparacoes.RemoveRange(all);
            // Salva as alterações no contexto
            await _context.SaveChangesAsync();

            // Redireciona para a ação de histórico de comparação
            return RedirectToAction("HistoricoComparacao");
        }
    }
}