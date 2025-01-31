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
    public class LiquidacionController : Controller
    {
        private RecursoHumanoQuercusEntities db = new RecursoHumanoQuercusEntities();

        public ActionResult Error()
        {
            return View("Error");
        }

        // GET: Liquidacion
        public ActionResult Index()
        {
            var userRol = (int)Session["UserRol"];
            var userId = (int)Session["UserId"];

            if(userRol == 1) 
            {
                var liquidacion = db.Liquidacion
                        .Include(a => a.Persona)
                        .Where(a => a.Persona.IDCedula == userId);
                        return View(liquidacion.ToList());
            }
            if(userRol == 2 || userRol == 3)
            {
                var liquidacion = db.Liquidacion.Include(l => l.Estado1).Include(l => l.Persona).Include(l => l.MotivoLiq);
                return View(liquidacion.ToList());
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
                var liquidacion = db.Liquidacion.Include(l => l.Estado1).Include(l => l.Persona).Include(l => l.MotivoLiq);
                return View(liquidacion.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // GET: Liquidacion/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Liquidacion liquidacion = db.Liquidacion.Find(id);
            if (liquidacion == null)
            {
                return HttpNotFound();
            }
            return View(liquidacion);
        }

        // GET: Liquidacion/Create
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
                ViewBag.Motivo = new SelectList(db.MotivoLiq, "IDMotivo", "Nombre");
                return View();
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // POST: Liquidacion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TiempoLaborado, IDLiquidacion,Empleado,FechaIngreso,FechaSalida,Motivo,Estado,SalarioMensual,PromedioDiaLaboral,Cesantia,Preaviso,Aguinaldo,Vacaciones,MontoTotalLiq,Observaciones")] Liquidacion liquidacion)
        {
            ViewBag.persona = from p in db.Persona.ToList()
                              orderby p.NombrePers ascending
                              select new
                              {
                                  Empleado = p.IDCedula,
                                  NombrePers = p.NombrePers + " " + p.Apellidos
                              };

            if (ModelState.IsValid)
            {               

                try
                {
                    
                liquidacion.Estado = 2;

                // CONSULTA SALARIO DEL COLABORARDOR
                double salario = (from p in db.Persona
                                  join o in db.Ocupacion
                                  on p.Ocupacion equals o.IDOcupacion
                                  where liquidacion.Empleado == p.IDCedula
                                  select o.Salario).FirstOrDefault();

                // CONSULTA CANT. VACACIONES DEL COLABORARDOR
                double vacas = (double)(from p in db.Persona
                                        where liquidacion.Empleado == p.IDCedula
                                        select p.CantVacaciones).FirstOrDefault();

                // SE OBTIENE FECHA DE INGRESO A LABORAR DEL COLABORADOR
                var fechaIngreso = (from p in db.Persona
                                    where liquidacion.Empleado == p.IDCedula
                                    select p.FechaIngreso).FirstOrDefault();

                int años = liquidacion.FechaSalida.Year - fechaIngreso.Year;
                int meses = liquidacion.FechaSalida.Month - fechaIngreso.Month;
                int ano = liquidacion.FechaSalida.Year - 1;

                // CONSULTA PLANILLAS DEL COLABORARDOR
                var planillas = (from p in db.Persona
                                 join pla in db.Planilla
                                 on p.IDCedula equals pla.Empleado
                                 where liquidacion.Empleado == p.IDCedula
                                 select pla);

                // CALCULO ULTIMOS 6 MESE DE SALARIOS PARA OBTENER EL PROMEDIO DEL DÍA LABORAL
                double montoSalarios = 0;
                var ultimasPlanillas = planillas
                    .OrderByDescending(pla => pla.FechaPlanill)
                    .Take(12)
                    .ToList();

                foreach (var pla in ultimasPlanillas)
                {
                    montoSalarios += pla.Total;
                }

                // CALCULO ULTIMOS 12 MESES DE SALARIOS PARA OBTENER EL AGUINALDO
                int año = liquidacion.FechaSalida.Year - 1;
                double montoAguinaldo = 0;
                var aguinaldo = planillas
                    .Where(pla => pla.FechaPlanill >= new DateTime(ano, 11, 1))
                    .OrderByDescending(pla => pla.FechaPlanill)
                    .Take(24)
                    .ToList();

                foreach (var pla in aguinaldo)
                {
                    montoAguinaldo += pla.Total;
                }

                // CALCULO DE PROMEDIO DIA LABORAL SEGÚN TIEMPO LABORADO
                if (años <= 0 && meses < 6)
                {
                    liquidacion.PromedioDiaLaboral = Math.Round(montoSalarios / (meses * 30), 0);
                }
                else
                {
                    liquidacion.PromedioDiaLaboral = Math.Round(montoSalarios / 180, 0);
                }

                if (liquidacion.Motivo == 1)
                {

                    // CALCULO DE PREAVISO
                    if (liquidacion.Preaviso == 0 /*|| liquidacion.Preaviso == null*/)
                    {
                        liquidacion.Preaviso = 0;
                    }
                    else
                    {
                        liquidacion.Preaviso = liquidacion.Preaviso * liquidacion.PromedioDiaLaboral;
                    }

                    // CALCULO DE CESANTIA
                    if (años <= 0)
                    {
                        if (meses < 3)
                        {
                            liquidacion.Cesantia = 0;
                        }
                        else if (meses >= 3 && meses < 6)
                        {
                            liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * 5, 0);
                        }
                        else if (meses >= 6 && meses < 12)
                        {
                            liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * 10, 0);
                        }
                    }
                    else if (años >= 13)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (20 * años), 0);
                    }
                    else if (años >= 12)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (20.5 * 12), 0);
                    }
                    else if (años >= 11)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (21 * 11), 0);
                    }
                    else if (años >= 10)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (21.5 * 10), 0);
                    }
                    else if (años >= 7)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (22 * años), 0);
                    }
                    else if (años >= 6)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (21.5 * 6), 0);
                    }
                    else if (años >= 5)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (21.24 * 5), 0);
                    }
                    else if (años >= 4)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (21 * 4), 0);
                    }
                    else if (años >= 3)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (20.5 * 3), 0);
                    }
                    else if (años >= 2)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (20 * 2), 0);
                    }
                    else if (años >= 1)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * 19.5, 0);
                    }

                    liquidacion.FechaIngreso = fechaIngreso;
                    liquidacion.SalarioMensual = salario;
                    liquidacion.Vacaciones = Math.Round(liquidacion.PromedioDiaLaboral * vacas, 0);
                    liquidacion.Aguinaldo = Math.Round(montoAguinaldo / 12, 0);
                    liquidacion.MontoTotalLiq = liquidacion.Vacaciones + liquidacion.Aguinaldo + liquidacion.Preaviso + liquidacion.Cesantia;
                    //liquidacion.PromedioDiaLaboral = Math.Round(montoSalarios / 180, 2);
                    //liquidacion.Preaviso = 0;
                    //liquidacion.Cesantia = 0;
                }
                else
                {
                    liquidacion.FechaIngreso = fechaIngreso;
                    liquidacion.SalarioMensual = salario;
                    liquidacion.Vacaciones = Math.Round(liquidacion.PromedioDiaLaboral * vacas, 0);
                    liquidacion.Aguinaldo = Math.Round(montoAguinaldo / 12, 0);
                    liquidacion.Preaviso = 0;
                    liquidacion.Cesantia = 0;
                    liquidacion.MontoTotalLiq = liquidacion.Vacaciones + liquidacion.Aguinaldo + liquidacion.Preaviso + liquidacion.Cesantia;
                    //liquidacion.PromedioDiaLaboral = Math.Round(montoSalarios / 180, 2);                
                }

                TempData["AlertMessage"] = "Proceso registrado satisfactoriamente!!!";

                db.Liquidacion.Add(liquidacion);
                db.SaveChanges();
                return RedirectToAction("Index");
                }
                catch
                {
                    TempData["AlertMessage"] = "Por favor verificar las fechas de entrada y salida a laborar!!!";
                    ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud", liquidacion.Estado);
                    ViewBag.Empleado = new SelectList(ViewBag.persona, "Empleado", "NombrePers", liquidacion.Empleado);
                    ViewBag.Motivo = new SelectList(db.MotivoLiq, "IDMotivo", "Nombre", liquidacion.Motivo);
                    return View(liquidacion);
                }
            }

            ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud", liquidacion.Estado);
            ViewBag.Empleado = new SelectList(ViewBag.persona, "Empleado", "NombrePers", liquidacion.Empleado);
            ViewBag.Motivo = new SelectList(db.MotivoLiq, "IDMotivo", "Nombre", liquidacion.Motivo);
            return View(liquidacion);
        }

        // GET: Liquidacion/Edit/5
        public ActionResult Edit(int? id)
        {
            var userRol = (int)Session["UserRol"];

            if (userRol == 2 || userRol == 3)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Liquidacion liquidacion = db.Liquidacion.Find(id);
                if (liquidacion == null)
                {
                    return HttpNotFound();
                }
                ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud", liquidacion.Estado);
                ViewBag.Empleado = new SelectList(db.Persona, "IDCedula", "NombrePers", liquidacion.Empleado);
                ViewBag.Motivo = new SelectList(db.MotivoLiq, "IDMotivo", "Nombre", liquidacion.Motivo);
                return View(liquidacion);
            }
            else
            {
                return RedirectToAction("Error");
            }            
        }

        // POST: Liquidacion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDLiquidacion,Empleado,FechaIngreso,FechaSalida,Motivo,Estado,SalarioMensual,PromedioDiaLaboral,Cesantia,Preaviso,Aguinaldo,Vacaciones,MontoTotalLiq,Observaciones")] Liquidacion liquidacion)
        {
            ViewBag.persona = from p in db.Persona.ToList()
                              orderby p.NombrePers ascending
                              select new
                              {
                                  Empleado = p.IDCedula,
                                  NombrePers = p.NombrePers + " " + p.Apellidos
                              };

            if (ModelState.IsValid)
            {                
                try
                {
                    liquidacion.Estado = 2;

                // CONSULTA SALARIO DEL COLABORARDOR
                double salario = (from p in db.Persona
                                  join o in db.Ocupacion
                                  on p.Ocupacion equals o.IDOcupacion
                                  where liquidacion.Empleado == p.IDCedula
                                  select o.Salario).FirstOrDefault();

                // CONSULTA CANT. VACACIONES DEL COLABORARDOR
                double vacas = (double)(from p in db.Persona
                                        where liquidacion.Empleado == p.IDCedula
                                        select p.CantVacaciones).FirstOrDefault();

                // SE OBTIENE FECHA DE INGRESO A LABORAR DEL COLABORADOR
                var fechaIngreso = (from p in db.Persona
                                    where liquidacion.Empleado == p.IDCedula
                                    select p.FechaIngreso).FirstOrDefault();

                int años = liquidacion.FechaSalida.Year - fechaIngreso.Year;
                int meses = liquidacion.FechaSalida.Month - fechaIngreso.Month;
                int ano = liquidacion.FechaSalida.Year - 1;

                // CONSULTA PLANILLAS DEL COLABORARDOR
                var planillas = (from p in db.Persona
                                 join pla in db.Planilla
                                 on p.IDCedula equals pla.Empleado
                                 where liquidacion.Empleado == p.IDCedula
                                 select pla);

                // CALCULO ULTIMOS 6 MESE DE SALARIOS PARA OBTENER EL PROMEDIO DEL DÍA LABORAL
                double montoSalarios = 0;
                var ultimasPlanillas = planillas
                    .OrderByDescending(pla => pla.FechaPlanill)
                    .Take(12)
                    .ToList();

                foreach (var pla in ultimasPlanillas)
                {
                    montoSalarios += pla.Total;
                }

                // CALCULO ULTIMOS 12 MESES DE SALARIOS PARA OBTENER EL AGUINALDO
                int año = liquidacion.FechaSalida.Year - 1;
                double montoAguinaldo = 0;
                var aguinaldo = planillas
                    .Where(pla => pla.FechaPlanill >= new DateTime(ano, 11, 1))
                    .OrderByDescending(pla => pla.FechaPlanill)
                    .Take(24)
                    .ToList();

                foreach (var pla in aguinaldo)
                {
                    montoAguinaldo += pla.Total;
                }

                // CALCULO DE PROMEDIO DIA LABORAL SEGÚN TIEMPO LABORADO
                if (años <= 0 && meses < 6)
                {
                    liquidacion.PromedioDiaLaboral = Math.Round(montoSalarios / (meses * 30), 0);
                }
                else
                {
                    liquidacion.PromedioDiaLaboral = Math.Round(montoSalarios / 180, 0);
                }

                if (liquidacion.Motivo == 1)
                {

                    // CALCULO DE PREAVISO
                    if (liquidacion.Preaviso == 0)
                    {
                        liquidacion.Preaviso = 0;
                    }
                    else
                    {
                        liquidacion.Preaviso = liquidacion.Preaviso * liquidacion.PromedioDiaLaboral;
                    }

                    // CALCULO DE CESANTIA
                    if (años <= 0)
                    {
                        if (meses < 3)
                        {
                            liquidacion.Cesantia = 0;
                        }
                        else if (meses >= 3 && meses < 6)
                        {
                            liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * 5, 0);
                        }
                        else if (meses >= 6 && meses < 12)
                        {
                            liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * 10, 0);
                        }
                    }
                    else if (años >= 13)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (20 * años), 0);
                    }
                    else if (años >= 12)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (20.5 * 12), 0);
                    }
                    else if (años >= 11)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (21 * 11), 0);
                    }
                    else if (años >= 10)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (21.5 * 10), 0);
                    }
                    else if (años >= 7)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (22 * años), 0);
                    }
                    else if (años >= 6)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (21.5 * 6), 0);
                    }
                    else if (años >= 5)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (21.24 * 5), 0);
                    }
                    else if (años >= 4)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (21 * 4), 0);
                    }
                    else if (años >= 3)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (20.5 * 3), 0);
                    }
                    else if (años >= 2)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * (20 * 2), 0);
                    }
                    else if (años >= 1)
                    {
                        liquidacion.Cesantia = Math.Round(liquidacion.PromedioDiaLaboral * 19.5, 0);
                    }

                    liquidacion.FechaIngreso = fechaIngreso;
                    liquidacion.SalarioMensual = salario;
                    liquidacion.Vacaciones = Math.Round(liquidacion.PromedioDiaLaboral * vacas, 0);
                    liquidacion.Aguinaldo = Math.Round(montoAguinaldo / 12, 0);
                    liquidacion.MontoTotalLiq = liquidacion.Vacaciones + liquidacion.Aguinaldo + liquidacion.Preaviso + liquidacion.Cesantia;
                    //liquidacion.PromedioDiaLaboral = Math.Round(montoSalarios / 180, 2);
                    //liquidacion.Preaviso = 0;
                    //liquidacion.Cesantia = 0;
                }
                else
                {
                    liquidacion.FechaIngreso = fechaIngreso;
                    liquidacion.SalarioMensual = salario;
                    liquidacion.Vacaciones = Math.Round(liquidacion.PromedioDiaLaboral * vacas, 0);
                    liquidacion.Aguinaldo = Math.Round(montoAguinaldo / 12, 0);
                    liquidacion.Preaviso = 0;
                    liquidacion.Cesantia = 0;
                    liquidacion.MontoTotalLiq = liquidacion.Vacaciones + liquidacion.Aguinaldo + liquidacion.Preaviso + liquidacion.Cesantia;
                    //liquidacion.PromedioDiaLaboral = Math.Round(montoSalarios / 180, 2);                
                }

                TempData["AlertMessage"] = "Proceso actualizado satisfactoriamente!!!";

                db.Entry(liquidacion).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
                }
                catch
                {
                    TempData["AlertMessage"] = "Por favor verificar las fechas de entrada y salida a laborar!!!";
                    ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud", liquidacion.Estado);
                    ViewBag.Empleado = new SelectList(ViewBag.persona, "Empleado", "NombrePers", liquidacion.Empleado);
                    ViewBag.Motivo = new SelectList(db.MotivoLiq, "IDMotivo", "Nombre", liquidacion.Motivo);
                    return View(liquidacion);
                }
            }
            ViewBag.Estado = new SelectList(db.Estado, "IDEstado", "EstadoSolicitud", liquidacion.Estado);
            ViewBag.Empleado = new SelectList(ViewBag.persona, "Empleado", "NombrePers", liquidacion.Empleado);
            ViewBag.Motivo = new SelectList(db.MotivoLiq, "IDMotivo", "Nombre", liquidacion.Motivo);
            return View(liquidacion);
        }

        // GET: Liquidacion/Delete/5
        public ActionResult Delete(int? id)
        {
            var userRol = (int)Session["UserRol"];

            if (userRol == 2 || userRol == 3)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Liquidacion liquidacion = db.Liquidacion.Find(id);
                if (liquidacion == null)
                {
                    return HttpNotFound();
                }
                return View(liquidacion);
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // POST: Liquidacion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Liquidacion liquidacion = db.Liquidacion.Find(id);

            TempData["AlertMessage"] = "Proceso eliminado satisfactoriamente!!!";

            db.Liquidacion.Remove(liquidacion);
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
