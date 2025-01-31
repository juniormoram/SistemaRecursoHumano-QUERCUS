using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RHQuercus;

namespace RHQuercus.Controllers
{
    public class VacacionesController : Controller
    {
        private RecursoHumanoQuercusEntities db = new RecursoHumanoQuercusEntities();

        public ActionResult Error()
        {
            return View();
        }

        // GET: Vacaciones
        public ActionResult Index()
        {
            var userRol = (int)Session["UserRol"];
            var userId = (int)Session["UserId"];

            if (userRol == 1)
            {
                var vacaciones = db.Vacaciones
                    .Include(r => r.Persona)
                    .Where(r => r.Persona.IDCedula == userId);
                return View(vacaciones.ToList());
            }
            else if (userRol == 2 || userRol == 3)
            {
                var vacaciones = db.Vacaciones.Include(v => v.Estado1).Include(v => v.Persona);
                return View(vacaciones.ToList());
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
                var vacaciones = db.Vacaciones.Include(v => v.Estado1).Include(v => v.Persona);
                return View(vacaciones.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // GET: Vacaciones/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vacaciones vacaciones = db.Vacaciones.Find(id);
            if (vacaciones == null)
            {
                return HttpNotFound();
            }
            return View(vacaciones);
        }

        // GET: Vacaciones/Create
        public ActionResult Create()
        {
            var userId = (int)Session["UserId"];

            var CantVacas = (from p in db.Persona                             
                             where userId == p.IDCedula
                             select p.CantVacaciones).FirstOrDefault();

            TempData["CantVacas"] = CantVacas;

            ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud");
            ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers");
            return View();
        }

        // POST: Vacaciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDVacas,Empleado,Estado,CantDias,FechaInicio,FechaFinal,Observacion")] Vacaciones vacaciones)
        {
            if (ModelState.IsValid)
            {
                var userId = (int)Session["UserId"];

                var CantVacas = (from p in db.Persona
                                 where userId == p.IDCedula
                                 select p.CantVacaciones).FirstOrDefault();

                TempData["CantVacas"] = CantVacas;

                bool existeColaborador = db.Persona
                        .Any(r => r.IDCedula == userId);

                if (existeColaborador == false)
                {
                    TempData["AlertMessage"] = "No existe un colaborador asignado a este usuario, por favor contacte al administrador.";
                    return View(vacaciones);
                }

                // VALIDACION DE FECHAS DE LA SOLICITUD DE VACACIONES
                if (vacaciones.FechaInicio < vacaciones.FechaFinal)
                {
                    vacaciones.Empleado = userId;
                    vacaciones.Estado = 2;
                }
                else
                {
                    ModelState.AddModelError("FechaInicio", "La fecha inicial debe encontrarse antes de la fecha final de la solicitud de vacaciones.");
                    return View(vacaciones);
                }

                // CONSULTA CANTIDAD DE VACACIONES DEL COLABORADOR PARA RESTAR VACACIONES SEGUN SOLICITUD
                var CantVacasDis = (from p in db.Persona
                                 where vacaciones.Empleado == p.IDCedula
                                 select p.CantVacaciones).FirstOrDefault();

                // VALIDACION CANTIDAD DE DIAS DE VACACIONES DISPONIBLES Y CANTIDAD DE DIAS SOLICITADOS
                if (vacaciones.CantDias > CantVacasDis)
                {
                    ModelState.AddModelError("CantDias", "No cuenta con suficientes días de vacaciones, por favor consulte al administrador.");
                    return View(vacaciones);
                }

                TempData["AlertMessage"] = "Solicitud de vacaciones registrada satisfactoriamente!!!";
                db.Vacaciones.Add(vacaciones);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud", vacaciones.Estado);
            ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers", vacaciones.Empleado);
            return View(vacaciones);
        }

        // GET: Vacaciones/Edit/5
        public ActionResult Edit(int? id)
        {
            var userRol = (int)Session["UserRol"];

            if (userRol == 2 || userRol == 3)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Vacaciones vacaciones = db.Vacaciones.Find(id);
                if (vacaciones == null)
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

                ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud", vacaciones.Estado);
                ViewBag.Empleado = new SelectList(@ViewBag.prueba, "Empleado", "NombrePers");
                return View(vacaciones);
            }
            else
            {
                return RedirectToAction("Error");
            }            
        }

        // POST: Vacaciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDVacas,Empleado,Estado,CantDias,FechaInicio,FechaFinal,Observacion")] Vacaciones vacaciones)
        {
            if (ModelState.IsValid)
            {
                if (vacaciones.Estado == 1)
                {
                    // CONSULTA CANTIDAD DE VACACIONES DEL COLABORADOR PARA RESTAR VACACIONES SEGUN SOLICITUD
                    var CantVacas = (from p in db.Persona
                                     where vacaciones.Empleado == p.IDCedula
                                     select p.CantVacaciones).FirstOrDefault();
                    CantVacas = CantVacas - vacaciones.CantDias;

                    // EJECUCION DE PA PARA ACTUALIZAR LA CANTIDAD DE VACACIONES DEL COLABORADOR
                    var Cedula = new SqlParameter("@IDCedula", vacaciones.Empleado);
                    var Vacas = new SqlParameter("@CantVacas", CantVacas);
                    db.Database.ExecuteSqlCommand("EXEC sp_ActualizarVacaciones @IDCedula, @CantVacas", Cedula, Vacas);
                }

                TempData["AlertMessage"] = "Solicitud actualizada satisfactoriamente!!!";
                db.Entry(vacaciones).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud", vacaciones.Estado);
            ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers", vacaciones.Empleado);
            return View(vacaciones);
        }

        // GET: Vacaciones/Delete/5
        public ActionResult Delete(int? id)
        {
            var userRol = (int)Session["UserRol"];

            if (userRol == 2 || userRol == 3)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Vacaciones vacaciones = db.Vacaciones.Find(id);
                if (vacaciones == null)
                {
                    return HttpNotFound();
                }
                return View(vacaciones);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // POST: Vacaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Vacaciones vacaciones = db.Vacaciones.Find(id);
            TempData["AlertMessage"] = "Solicitud eliminada satisfactoriamente!!!";
            db.Vacaciones.Remove(vacaciones);
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
