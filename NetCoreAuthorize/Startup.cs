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
                options.Filters.Add<GlobalExceptionFilter>();//�����쳣������ �� netframework��ͬ�� ���ｫ�쳣���������ˣ���netframework����һ����ˣ����ڲ���������
                options.ModelMetadataDetailsProviders.Add(new ModelBindingMetadataProvider());//����ʵ��provider������ʵ����ʾ��empty��whitespace�ַ���ת����null,ֻ��Ԫ����Metadata��Ч����������Ϊ��ǿ��ת��
            }).AddNewtonsoftJson();//ͨ��nuget����Microsoft.AspNetCore.Mvc.NewtonsoftJson������AddNewtonsoftJson����������controller�Ϳ��Խ���jsonresult����ֵ��

            //services.AddControllers();
            //services.AddMvc();
            #region ���֧��Session���м�� 
            services.AddSession(o =>
            {
                o.IdleTimeout = TimeSpan.FromSeconds(1800);//session����ʱ��1800��
            });
            #endregion
            #region ֧��Cookie GDPR�淶
            //��������Ϊָ����ǰҪ���õ����ͣ�����ί���ｫ�õ������͵�ʵ�������Ե�������������
            //CookiePolicyOptions����������Cookie����
            //services.Configure<CookiePolicyOptions>(options =>
            //{ 
            //    /*���������һ���ܸ��ӵ��жϣ���������Ӱ���Ƿ�cookie���͸��ͻ��� fasleΪ����
            //     *��ʱ���Ϊ��ϵͳ�������Զ����Cookie��Ϊ�ǷǱ�ҪCookie�����ԣ������Զ���Cookie�ᱻϵͳ����������֤��������Cookie����ʧ��
            //     *����������Ϊfales����ȡ��һЩ��֤
            //     *���õĽ�������ǣ�ֱ���ڴ���Cookie��ʱ��������������IsEssentialΪtrue�����óɵ�ǰӦ�ñ�Ҫ��Cookie
            //    */
            //    options.CheckConsentNeeded = context => false;
            //    /*ʹ��ǰվ���������Cookie��������ƪ���½��ܵĺܺ�
            //     https://blog.csdn.net/weixin_44269886/article/details/102459425*/
            //    options.MinimumSameSitePolicy = SameSiteMode.None;

            //});

            #endregion
            services.AddHttpContextAccessor();//������ݷ����ߵ�Ĭ��ʵ����ʹ�����ط�����ͨ�����õ�HttpContext������CookieHelper

            GlobalContext.SystemConfig = Configuration.GetSection("SystemConfig").Get<SystemConfig>();
            GlobalContext.Services = services;
            GlobalContext.ServiceProvider = services.BuildServiceProvider();
            GlobalContext.Configuration = Configuration;

            #region ����ע��
            services.AddTransient<IKiba, Kiba>(); //����ע�� ÿһ�ζ��ᴴ��һ���µ�ʵ����
            services.AddTransient<IKiba2, Kiba2>(); //����ע��
            /* services.AddScoped<IKiba, Kiba>(); //����ע�� ��ͬһ��Scope��ֻ��ʼ��һ��ʵ��
            services.AddSingleton<IKiba, Kiba>(); //����ע�� ����Ӧ�ó���������������ֻ����һ��ʵ�����൱��һ����̬�ࡣ*/
            #endregion




            #region Authorize ����δͨ��
            //ʹ��ʱ��Controller�ϼ� [Authorize("Adult1")] ������AuthorizeFilter��ͬ����Ȩ����֤��AdultAuthorizationHandler��������֤����
            //�ο���վhttps://blog.csdn.net/Free_Wind22/article/details/80802990
            services.AddAuthorization(options =>
            {
                //����δͨ����TestAddAuthorizeController��ʾ�Ҳ���Policy ���Ҿ�˵���Ƿ���controller�ϵģ����Ƿ���Action�ϵ�
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

            #region ͨ��AddOptions��������ע��
            services.AddOptions();//������ʲô������������п����ǣ�����Configure���Զ������õ�action�ģ�����cookie���õĲ��ԣ���ΪAddOptions��Դ�������д�����ЩAction�ķ����������캯��û���ã���ȷ��ʲôʱ�����
            //services.Configure<SystemConfig>(p => p = systemConfig); //Configure��ȡ����ǰ���õ����ݣ�Ȼ������޸ģ�������������ע��֮ǰ
            IOptions<SystemConfig> optionsAccessor = GlobalContext.ServiceProvider.GetService<IOptions<SystemConfig>>();//IOptionsȡֵ
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
            #region ����HTTP�ɷ��ʵ� ��̬��Դ�ļ�������ָ��Ӧ�ó���Ŀ¼���Ŀ¼ C:/abc
            string resource = Path.Combine(env.ContentRootPath, "Resource");//׷��һ��ҳ��ɷ��ʵ���Դ��ַ������ͼƬ��
            if (!Directory.Exists(resource))
            {
                Directory.CreateDirectory(resource);
            }
            app.UseStaticFiles(
                new StaticFileOptions
                {
                    RequestPath = "/Resource",//��ҳ���з��ʸþ�̬·�������ƣ�����~/Resource/kiba.jpg
                    FileProvider = new PhysicalFileProvider(resource),
                    OnPrepareResponse = GlobalContext.SetCacheControl//����ͼƬ�ڱ��ػ����ʱ�䣬��������Ϊ1�꣬�������ǰ�������Դû���޸ģ�ˢ��ҳ�滹ʹ�ñ�����Դ�����ٵ���������Դ
                }
            );
            #endregion
            app.UseRouting();
            app.UseAuthentication();
            
            app.UseAuthorization();//��˵Ҫ�� UseRouting֮�� UseEndpoints֮ǰ


            #region Session����
            app.UseSession();//�����ã�sessionд�벻��ȥ
            #endregion

            #region Cookie ���� ʹ�ÿ���
            //app.UseCookiePolicy();
            //��ʹ��Cookie���ߵ�ʱ�򣬿����������ã�δ����
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
            // webapi��Ĭ��UseEndpoints
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
                   pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");//��Ҫ����
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
