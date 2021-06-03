using Newtonsoft.Json;
using Office365Service.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Office365Service
{
    public class UserService
    {
        Services services = new Services();
        MasterDBServices masterDBService = new MasterDBServices();
        Token BearerToken = new Token();

        /// <summary>
        /// Method posting an incoming new user into the MS Graph API.
        /// </summary>
        /// <param name="rabbitMQUser">New user sent by the RabbitMQ message broker</param>
        public void UserCreate(RabbitMQUser rabbitMQUser)
        {
            //Mock data om nieuw user aan te maken
            //rabbitMQUser = new RabbitMQUser();
            //rabbitMQUser.FirstName = "Steve";
            //rabbitMQUser.LastName = "Herman";
            //rabbitMQUser.Role = "Clown";

            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();

            User user = new User();
            //required
            user.AccountEnabled = true;
            user.DisplayName = rabbitMQUser.FirstName + " " + rabbitMQUser.LastName;
            user.MailNickname = rabbitMQUser.FirstName.Replace(' ', '.') + "." + rabbitMQUser.LastName;
            user.UserPrincipalName = user.MailNickname + "@ipwt3.onmicrosoft.com";
            user.Mail = user.UserPrincipalName;
            user.PasswordProfile = new UserPasswordProfile();
            user.PasswordProfile.ForceChangePasswordNextSignIn = false;
            user.PasswordProfile.Password = Constant.StandardPassword;
            user.UsageLocation = "BE";

            user.GivenName = rabbitMQUser.FirstName;
            user.SurName = rabbitMQUser.LastName;
            user.JobTitle = rabbitMQUser.Role;

            BearerToken = services.RefreshAccesToken();

            var json = JsonConvert.SerializeObject(user);
            restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            restRequest.AddJsonBody(json);
            Console.WriteLine(json);

            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users");
            var response = restClient.Post(restRequest);

            Console.WriteLine(response.StatusCode);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                GraphLicense licenses = new GraphLicense();
                GraphLicenseAddLicense license = new GraphLicenseAddLicense();
                license.SkuId = Constant.M365Lisence;
                licenses.AddLicenses.Add(license);
                var jsonlicense = JsonConvert.SerializeObject(licenses);

                var responseJson = response.Content;
                User responseUser = JsonConvert.DeserializeObject<User>(responseJson);
                Console.WriteLine(responseUser.Id);
                Console.WriteLine(jsonlicense);

                RestClient restClientLicense = new RestClient();
                RestRequest restRequestLicense = new RestRequest();
                restRequestLicense.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
                restRequestLicense.AddJsonBody(jsonlicense);
                restClientLicense.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{responseUser.Id}/assignLicense");
                var responseLicense = restClientLicense.Post(restRequestLicense);
                Console.WriteLine(responseLicense.StatusCode);
                Console.WriteLine(responseLicense.ErrorMessage);

                masterDBService.CreateEntity(rabbitMQUser.UUID, responseUser.Id, "User");
            }
        }


        /// <summary>
        /// Method posting an incoming change of a user into the MS Graph API.
        /// </summary>
        /// <param name="rabbitMQUser">Updated user sent by the RabbitMQ message broker</param>
        public void UserUpdate(RabbitMQUser rabbitMQUser)
        {
            //Console.WriteLine("Update van User nog niet klaar!");
            Master masterUserId = masterDBService.GetGraphIdFromMUUID(rabbitMQUser.UUID);
            if (masterUserId != null && masterDBService.CheckSourceEntityVersionIsHigher(rabbitMQUser.UUID, rabbitMQUser.Header.Source))
            {
                RestClient restClient = new RestClient();
                RestRequest restRequest = new RestRequest();

                User user = new User();
                //required
                user.AccountEnabled = true;
                user.DisplayName = rabbitMQUser.FirstName + " " + rabbitMQUser.LastName;
                user.MailNickname = rabbitMQUser.FirstName.Replace(' ', '.') + "." + rabbitMQUser.LastName;
                user.UserPrincipalName = user.MailNickname + "@ipwt3.onmicrosoft.com";
                user.Mail = user.UserPrincipalName;
                user.UsageLocation = "BE";
                //user.PasswordProfile = new UserPasswordProfile();
                //user.PasswordProfile.ForceChangePasswordNextSignIn = false;
                //user.PasswordProfile.Password = Constant.StandardPassword;

                user.GivenName = rabbitMQUser.FirstName;
                user.SurName = rabbitMQUser.LastName;
                user.JobTitle = rabbitMQUser.Role;

                BearerToken = services.RefreshAccesToken();

                var json = JsonConvert.SerializeObject(user);
                restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
                restRequest.AddJsonBody(json);
                Console.WriteLine(json);

                restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{masterUserId.SourceEntityId}");
                Console.WriteLine(restClient.BaseUrl);
                var response = restClient.Patch(restRequest);

                Console.WriteLine(response.StatusCode);

                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    masterDBService.ChangeEntityVersion(rabbitMQUser.UUID);
                }
            }
        }


        /// <summary>
        /// Method posting an incoming delete of a user into the MS Graph API.
        /// </summary>
        /// <param name="rabbitMQUser">Deleted user sent by the RabbitMQ message broker</param>
        public void UserDelete(RabbitMQUser rabbitMQUser)
        {
            //Console.WriteLine("Delete van User nog niet klaar!");
            Master masterUserId = masterDBService.GetGraphIdFromMUUID(rabbitMQUser.UUID);
            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();
            BearerToken = services.RefreshAccesToken();

            restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            Console.WriteLine(rabbitMQUser.UUID);
            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{masterUserId.SourceEntityId}");
            var response = restClient.Delete(restRequest);

            Console.WriteLine(response.StatusCode);
        }

    }
}
