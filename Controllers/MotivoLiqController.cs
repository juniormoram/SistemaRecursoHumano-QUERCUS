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
    public class MotivoLiqController : Controller
    {
        private RecursoHumanoQuercusEntities db = new RecursoHumanoQuercusEntities();

        public ActionResult Error()
        {
            return View();
        }

        // GET: MotivoLiq
        public ActionResult Index()
        {
            var userRol = (int)Session["UserRol"];

            if (userRol == 3)
            {
                return View(db.MotivoLiq.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }           
        }

        // GET: MotivoLiq/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MotivoLiq motivoLiq = db.MotivoLiq.Find(id);
            if (motivoLiq == null)
            {
                return HttpNotFound();
            }
            return View(motivoLiq);
        }

        // POST: MotivoLiq/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDMotivo,Nombre")] MotivoLiq motivoLiq)
        {
            if (ModelState.IsValid)
            {
                db.Entry(motivoLiq).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(motivoLiq);
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
