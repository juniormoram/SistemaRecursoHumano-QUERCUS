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
    public class PermisoLaboralController : Controller
    {
        private RecursoHumanoQuercusEntities db = new RecursoHumanoQuercusEntities();

        public ActionResult Error()
        {
            return View();
        }

        // GET: PermisoLaboral
        public ActionResult Index()
        {
            var userRol = (int)Session["UserRol"];
            var userId = (int)Session["UserId"];

            if (userRol == 1)
            {
                var permisoLaboral = db.PermisoLaboral
                    .Include(r => r.Persona)
                    .Where(r => r.Persona.IDCedula == userId);
                    return View(permisoLaboral.ToList());
            }
            else if (userRol == 2 || userRol == 3)
            {
                var permisoLaboral = db.PermisoLaboral.Include(p => p.Estado1).Include(p => p.Persona).Include(p => p.TipoPermiso1);
                return View(permisoLaboral.ToList());
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
                var permisoLaboral = db.PermisoLaboral.Include(p => p.Estado1).Include(p => p.Persona).Include(p => p.TipoPermiso1);
                return View(permisoLaboral.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // GET: PermisoLaboral/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PermisoLaboral permisoLaboral = db.PermisoLaboral.Find(id);
            if (permisoLaboral == null)
            {
                return HttpNotFound();
            }
            return View(permisoLaboral);
        }

        // GET: PermisoLaboral/Create
        public ActionResult Create()
        {
            ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud");
            ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers");
            ViewBag.TipoPermiso = new SelectList(db.TipoPermiso, "IDTipoPermi", "Nombre");
            return View();
        }

        // POST: PermisoLaboral/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDPermiso,Empleado,TipoPermiso,Estado,FechaInicio,FechaFinal,CantDias,Observacion,PagoObligatorio")] PermisoLaboral permisoLaboral)
        {
            if (ModelState.IsValid)
            {
                var userId = (int)Session["UserId"];

                bool existeColaborador = db.Persona
                        .Any(r => r.IDCedula == userId);

                if (existeColaborador == false)
                {
                    TempData["AlertMessage"] = "No existe un colaborador asignado a este usuario, por favor contacte al administrador.";
                    ViewBag.TipoPermiso = new SelectList(db.TipoPermiso, "IDTipoPermi", "Nombre");
                    return View(permisoLaboral);
                }

                if (permisoLaboral.FechaInicio < permisoLaboral.FechaFinal)
                {
                    permisoLaboral.Empleado = userId;
                    permisoLaboral.Estado = 2;
                    permisoLaboral.PagoObligatorio = true;
                }
                else
                {
                    ModelState.AddModelError("FechaInicio", "La fecha inicial debe encontrarse antes de la fecha final del permiso.");
                    ViewBag.TipoPermiso = new SelectList(db.TipoPermiso, "IDTipoPermi", "Nombre");
                    return View(permisoLaboral);
                }

                TempData["AlertMessage"] = "Permiso registrado satisfactoriamente!!!";
                db.PermisoLaboral.Add(permisoLaboral);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud", permisoLaboral.Estado);
            ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers", permisoLaboral.Empleado);
            ViewBag.TipoPermiso = new SelectList(db.TipoPermiso, "IDTipoPermi", "Nombre", permisoLaboral.TipoPermiso);
            return View(permisoLaboral);
        }

        // GET: PermisoLaboral/Edit/5
        public ActionResult Edit(int? id)
        {
            var userRol = (int)Session["UserRol"];

            if (userRol == 2 || userRol == 3)
            {       
                if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PermisoLaboral permisoLaboral = db.PermisoLaboral.Find(id);
            if (permisoLaboral == null)
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

                ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud", permisoLaboral.Estado);
                ViewBag.Empleado = new SelectList(@ViewBag.prueba, "Empleado", "NombrePers");
                ViewBag.TipoPermiso = new SelectList(db.TipoPermiso, "IDTipoPermi", "Nombre", permisoLaboral.TipoPermiso);
            return View(permisoLaboral);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // POST: PermisoLaboral/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDPermiso,Empleado,TipoPermiso,Estado,FechaInicio,FechaFinal,CantDias,Observacion,PagoObligatorio")] PermisoLaboral permisoLaboral)
        {
            if (ModelState.IsValid)
            {
                TempData["AlertMessage"] = "Permiso actualizado satisfactoriamente!!!";
                db.Entry(permisoLaboral).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud", permisoLaboral.Estado);
            ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers", permisoLaboral.Empleado);
            ViewBag.TipoPermiso = new SelectList(db.TipoPermiso, "IDTipoPermi", "Nombre", permisoLaboral.TipoPermiso);
            return View(permisoLaboral);
        }

        // GET: PermisoLaboral/Delete/5
        public ActionResult Delete(int? id)
        {
            var userRol = (int)Session["UserRol"];

            if (userRol == 2 || userRol == 3)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                PermisoLaboral permisoLaboral = db.PermisoLaboral.Find(id);
                if (permisoLaboral == null)
                {
                    return HttpNotFound();
                }
                return View(permisoLaboral);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // POST: PermisoLaboral/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PermisoLaboral permisoLaboral = db.PermisoLaboral.Find(id);
            TempData["AlertMessage"] = "Permiso eliminado satisfactoriamente!!!";

            db.PermisoLaboral.Remove(permisoLaboral);
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
