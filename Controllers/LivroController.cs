using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bibliotec.Contexts;
using Bibliotec.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bibliotec_mvc.Controllers
{
    [Route("[controller]")]
    public class LivroController : Controller
    {
        private readonly ILogger<LivroController> _logger;

        public LivroController(ILogger<LivroController> logger)
        {
            _logger = logger;
        }

        Context context = new Context();

        public IActionResult Index()
        {
            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            List<Livro> listaLivros = context.Livro.ToList();

            var livrosReservados = context.LivroReserva.ToDictionary(livro => livro.LivroID, livror => livror.DtReserva);

            ViewBag.Livros = listaLivros;
            ViewBag.LivrosComReserva = livrosReservados;

            

            return View();
        }

        [Route("Cadastro")]
        //metodo que retorna a tela de cadastro:
        public IActionResult Cadastro(){

            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            ViewBag.Categorias = context.Categoria.ToList();

            //retorna a View de cadastro:
            return View();
        }

        //metodo para cadastrar um livro:
        [Route("Cadastrar")]
        public IActionResult Cadastrar(IFormCollection from){

            Livro novoLivro = new Livro();

            novoLivro.Nome = from["Nome"].ToString();
            novoLivro.Descricao = from["Descricao"].ToString();
            novoLivro.Editora = from["Editora"].ToString();
            novoLivro.Escritor = from["Escritor"].ToString();
            novoLivro.Idioma = from["Idioma"].ToString();

            if(from.Files.Count > 0){
                var arquivo = from.Files[0];

                var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Livros");
                if(Directory.Exists(pasta)){
                    Directory.CreateDirectory(pasta);
                }

                var caminho = Path.Combine(pasta, arquivo.FileName);

                using (var stream = new FileStream(caminho, FileMode.Create)){
                    arquivo.CopyTo(stream);
                }

                novoLivro.Imagem = arquivo.FileName;
            }else{
                novoLivro.Imagem = "padrao.png";
            }


            //img
            context.Livro.Add(novoLivro);

            context.SaveChanges();

            //SEGUNDA PARTE: eh adicionar dentro de LivroCategoria a cetgoria que pertence ao novoLivro
            List<LivroCategoria> listaLivroCategorias = new List<LivroCategoria>();

            //Array que possui as categorias
            string[] categoriasSelecionadas = from["Categoria"].ToString().Split(',');

            foreach(string categoria in categoriasSelecionadas){
                LivroCategoria livroCategoria = new LivroCategoria();

                livroCategoria.CategoriaID = int.Parse(categoria);
                livroCategoria.LivroID = novoLivro.LivroID;

                listaLivroCategorias.Add(livroCategoria);
            }

            context.LivroCategoria.AddRange(listaLivroCategorias);
            context.SaveChanges();

            return LocalRedirect("/Livro/Cadastro");



        }

        [Route("Editar/{id}")]
        public IActionResult Editar(int id){

            ViewBag.CategoriaDoSistema = context.Categoria.ToList()!;

            ViewBag.Admin = HttpContext.Session.GetString("Admin")!;

            Livro livroEncontrado =  context.Livro.FirstOrDefault(Livro =>Livro.LivroID == id)!;

            var categoriasDoLivroEncontrado = context.LivroCategoria.Where(identificadorLivro => identificadorLivro.LivroID == id).Select(livro => livro.Categoria).ToList();  

            ViewBag.Livro = livroEncontrado;
            ViewBag.Categoria = categoriasDoLivroEncontrado;


            return View();
        }

        [Route("Atualizar/{id}")]
        public IActionResult Atualizar (IFormCollection form,int id, IFormFile imagem){

            Livro livroAtualizado = context.Livro.FirstOrDefault(livro => livro.LivroID == id)!;

            livroAtualizado.Nome = form["Nome"];
            livroAtualizado.Escritor = form["Escritor"];
            livroAtualizado.Editora = form["Editora"];
            livroAtualizado.Idioma = form["Idioma"];
            livroAtualizado.Descricao = form["Descricao"];



        if ( imagem.Length > 0)
        {
            var caminhoImagem = Path.Combine("wwwroot/images/livros", imagem.FileName)!;

            if (!string.IsNullOrEmpty(livroAtualizado.Imagem))
            {
                var caminhoImagemAntiga = Path.Combine("wwwroot/images/livros", livroAtualizado.Imagem)!;


                if (System.IO.File.Exists(caminhoImagemAntiga))
            {
                System.IO.File.Delete(caminhoImagemAntiga);
            }


            }
            using(var stream = new FileStream(caminhoImagem, FileMode.Create))
            {
                imagem.CopyTo(stream);
            }

            livroAtualizado.Imagem = imagem.FileName;

        }

        var categoriasSelecionadas = form ["Categoria"].ToList();

        var CategoriasAtuais = context.LivroCategoria.Where(livro => livro.LivroID == id).ToList();

        foreach (var categoria in CategoriasAtuais)
        {
            if(categoriasSelecionadas.Contains(categoria.CategoriaID.ToString())){

                context.LivroCategoria.Remove(categoria);

            }
        }

        foreach (var categoria in categoriasSelecionadas)
        {
            if (!CategoriasAtuais.Any(c => c.CategoriaID.ToString() == categoria))      
            {
                context.LivroCategoria.Add(new LivroCategoria{

                LivroID = id,
                CategoriaID = int.Parse(categoria)
                    
                });
            }
        }
        context.SaveChanges();

        return LocalRedirect("/Livro");
            }

            [Route("Excluir/{id}")]
        public IActionResult Excluir(int id){
            Livro livroEncontrado = context.Livro.First(livro => livro.LivroID == id);

            var categoriasDoLivro = context.LivroCategoria.Where(livro => livro.LivroID == id).ToList();

            foreach(var categoria in categoriasDoLivro){
                context.LivroCategoria.Remove(categoria);
            }

            context.Livro.Remove(livroEncontrado);
            context.SaveChanges();



            return LocalRedirect("/Livro");
        }

        }



        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }

