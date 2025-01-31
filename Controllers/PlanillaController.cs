using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages.Html;
using RHQuercus;
using SelectListItem = System.Web.WebPages.Html.SelectListItem;

namespace RHQuercus.Controllers
{
    public class PlanillaController : Controller   {        

        private RecursoHumanoQuercusEntities db = new RecursoHumanoQuercusEntities();

        public ActionResult Error()
        {
            return View (); 
        }

        // GET: Planilla
        public ActionResult Index()
        {
            var userId = (int)Session["userId"];
            var userRol = (int)Session["userRol"];

            if (userRol == 2 || userRol == 3)
            {
                var planilla = db.Planilla.Include(p => p.Persona);
                return View(planilla.ToList());
            }
            if(userRol == 1)
            {
                var planilla = db.Planilla
                    .Include(e => e.Persona)
                    .Where(e => e.Persona.IDCedula == userId);
                return View(planilla.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }            
        }

        public ActionResult Reportes()
        {
            var userRol = (int)Session["userRol"];

            if (userRol == 2 || userRol == 3)
            {
                var planilla = db.Planilla.Include(p => p.Persona);
                return View(planilla.ToList());
            }            
            else
            {
                return RedirectToAction("Error");
            }
        }

        // GET: Planilla/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Planilla planilla = db.Planilla.Find(id);
            if (planilla == null)
            {
                return HttpNotFound();
            }
            return View(planilla);
        }

        // GET: Planilla/Create
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
               
