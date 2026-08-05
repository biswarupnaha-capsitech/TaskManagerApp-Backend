using Capsitech;
using Capsitech.Data.MongoDB;
using Capsitech.Extensions;
using Capsitech.Storage;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Projects.Common
{
    public class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Database configuration
        /// </summary>
        protected readonly DBConfiguration _dbConfig;

        protected readonly AzureBlobClient _azureBlobClient;

        /// <summary>
        /// Created instance of controller base class
        /// </summary>
        /// <param name="dbConfig"></param>
        public ApiControllerBase(DBConfiguration dbConfig)
        {
        _dbConfig = dbConfig;
        }
        public ApiControllerBase(DBConfiguration dbConfig, AzureBlobClient azureBlobClient)
        {
            _dbConfig = dbConfig;
            _azureBlobClient = azureBlobClient;
        }

        /// <summary>
        /// Validate input model and add error messages to the response
        /// </summary>
        /// <param name="response"><see cref="IApiResponse"/> object to hold error messages</param>
        /// <returns><see cref="bool"/></returns>
        protected virtual bool ValidateModel(IApiResponse response)
        {
            if (!ModelState.IsValid)
            {
                if (ModelState.Count > 0)
                    ModelState.Values.SelectMany(v => v.Errors).Foreach(e =>
                    {
                        response.AddError(e.ErrorMessage);
                    });
                else
                    response.AddError("Please check the supplied values");
            }
            return true;
        }        
        
        protected string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
        protected string GetUserAgent()
        {
            if (Request.Headers.ContainsKey("User-Agent"))
                return Request.Headers["User-Agent"].ToString();
            return null;
        }
    }
}
