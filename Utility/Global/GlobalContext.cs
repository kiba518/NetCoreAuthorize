using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
 

namespace Utility
{
    public class GlobalContext
    {
        /// <summary>
        ///  
        /// </summary>
        public static IServiceCollection Services { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static IServiceProvider ServiceProvider { get; set; }

        public static IConfiguration Configuration { get; set; }

        public static IHostingEnvironment HostingEnvironment { get; set; }
        public static SystemConfig SystemConfig { get; set; }
        public static string GetVersion()
        {
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            return version.Major + "." + version.Minor;
        }

        /// <summary>
        /// 设置缓存为一年
        /// </summary>
        /// <param name="context"></param>
        public static void SetCacheControl(StaticFileResponseContext context)
        {
            int second = 365 * 24 * 60 * 60;
            //max-age表示当访问此网页后的max-age秒内再次访问不会去服务器请求，其功能与Expires类似，只是Expires是根据某个特定日期值做比较。
            //一但缓存者自身的时间不准确.则结果可能就是错误的，而max-age,显然无此问题.。Max-age的优先级也是高于Expires的。
            context.Context.Response.Headers.Add("Cache-Control", new[] { "public,max-age=" + second });
            context.Context.Response.Headers.Add("Expires", new[] { DateTime.UtcNow.AddYears(1).ToString("R") }); // Format RFC1123
        }
    }
}
