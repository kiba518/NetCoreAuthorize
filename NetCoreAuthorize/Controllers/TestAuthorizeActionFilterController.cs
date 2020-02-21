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
   
    public class TestAuthorizeActionFilterController : MvcBaseController
    {
        
        [HttpGet]
        [AuthorizeFilter("system")]
        public string Authorize()
        {
            return "正常";
        }
      
    }
}
