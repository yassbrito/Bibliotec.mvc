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

            //img
            context.Livro.Add(novoLivro);

            context.SaveChanges();

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View("Error!");
        // }
    }
}