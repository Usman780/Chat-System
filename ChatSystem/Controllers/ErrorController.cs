using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChatSystem.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult NotFound()
        {
            return View();
        }


        public ActionResult ChatSupportError(string msg)
        {
            ViewBag.Message = msg;
            return View();
        }

        public ActionResult ChatSupportHeaderError(string msg)
        {
            ViewBag.Message = msg;
            return View();
        }

        public ActionResult ChatSupportHeaderDefault()
        {
            return View();
        }
    }
}