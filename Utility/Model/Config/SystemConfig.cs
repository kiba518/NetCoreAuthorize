using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Utility
{
    public class SystemConfig
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// 数据库连接超时时间
        /// </summary>
        public int ConnectionTimeout { get; set; }
        /// <summary>
        /// 用户的Tokenl类型Cookies or Session or Other
        /// </summary>
        public string UserTokenType { get; set; }
        /// <summary>
        /// 用户的Token的名称 默认值UserTokenName
        /// </summary>
        public string UserTokenName { get; set; }
       
        

    }
}
