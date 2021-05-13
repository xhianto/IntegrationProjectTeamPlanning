using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
    public class UserPasswordProfile
    {
        [JsonProperty("forceChangePasswordNextSignIn")]
        public bool ForceChangePasswordNextSignIn { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
