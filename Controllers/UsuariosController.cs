using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using RHQuercus;

namespace RHQuercus.Controllers
{
    public class UsuariosController : Controller
    {
        private RecursoHumanoQuercusEntities db = new RecursoHumanoQuercusEntities();

        public ActionResult Error()
        {
            return View();
        }

        // GET: Usuarios
        public ActionResult Index()
        {
            var userRol = (int)Session["UserRol"];

            if(userRol == 3)
            {
                var usuario = db.Usuario.Include(u => u.TipoRol);
                return View(usuario.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }            
        }

        public ActionResult Reportes()
        {
            var userRol = (int)Session["UserRol"];

            if (userRol == 3 || userRol == 2)
            {
                var usuario = db.Usuario.Include(u => u.TipoRol);
                return View(usuario.ToList());
            }
            else
            {
                return RedirectToAction("Error");
            }
        }

        // GET: Usuarios/Create
        public ActionResult Create()
        {
            ViewBag.Rol = new SelectList(db.TipoRol, "IDRol", "NombreRol");
            return View();
        }

        //CIFRADO DE CONTRASENAS
        public class HasherContrasena
        {
            public static string HashContrasena(string Contrasena, string Salt)
            {
                using (var sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Contrasena + Salt));
                    return Convert.ToBase64String(hashedBytes);
                }
            }

            public static string GenerateSalt(int length = 20)
            {
                var random = new RNGCryptoServiceProvider();
                var Salt = new byte[length];
                random.GetBytes(Salt);
                return Convert.ToBase64String(Salt);
            }
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IDUsuario,ConfirContrasena,Contrasena,Salt,Rol")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                if (db.Usuario.Any(u => u.IDUsuario == usuario.IDUsuario))
                {
                    ModelState.AddModelError("IDUsuario", "El nombre de usuario ingresado ya existe en el sistema.");
                    return View(usuario);
                }

                if (usuario.IDUsuario < 100000000)
                {
                    ModelState.AddModelError("IDUsuario", "La identificación debe contener al menos 9 dígitos.");
                    return View(usuario);
                }

                if (usuario.Contrasena == null)
                {
                    ModelState.AddModelError("Contrasena", "La contraseña debe contener al menos 6 caracteres.");
                    return View(usuario);
                }

                if (usuario.Contrasena.Length < 6)
                {
                    ModelState.AddModelError("Contrasena", "La contraseña debe contener al menos 6 caracteres.");
                    return View(usuario);
                }

                if (usuario.Contrasena == usuario.ConfirContrasena)
                {
                    string Salt = HasherContrasena.GenerateSalt();
                    string hashedPassword = HasherContrasena.HashContrasena(usuario.Contrasena, Salt);

                    usuario.Contrasena = hashedPassword;
                    usuario.Salt = Salt;
                    usuario.Rol = 4;

                    TempData["AlertMessage"] = "Usuario registrado satisfactoriamente!!!";

                    db.Usuario.Add(usuario);
                    db.SaveChanges();
                    return RedirectToAction("Acceso");                    
                }
                else
                {
                    ModelState.AddModelError("Contrasena", "Las contraseñas son diferentes, por favor intente nuevamente.");
                    return View(usuario);
                }
            }
            return View(usuario);
        }
        
        // PANTALLA LOGIN
        public ActionResult Acceso()
        {            
            return View();
        }
                       
        // VALIDACION USUARIO Y CONTRASENA - LOGIN
        [HttpPost]
        public ActionResult Acceso(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                var user = db.Usuario.SingleOrDefault(u => u.IDUsuario == usuario.IDUsuario);
                if (user != null)
                {
                    string hashedPassword = HasherContrasena.HashContrasena(usuario.Contrasena, user.Salt);

                    if (user.Contrasena == hashedPassword)
                    {
                        Session["UserId"] = user.IDUsuario;
                        Session["UserRol"] = user.Rol;

                        var NombreUsuario = (from u in db.Usuario
                                             join p in db.Persona
                                             on u.IDUsuario equals p.IDCedula
                                             where u.IDUsuario == usuario.IDUsuario
                                             select p.NombrePers).FirstOrDefault();

                        TempData["UserId"] = NombreUsuario;
                        TempData["AlertMessage"] = "Acceso satisfactorio!!!";

                        // Autenticación exitosa, redirigir al usuario a una página segura
                        return RedirectToAction("Index", "Home");                        
                    }
                }
            }
            TempData["AlertMessage"] = "Nombre de usuario o contrasena incorrectos.";
            //ModelState.AddModelError("", "Nombre de usuario o contraseña incorrectos.");
            return View(usuario);
        }

        //LOGOUT DEL USUARIO

        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Acceso");
        }

        // GET: Usuarios/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario usuario = db.Usuario.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            ViewBag.Rol = new SelectList(db.TipoRol, "IDRol", "NombreRol", usuario.Rol);
            return View(usuario);
        }

        // POST: Usuarios/Edit/5        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "IDUsuario,Contrasena,Salt,Rol")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                TempData["AlertMessage"] = "Usuario actualizado satisfactoriamente!!!";
                db.Entry(usuario).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Rol = new SelectList(db.TipoRol, "IDRol", "NombreRol", usuario.Rol);
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario usuario = db.Usuario.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Usuario usuario = db.Usuario.Find(id);

            TempData["AlertMessage"] = "Usuario eliminado satisfactoriamente!!!";

            db.Usuario.Remove(usuario);
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
