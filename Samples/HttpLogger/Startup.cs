using System.Web.Http;
using AccidentalFish.ApplicationSupport.Owin;
using AccidentalFish.ApplicationSupport.Owin.Azure;
using Owin;

namespace HttpLogger
{
    public class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.UseAzureHttpLogger("DefaultEndpointsProtocol=https;AccountName=applicationsupport;AccountKey=/VxOZ9YSgtWlzbyvIelMH4QBaMdFc58DE0pgk/N1g7yoAMvyZy6lFfO5c7/AMcyTRU2SE4PNnVNUGoWWe1Zn8g==",
                captureRequestParams:true,
                captureRequestHeaders: new [] {"Connection"},
                captureResponseHeaders:new [] { "*" });

            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            
            appBuilder.UseWebApi(config);
        }
    }
}
