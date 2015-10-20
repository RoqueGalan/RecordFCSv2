using Newtonsoft.Json;
using RecordFCS_Alt.App_Start;
using RecordFCS_Alt.Helpers.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace RecordFCS_Alt
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            // Manually installed WebAPI 2.2 after making an MVC project.
            //GlobalConfiguration.Configure(WebApiConfig.Register); // NEW way
            //WebApiConfig.Register(GlobalConfiguration.Configuration); // DEPRECATED
            // Default stuff
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Normally, you'd probably be doing this in a setup method that
            // accepts the configuration as a parameter and this line wouldn't
            // be necessary, but you can do it right in Application_Start too.
            //var config = GlobalConfiguration.Configuration;

            //config.Formatters.Remove(config.Formatters.XmlFormatter);
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                //Se extrae la cookie de autentication 
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                CustomPrincipalSerializeModel serializeModel = JsonConvert.DeserializeObject<CustomPrincipalSerializeModel>(authTicket.UserData);

                CustomPrincipal newUser = new CustomPrincipal(authTicket.Name);

                newUser.UsuarioID = serializeModel.UsuarioID;
                newUser.Nombre = serializeModel.Nombre;
                newUser.Apellido = serializeModel.Apellido;
                newUser.ListaRoles = serializeModel.ListaRoles;
                newUser.Tiempo = serializeModel.Tiempo;

                HttpContext.Current.User = newUser;
            }
        }

        //protected void Session_End(object sender, EventArgs e)
        //{
        //    System.Web.Security.FormsAuthentication.SignOut();
        //    System.Web.Security.FormsAuthentication.RedirectToLoginPage();

        //    HttpContext.Current.Response.Redirect("~/");

        //    //Response.Redirect("~/");
        //}
    }
}