                ViewBag.Empleado = new SelectList(ViewBag.persona, "Empleado", "NombrePers");
                return View();
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // POST: Planilla/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDPlanilla,Empleado,Periodo,FechaPlanill,Salario,CCSS,Renta,MontoInca,Total")] Planilla planilla)
        {            
                if (ModelState.IsValid)
                {    

                // CONSULTA SALARIO DEL COLABORARDOR
                double salario = (from p in db.Persona
                                  join o in db.Ocupacion
                                  on p.Ocupacion equals o.IDOcupacion
                                  where planilla.Empleado == p.IDCedula
                                  select o.Salario).FirstOrDefault();

                planilla.FechaPlanill = DateTime.Now;
                double diasPerm = 0;

                // CONSULTA PERMISOS LABORALES DEL COLABORARDOR
                var permisosLab = (from p in db.Persona
                                   join pl in db.PermisoLaboral
                                   on p.IDCedula equals pl.Empleado
                                   where planilla.Empleado == p.IDCedula && planilla.FechaPlanill.Month == pl.FechaInicio.Month && pl.TipoPermiso == 5 && pl.Estado == 1
                                   // SIN GOCE SALARIAL PARA NO TOMAR EN CUENTA LOS DIAS DEL PERMISO EN LA PLANILLA -- TipoPermiso
                                   select pl);

                // SUMATORIA DE DIAS DEL PERMISO SIN GOCE SALARIAL
                foreach (var pl in permisosLab)
                {
                    diasPerm += pl.CantDias;
                }
                                
                double montoInca = 0;
                double diasInca = 0;

                // CONSULTA INCAPACIDADES DEL COLABORARDOR
                var incapacidades = (from p in db.Persona
                                     join i in db.Incapacidad
                                     on p.IDCedula equals i.Empleado
                                     where planilla.Empleado == p.IDCedula && planilla.FechaPlanill.Month == i.FechaInicio.Month && i.Estado == 1
                                     // INCAPACIDAD DEBE TENER ESTADO APROBADO PARA SER CONSIDERADA EN LA PLANILLA -- i.Estado
                                     select i);

                // SUMATORIA DIAS Y MONTOS INCAPACIDADES DEL COLABORARDOR
                foreach (var i in incapacidades)
                {
                    montoInca += i.MontoIncaTotal;
                    diasInca += i.CantDias;
                }

                // ESTABLECER QUINCENA SEGUN FECHA DE LA PLANILLA
                if (DateTime.Now.Day < 16)
                {
                    planilla.Periodo = "Primera";
                }
                else
                {
                    planilla.Periodo = "Segunda";
                }

                // VALIDACIÓN DE REGISTRO DE PLANILLA PARA EL PERIODO INDICADO Y USUARIO SELECCIONADO
                bool existeQuincena = db.Planilla
                        .Include(r => r.Persona)
                        .Any(r => r.FechaPlanill.Month == planilla.FechaPlanill.Month && r.Periodo == planilla.Periodo && r.Persona.IDCedula == planilla.Empleado);

                if (existeQuincena)
                {
                    TempData["AlertMessage"] = "Ya existe un registro de planilla para el colaborador en este periodo, por favor revisar.";
                    ViewBag.persona = from p in db.Persona.ToList()
                                      orderby p.NombrePers ascending
                                      select new
                                      {
                                          Empleado = p.IDCedula,
                                          NombrePers = p.NombrePers + " " + p.Apellidos
                                      };

                    ViewBag.Empleado = new SelectList(ViewBag.persona, "Empleado", "NombrePers");
                    return View(planilla);
                }

                double salarioBruto = (salario * 0.1067) + salario;         // SALARIO BRUTO PARA CALCULAR RENTA
                double salarioCaja = salario / 2;                           // SALARIO QUINCENAL PARA CALCULAR LA CAJA
                salario = salario / 20;                                     // SALARIO DIARIO
                double salarioQuin = Math.Round((10 - diasInca - diasPerm) * salario, 0);  // SALARIO QUINCENAL CONSIDERANDO DIAS INCAPACIDAD Y PERMISOS LABORALES              

                // CALCULO DE RENTA SEGUN SALARIO DEL COLABORADOR
                if (salarioBruto <= 929000)
                {
                    planilla.FechaPlanill = DateTime.Now;
                    planilla.Salario = salarioQuin;
                    planilla.CCSS = Math.Round(salarioCaja * 0.1067, 0);
                    planilla.Renta = 0;
                    planilla.MontoInca = montoInca;
                    planilla.Total = planilla.Salario + planilla.CCSS + planilla.Renta + planilla.MontoInca;
                }
                if (salarioBruto >= 929001 && salarioBruto <= 1363000)
                {
                    planilla.FechaPlanill = DateTime.Now;
                    planilla.Salario = salarioQuin;
                    planilla.CCSS = Math.Round(salarioCaja * 0.1067, 0);
                    planilla.Renta = Math.Round(((salarioBruto - 929001) / 2) * 0.10, 0);
                    planilla.MontoInca = montoInca;
                    planilla.Total = planilla.Salario + planilla.CCSS + planilla.Renta + planilla.MontoInca;
                }
                if (salarioBruto >= 1363001 && salarioBruto <= 2392000)
                {
                    planilla.FechaPlanill = DateTime.Now;
                    planilla.Salario = salarioQuin;
                    planilla.CCSS = Math.Round(salarioCaja * 0.1067, 0);
                    planilla.Renta = Math.Round(((salarioBruto - 1363001) / 2) * 0.15, 0);
                    planilla.MontoInca = montoInca;
                    planilla.Total = planilla.Salario + planilla.CCSS + planilla.Renta + planilla.MontoInca;
                }
                if (salarioBruto >= 2392001 && salarioBruto <= 4783000)
                {
                    planilla.FechaPlanill = DateTime.Now;
                    planilla.Salario = salarioQuin;
                    planilla.CCSS = Math.Round(salarioCaja * 0.1067, 0);
                    planilla.Renta = Math.Round(((salarioBruto - 2392001) / 2) * 0.20, 0);
                    planilla.MontoInca = montoInca;
                    planilla.Total = planilla.Salario + planilla.CCSS + planilla.Renta + planilla.MontoInca;
                }
                if (salarioBruto >= 4783001)
                {
                    planilla.FechaPlanill = DateTime.Now;
                    planilla.Salario = salarioQuin;
                    planilla.CCSS = Math.Round(salarioCaja * 0.1067, 0);
                    planilla.Renta = Math.Round(((salarioBruto - 4783001) / 2) * 0.25, 0);
                    planilla.MontoInca = montoInca;
                    planilla.Total = planilla.Salario + planilla.CCSS + planilla.Renta + planilla.MontoInca;
                }
                
                // CONSULTA CANTIDAD DE VACACIONES DEL COLABORADOR PARA SUMAR EL 0.5 DE ACUERDO A LA QUINCENA EFECTUADA
                var CantVacas = (from p in db.Persona
                                 where planilla.Empleado == p.IDCedula
                                 select p.CantVacaciones).FirstOrDefault();
                CantVacas = CantVacas + 0.5;

                // EJECUCION DE PA PARA ACTUALIZAR LA CANTIDAD DE VACACIONES DEL COLABORADOR
                var Cedula = new SqlParameter("@IDCedula", planilla.Empleado);
                var Vacas = new SqlParameter("@CantVacas", CantVacas);               
                db.Database.ExecuteSqlCommand("EXEC sp_ActualizarVacaciones @IDCedula, @CantVacas", Cedula, Vacas);

                TempData["AlertMessage"] = "Planilla registrada satisfactoriamente!!!";

                    db.Planilla.Add(planilla);
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

                ViewBag.Empleado = new SelectList(ViewBag.persona, "Empleado", "NombrePers");
                //ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers", planilla.Empleado);
                return View(planilla);                      
        }

        // GET: Planilla/Edit/5
        public ActionResult Edit(int? id)
        {
            var userRol = (int)Session["UserRol"];

            if(userRol == 2 || userRol == 3)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Planilla planilla = db.Planilla.Find(id);
                if (planilla == null)
                {
                    return HttpNotFound();
                }
                ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers", planilla.Empleado);
                return View(planilla);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // POST: Planilla/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDPlanilla,Empleado,Periodo,FechaPlanill,Salario,CCSS,Renta,MontoInca,Total")] Planilla planilla)
        {
            if (ModelState.IsValid)
            {
                if (planilla.Periodo == null)
                {
                    ModelState.AddModelError("Periodo", "El periodo de la planilla es oblogatorio.");                    
                    return View(planilla);
                }

                // CONSULTA SALARIO DEL COLABORARDOR
                double salario = (from p in db.Persona
                                  join o in db.Ocupacion
                                  on p.Ocupacion equals o.IDOcupacion
                                  where planilla.Empleado == p.IDCedula
                                  select o.Salario).FirstOrDefault();

                //planilla.FechaPlanill = DateTime.Now;
                double diasPerm = 0;

                // CONSULTA PERMISOS LABORALES DEL COLABORARDOR
                var permisosLab = (from p in db.Persona
                                   join pl in db.PermisoLaboral
                                   on p.IDCedula equals pl.Empleado
                                   where planilla.Empleado == p.IDCedula && planilla.FechaPlanill.Month == pl.FechaInicio.Month && pl.TipoPermiso == 5 && pl.Estado == 1
                                   // SIN GOCE SALARIAL PARA NO TOMAR EN CUENTA LOS DIAS DEL PERMISO EN LA PLANILLA -- TipoPermiso
                                   select pl);

                // SUMATORIA DE DIAS DEL PERMISO SIN GOCE SALARIAL
                foreach (var pl in permisosLab)
                {
                    diasPerm += pl.CantDias;
                }

                double montoInca = 0;
                double diasInca = 0;

                // CONSULTA INCAPACIDADES DEL COLABORARDOR
                var incapacidades = (from p in db.Persona
                                     join i in db.Incapacidad
                                     on p.IDCedula equals i.Empleado
                                     where planilla.Empleado == p.IDCedula && planilla.FechaPlanill.Month == i.FechaInicio.Month && i.Estado == 1
                                     // INCAPACIDAD DEBE TENER ESTADO APROBADO PARA SER CONSIDERADA EN LA PLANILLA -- i.Estado
                                     select i);

                // SUMATORIA DIAS Y MONTOS INCAPACIDADES DEL COLABORARDOR
                foreach (var i in incapacidades)
                {
                    montoInca += i.MontoIncaTotal;
                    diasInca += i.CantDias;
                }

                // ESTABLECER QUINCENA SEGUN FECHA DE LA PLANILLA
                if (planilla.FechaPlanill.Day < 16)
                {
                    planilla.Periodo = "Primera";
                }
                else
                {
                    planilla.Periodo = "Segunda";
                }                

                double salarioBruto = (salario * 0.1067) + salario;         // SALARIO BRUTO PARA CALCULAR RENTA
                double salarioCaja = salario / 2;                           // SALARIO QUINCENAL PARA CALCULAR LA CAJA
                salario = salario / 20;                                     // SALARIO DIARIO
                double salarioQuin = Math.Round((10 - diasInca - diasPerm) * salario, 0);  // SALARIO QUINCENAL CONSIDERANDO DIAS INCAPACIDAD Y PERMISOS LABORALES              

                // CALCULO DE RENTA SEGUN SALARIO DEL COLABORADOR
                if (salarioBruto <= 929000)
                {
                    //planilla.FechaPlanill = DateTime.Now;
                    planilla.Salario = salarioQuin;
                    planilla.CCSS = Math.Round(salarioCaja * 0.1067, 0);
                    planilla.Renta = 0;
                    planilla.MontoInca = montoInca;
                    planilla.Total = planilla.Salario + planilla.CCSS + planilla.Renta + planilla.MontoInca;
                }
                if (salarioBruto >= 929001 && salarioBruto <= 1363000)
                {
                    //planilla.FechaPlanill = DateTime.Now;
                    planilla.Salario = salarioQuin;
                    planilla.CCSS = Math.Round(salarioCaja * 0.1067, 0);
                    planilla.Renta = Math.Round(((salarioBruto - 929001) / 2) * 0.10, 0);
                    planilla.MontoInca = montoInca;
                    planilla.Total = planilla.Salario + planilla.CCSS + planilla.Renta + planilla.MontoInca;
                }
                if (salarioBruto >= 1363001 && salarioBruto <= 2392000)
                {
                    //planilla.FechaPlanill = DateTime.Now;
                    planilla.Salario = salarioQuin;
                    planilla.CCSS = Math.Round(salarioCaja * 0.1067, 0);
                    planilla.Renta = Math.Round(((salarioBruto - 1363001) / 2) * 0.15, 0);
                    planilla.MontoInca = montoInca;
                    planilla.Total = planilla.Salario + planilla.CCSS + planilla.Renta + planilla.MontoInca;
                }
                if (salarioBruto >= 2392001 && salarioBruto <= 4783000)
                {
                    //planilla.FechaPlanill = DateTime.Now;
                    planilla.Salario = salarioQuin;
                    planilla.CCSS = Math.Round(salarioCaja * 0.1067, 0);
                    planilla.Renta = Math.Round(((salarioBruto - 2392001) / 2) * 0.20, 0);
                    planilla.MontoInca = montoInca;
                    planilla.Total = planilla.Salario + planilla.CCSS + planilla.Renta + planilla.MontoInca;
                }
                if (salarioBruto >= 4783001)
                {
                    //planilla.FechaPlanill = DateTime.Now;
                    planilla.Salario = salarioQuin;
                    planilla.CCSS = Math.Round(salarioCaja * 0.1067, 0);
                    planilla.Renta = Math.Round(((salarioBruto - 4783001) / 2) * 0.25, 0);
                    planilla.MontoInca = montoInca;
                    planilla.Total = planilla.Salario + planilla.CCSS + planilla.Renta + planilla.MontoInca;
                }

                TempData["AlertMessage"] = "Planilla actualizada satisfactoriamente!!!";
                db.Entry(planilla).State = EntityState.Modified; ;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers", planilla.Empleado);
            return View(planilla);
        }

        // GET: Planilla/Delete/5
        public ActionResult Delete(int? id)
        {
            var userRol = (int)Session["UserRol"];

            if(userRol == 2 || userRol == 3)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Planilla planilla = db.Planilla.Find(id);
                if (planilla == null)
                {
                    return HttpNotFound();
                }
                return View(planilla);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // POST: Planilla/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Planilla planilla = db.Planilla.Find(id);
            TempData["AlertMessage"] = "Planilla eliminada satisfactoriamente!!!";
            db.Planilla.Remove(planilla);
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
