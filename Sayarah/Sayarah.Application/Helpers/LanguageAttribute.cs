using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Sayarah.Application.Helpers
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LanguageAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _headerName;

        public LanguageAttribute(string headerName)
        {
            _headerName = headerName;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var headerValue = httpContext.Request.Headers[_headerName].ToString();

            if (!string.IsNullOrEmpty(headerValue))
            {
                headerValue = headerValue == "ar" ? "ar-EG" : headerValue;
                var culture = new CultureInfo(headerValue);

                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }

            await next();
        }
    }
}