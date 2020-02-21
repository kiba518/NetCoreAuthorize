using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Utility;

namespace NetCoreAuthorize.Controllers
{
   
    public class TestOptionsController : Controller
    {
        
        [HttpGet]
        public string Options(IOptions<SystemConfig> options)
        {
             
            return $"kiba518{options.Value.ConnectionTimeout}";
        }
     
    }
}
