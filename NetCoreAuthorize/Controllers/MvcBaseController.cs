using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Comm;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Utility;

namespace NetCoreAuthorize.Controllers
{
   
    public abstract class MvcBaseController : Controller
    {
        /// <summary>
        /// 每次请求都或截获，包括刷新
        /// 优先被触发，执行完 next()后，AuthorizeFilterAttribute才会被触发
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start(); 
           
            User user = GetUserByToken(); 

            if (context.HttpContext.Request.Method.ToUpper() == "POST")
            {
                if (false)//过滤条件 根据user来过滤条件
                {
                    MessageData obj = new MessageData();
                    obj.Message = "提示信息";
                    context.Result = new CustomJsonResult { Value = obj };
                    return;
                }
            }
            if (context.HttpContext.Request.Method.ToUpper() == "GET")
            {
                if (false)//过滤条件 根据user来过滤条件
                {
                    MessageData obj = new MessageData();
                    obj.Message = "提示信息";
                    context.Result = new CustomJsonResult { Value = obj };
                    return;
                }
            }
            //等待Controller执行结束
            var resultContext = await next();

            sw.Stop();
            string ip = NetHelper.Ip;
            MessageLogin msgLogin = new MessageLogin();
            var areaName = context.RouteData.DataTokens["area"] + "/";
            var controllerName = context.RouteData.Values["controller"] + "/";
            string action = context.RouteData.Values["Action"].ToString();
            string currentUrl = "/" + areaName + controllerName + action;

            if (action.ParseToString().ToLower() != "GetServerJson".ToLower() && action.ParseToString().ToLower() != "Error".ToLower())
            {
                #region 获取请求参数
                switch (context.HttpContext.Request.Method.ToUpper())
                {
                    case "GET":
                        msgLogin.ExecuteParam = context.HttpContext.Request.QueryString.Value.ParseToString();
                        break;

                    case "POST":
                        Dictionary<string, string> param = new Dictionary<string, string>();
                        foreach (var item in context.ActionDescriptor.Parameters)
                        {
                            var itemType = item.ParameterType;
                            if (itemType.IsClass && itemType.Name != "String")
                            {
                                PropertyInfo[] infos = itemType.GetProperties();
                                foreach (PropertyInfo info in infos)
                                {
                                    if (info.CanRead)
                                    {
                                        var propertyValue = context.HttpContext.Request.Form[info.Name];
                                        if (!param.ContainsKey(info.Name))
                                        {
                                            if (!string.IsNullOrEmpty(propertyValue))
                                            {
                                                param.Add(info.Name, propertyValue);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (param.Count > 0)
                        {
                            msgLogin.ExecuteUrl += context.HttpContext.Request.QueryString.Value.ParseToString(); 
                            string tempStrParam = JsonConvert.SerializeObject(param);
                            msgLogin.ExecuteParam = tempStrParam.Substring(0, tempStrParam.Length > 8000 ? 8000 : tempStrParam.Length);
                        }
                        else
                        {
                            msgLogin.ExecuteParam = context.HttpContext.Request.QueryString.Value.ParseToString();
                        }
                        break;
                }
                #endregion

                #region 异常获取
                StringBuilder sbException = new StringBuilder();
                if (resultContext.Exception != null)
                {
                    Exception exception = resultContext.Exception;
                    sbException.AppendLine(exception.Message);
                    while (exception.InnerException != null)
                    {
                        sbException.AppendLine(exception.InnerException.Message);
                        exception = exception.InnerException;
                    }
                    sbException.AppendLine(resultContext.Exception.StackTrace);
                    msgLogin.LogStatus = 0;
                }
                else
                {
                    msgLogin.LogStatus = 1;
                }
                #endregion

                #region 日志实体                  
                if (user != null)
                {
                    msgLogin.UserId = user.UserId;
                }

                msgLogin.ExecuteTime = sw.ElapsedMilliseconds.ParseToInt();//耗时
                msgLogin.IpAddress = ip;
                msgLogin.ExecuteUrl = currentUrl.Replace("//", "/");
                string tempStrEx = sbException.ToString();
                msgLogin.ExecuteResult = tempStrEx.Substring(0, tempStrEx.Length> 4000 ? 4000 : tempStrEx.Length);
                #endregion

                #region 将msgLogin信息写进数据库 或 记录日志
                Action taskAction = async () =>
                {
                    // 让底层不用获取HttpContext
                    msgLogin.UserId = msgLogin.UserId ?? 0;

                    // 耗时的任务异步完成
                    msgLogin.IpLocation = IpLocationHelper.GetIpLocation(ip);//网络地址查询，内外ip没有地址
                    #region 这里将登陆信息保存进数据库 或写入日志 可以使用await
                    #endregion
                };
                try
                { 
                    Task task = new Task(taskAction);
                    task.Start();
                }
                catch
                {

                }
                #endregion

            }
        }

        private User GetUserByToken(string tokenName = "")
        {
            User user = null;
            string token = string.Empty;
            switch (GlobalContext.SystemConfig.UserTokenType)
            {
                case "Cookie":
                    if (HttpContext != null)
                    {
                        token = new CookieHelper().GetCookie(GlobalContext.SystemConfig.UserTokenName);
                    }
                    break;
                case "Session":
                    if (HttpContext != null)
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
