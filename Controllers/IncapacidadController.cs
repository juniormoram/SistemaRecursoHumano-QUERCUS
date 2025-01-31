using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using RHQuercus;

namespace RHQuercus.Controllers
{
    public class IncapacidadController : Controller
    {
        private RecursoHumanoQuercusEntities db = new RecursoHumanoQuercusEntities();

        public ActionResult Error()
        {
            return View();
        }

        // GET: Incapacidad
        public ActionResult Index()
        {
            var userRol = (int)Session["UserRol"];
            var userId = (int)Session["UserId"];

            if (userRol == 1)
            {
                var incapacidad = db.Incapacidad
                    .Include(e => e.Persona)
                    .Where(e => e.Persona.IDCedula == userId);
                    return View(incapacidad.ToList());
            }
            if (userRol == 2 || userRol == 3)
            {
                var incapacidad = db.Incapacidad.Include(i => i.Estado1).Include(i => i.Persona).Include(i => i.TipoIncapacidad1);
                return View(incapacidad.ToList());
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
                var incapacidad = db.Incapacidad.Include(i => i.Estado1).Include(i => i.Persona).Include(i => i.TipoIncapacidad1);
                return View(incapacidad.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // GET: Incapacidad/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Incapacidad incapacidad = db.Incapacidad.Find(id);
            if (incapacidad == null)
            {
                return HttpNotFound();
            }
            return View(incapacidad);
        }

        // GET: Incapacidad/Create
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

                ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud");
                ViewBag.Empleado = new SelectList(ViewBag.persona, "Empleado", "NombrePers");
                ViewBag.TipoIncapacidad = new SelectList(db.TipoIncapacidad, "IDTipoInca", "NombreInca");
                return View();
            }
            else
            {
                return RedirectToAction("Error");
            }
            
        }

