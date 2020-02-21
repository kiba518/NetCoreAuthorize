using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Utility;

namespace NetCoreAuthorize.Controllers
{
   
    public class TestSessionController : Controller
    {
        SessionHelper sessionHelper = new SessionHelper();
        //public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        //{
        //    return base.OnActionExecutionAsync(context, next);
        //}

        [HttpGet]
        public string WriteSession()
        {
            sessionHelper.WriteSession("kiba", "518");
            return "kiba518";
        }
        [HttpGet]
        public string GetSession()
        {
            string sessionStr = sessionHelper.GetSession("kiba");
            return sessionStr;
        }
    }
}
