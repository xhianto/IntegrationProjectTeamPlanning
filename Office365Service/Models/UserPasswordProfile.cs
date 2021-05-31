using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
/// <summary>
/// Model Class to handle the user password. 
/// Attributes can be send to MS Graph API using the annotated Json properties.
/// </summary>
    public class UserPasswordProfile
    {
        [JsonProperty("forceChangePasswordNextSignIn")]
        public bool ForceChangePasswordNextSignIn { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
