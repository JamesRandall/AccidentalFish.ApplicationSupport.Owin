using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace AccidentalFish.ApplicationSupport.Owin.WebApi
{
    /// <summary>
    /// This action filter attribute works with the HttpCorrelator (and HttpLogger) in AccidentalFish.ApplicationSupport.Owin
    /// to pull the correlation ID out of the header and set it in the call context.
    /// 
    /// The reason this is necessary is that when hosting Web API in IIS a new call context scope is created between OWIN and
    /// the controller being invoked and therefore any call context values set in the OWIN middleware are not visible within Web API
    /// controllers and beyond.
    /// 
    /// The easiest way to use this is to add it to the global filter set which in a typical Web API project is configured
    /// in App_Start\WebAPiConfig.cs:
    ///     config.Filters.Add(new HttpCorrelatorAttribute());
    /// </summary>
    public class HttpCorrelatorAttribute : ActionFilterAttribute
    {
        private readonly string _correlationIdName;

        public HttpCorrelatorAttribute()
        {
            _correlationIdName = "correlation-id";
        }

        public HttpCorrelatorAttribute(string correlationIdName)
        {
            _correlationIdName = correlationIdName;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            IEnumerable<string> headerValues = HttpContext.Current.Request.Headers.GetValues(_correlationIdName);
            if (headerValues != null && headerValues.Any())
            {
                CallContext.LogicalSetData(_correlationIdName, headerValues.First());
            }
        }
    }
}
