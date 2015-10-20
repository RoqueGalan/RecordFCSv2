using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RecordFCS_Alt.Controllers
{
    public class ListaPiezaController : Controller
    {
        // GET: ListaPieza
        public ActionResult RenderPieza()
        {
            return View();
        }
    }
}