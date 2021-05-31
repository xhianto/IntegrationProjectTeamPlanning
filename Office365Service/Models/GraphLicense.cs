using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{

/// <summary>
/// Service Class to manage a list of Office 365 licenses. 
/// Attributes can be send to MS Graph API using the annotated Json properties.
/// </summary>
    public class GraphLicense
    {

        public GraphLicense()
        {
            AddLicenses = new List<GraphLicenseAddLicense>();
        }


        [JsonProperty("addLicenses")]
        public List<GraphLicenseAddLicense> AddLicenses { get; set; }



        private string[] RemoveLicensesValue = { };

        [JsonProperty("removeLicenses")]
        public string[] RemoveLicenses
        {
            get
            {
                return RemoveLicensesValue;
            }
            set
            {
                RemoveLicensesValue = value;
            }
        }
    }
}
