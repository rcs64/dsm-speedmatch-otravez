using ApplicationCore.Domain.CEN;
using Infrastructure;
using Infrastructure.NHibernate;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebSpeedmatch.Models;

namespace WebSpeedmatch.Controllers
{
    public class HomeController : Controller
    {
        private UnitOfWork _uow;
        private readonly ILogger<HomeController> _logger;
        private NHibernate.ISession _session;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _session = NHibernateHelper.GetSession();
            _uow = new UnitOfWork(_session);
            UsuarioRepository usuarioRepository = new UsuarioRepository(_session);
            UsuarioCEN usuarioCEN = new UsuarioCEN(usuarioRepository, _uow);  
            IEnumerable<ApplicationCore.Domain.EN.Usuario> usuarios = usuarioRepository.GetAll();

            return View(usuarios);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
