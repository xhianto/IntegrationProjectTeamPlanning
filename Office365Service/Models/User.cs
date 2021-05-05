using System;
using System.Collections.Generic;
using System.Text;

namespace Office365Service.Models
{
    public class User
    {
        public string Odatacontext { get; set; }
        public string[] BusinessPhones { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public object JobTitle { get; set; }
        public string Mail { get; set; }
        public object MobilePhone { get; set; }
        public object OfficeLocation { get; set; }
        public object PreferredLanguage { get; set; }
        public string Surname { get; set; }
        public string UserPrincipalName { get; set; }
        public string Id { get; set; }

    }
}
