using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using NetCoreAuthorize.DI;
using Utility;

namespace NetCoreAuthorize.Controllers
{
   
    public class TestDIController : Controller
    {
        public string name; public string name2;
        public TestDIController(IKiba _kiba, IKiba2 _kiba2)
        {
              name = _kiba.GetName();
              name2 = _kiba2.GetName();
        }

        [HttpGet]
        public string DI()
        {
             
            return $"name{name} ==name2{name2}";
        }
      
    }
}
