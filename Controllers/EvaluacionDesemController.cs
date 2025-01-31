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
    public class EvaluacionDesemController : Controller
    {
        private RecursoHumanoQuercusEntities db = new RecursoHumanoQuercusEntities();

        public ActionResult Error()
        {
            return View();
        }

        // GET: EvaluacionDesem
        public ActionResult Index()
        {
            var userRol = (int)Session["UserRol"];
            var userId = (int)Session["UserId"];

            if(userRol == 1) {
                var evaluacionDesem = db.EvaluacionDesem
                    .Include(e => e.Persona)
                    .Where(e => e.Persona.IDCedula == userId);
                     return View(evaluacionDesem.ToList());
            }
            else if(userRol == 2 || userRol == 3)
            {
                var evaluacionDesem = db.EvaluacionDesem.Include(e => e.Persona);
                return View(evaluacionDesem.ToList());
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
                var evaluacionDesem = db.EvaluacionDesem.Include(e => e.Persona);
                return View(evaluacionDesem.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // GET: EvaluacionDesem/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EvaluacionDesem evaluacionDesem = db.EvaluacionDesem.Find(id);
            if (evaluacionDesem == null)
            {
                return HttpNotFound();
            }
            return View(evaluacionDesem);
        }

        // GET: EvaluacionDesem/Create
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
                ViewBag.Empleado = new SelectList(@ViewBag.persona, "Empleado", "NombrePers");
                return View();
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // POST: EvaluacionDesem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDEvaluacion,Empleado,FechaEva,Pregunta1,Calificacion1,Pregunta2,Calificacion2,Pregunta3,Calificacion3,Pregunta4,Calificacion4,Pregunta5,Calificacion5,CalificacionFinal,Observaciones")] EvaluacionDesem evaluacionDesem)
        {            
            if (ModelState.IsValid)
            {
                if (evaluacionDesem.Calificacion1 < 1 || evaluacionDesem.Calificacion1 > 5)
                {
                    ModelState.AddModelError("Calificacion1", "La calificación debe ser entre 1 y 5.");
                    ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers");
                    return View(evaluacionDesem);
                }
                else if (evaluacionDesem.Calificacion2 < 1 || evaluacionDesem.Calificacion2 > 5)
                {
                    ModelState.AddModelError("Calificacion2", "La calificación debe ser entre 1 y 5.");
                    ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers");
                    return View(evaluacionDesem);
                }
                else if (evaluacionDesem.Calificacion3 < 1 || evaluacionDesem.Calificacion3 > 5)
                {
                    ModelState.AddModelError("Calificacion3", "La calificación debe ser entre 1 y 5.");
                    ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers");
                    return View(evaluacionDesem);
                }
                else if (evaluacionDesem.Calificacion4 < 1 || evaluacionDesem.Calificacion4 > 5)
                {
                    ModelState.AddModelError("Calificacion4", "La calificación debe ser entre 1 y 5.");
                    ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers");
                    return View(evaluacionDesem);
                }
                else if (evaluacionDesem.Calificacion5 < 1 || evaluacionDesem.Calificacion5 > 5)
                {
                    ModelState.AddModelError("Calificacion5", "La calificación debe ser entre 1 y 5.");
                    ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers");
                    return View(evaluacionDesem);
                }
                else { 
                evaluacionDesem.Pregunta1 = "1. ¿El colaborador es puntual?";
                evaluacionDesem.Pregunta2 = "2. ¿El colaborador es comunicativo ante cualquier situación?";
                evaluacionDesem.Pregunta3 = "3. ¿El colaborador muestra un comportamiento amable y respetuoso?";
                evaluacionDesem.Pregunta4 = "4. ¿El colaborador muestra iniciativa ante cualquier labor que se realice?";
                evaluacionDesem.Pregunta5 = "5. ¿Qué tan satisfecho se siente el colaborador en su trabajo diario?";
                evaluacionDesem.CalificacionFinal = (evaluacionDesem.Calificacion1 + evaluacionDesem.Calificacion2 + evaluacionDesem.Calificacion3 + evaluacionDesem.Calificacion4 + evaluacionDesem.Calificacion5) * 4;
                evaluacionDesem.FechaEva = DateTime.Today;

                TempData["AlertMessage"] = "Proceso registrado satisfactoriamente!!!";

                db.EvaluacionDesem.Add(evaluacionDesem);
                db.SaveChanges();
                return RedirectToAction("Index");
                }
            }                        
            ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers", evaluacionDesem.Empleado);
            return View(evaluacionDesem);
        }

        // GET: EvaluacionDesem/Edit/5
        public ActionResult Edit(int? id)
        {
            var userRol = (int)Session["UserRol"];

            if (userRol == 2 || userRol == 3)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                EvaluacionDesem evaluacionDesem = db.EvaluacionDesem.Find(id);
                if (evaluacionDesem == null)
                {
                    return HttpNotFound();
                }
                ViewBag.prueba = from p in db.Persona.ToList()
                                 orderby p.NombrePers ascending
                                 select new
                                 {
                                     Empleado = p.IDCedula,
                                     NombrePers = p.NombrePers + " " + p.Apellidos
                                 };
                ViewBag.Empleado = new SelectList(@ViewBag.prueba, "Empleado", "NombrePers");
                return View(evaluacionDesem);
            }
            else
            {
                return RedirectToAction("Error");
            }            
        }

        // POST: EvaluacionDesem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDEvaluacion,Empleado,FechaEva,Pregunta1,Calificacion1,Pregunta2,Calificacion2,Pregunta3,Calificacion3,Pregunta4,Calificacion4,Pregunta5,Calificacion5,CalificacionFinal,Observaciones")] EvaluacionDesem evaluacionDesem)
        {
            if (ModelState.IsValid)
            {
                if (evaluacionDesem.Calificacion1 < 1 || evaluacionDesem.Calificacion1 > 5)
                {
                    ModelState.AddModelError("Calificacion1", "La calificación debe ser entre 1 y 5.");
                    ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers");
                    return View(evaluacionDesem);
                }
                else if (evaluacionDesem.Calificacion2 < 1 || evaluacionDesem.Calificacion2 > 5)
                {
                    ModelState.AddModelError("Calificacion2", "La calificación debe ser entre 1 y 5.");
                    ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers");
                    return View(evaluacionDesem);
                }
                else if (evaluacionDesem.Calificacion3 < 1 || evaluacionDesem.Calificacion3 > 5)
                {
                    ModelState.AddModelError("Calificacion3", "La calificación debe ser entre 1 y 5.");
                    ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers");
                    return View(evaluacionDesem);
                }
                else if (evaluacionDesem.Calificacion4 < 1 || evaluacionDesem.Calificacion4 > 5)
                {
                    ModelState.AddModelError("Calificacion4", "La calificación debe ser entre 1 y 5.");
                    ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers");
                    return View(evaluacionDesem);
                }
                else if (evaluacionDesem.Calificacion5 < 1 || evaluacionDesem.Calificacion5 > 5)
                {
                    ModelState.AddModelError("Calificacion5", "La calificación debe ser entre 1 y 5.");
                    ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers");
                    return View(evaluacionDesem);
                }
                else
                {
                    evaluacionDesem.Pregunta1 = "1. ¿El colaborador es puntual?";
                    evaluacionDesem.Pregunta2 = "2. ¿El colaborador es comunicativo ante cualquier situación?";
                    evaluacionDesem.Pregunta3 = "3. ¿El colaborador muestra un comportamiento amable y respetuoso?";
                    evaluacionDesem.Pregunta4 = "4. ¿El colaborador muestra iniciativa ante cualquier labor que se realice?";
                    evaluacionDesem.Pregunta5 = "5. ¿Qué tan satisfecho se siente el colaborador en su trabajo diario?";
                    evaluacionDesem.CalificacionFinal = (evaluacionDesem.Calificacion1 + evaluacionDesem.Calificacion2 + evaluacionDesem.Calificacion3 + evaluacionDesem.Calificacion4 + evaluacionDesem.Calificacion5) * 4;
                    evaluacionDesem.FechaEva = DateTime.Today;

                    TempData["AlertMessage"] = "Registro actualizado satisfactoriamente!!!";

                    db.Entry(evaluacionDesem).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers", evaluacionDesem.Empleado);
            return View(evaluacionDesem);
        }

        // GET: EvaluacionDesem/Delete/5
        public ActionResult Delete(int? id)
        {
            var userRol = (int)Session["UserRol"];

            if (userRol == 2 || userRol == 3)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                EvaluacionDesem evaluacionDesem = db.EvaluacionDesem.Find(id);
                if (evaluacionDesem == null)
                {
                    return HttpNotFound();
                }
                return View(evaluacionDesem);
            }
            else
            {
                return RedirectToAction("Error");
            }            
        }

        // POST: EvaluacionDesem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            EvaluacionDesem evaluacionDesem = db.EvaluacionDesem.Find(id);

            TempData["AlertMessage"] = "Registro eliminado satisfactoriamente!!!";

            db.EvaluacionDesem.Remove(evaluacionDesem);
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
