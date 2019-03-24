/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Microsoft.AspNetCore.Mvc;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}