using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Comm;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Utility
{
    /// <summary>
    /// 当MvcBaseController无法满足验证时，或有特殊Controller需要特殊验证时，可以使用和修改下面的Attribute
    /// 用法[AuthorizeFilter("system")] 或[AuthorizeFilter("system,manager")]权限名组合
    /// </summary>
    public class AuthorizeFilterAttribute : ActionFilterAttribute
    {
        public AuthorizeFilterAttribute() { }

        public AuthorizeFilterAttribute(string authorize)
        {
            this.Authorize = authorize;
        }

        /// <summary>
        /// 权限字符串，例如 organization:user:view
        /// </summary>
        public string Authorize { get; set; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            bool hasPermission = false;//是否允许访问

            User user = GetUserByToken();
            if (user == null || user.UserId == 0)
            {
                #region 没有登录
                if (context.HttpContext.Request.IsAjaxRequest())//可以通过这个判断来拒绝Ajax请求，这里可以活用
                {
                    MessageData obj = new MessageData();
                    obj.Message = "抱歉，没有登录或登录已超时";
                    context.Result = new CustomJsonResult { Value = obj };
                    return;
                }
                else
                {
                    context.Result = new RedirectResult("~/Home/Login");
                    return;
                }
                #endregion
            }
            else
            {
                //可以在这里做一些权限判断 比如 我们接受的authorize字符串，可以设置为权限名，这样，在这里可以通过权限名来查询数据库权限，来做相应的操作     
             
                if (!string.IsNullOrEmpty(Authorize))
                {
                    string[] authorizeList = Authorize.Split(',');
                    //权限获取逻辑
                    if (true)
                    {
                        hasPermission = true;

                        // 特殊action判断
                        if (context.RouteData.Values["Action"].ToString() == "Special")
                        {
                            //权限获取逻辑 
                            hasPermission = false;

                        }
                        //Id判断
                        //var id = context.HttpContext.Request.Form["Id"]; 
                        //if (id.ParseToLong() < 0)
                        //{
                        //    hasPermission = false;
                        //}
                        
                    }
                    if (!hasPermission)
                    {
                        if (context.HttpContext.Request.IsAjaxRequest())
                        {
                            MessageData obj = new MessageData();
                            obj.Message = "抱歉，没有权限";
                            context.Result = new CustomJsonResult { Value = obj };
                        }
                        else
                        {
                            context.Result = new RedirectResult("~/Home/NoPermission");
                        }
                    }
                }
                else
                {
                    hasPermission = true;
                }

                if (hasPermission)
                {
                    var resultContext = await next();
                }
            }
        }
        //该获取认证token的方法，复制于MvcBaseController,实战中，需要使用应用所需的认证
        private User GetUserByToken(string tokenName = "")
        {
            IHttpContextAccessor hca = GlobalContext.ServiceProvider?.GetService<IHttpContextAccessor>();
            User user = null;
            string token = string.Empty;
            switch (GlobalContext.SystemConfig.UserTokenType)
            {
                case "Cookie":
                    if (hca.HttpContext != null)
                    {
                        token = new CookieHelper().GetCookie(GlobalContext.SystemConfig.UserTokenName);
                    }
                    break;
                case "Session":
                    if (hca.HttpContext != null)
                    {
                        token = new SessionHelper().GetSession(GlobalContext.SystemConfig.UserTokenName);
                    }
                    break;
                case "Other"://类型为Other时，不使用配置里的TokenName
                    token = tokenName;
                    break;
            }
            if (string.IsNullOrEmpty(token))
            {
                return user;
            }
            token = token.Trim('"');
            user = CacheHelper.GetCache<User>(token);//先从缓存中取，缓存中没有去数据库中取
            if (user == null)
            {
                user = GetDBUserInfo(token);
                if (user != null)
                {
                    CacheHelper.AddCache(token, user);
                }
            }
            return user;
        }
        private User GetDBUserInfo(string token)
        {
            User op = new User();
            op.UserName = "kiba518";
            //实战中，用户信息该去数据取token在用户表中可以是用户名，也可以是一个单独的token字段
            return op;
        }
    }
}
