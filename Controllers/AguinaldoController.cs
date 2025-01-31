using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Web;
using System.Web.Mvc;
using RHQuercus;

namespace RHQuercus.Controllers
{
    public class AguinaldoController : Controller
    {
        private RecursoHumanoQuercusEntities db = new RecursoHumanoQuercusEntities();

        public ActionResult Error()
        {
            return View("Error");
        }

        // GET: Aguinaldo
        public ActionResult Index()
        {
            var userRol = (int)Session["UserRol"];
            var userId = (int)Session["UserId"];

            if(userRol == 1)
            {
                var aguinaldo = db.Aguinaldo
                    .Include(a => a.Persona)
                    .Where(a => a.Persona.IDCedula == userId);
                    return View(aguinaldo.ToList());
            }
            if (userRol == 2 || userRol == 3)
            {
                var aguinaldo = db.Aguinaldo.Include(a => a.Persona);
                return View(aguinaldo.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }            
        }

        public ActionResult Reportes()
        {
            var userRol = (int)Session["UserRol"];
            
            if (userRol == 2 || userRol == 3)
            {
                var aguinaldo = db.Aguinaldo.Include(a => a.Persona);
                return View(aguinaldo.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // GET: Aguinaldo/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aguinaldo aguinaldo = db.Aguinaldo.Find(id);
            if (aguinaldo == null)
            {
                return HttpNotFound();
            }
            return View(aguinaldo);
        }

        // GET: Aguinaldo/Create
        public ActionResult Create()
        {
            var userRol = (int)Session["UserRol"];

            if (userRol == 2 || userRol == 3)
            {
                ViewBag.persona = from p in db.Persona.ToList()
                                  orderby p.NombrePers ascending
                                  select new
                                  {
                                      Empleado = p.IDCedula,
                                      NombrePers = p.NombrePers + " " + p.Apellidos
                                  };

                ViewBag.IDCedula = new SelectList(ViewBag.persona, "Empleado", "NombrePers");
                return View();
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // POST: Aguinaldo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDAguinaldo,IDCedula,Anno,MontoAguinaldo")] Aguinaldo aguinaldo)
        {
            if (ModelState.IsValid)
            {
                double montoPlani = 0;
                aguinaldo.Anno = DateTime.Now.Year;

                // CONSULTA PLANILLAS DEL COLABORARDOR
                var planillas = (from p in db.Persona
                                 join pla in db.Planilla
                                 on p.IDCedula equals pla.Empleado
                                 where aguinaldo.IDCedula == p.IDCedula
                                 select pla);

                var ultimasPlanillas = planillas
                    .OrderByDescending(pla => pla.FechaPlanill)
                    .Take(24)
                    .ToList();

                foreach(var pla in ultimasPlanillas)
                {
                    montoPlani += pla.Total;
                }

                // VALIDACIÓN DE REGISTRO DE AGUINALDO PARA EL PERIODO INDICADO Y USUARIO SELECCIONADO
                bool existeAguinaldo = db.Aguinaldo
                        .Include(r => r.Persona)
                        .Any(r => r.Anno == aguinaldo.Anno && r.Persona.IDCedula == aguinaldo.IDCedula);

                if (existeAguinaldo)
                {
                    TempData["AlertMessage"] = "Ya existe un registro de aguinaldo para el colaborador en este periodo, por favor revisar.";
                    ViewBag.persona = from p in db.Persona.ToList()
                                      orderby p.NombrePers ascending
                                      select new
                                      {
                                          Empleado = p.IDCedula,
                                          NombrePers = p.NombrePers + " " + p.Apellidos
                                      };

                    ViewBag.IDCedula = new SelectList(ViewBag.persona, "Empleado", "NombrePers");
                    return View(aguinaldo);
                }
                                
                aguinaldo.MontoAguinaldo = Math.Round(montoPlani / 12, 0);

                TempData["AlertMessage"] = "Aguinaldo registrado satisfactoriamente!!!";

                db.Aguinaldo.Add(aguinaldo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.persona = from p in db.Persona.ToList()
                              orderby p.NombrePers ascending
                              select new
                              {
                                  Empleado = p.IDCedula,
                                  NombrePers = p.NombrePers + " " + p.Apellidos
                              };

            ViewBag.IDCedula = new SelectList(ViewBag.persona, "Empleado", "NombrePers");
            return View(aguinaldo);
        }

        // GET: Aguinaldo/Edit/5
        public ActionResult Edit(int? id)
        {
            var userRol = (int)Session["UserRol"];

            if (userRol == 2 || userRol == 3)
            {
                if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aguinaldo aguinaldo = db.Aguinaldo.Find(id);
            if (aguinaldo == null)
            {
                return HttpNotFound();
            }
            ViewBag.IDCedula = new SelectList(db.Persona, "IDCedula", "NombrePers", aguinaldo.IDCedula);
            return View(aguinaldo);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // POST: Aguinaldo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDAguinaldo,IDCedula,Anno,MontoAguinaldo")] Aguinaldo aguinaldo)
        {
            if (ModelState.IsValid)
            {
                double montoPlani = 0;

                // CONSULTA PLANILLAS DEL COLABORARDOR
                var planillas = (from p in db.Persona
                                 join pla in db.Planilla
                                 on p.IDCedula equals pla.Empleado
                                 where aguinaldo.IDCedula == p.IDCedula
                                 select pla);

                var ultimasPlanillas = planillas
                    .OrderByDescending(pla => pla.FechaPlanill)
                    .Take(24)
                    .ToList();

                foreach (var pla in ultimasPlanillas)
                {
                    montoPlani += pla.Total;
                }

                aguinaldo.Anno = DateTime.Now.Year;
                aguinaldo.MontoAguinaldo = Math.Round(montoPlani / 12, 0);

                TempData["AlertMessage"] = "Aguinaldo actualizado satisfactoriamente!!!";

                db.Entry(aguinaldo).State = EntityState.Modified; ;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IDCedula = new SelectList(db.Persona, "IDCedula", "NombrePers", aguinaldo.IDCedula);
            return View(aguinaldo);
        }

        // GET: Aguinaldo/Delete/5
        public ActionResult Delete(int? id)
        {
            var userRol = (int)Session["UserRol"];

            if (userRol == 2 || userRol == 3)
            {
                if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Aguinaldo aguinaldo = db.Aguinaldo.Find(id);
            if (aguinaldo == null)
            {
                return HttpNotFound();
            }
            return View(aguinaldo);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // POST: Aguinaldo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Aguinaldo aguinaldo = db.Aguinaldo.Find(id);
            TempData["AlertMessage"] = "Aguinaldo eliminado satisfactoriamente!!!";

            db.Aguinaldo.Remove(aguinaldo);
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
