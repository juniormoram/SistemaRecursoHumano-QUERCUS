using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;
using RHQuercus;

namespace RHQuercus.Controllers
{
    public class RegistroMarcasController : Controller
    {       

        private RecursoHumanoQuercusEntities db = new RecursoHumanoQuercusEntities();
        
        public ActionResult Error()
        {            
            return View();
        }

        // GET: RegistroMarcas
        public ActionResult Index()
        {
            var userRol = (int)Session["UserRol"];
            var userId = (int)Session["UserId"];

            if (userRol == 1) { 
                var registroMarca = db.RegistroMarca
                    .Include(r => r.Persona)
                    .Where(r => r.Persona.IDCedula == userId)
                    .OrderByDescending(r => r.HoraIngreso);
                    return View(registroMarca.ToList());
            }
            else if (userRol == 2 || userRol ==3)
            {
                var registroMarca = db.RegistroMarca.Include(r => r.Persona);
                return View(registroMarca.ToList());
            }
            else {
                return RedirectToAction("Error");
            }
        }

        public ActionResult Reportes()
        {
            var userRol = (int)Session["UserRol"];

            if (userRol == 2 || userRol == 3)
            {
                var registroMarca = db.RegistroMarca.Include(r => r.Persona);
                return View(registroMarca.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // GET: RegistroMarcas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RegistroMarca registroMarca = db.RegistroMarca.Find(id);
            if (registroMarca == null)
            {
                return HttpNotFound();
            }
            return View(registroMarca);
        }

        // GET: RegistroMarcas/Create
        public ActionResult Create()
        {       
            ViewBag.IDCedula = new SelectList(db.Persona, "IDCedula", "NombrePers");
            return View();
        }

        // POST: RegistroMarcas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDMarca,IDCedula,HoraIngreso,HoraSalida,Observacion")] RegistroMarca registroMarca)
        {
            if (ModelState.IsValid)
            {
                var userId = (int)Session["UserId"];
                using (var db = new RecursoHumanoQuercusEntities())
                {
                    bool existeColaborador = db.Persona
                        .Any(r => r.IDCedula == userId);

                    bool existeMarca = db.RegistroMarca
                        .Include(r => r.Persona)
                        .Any(r => DbFunctions.TruncateTime(r.HoraIngreso) == DateTime.Today && r.Persona.IDCedula == userId);

                    if (existeColaborador == false)
                    {
                        TempData["AlertMessage"] = "No existe un colaborador asignado a este usuario, por favor contacte al administrador.";
                        return View(registroMarca);
                    }
                    if (existeMarca)
                    {
                        ModelState.AddModelError("HoraIngreso", "Ya existe una marca laboral para esta jornada, por favor contactar al administrador!");
                        return View(registroMarca);
                    }
                    else
                    {
                        registroMarca.IDCedula = userId;
                        registroMarca.HoraIngreso = DateTime.Now;
                        registroMarca.HoraSalida = DateTime.Now;

                        TempData["AlertMessage"] = "Marca registrada satisfactoriamente!!!";

                        db.RegistroMarca.Add(registroMarca);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }                
            }
            ViewBag.IDCedula = new SelectList(db.Persona, "IDCedula", "NombrePers", registroMarca.IDCedula);
            return View(registroMarca);
        }

        // GET: RegistroMarcas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RegistroMarca registroMarca = db.RegistroMarca.Find(id);
            if (registroMarca == null)
            {
                return HttpNotFound();
            }
            ViewBag.IDCedula = new SelectList(db.Persona, "IDCedula", "NombrePers", registroMarca.IDCedula);
            return View(registroMarca);
        }

        // POST: RegistroMarcas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDMarca,IDCedula,HoraIngreso,HoraSalida,Observacion")] RegistroMarca registroMarca)
        {
            var userRol = (int)Session["UserRol"];

            if (ModelState.IsValid)
            {
                if (registroMarca.HoraIngreso == registroMarca.HoraSalida || userRol == 2 || userRol == 3)
                {
                registroMarca.HoraSalida = DateTime.Now;

                TempData["AlertMessage"] = "Hora de salida registrada satisfactoriamente!!!";
                db.Entry(registroMarca).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("IDCedula", "Solamente se puede registrar la hora de salida una única vez, por favor contacte al administrador.");
                    ViewBag.IDCedula = new SelectList(db.Persona, "IDCedula", "NombrePers", registroMarca.IDCedula);
                    return View(registroMarca);
                }
            }
            ViewBag.IDCedula = new SelectList(db.Persona, "IDCedula", "NombrePers", registroMarca.IDCedula);
            return View(registroMarca);
        }

        // GET: RegistroMarcas/Delete/5
        public ActionResult Delete(int? id)
        {
            var userRol = (int)Session["UserRol"];

            if (userRol == 2 || userRol == 3)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                RegistroMarca registroMarca = db.RegistroMarca.Find(id);
                if (registroMarca == null)
                {
                    return HttpNotFound();
                }
                return View(registroMarca);
            }            
            else
            {
                return RedirectToAction("Error");
            }

        }

        // POST: RegistroMarcas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RegistroMarca registroMarca = db.RegistroMarca.Find(id);
            TempData["AlertMessage"] = "Marca eliminada satisfactoriamente!!!";
            db.RegistroMarca.Remove(registroMarca);
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
