using RecordFCS_Alt.Helpers;
using RecordFCS_Alt.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Controllers
{
    public class HomeController : BaseController
    {
        private RecordFCSContext db = new RecordFCSContext();



        public ActionResult Index(string mensaje = "")
        {
            //string FullName = User.Nombre + " " + User.Apellido;
            if (IsAuthenticated)
            {
                return View("IndexUsuario");
            }

            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                AlertaInfo(mensaje, true);
            }

            return View();
        }


        public ActionResult RedirectToLogin()
        {
            return PartialView("_RedirectToLogin");
        }


        public ActionResult MensajeModal(string mensaje = "", bool esModal = true)
        {
            if (!esModal)
            {
                AlertaDefault(string.Format(mensaje), true);
                return Json(new { success = true, esModal = esModal },JsonRequestBehavior.AllowGet);
            }

            return PartialView("_MensajeModal", mensaje);
        }

        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}
    }
}