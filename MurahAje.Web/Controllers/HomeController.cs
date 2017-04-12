﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MurahAje.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.:";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult AddStore()
        {
            ViewData["Message"] = "Add Store.";

            return View();
        }
        public IActionResult DetailStore()
        {
            ViewData["Message"] = "Detail Store.";

            return View();
        }
        public IActionResult Store()
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
    }
}
