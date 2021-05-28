using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
    public class User
    {
        [JsonProperty("accountEnabled")]
        public bool AccountEnabled { get; set; }
        [JsonProperty("mailNickname")]
        public string MailNickname { get; set; }
        [JsonProperty("passwordProfile")]
        public UserPasswordProfile PasswordProfile { get; set; }
        [JsonProperty("givenName")]
        public string GivenName { get; set; }
        [JsonProperty("surName")]
        public string SurName { get; set; }
        [JsonProperty("jobTitle")]
        public string JobTitle { get; set; }
        [JsonProperty("userPrincipalName")]
        public string UserPrincipalName { get; set; }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        private string[] businessPhonesValue = { };
        [JsonProperty("businessPhones")]
        public string[] BusinessPhones {
            get
            {
                return businessPhonesValue;
            }
            set
            {
                businessPhonesValue = value;
            }
        }
        [JsonProperty("mail")]
        public string Mail { get; set; }
        [JsonProperty("mobilePhone")]
        public object MobilePhone { get; set; }
        [JsonProperty("officeLocation")]
        public object OfficeLocation { get; set; }
        [JsonProperty("preferredLanguage")]
        public object PreferredLanguage { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("usageLocation")]
        public string UsageLocation { get; set; }
    }
}
