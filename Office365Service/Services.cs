using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Office365Service.Models;
using RestSharp;

namespace Office365Service
{
    /// <summary>
    /// Class <c> Services </c> handles the generation and refreshing op the OAuth 2.0 security BearerToken
    /// </summary>
    public class Services
    {
        public Token BearerToken = new Token();
        public Token RefreshAccesToken()
        {
            if (!string.IsNullOrEmpty(Constant.Tenant_Id))
            {
                RestClient restClient = new RestClient();
                RestRequest restRequest = new RestRequest();

                restRequest.AddParameter("client_id", Constant.Client_Id);
                restRequest.AddParameter("scope", Constant.Scopes);
                restRequest.AddParameter("grant_type", "client_credentials");
                restRequest.AddParameter("client_secret", Constant.Client_Secret);

                restClient.BaseUrl = new Uri($"https://login.microsoftonline.com/{Constant.Tenant_Id}/oauth2/v2.0/token");
                var response = restClient.Post(restRequest);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    BearerToken = JsonConvert.DeserializeObject<Token>(response.Content);
                }
            }
            return BearerToken;
        }
    }
}
