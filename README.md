# ByteHunt

## Sobre o Projeto

**ByteHunt** é uma aplicação web desenvolvida no âmbito da unidade curricular de **Desenvolvimento Web**, no ano letivo de **2024/2025**, no curso de Engenharia Informática do [Instituto Politécnico de Tomar](https://portal2.ipt.pt/pt/cursos/licenciaturas/l_-_ei/).

O principal objetivo da aplicação é oferecer uma **plataforma intuitiva e dinâmica para comparação de produtos tecnológicos**. Através de uma galeria visual e organizada, os utilizadores podem explorar diferentes itens e realizar comparações lado a lado, com **destaque automático dos melhores e piores atributos** com base em critérios definidos.

A estrutura da aplicação permite ainda a **colaboração da comunidade**, permitindo que utilizadores registados possam **submeter sugestões de novos produtos**, através de um sistema de contribuições. Estas sugestões são revistas por **moderadores**, que podem aprovar ou rejeitar conteúdos. Os **administradores** têm acesso total ao sistema, podendo gerir utilizadores, categorias, contribuições e permissões.

## 🔧 Características principais

- Comparação inteligente de atributos com base em valores quantitativos e qualitativos;
- Armazenamento dinâmico de atributos em formato JSON, facilitando a flexibilidade de dados;
- Interface moderna e responsiva com suporte para pesquisa e filtros por categoria;
- Sistema de autenticação e autorização com gestão de papéis (Utilizador, Moderador, Administrador);
- Upload de imagens e submissão de descrições para novos itens;
- Painel de administração para gestão de conteúdos e aprovações;
- **API REST pública e protegida** com suporte a autenticação JWT e documentação via **Swagger**:
  - Endpoints abertos a todos os utilizadores;
  - Endpoints reservados para utilizadores autenticados;
  - Endpoints exclusivos para moderadores e administradores.

## 🎓 Objetivos académicos

- Programação com ASP.NET Core MVC;
- Utilização de Entity Framework Core para persistência de dados;
- Implementação de autenticação com Identity e JWT;
- Criação e consumo de APIs RESTful com Swagger;
- Manipulação de dados dinâmicos via JSON;
- Separação clara entre lógica de negócio, dados e apresentação (MVC);
- Desenvolvimento de interfaces web responsivas com HTML, CSS e Bootstrap;
- Tratamento de emails com MailKit.

---

**Autores:**  
[André Benquerer](https://github.com/Benquerer) & [Diogo Larangeira](https://github.com/DLarangeira03)  
**Curso:** Licenciatura em Engenharia Informática  
**Instituição:** Instituto Politécnico de Tomar  
**Ano letivo:** 2024/2025
