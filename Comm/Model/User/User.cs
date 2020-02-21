using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comm
{
    public class User
    {
        public long? UserId { get; set; }
        public int? UserStatus { get; set; }
        public int? IsOnline { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string WebToken { get; set; }
        public string ApiToken { get; set; }
        public int? IsSystem { get; set; }
        public string Profile { get; set; } 
      
    } 
}
