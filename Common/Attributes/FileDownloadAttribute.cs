using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace TaskManager.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class FileDownloadAttribute : ActionFilterAttribute
    {
        public FileDownloadAttribute(string cookieName = "fileDownload", string cookiePath = "/")
        {
            CookieName = cookieName;
            CookiePath = cookiePath;
        }

        public string CookieName { get; set; }

        public string CookiePath { get; set; }

        /// <summary>
        /// If the current response is a FileResult (an MVC base class for files) then write a
        /// cookie to inform jquery.fileDownload that a successful file download has occured
        /// </summary>
        /// <param name="filterContext"></param>
        private void CheckAndHandleFileResult(ActionExecutedContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            var response = httpContext.Response;

            if (filterContext.Result is FileResult)
            {
                //jquery.fileDownload uses this cookie to determine that a file download has completed successfully
                response.Cookies.Append(CookieName, "true", new Microsoft.AspNetCore.Http.CookieOptions { Path = CookiePath });
            }
            else
                //ensure that the cookie is removed in case someone did a file download without using jquery.fileDownload
                if (httpContext.Request.Cookies[CookieName] != null)
                    response.Cookies.Append(CookieName, "true", new Microsoft.AspNetCore.Http.CookieOptions { Path = CookiePath, Expires = DateTime.Now.AddYears(-1) });
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            CheckAndHandleFileResult(filterContext);

            base.OnActionExecuted(filterContext);
        }
    }
}