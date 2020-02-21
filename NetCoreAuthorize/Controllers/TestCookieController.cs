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
   
    public class TestCookieController : Controller
    {
        public CookieHelper cookieHelper = new CookieHelper();
        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            return base.OnActionExecutionAsync(context, next);
        }

        [HttpGet]
        public IActionResult WriteCookie()
        {
            cookieHelper.WriteCookie("kiba", "518");
            string getCookieStr = cookieHelper.GetCookie("kiba");
            return View();
            
        }
        //[HttpGet]
        //public string GetCookie()
        //{
        //    string getCookieStr = cookieHelper.GetCookie("kiba");
        //    return $"Cookie内容：{getCookieStr}";
        //}
        
        /// <summary>
        /// 写cookie值
        /// </summary>
        /// <param name="sName">名称</param>
        /// <param name="sValue">值</param>
        /// <param name="domain">www.baidu.com</param> 
        /// <param name="path">user,约定当前Cookie只可以在www.baidu.com/user下使用</param> 
        public void WriteCookie(string sName, string sValue, string domain = "", string path = "")
        {
            
            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddDays(30);
            option.IsEssential = true;//指定当前Cookie为当前应用必要的Cookie
            if (domain != "")
            { option.Domain = domain; }
            if (path != "")
            { option.Path = path; }
            HttpContext.Response.Cookies.Append(sName, sValue, option); //HttpContext
        }
    }
}
