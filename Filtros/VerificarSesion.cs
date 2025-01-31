using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RHQuercus;
using RHQuercus.Controllers;

namespace RHQuercus.Filtros
{
    public class VerificarSesion : ActionFilterAttribute
    {
        // FILTRO INICIO DE SESION // LOGIN
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var SesionUser = HttpContext.Current.Session["UserId"];

            if (SesionUser == null)
            {
                if(filterContext.Controller is UsuariosController == false)
                {
                    filterContext.HttpContext.Response.Redirect("~/Usuarios/Acceso");
                }                
            }            
            base.OnActionExecuting(filterContext);
        }
    }
}