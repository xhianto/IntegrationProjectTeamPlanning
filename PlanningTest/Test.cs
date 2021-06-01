using Microsoft.VisualStudio.TestTools.UnitTesting;
using Office365Service;
using Office365Service.Models;
using System;

namespace PlanningTest
{
    [TestClass]
    public class Test
    {
        Services services = new Services();
        
        [TestMethod]
        public void BearerTokenNotNull()
        {
            Assert.IsNotNull(services.RefreshAccesToken());
        }

        [TestMethod]
        public void CheckIfBearerToken()
        {
            Token token = services.RefreshAccesToken();
            Console.WriteLine(token);
            Assert.IsTrue(token.Token_type == "Bearer");
        }
    }
}
