using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RHQuercus;

namespace RHQuercus.Controllers
{
    public class TipoIncapacidadController : Controller
    {
        private RecursoHumanoQuercusEntities db = new RecursoHumanoQuercusEntities();

        public ActionResult Error()
        {
            return View();
        }

        // GET: TipoIncapacidad
        public ActionResult Index()
        {
            var userRol = (int)Session["UserRol"];

            if(userRol == 3)
            {
                return View(db.TipoIncapacidad.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // GET: TipoIncapacidad/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoIncapacidad tipoIncapacidad = db.TipoIncapacidad.Find(id);
            if (tipoIncapacidad == null)
            {
                return HttpNotFound();
            }
            return View(tipoIncapacidad);
        }

        // POST: TipoIncapacidad/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDTipoInca,NombreInca")] TipoIncapacidad tipoIncapacidad)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tipoIncapacidad).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tipoIncapacidad);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
