using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;
using Stripe.Radar;

namespace paymentAPI
{
    public static class StripeController
    {

        // User
        [FunctionName("GetCustomer")]
        public static IActionResult GetUserCardsList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {

            StripeConfiguration.ApiKey = "";

            // Customer Id
            string id = req.GetQueryParameterDictionary()["id"];


            var service = new CustomerService();
            return new JsonResult(service.Get(id));

        }
        // User
        [FunctionName("GetCustomerCardsList")]
        public static IActionResult GetCustomerCardsList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {

            StripeConfiguration.ApiKey = "";

            string id = req.GetQueryParameterDictionary()["id"];

            var service = new CardService();
            var options = new CardListOptions
            {
                Limit = 10,
            };
            var cardlist = service.List(id, options);
            return new JsonResult(cardlist.Data);

        }



        // User
        [FunctionName("CreateCustomer")]
        public static async Task<IActionResult> CreateCustomerAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {

            StripeConfiguration.ApiKey = "";
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var customer = JsonConvert.DeserializeObject<CustomerCreateOptions>(body);

            var address = new AddressOptions
            {
                City = customer.Address.City,
                Country = customer.Address.Country,
                Line1 = customer.Address.Line1,
                Line2 = customer.Address.Line2,
                PostalCode = customer.Address.PostalCode,
                State = customer.Address.State
            };

            var options = new CustomerCreateOptions
            {
                Name = customer.Name,
                Description = customer.Description,
                Phone = customer.Phone,
                Address = address,
                Email = customer.Email,
            };
            var service = new CustomerService();
            // Add logic to store customer ID
            return new JsonResult(service.Create(options));

        }
        // Update Stripe Customer
        [FunctionName("UpdateCustomer")]
        public static async Task<IActionResult> UpdateStripeCustomerAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {

            StripeConfiguration.ApiKey = "";

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var customer = JsonConvert.DeserializeObject<CustomerUpdateOptions>(body);
            // // CustomerID
            string cid = req.GetQueryParameterDictionary()["cid"];

            var address = new AddressOptions
            {
                City = customer.Address.City,
                Country = customer.Address.Country,
                Line1 = customer.Address.Line1,
                Line2 = customer.Address.Line2,
                PostalCode = customer.Address.PostalCode,
                State = customer.Address.State
            };
            var options = new CustomerUpdateOptions
            {
                Name = customer.Name,
                Description = customer.Description,
                Phone = customer.Phone,
                Address = address,
                Email = customer.Email
            };

            var service = new CustomerService();

            return new JsonResult(service.Update(cid, options));

        }


        // Admin

        // Create Customer Charge
        [FunctionName("CreateStripeCharge")]
        public static async Task<IActionResult> CreateStripeChargeAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {

            StripeConfiguration.ApiKey = "";
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var charge = JsonConvert.DeserializeObject<ChargeCreateOptions>(body);
            string id = req.GetQueryParameterDictionary()["id"];

            var options = new ChargeCreateOptions
            {
                // Working Information
                Amount = charge.Amount,
                Currency = "usd",
                Source = charge.Source,
                Description = "Description",
                Customer = charge.Customer
            };
            var service = new ChargeService();
            // var date = new DateTime().

            var checkoutout = service.Create(options);

            if (checkoutout.Status != "failed")
            {
                // If successful Transaction do something
                // Need to update transaction table 
                // Add updating the value once property has returned to succeded

            }
            return new JsonResult(checkoutout.Status);

        }

        // Get All Stripe Customers
        [FunctionName("GetAllCustomers")]
        public static IActionResult GettAllStripeCustomers(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {

            StripeConfiguration.ApiKey = "";
            StripeConfiguration.MaxNetworkRetries = 2;
            var options = new CustomerListOptions
            {
                Limit = 40,
            };
            var service = new CustomerService();
            var customer = service.List(options);

            return new JsonResult(customer.Data);

        }

        // Get All Stripe Charges
        [FunctionName("GetAllStripeCharges")]
        public static IActionResult GetAllCharges(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {

            StripeConfiguration.ApiKey = "";
            var options = new ChargeListOptions { Limit = 100 };
            var service = new ChargeService();

            return new JsonResult(service.List(options));

        }
        // Fraud
        [FunctionName("FraudWarnings")]
        public static IActionResult FraudWarnings(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {

            StripeConfiguration.ApiKey = "";
            var options = new EarlyFraudWarningListOptions
            {
                Limit = 3,
            };
            var service = new EarlyFraudWarningService();

            return new JsonResult(service.List(options));

        }
    }
}
