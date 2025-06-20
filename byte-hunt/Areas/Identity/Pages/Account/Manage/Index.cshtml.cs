// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using byte_hunt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace byte_hunt.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;

        public IndexModel(
            UserManager<Utilizador> userManager,
            SignInManager<Utilizador> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     Nome de utilizador registado no sistema.
        ///     Exibido na página para identificação do perfil.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     Mensagem de estado exibida ao utilizador após operações de alteração de perfil.
        ///     Persiste entre pedidos através do TempData.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }


        /// <summary>
        ///     Modelo de entrada de dados para o formulário de edição de perfil.
        ///     Contém campos para atualização de informações do utilizador.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     Classe que define os campos necessários para edição do perfil.
        ///     Inclui informações pessoais do utilizador que podem ser atualizadas.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     Número de telemóvel do utilizador.
            /// </summary>
            [Phone]
            [Display(Name = "Nº Telemóvel")]
            public string PhoneNumber { get; set; }

            /// <summary>
            ///     Caminho para a nova foto de perfil do utilizador.
            ///     Utilizado quando o utilizador atualiza a sua imagem.
            /// </summary>
            public string? FotoPerfilNova { get; set; }
        }


        /// <summary>
        ///     Carrega os dados do utilizador para exibição no perfil.
        /// </summary>
        /// <param name="user">O utilizador para o qual carregar os dados.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        private async Task LoadAsync(Utilizador user)
        {
            /// Obtém o nome de utilizador e o número de telefone do utilizador atual.
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            
            // Define o nome de utilizador para exibição na página.
            Username = userName;
            
            // Preenche o modelo de entrada com os dados do utilizador.
            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FotoPerfilNova = null
            };
        }

        /// <summary>
        ///     Carrega a página de perfil com os dados do utilizador atual.
        /// </summary>
        /// <returns>
        ///     Retorna a página de perfil se o utilizador for encontrado.
        ///     Retorna NotFound se o utilizador não for encontrado.
        /// </returns>
        public async Task<IActionResult> OnGetAsync()
        {
            // Obtém o utilizador atual com base no ID do utilizador autenticado.
            var user = await _userManager.GetUserAsync(User);
            // Verifica se o utilizador foi encontrado.
            if (user == null)
            {
                // Se não for encontrado, retorna uma mensagem de erro.
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            
            // Carrega os dados do utilizador para exibição na página.
            await LoadAsync(user);
            // Retorna a página de perfil com os dados carregados.
            return Page();
        }

        /// <summary>
        ///     Processa o pedido de atualização de informações de perfil.
        /// </summary>
        /// <returns>
        ///     Redireciona para a mesma página com uma mensagem de sucesso se a atualização for bem-sucedida.
        ///     Retorna a página com erros de validação se o formulário for inválido.
        /// </returns>
        public async Task<IActionResult> OnPostAsync()
        {
            // Obtém o utilizador atual com base no ID do utilizador autenticado.
            var user = await _userManager.GetUserAsync(User);
            // Verifica se o utilizador foi encontrado.
            if (user == null)
            {
                // Se não for encontrado, retorna uma mensagem de erro.
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            
            // Preenche o modelo de entrada com os dados do formulário.
            if (!ModelState.IsValid)
            {
                //  Recarrega os dados do utilizador.
                await LoadAsync(user);
                // Retorna a página com os dados do utilizador.
                return Page();
            }
            
            // Obtém o nome de utilizador atual.
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            // Verifica se o número de telefone é diferente do atual.
            if (Input.PhoneNumber != phoneNumber)
            {
                // Se o número de telefone for diferente, atualiza o modelo de entrada.
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                // Verifica se a atualização do número de telefone foi bem-sucedida.
                if (!setPhoneResult.Succeeded)
                {
                    // Se não for bem-sucedida, adiciona erros de validação ao modelo de estado.
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    // Redireciona para a mesma página com os dados do utilizador.
                    return RedirectToPage();
                }
            }
            
            // Atualiza a "sessão" do utilizador.
            await _signInManager.RefreshSignInAsync(user);
            // Mensagem de sucesso após a atualização do perfil.
            StatusMessage = "Seus dados foram atualizados com sucesso!";
            // Redireciona para a mesma página para exibir os dados atualizados.
            return RedirectToPage();
        }

        /// <summary>
        ///     Ficheiro de imagem carregado pelo utilizador para atualizar a foto de perfil.
        /// </summary>
        [BindProperty]
        public IFormFile FotoPerfil { get; set; }

        /// <summary>
        ///     Processa o carregamento de uma nova foto de perfil.
        /// </summary>
        /// <returns>
        ///     Redireciona para a página de perfil após o processamento.
        ///     Retorna NotFound se o utilizador não for encontrado.
        /// </returns>
        public async Task<IActionResult> OnPostUploadFotoPerfilAsync()
        {
            // Obtém o utilizador atual com base no ID do utilizador autenticado.
            var user = await _userManager.GetUserAsync(User);
            // Verifica se o utilizador foi encontrado.
            if (user == null) 
                // Se não for encontrado, retorna NotFound.
                return NotFound();
            
            // Verifica se o ficheiro de foto de perfil foi carregado.
            if (FotoPerfil != null)
            {
                // Se o ficheiro não for nulo, tenta guardar a imagem.
                var caminhoAntigo = user.FotoPerfil;
                
                // Guarda a imagem e obtém o nome do ficheiro.
                var nomeImagem = await GuardarImagemAsync(FotoPerfil);
                // Se o nome da imagem for válido, atualiza o perfil do utilizador.
                if (nomeImagem != null)
                {
                    // Define o caminho da nova foto de perfil.
                    user.FotoPerfil = "/fotosPerfil/" + nomeImagem;

                    // Apagar imagem antiga (se existir)
                    if (!string.IsNullOrEmpty(caminhoAntigo))
                    {
                        try
                        {
                            // Combina o diretório atual com o caminho da imagem antiga.
                            var caminhoFisico = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                                caminhoAntigo.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                            // Verifica se o ficheiro existe e apaga-o.
                            if (System.IO.File.Exists(caminhoFisico))
                            {
                                // Apaga o ficheiro da imagem antiga.
                                System.IO.File.Delete(caminhoFisico);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Se ocorrer um erro ao apagar a imagem antiga.
                            Console.WriteLine($"Erro ao apagar imagem antiga: {ex.Message}");
                        }
                    }
                }
                else
                {
                    // Se o nome da imagem for inválido, define a foto de perfil como vazia.
                    user.FotoPerfil = string.Empty;
                }
                
                // Atualiza o utilizador com a nova foto de perfil.
                await _userManager.UpdateAsync(user);
            }
            else
            {
                // Se o ficheiro de foto de perfil for nulo, define a foto de perfil como vazia.
                Console.WriteLine("Imagem nula");
                user.FotoPerfil = string.Empty;
                // Atualiza os dados utilizador.
                await _userManager.UpdateAsync(user);
            }
            
            // Redireciona para a página de perfil após o upload.
            return RedirectToPage();
        }

        /// <summary>
        ///     Remove a foto de perfil atual do utilizador.
        /// </summary>
        /// <returns>
        ///     Redireciona para a página de perfil após a remoção.
        ///     Retorna NotFound se o utilizador não for encontrado.
        /// </returns>
        public async Task<IActionResult> OnPostRemoverFotoPerfilAsync()
        {
            // Obtém o utilizador atual com base no ID do utilizador autenticado.
            var user = await _userManager.GetUserAsync(User);
            // Verifica se o utilizador foi encontrado.
            if (user == null) 
                // Se não for encontrado, retorna NotFound.
                return NotFound();
            
            // Obtém o caminho da imagem atual do perfil do utilizador.
            var caminhoImagem = user.FotoPerfil;
            
            // Verifica se o caminho da imagem não é nulo ou vazio.
            if (!string.IsNullOrEmpty(caminhoImagem))
            {
                try
                {
                    // Combina o diretório atual com o caminho da imagem para obter o caminho físico.
                    var caminhoFisico = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                        caminhoImagem.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    // Verifica se o ficheiro existe no sistema de ficheiros.
                    if (System.IO.File.Exists(caminhoFisico))
                    {
                        // Se o ficheiro existir, apaga-o.
                        System.IO.File.Delete(caminhoFisico);
                    }
                }
                catch (Exception ex)
                {
                    // Se ocorrer um erro ao apagar a imagem antiga, exibe uma mensagem de erro.
                    Console.WriteLine($"Erro ao apagar imagem antiga: {ex.Message}");
                }

                // Limpar o caminho da imagem do user
                user.FotoPerfil = string.Empty;
                // Atualiza o utilizador para remover a foto de perfil.
                await _userManager.UpdateAsync(user);
            }
            
            // Redireciona para a página de perfil após a remoção da foto.
            return RedirectToPage();
        }

        /// <summary>
        ///     Guarda a imagem do perfil no sistema de ficheiros.
        /// </summary>
        /// <param name="ficheiro">O ficheiro de imagem a ser guardado.</param>
        /// <returns>
        ///     O nome único do ficheiro se for guardado com sucesso.
        /// </returns>
        private async Task<string> GuardarImagemAsync(IFormFile ficheiro)
        {
            // Verifica se o ficheiro é nulo ou vazio.
            if (ficheiro != null && ficheiro.Length > 0)
            {
                // Obtém a extensão do ficheiro e converte para minúsculas.
                var extensao = Path.GetExtension(ficheiro.FileName).ToLower();
                // Define as extensões permitidas para imagens.
                var permitidas = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                
                // Verifica se a extensão do ficheiro é permitida e se o tipo de conteúdo é uma imagem.
                if (!permitidas.Contains(extensao)) 
                    // Se a extensão não for permitida, retorna null.
                    return null;
                
                // Verifica se o tipo de conteúdo do ficheiro começa com "image/" 
                if (!ficheiro.ContentType.StartsWith("image/")) 
                    // Se não for uma imagem, retorna null.
                    return null;
                
                // Verifica se o tamanho do ficheiro é maior que 5MB.
                if (ficheiro.Length > 5 * 1024 * 1024) 
                    // Se o tamanho for maior que 5MB, retorna null.
                    return null;
                
                // Gera um nome único para o ficheiro usando um GUID e a extensão original.
                var nomeUnico = Guid.NewGuid().ToString() + extensao;
                // Define o caminho onde as imagens serão guardadas.
                var pastaUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fotosPerfil");
                
                // Verifica se a diretoria onde vamos colocar existe.
                if (!Directory.Exists(pastaUploads))
                {
                    // Cria a diretória onde vamos colocar as imagens.
                    Directory.CreateDirectory(pastaUploads);
                }
                
                // Combina o caminho da pasta com o nome único do ficheiro.
                var caminho = Path.Combine(pastaUploads, nomeUnico);
                
                // Abre um fluxo de ficheiro para escrever o conteúdo do ficheiro carregado.
                using var stream = new FileStream(caminho, FileMode.Create);
                // Copia o conteúdo do ficheiro carregado para o fluxo de ficheiro.
                await ficheiro.CopyToAsync(stream);
                
                // Retorna o nome único do ficheiro guardado.
                return nomeUnico;
            }
            
        // Se o ficheiro for nulo ou vazio, retorna null.
            return null;
        }
    }
}