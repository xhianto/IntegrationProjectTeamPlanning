using System;
using System.Collections.Generic;
using System.Text;

namespace Office365Service.Models
{
    /// <summary>
    /// Class <c>Token</c> models a Bearer token for user authorisation, used by Service when Producer calls
    /// </summary>
    /// 
    public class Token
    {
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public int ext_expires_in { get; set; }
        public string access_token { get; set; }
    }
}
