/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.Microservice.Models;
using Microsoft.AspNetCore.Mvc;

namespace MDD4All.SpecIF.Microservice.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(new BaseViewModel());
        }
    }
}