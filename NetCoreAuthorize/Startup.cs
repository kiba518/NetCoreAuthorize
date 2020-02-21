using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetCoreAuthorize.Authorize;
using NetCoreAuthorize.DI;
using Utility;

namespace NetCoreAuthorize
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddControllersWithViews();
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();//增加异常过滤器 与 netframework不同， 这里将异常单独过滤了，而netframework是在一起过滤，在内部单独处理
                options.ModelMetadataDetailsProviders.Add(new ModelBindingMetadataProvider());//设置实体provider，控制实体显示，empty和whitespace字符串转换成null,只对元数据Metadata生效，这里设置为不强制转换
            }).AddNewtonsoftJson();//通过nuget引入Microsoft.AspNetCore.Mvc.NewtonsoftJson，增加AddNewtonsoftJson方法，这样controller就可以接受jsonresult返回值了

            //services.AddControllers();
            //services.AddMvc();
            #region 添加支持Session的中间件 
            services.AddSession(o =>
            {
                o.IdleTimeout = TimeSpan.FromSeconds(1800);//session空闲时间1800秒
            });
            #endregion
            #region 支持Cookie GDPR规范
            //泛型类型为指定当前要配置的类型，匿名委托里将得到该类型的实例，可以调用其属性配置
            //CookiePolicyOptions类用于配置Cookie政策
            //services.Configure<CookiePolicyOptions>(options =>
            //{ 
            //    /*这个属性是一个很复杂的判断，最终他会影响是否将cookie发送给客户端 fasle为发送
            //     *暂时理解为，系统将我们自定义的Cookie认为是非必要Cookie，所以，发送自定义Cookie会被系统进行重重验证，因此造成Cookie发送失败
            //     *在这里设置为fales，将取消一些验证
            //     *更好的解决方案是，直接在创建Cookie的时候，设置它的属性IsEssential为true，设置成当前应用必要的Cookie
            //    */
            //    options.CheckConsentNeeded = context => false;
            //    /*使当前站点接受外来Cookie，下面这篇文章介绍的很好
            //     https://blog.csdn.net/weixin_44269886/article/details/102459425*/
            //    options.MinimumSameSitePolicy = SameSiteMode.None;

            //});

            #endregion
            services.AddHttpContextAccessor();//添加内容访问者的默认实例，使其他地方可以通过它得到HttpContext，例如CookieHelper

            GlobalContext.SystemConfig = Configuration.GetSection("SystemConfig").Get<SystemConfig>();
            GlobalContext.Services = services;
            GlobalContext.ServiceProvider = services.BuildServiceProvider();
            GlobalContext.Configuration = Configuration;

            #region 依赖注入
            services.AddTransient<IKiba, Kiba>(); //依赖注入 每一次都会创建一个新的实例。
            services.AddTransient<IKiba2, Kiba2>(); //依赖注入
            /* services.AddScoped<IKiba, Kiba>(); //依赖注入 在同一个Scope内只初始化一个实例
            services.AddSingleton<IKiba, Kiba>(); //依赖注入 整个应用程序生命周期以内只创建一个实例，相当于一个静态类。*/
            #endregion




            #region Authorize 测试未通过
            //使用时在Controller上加 [Authorize("Adult1")] 类似于AuthorizeFilter，同样做权限认证，AdultAuthorizationHandler将进行验证处理
            //参考网站https://blog.csdn.net/Free_Wind22/article/details/80802990
            services.AddAuthorization(options =>
            {
                //测试未通过，TestAddAuthorizeController提示找不到Policy 而且据说他是放在controller上的，不是放在Action上的
                options.AddPolicy("Adult1", policy =>
                  {
                      policy.Requirements.Add(new AdultPolicyRequirement(12)); policy.Build();
                  }
                );
                options.AddPolicy("Adult2", policy =>
                    policy.Requirements.Add(new AdultPolicyRequirement(18)));
                //options.DefaultPolicy = options.GetPolicy("Adult1");
            });
            #endregion

            #region 通过AddOptions进行依赖注入
            services.AddOptions();//具体做什么不清楚，但极有可能是，触发Configure里自定义配置的action的，比如cookie配置的策略，因为AddOptions的源代码里有触发这些Action的方法，但构造函数没调用，不确定什么时候调用
            //services.Configure<SystemConfig>(p => p = systemConfig); //Configure是取出当前配置的内容，然后进行修改，该配置在依赖注入之前
            IOptions<SystemConfig> optionsAccessor = GlobalContext.ServiceProvider.GetService<IOptions<SystemConfig>>();//IOptions取值
            SystemConfig systemConfig = optionsAccessor.Value;
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
           
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            } 

            app.UseHttpsRedirection();
            #region 设置HTTP可访问的 静态资源文件，可以指定应用程序目录外的目录 C:/abc
            string resource = Path.Combine(env.ContentRootPath, "Resource");//追加一个页面可访问的资源地址（例如图片）
            if (!Directory.Exists(resource))
            {
                Directory.CreateDirectory(resource);
            }
            app.UseStaticFiles(
                new StaticFileOptions
                {
                    RequestPath = "/Resource",//在页面中访问该静态路径的名称，例如~/Resource/kiba.jpg
                    FileProvider = new PhysicalFileProvider(resource),
                    OnPrepareResponse = GlobalContext.SetCacheControl//设置图片在本地缓存的时间，这里设置为1年，缓存过期前，如果资源没有修改，刷新页面还使用本地资源，不再调用网络资源
                }
            );
            #endregion
            app.UseRouting();
            app.UseAuthentication();
            
            app.UseAuthorization();//据说要在 UseRouting之后 UseEndpoints之前


            #region Session启用
            app.UseSession();//不启用，session写入不进去
            #endregion

            #region Cookie 策略 使用开启
            //app.UseCookiePolicy();
            //在使用Cookie政策的时候，可以如下设置，未测试
            //app.UseCookiePolicy(new CookiePolicyOptions
            //{
            //    CheckConsentNeeded = _ => true,
            //    HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.None,
            //    MinimumSameSitePolicy = SameSiteMode.Strict,
            //    Secure = CookieSecurePolicy.SameAsRequest,
            //    OnAppendCookie = (context) =>
            //    {
            //        context.IssueCookie = true;
            //    },
            //    OnDeleteCookie = (context) =>
            //    {
            //    }
            //});
            #endregion
            #region UseEndpoints
            // webapi的默认UseEndpoints
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                   name: "areas",
                   pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");//需要测试
            });
            #endregion
            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //       name: "areas",
            //       template: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            //     );

            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{id?}");
            //});
        }
    }
}
