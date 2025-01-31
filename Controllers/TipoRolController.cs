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
    public class TipoRolController : Controller
    {
        private RecursoHumanoQuercusEntities db = new RecursoHumanoQuercusEntities();

        public ActionResult Error()
        {
            return View();
        }

        // GET: TipoRol
        public ActionResult Index()
        {
            var userRol = (int)Session["UserRol"];

            if(userRol == 3)
            {
                return View(db.TipoRol.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }            
        }

        // GET: TipoRol/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoRol tipoRol = db.TipoRol.Find(id);
            if (tipoRol == null)
            {
                return HttpNotFound();
            }
            return View(tipoRol);
        }

        // POST: TipoRol/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDRol,NombreRol")] TipoRol tipoRol)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tipoRol).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tipoRol);
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
