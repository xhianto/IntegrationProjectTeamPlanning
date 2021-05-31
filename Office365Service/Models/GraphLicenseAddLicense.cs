using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;


namespace Office365Service.Models

/// <summary>
/// Model Class to model Office 365 licenses. 
/// Attributes can be send to MS Graph API using the annotated Json properties.
/// </summary>
{
    public class GraphLicenseAddLicense
    {
        private string[] DisabledPlansValue = { };

        [JsonProperty("disabledPlans")]
        public string[] DisabledPlans
        {
            get
            {
                return DisabledPlansValue;
            }
            set
            {
                DisabledPlansValue = value;
            }
        }

        [JsonProperty("skuId")]
        public string SkuId { get; set; }
    }
}
