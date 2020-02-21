using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreAuthorize.Authorize
{
    public class AdultPolicyRequirement : IAuthorizationRequirement
    {
        public int Age { get; }
        public AdultPolicyRequirement(int age)
        {
            //年龄限制
            this.Age = age;
        }
    }

}
