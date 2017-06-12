using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MurahAje.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult DetailFoto()
        {
            return View();
        }
        public IActionResult DetailHome()
        {
            return View();
        }


        public IActionResult DetailMenu()
        {
            return View();
        }
        public IActionResult DetailUlasan()
        {
            return View();
        }
        [ViewLayout("_LayoutMainNoSidebar")]
        public IActionResult Daftar()
        {
            return View();
        }
        [ViewLayout("_LayoutLogin")]
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
        public IActionResult Home()
        {
            return View();
        }
        public IActionResult AddStore()
        {
            return View();
        }
        public IActionResult DetailStore()
        {
            return View();
        }
    }
}
