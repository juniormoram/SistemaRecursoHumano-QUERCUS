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
    public class TipoPermisoController : Controller
    {
        private RecursoHumanoQuercusEntities db = new RecursoHumanoQuercusEntities();

        public ActionResult Error()
        {
            return View();
        }

        // GET: TipoPermiso
        public ActionResult Index()
        {
            var userRol = (int)Session["UserRol"];

            if(userRol == 3)
            {
                return View(db.TipoPermiso.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }            
        }

        // GET: TipoPermiso/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TipoPermiso tipoPermiso = db.TipoPermiso.Find(id);
            if (tipoPermiso == null)
            {
                return HttpNotFound();
            }
            return View(tipoPermiso);
        }

        // POST: TipoPermiso/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDTipoPermi,Nombre")] TipoPermiso tipoPermiso)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tipoPermiso).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tipoPermiso);
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
