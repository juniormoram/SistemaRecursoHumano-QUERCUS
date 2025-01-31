using RHQuercus.Filtros;
using System.Web;
using System.Web.Mvc;

namespace RHQuercus
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new VerificarSesion());
        }
    }
}
