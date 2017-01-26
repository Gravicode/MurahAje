using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MurahAje.Web.Controllers
{
    public class ManageController : Controller
    {

        public IActionResult Register()
        {
            ViewData["Message"] = "xxx";

            return View();
        }


        public IActionResult ListUser()
        {
            ViewData["Message"] = "xxx";

            return View();
        }
        //
    }
}
