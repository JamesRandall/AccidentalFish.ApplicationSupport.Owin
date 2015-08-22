# AccidentalFish.ApplicationSupport.Owin

This project contains a couple of pieces of useful OWIN middleware: a HTTP logger and a small piece of middleware for adding a correlation ID. They can be used independently but also work well together. The project also contains Web API and MVC filters for moving HTTP headers into a CallContext (see note below for why this is necessary and can't be done directly from OWIN).

The project is split into four NuGet packages so as to bring in just the dependencies they need and prevent your project being polluted with unneeded packages. The NuGet packages are:

Package|Description
-------|-----------
AccidentalFish.ApplicationSupport.Owin|Contains the core middleware packages. To use the logger you also need a repository package.
AccidentalFish.ApplicationSupport.Owin.Azure|Azure Table Storage repository for the HTTP logger.
AccidentalFish.ApplicationSupport.Owin.WebApi|A filter for Web Api that takes a correlation ID header and pushes it into a CallContext.
AccidentalFish.ApplicationSupport.Owin.Mvc|A filter for MVC that takes a correlation ID header and pushes it into a CallContext.

Full API documentation can be found at:

https://jamesrandall.github.io/docs/accidentalfish.applicationsupport.owin/html/R_Project_Documentation.htm

## HTTP Logger

To capture all your request and response information the HTTP logger needs to be the first or second piece of middleware in the OWIN pipeline: first if you are not interested in a correlation ID, or second just after the correlation ID middleware if you are. Assuming you want to use Azure to store log information (recommended, it's cost effective for large amounts of data) then you add it using the app builder extension. The example below shows typical usage alongside the correlation ID middleware:

```
public class Startup
{
    public void Configuration(IAppBuilder appBuilder)
    {
        appBuilder.UseHttpCorrelator();
        appBuilder.UseAzureHttpLogger("DefaultEndpointsProtocol=https;AccountName=<accountname>;AccountKey=<accountkey>");
    }
}
```

As a privacy / security feature by default the logger will capture only minimal information: the URL but with any query parameters removed, the HTTP verb, a HTTP correlation ID if present, and the elapsed time of the request.

After adding this logger data is stored in two Azure tables: httprequestbydatedescending and httprequestbycorrelationid. In the former case the partition key is formed from the date and time the request was made and the granularity of the partition (daily, hourly, minute, second) can be controlled by an option on UseAzureHttpLogger to aid querying. In the latter case the partition key is the correlation ID.

To include query parameters with the URL then simply set captureRequestParams to true on UseAzureHttpLogger:

```
public class Startup
{
    public void Configuration(IAppBuilder appBuilder)
    {
        appBuilder.UseHttpCorrelator();
        appBuilder.UseAzureHttpLogger("DefaultEndpointsProtocol=https;AccountName=<accountname>;AccountKey=<accountkey>",
          captureRequestParams: true);
    }
}
```

Similarly to capture header information there are two string array parameters captureRequestHeaders and captureResponseHeaders. If these are empty, the default, then no header information is captured. To capture specific headers then simply specify the header names in the arrays or to capture all headers then use a wildcard. Example follows:

```
public class Startup
{
    public void Configuration(IAppBuilder appBuilder)
    {
        appBuilder.UseHttpCorrelator();
        appBuilder.UseAzureHttpLogger("DefaultEndpointsProtocol=https;AccountName=<accountname>;AccountKey=<accountkey>",
          captureRequestHeaders: new [] {"Connection", "Content-Type"}, // capture specific headers
          captureResponseHeaders:new [] { "*" } // capture all headers
        );
    }
}
```

By default the correlation ID referred to above is added, if not already present, to a header called correlation-id. This can be changed to a header name of your choosing or disabled by setting to null. The example below shows how to disable correlation-ids:

```
public class Startup
{
    public void Configuration(IAppBuilder appBuilder)
    {
        appBuilder.UseAzureHttpLogger("DefaultEndpointsProtocol=https;AccountName=<accountname>;AccountKey=<accountkey>",
          httpCorrelationHeaderKey:null
        );
    }
}
```

Note that without a correlation ID items will not be written to the httprequestbycorrelationid table.

Finally if you wish to avoid table naming conflicts or have a prefix based naming convention you can supply a prefix to the app builder extension that will prefix table names with a string of your choosing:

```
public class Startup
{
    public void Configuration(IAppBuilder appBuilder)
    {
        appBuilder.UseAzureHttpLogger("DefaultEndpointsProtocol=https;AccountName=<accountname>;AccountKey=<accountkey>",
          azurePrefix:"af"
        );
    }
}
```

### Using the logger with a different storage repository

Currently the logger only ships with an Azure Table Storage repository however implementing your own repository is simple. You need to create a class that implements the IHttpLoggerRepository interface and then supply that to the HttpLogger:

```
public class MyRepository : IHttpLoggerRepository
{
  public async Task Log(
      string uriToLog,
      bool didStripQueryParams,
      string verb,
      string correlationId,
      DateTimeOffset requestDateTime,
      long ellapsedMilliseconds,
      Dictionary<string, string[]> requestHeaders,
      Dictionary<string, string[]> responseHeaders)
  {
    ...
  }
}

public class Startup
{
    public void Configuration(IAppBuilder appBuilder)
    {
        appBuilder.UseHttpLogger(
          new MyRepository()
        );
    }
}
```

## Http Correlator

Usage of the http correlator middleware is shown above and it really is as simple as that:

```
public class Startup
{
    public void Configuration(IAppBuilder appBuilder)
    {
        appBuilder.UseHttpCorrelator();
    }
}
```

The only option is to change the name of the header to a different one. Note that if you are using this with the Http Logger then you also need to give the logger a matching name:

```
public class Startup
{
    public void Configuration(IAppBuilder appBuilder)
    {
        appBuilder.UseHttpCorrelator("my-correlation-header");
        appBuilder.UseAzureHttpLogger("DefaultEndpointsProtocol=https;AccountName=<accountname>;AccountKey=<accountkey>",
          httpCorrelationHeaderKey:"my-correlation-header"
        );
    }
}
```

## Filters

Correlation IDs are typically used to track events across multiple domains / servers. In order to do this your code will often need to grab hold of a correlation ID and it can be quite messy to pass this as a parameter on every method in your system. A way of dealing with this is to use the [CallContext](https://msdn.microsoft.com/en-us/library/system.runtime.remoting.messaging.callcontext(v=vs.110).aspx) class to share this information across a single http request end to end in your code. It's worth taking a little time to understand call contexts and their is an excellent [blog post on them here](http://blog.stephencleary.com/2013/04/implicit-async-context-asynclocal.html).

If you want to use correlation IDs in this manner in a typical IIS / Azure production environment then you need to set the call context from within the Web API / MVC environment as a new call context is created as OWIN hands over to Web API / MVC. This is not the case in an OWIN self hosted scenario but the filters supplied here will work in both contexts.

The respective Web Api and MVC NuGet packages contain a HttpCorrelatorAttribute class that derives from the respective ActionFilter. This will ensure that before your actions begin executing the correlation ID will be set in the call context.

The easiest way to use them is to add them as global filters.

In a typical Web Api project this can be done in App_Start\WebAPIConfig.cs by adding a line:
    config.Filters.Add(new HttpCorrelatorAttribute());
    
In a typical MVC project this can be done in App_Start\FilterConfig.cs by adding a line:
    filters.Add(new HttpCorrelatorAttribute());
    
By default the filters look for a header named correlation-id (the same default as the OWIN middleware) but both attributes take an optional constructor parameter that allows you to specify the HTTP header the correlation ID can be found in:
    filters.Add(new HttpCorrelatorAttribute("my-correlation-id"));

The call context data is set using the same key as the header name and can be subsequently retrieved using code such as:
    object correlationId = CallContext.LogicalGetData("correlation-id");
    
