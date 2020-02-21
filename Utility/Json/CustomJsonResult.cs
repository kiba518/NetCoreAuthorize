using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public class CustomJsonResult : JsonResult
    {
        public CustomJsonResult() : base(string.Empty)
        { }

        public override void ExecuteResult(ActionContext context)
        {
            this.ContentType = "text/json;charset=utf-8;";

            JsonSerializerSettings jsonSerizlizerSetting = new JsonSerializerSettings();
            jsonSerizlizerSetting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;//忽略循环

            string json = JsonConvert.SerializeObject(Value, Formatting.None, jsonSerizlizerSetting);
            Value = json;
            base.ExecuteResult(context);
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            this.ContentType = "text/json;charset=utf-8;";

            JsonSerializerSettings jsonSerizlizerSetting = new JsonSerializerSettings();
            jsonSerizlizerSetting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;//忽略循环

            string json = JsonConvert.SerializeObject(Value, Formatting.None, jsonSerizlizerSetting);
            Value = json.ToJObject();
            return base.ExecuteResultAsync(context);
        }
    }


}
