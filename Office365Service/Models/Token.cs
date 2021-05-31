using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

/// <summary>
/// Model Class to handle an Bearor Token, used for authenticating to the MS Graph API. 
/// </summary>
namespace Office365Service.Models
{
    public class Token
    {
        [JsonProperty("token_type")]
        public string Token_type { get; set; }
        [JsonProperty("expires_in")]
        public int Expires_in { get; set; }
        [JsonProperty("ext_expires_in")]
        public int Ext_expires_in { get; set; }
        [JsonProperty("access_token")]
        public string Access_token { get; set; }
        [JsonIgnore]
        public DateTime TimeStamp { get; set; }
    }
}