        // POST: Incapacidad/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDIncapacidad,Empleado,TipoIncapacidad,Estado,CantDias,FechaInicio,FechaFinal,MontoEmpresa,MontoInca,MontoIncaTotal")] Incapacidad incapacidad)
        {
            if (ModelState.IsValid)
            {
                if (incapacidad.FechaInicio < incapacidad.FechaFinal)
                {
                    // CONSULTA SALARIO DEL COLABORARDOR
                    double salario = (from p in db.Persona
                                      join o in db.Ocupacion
                                      on p.Ocupacion equals o.IDOcupacion
                                      where incapacidad.Empleado == p.IDCedula
                                      select o.Salario).FirstOrDefault();

                    // SALARIO DIARIO
                    double salarioDiario = salario / 20;

                    // MONTO INCAPACIDAD POR 3 O MENOS DIAS
                    if (incapacidad.CantDias <= 3)
                    {
                        salarioDiario = 0.60 * salarioDiario;

                        incapacidad.Estado = 2;
                        incapacidad.MontoEmpresa = salarioDiario * incapacidad.CantDias / 2;
                        incapacidad.MontoInca = salarioDiario * incapacidad.CantDias / 2;
                        incapacidad.MontoIncaTotal = incapacidad.MontoEmpresa + incapacidad.MontoInca;
                    }
                    // MONTO INCAPACIDAD MAYOR A 3 DIAS
                    if (incapacidad.CantDias >= 4)
                    {
                        double dias4 = incapacidad.CantDias - 3;
                        double monto3 = 0;
                        double monto4Empre = 0;
                        double monto4Inca = 0;
                        salarioDiario = 0.60 * salarioDiario;

                        // MONTOS INCAPACIDAD SEGUN PORCENTAJES QUE ASUME CCCSS, INS Y PATRONO SEGUN CANTIDAD DE DIAS
                        monto3 = salarioDiario * 3 / 2;
                        monto4Empre = salarioDiario * dias4 * 0.40;
                        monto4Inca = salarioDiario * dias4 * 0.60;

                        incapacidad.Estado = 2;
                        incapacidad.MontoEmpresa = monto3 + monto4Empre;
                        incapacidad.MontoInca = monto3 + monto4Inca;
                        incapacidad.MontoIncaTotal = incapacidad.MontoEmpresa + incapacidad.MontoInca;
                    }
                }
                else
                {
                    ModelState.AddModelError("FechaInicio", "La fecha inicial debe encontrarse antes de la fecha final de la incapacidad.");                   
                    
                    ViewBag.persona = from p in db.Persona.ToList()
                                      orderby p.NombrePers ascending
                                      select new
                                      {
                                          Empleado = p.IDCedula,
                                          NombrePers = p.NombrePers + " " + p.Apellidos
                                      };

                    ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud");
                    ViewBag.Empleado = new SelectList(ViewBag.persona, "Empleado", "NombrePers");
                    ViewBag.TipoIncapacidad = new SelectList(db.TipoIncapacidad, "IDTipoInca", "NombreInca");
                    return View(incapacidad);
                }

                TempData["AlertMessage"] = "Incapacidad registrada satisfactoriamente!!!";

                db.Incapacidad.Add(incapacidad);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud", incapacidad.Estado);
            ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers", incapacidad.Empleado);
            ViewBag.TipoIncapacidad = new SelectList(db.TipoIncapacidad, "IDTipoInca", "NombreInca", incapacidad.TipoIncapacidad);
            return View(incapacidad);
        }

        // GET: Incapacidad/Edit/5
        public ActionResult Edit(int? id)
        {
            var userRol = (int)Session["UserRol"];

            if(userRol == 2 || userRol == 3) { 

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Incapacidad incapacidad = db.Incapacidad.Find(id);
                if (incapacidad == null)
                {
                    return HttpNotFound();
                }
                ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud", incapacidad.Estado);
                ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers", incapacidad.Empleado);
                ViewBag.TipoIncapacidad = new SelectList(db.TipoIncapacidad, "IDTipoInca", "NombreInca", incapacidad.TipoIncapacidad);
                return View(incapacidad);
            }
            else
            {
                    return RedirectToAction("Error");
            }
        }

        // POST: Incapacidad/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDIncapacidad,Empleado,TipoIncapacidad,Estado,CantDias,FechaInicio,FechaFinal,MontoEmpresa,MontoInca,MontoIncaTotal")] Incapacidad incapacidad)
        {
            if (ModelState.IsValid)
            {
                // CONSULTA SALARIO DEL COLABORARDOR
                double salario = (from p in db.Persona
                                  join o in db.Ocupacion
                                  on p.Ocupacion equals o.IDOcupacion
                                  where incapacidad.Empleado == p.IDCedula
                                  select o.Salario).FirstOrDefault();

                // SALARIO DIARIO
                double salarioDiario = salario / 20;

                // MONTO INCAPACIDAD POR 3 O MENOS DIAS
                if (incapacidad.CantDias <= 3)
                {
                    salarioDiario = 0.60 * salarioDiario;

                    incapacidad.MontoEmpresa = salarioDiario * incapacidad.CantDias / 2;
                    incapacidad.MontoInca = salarioDiario * incapacidad.CantDias / 2;
                    incapacidad.MontoIncaTotal = incapacidad.MontoEmpresa + incapacidad.MontoInca;
                }
                // MONTO INCAPACIDAD POR MAYOR A 4 DIAS
                if (incapacidad.CantDias >= 4)
                {
                    double dias4 = incapacidad.CantDias - 3;
                    double monto3 = 0;
                    double monto4Empre = 0;
                    double monto4Inca = 0;
                    salarioDiario = 0.60 * salarioDiario;

                    // MONTOS INCAPACIDAD SEGUN PORCENTAJES QUE ASUME CCCSS, INS Y PATRONO SEGUN CANTIDAD DE DIAS
                    monto3 = salarioDiario * 3 / 2;
                    monto4Empre = salarioDiario * dias4 * 0.40;
                    monto4Inca = salarioDiario * dias4 * 0.60;

                    incapacidad.MontoEmpresa = monto3 + monto4Empre;
                    incapacidad.MontoInca = monto3 + monto4Inca;
                    incapacidad.MontoIncaTotal = incapacidad.MontoEmpresa + incapacidad.MontoInca;
                }

                TempData["AlertMessage"] = "Incapacidad actualizada satisfactoriamente!!!";

                db.Entry(incapacidad).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud", incapacidad.Estado);
            ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers", incapacidad.Empleado);
            ViewBag.TipoIncapacidad = new SelectList(db.TipoIncapacidad, "IDTipoInca", "NombreInca", incapacidad.TipoIncapacidad);
            return View(incapacidad);
        }

        // GET: Incapacidad/Delete/5
        public ActionResult Delete(int? id)
        {
            var userRol = (int)Session["UserRol"];

            if(userRol == 2 || userRol == 3) 
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Incapacidad incapacidad = db.Incapacidad.Find(id);
                if (incapacidad == null)
                {
                    return HttpNotFound();
                }
                return View(incapacidad);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // POST: Incapacidad/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Incapacidad incapacidad = db.Incapacidad.Find(id);
            TempData["AlertMessage"] = "incapacidad eliminada satisfactoriamente!!!";

            db.Incapacidad.Remove(incapacidad);
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
