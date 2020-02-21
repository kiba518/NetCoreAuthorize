using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreAuthorize.Authorize
{
    public class AdultAuthorizationHandler : AuthorizationHandler<AdultPolicyRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdultPolicyRequirement requirement)
        {
            //获取当前http请求的context对象
            var mvcContext = context.Resource as AuthorizationFilterContext;
            //以下代码都不是必须的，只是展示一些使用方法，你可以选择使用

            //......
            //var age = mvcContext.HttpContext.Request.Query.FirstOrDefault(u => u.Key == "age");
            //if (age.Value.Count <= 0 || Convert.ToInt16(age.Value[0]) < requirement.Age)
            var age = 16;
            if (age < requirement.Age)
            {
                context.Fail();
            }
            else
            {
                //通过验证，这句代码必须要有
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }

}
