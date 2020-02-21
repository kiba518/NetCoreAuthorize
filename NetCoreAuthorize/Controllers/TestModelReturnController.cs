using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Comm;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Utility;

namespace NetCoreAuthorize.Controllers
{
   
    public class TestModelReturnController : Controller
    {
        SessionHelper sessionHelper = new SessionHelper();
        

        [HttpGet]
        public User ModelReturn()
        {
            User user = new User();
            user.NickName = null;
            user.UserName = "kiba518";
            return user;
        }
        [HttpGet]
        public JsonResult ModelJsonResult()
        {
            User user = new User();
            user.NickName = null;
            user.UserName = "kiba518";
            CustomJsonResult customJsonResult = new CustomJsonResult();
            customJsonResult.Value = user;
            return customJsonResult;
        }
    }
}
