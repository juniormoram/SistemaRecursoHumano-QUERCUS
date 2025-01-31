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
    public class PersonasController : Controller
    {
        private RecursoHumanoQuercusEntities db = new RecursoHumanoQuercusEntities();

        public ActionResult Error()
        {
            return View();
        }

        // GET: Personas
        public ActionResult Index()
        {
            var userRol = (int)Session["UserRol"];

            if(userRol == 2 || userRol == 3)
            {
                var persona = db.Persona.Include(p => p.Ocupacion1);
                return View(persona.ToList());
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
                var persona = db.Persona.Include(p => p.Ocupacion1);
                return View(persona.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // GET: Personas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Persona persona = db.Persona.Find(id);
            if (persona == null)
            {
                return HttpNotFound();
            }
            return View(persona);
        }

        // GET: Personas/Create
        public ActionResult Create()
        {
            ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu");
            return View();
        }

        // POST: Personas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDCedula,NombrePers,Apellidos,Direccion,Celular,Correo,Estado,Ocupacion,FechaIngreso,CantVacaciones,NombreContacto,ParentescoContacto,CelularContacto")] Persona persona)
        {
            if (ModelState.IsValid)
            {
                if (db.Persona.Any(u => u.IDCedula == persona.IDCedula))
                {
                    ModelState.AddModelError("IDCedula", "El número de cédula ingresado ya existe en el sistema.");
                    ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu");
                    return View(persona);
                }

                if (persona.IDCedula < 100000000)
                {
                    ModelState.AddModelError("IDCedula", "La identificación debe contener al menos 9 dígitos.");
                    ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu");
                    return View(persona);
                }

                if (persona.Celular < 50000000)
                {
                    ModelState.AddModelError("Celular", "El número celular debe contener al menos 8 dígitos.");
                    ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu");
                    return View(persona);
                }

                if (persona.NombrePers == null)
                {
                    ModelState.AddModelError("NombrePers", "El campo Nombre es obligatorio.");
                    ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu");
                    return View(persona);
                }

                if (persona.Apellidos == null)
                {
                    ModelState.AddModelError("Apellidos", "El campo Apellidos es obligatorio.");
                    ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu");
                    return View(persona);
                }

                if (persona.Direccion == null)
                {
                    ModelState.AddModelError("Direccion", "El campo Dirección es obligatorio.");
                    ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu");
                    return View(persona);
                }

                if (persona.Correo == null)
                {
                    ModelState.AddModelError("Correo", "El campo Correo es obligatorio.");
                    ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu");
                    return View(persona);
                }                

                persona.CantVacaciones = 0;
                TempData["AlertMessage"] = "Colaborador registrado satisfactoriamente!!!";
                db.Persona.Add(persona);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu", persona.Ocupacion);
            return View(persona);
        }

        // GET: Personas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Persona persona = db.Persona.Find(id);
            if (persona == null)
            {
                return HttpNotFound();
            }
            ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu", persona.Ocupacion);
            return View(persona);
        }

        // POST: Personas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDCedula,NombrePers,Apellidos,Direccion,Celular,Correo,Estado,Ocupacion,FechaIngreso,CantVacaciones,NombreContacto,ParentescoContacto,CelularContacto")] Persona persona)
        {
            if (ModelState.IsValid)
            {                

                if (persona.IDCedula < 100000000)
                {
                    ModelState.AddModelError("IDCedula", "La identificación debe contener al menos 9 dígitos.");
                    ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu");
                    return View(persona);
                }

                if (persona.Celular < 50000000)
                {
                    ModelState.AddModelError("Celular", "El número celular debe contener al menos 8 dígitos.");
                    ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu");
                    return View(persona);
                }

                if (persona.NombrePers == null)
                {
                    ModelState.AddModelError("NombrePers", "El campo Nombre es obligatorio.");
                    ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu");
                    return View(persona);
                }

                if (persona.Apellidos == null)
                {
                    ModelState.AddModelError("Apellidos", "El campo Apellidos es obligatorio.");
                    ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu");
                    return View(persona);
                }

                if (persona.Direccion == null)
                {
                    ModelState.AddModelError("Direccion", "El campo Dirección es obligatorio.");
                    ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu");
                    return View(persona);
                }

                if (persona.Correo == null)
                {
                    ModelState.AddModelError("Correo", "El campo Correo es obligatorio.");
                    ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu");
                    return View(persona);
                }

                TempData["AlertMessage"] = "Colaborador actualizado satisfactoriamente!!!";
                db.Entry(persona).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Ocupacion = new SelectList(db.Ocupacion, "IDOcupacion", "NombreOcu", persona.Ocupacion);
            return View(persona);
        }

        // GET: Personas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Persona persona = db.Persona.Find(id);
            if (persona == null)
            {
                return HttpNotFound();
            }
            return View(persona);
        }

        // POST: Personas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Persona persona = db.Persona.Find(id);
            TempData["AlertMessage"] = "Colaborador eliminado satisfactoriamente!!!";
            db.Persona.Remove(persona);
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
