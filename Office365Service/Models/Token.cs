using System;
using System.Collections.Generic;
using System.Text;

namespace Office365Service.Models
{
    public class Token
    {
        public string Token_type { get; set; }
        public int Expires_in { get; set; }
        public int Ext_expires_in { get; set; }
        public string Access_token { get; set; }
    }
}
