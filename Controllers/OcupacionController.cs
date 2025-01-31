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
    public class OcupacionController : Controller
    {
        private RecursoHumanoQuercusEntities db = new RecursoHumanoQuercusEntities();

        public ActionResult Error()
        {
            return View();
        }

        // GET: Ocupacion
        public ActionResult Index()
        {
            var userRol = (int)Session["UserRol"];

            if (userRol == 2 || userRol == 3)
            {
                return View(db.Ocupacion.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }            
        }

        // GET: Ocupacion/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Ocupacion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDOcupacion,NombreOcu,Salario")] Ocupacion ocupacion)
        {
            if (ModelState.IsValid)
            {
                if (ocupacion.NombreOcu == null)
                {
                    ModelState.AddModelError("NombreOcu", "El nombre de la ocupación es obligatorio.");
                    return View(ocupacion);
                }

                TempData["AlertMessage"] = "Puesto registrado satisfactoriamente!!!";
                db.Ocupacion.Add(ocupacion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(ocupacion);
        }

        // GET: Ocupacion/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ocupacion ocupacion = db.Ocupacion.Find(id);
            if (ocupacion == null)
            {
                return HttpNotFound();
            }
            return View(ocupacion);
        }

        // POST: Ocupacion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDOcupacion,NombreOcu,Salario")] Ocupacion ocupacion)
        {
            if (ModelState.IsValid)
            {
                if (ocupacion.NombreOcu == null)
                {
                    ModelState.AddModelError("NombreOcu", "El nombre de la ocupación es obligatorio.");
                    return View(ocupacion);
                }

                TempData["AlertMessage"] = "Puesto actualizado satisfactoriamente!!!";
                db.Entry(ocupacion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ocupacion);
        }

        // GET: Ocupacion/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ocupacion ocupacion = db.Ocupacion.Find(id);
            if (ocupacion == null)
            {
                return HttpNotFound();
            }
            return View(ocupacion);
        }

        // POST: Ocupacion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ocupacion ocupacion = db.Ocupacion.Find(id);
            TempData["AlertMessage"] = "Puesto eliminado satisfactoriamente!!!";
            db.Ocupacion.Remove(ocupacion);
            db.SaveChanges();
            return RedirectToAction("Index");
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
