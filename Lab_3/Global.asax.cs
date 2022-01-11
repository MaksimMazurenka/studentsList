using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace Lab_3
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RegisterRoutes(RouteTable.Routes);
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            var formatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            formatter.SerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        void RegisterRoutes(RouteCollection routes)
        {
            routes.MapPageRoute(
                routeName: "DefaultToHTML",
                routeUrl: "search",
                physicalFile: "~/index.html",
                checkPhysicalUrlAccess: false,
                defaults: new RouteValueDictionary(),
                constraints: new RouteValueDictionary { {  "placeholder", "" } }
            );

        }
    }
}
