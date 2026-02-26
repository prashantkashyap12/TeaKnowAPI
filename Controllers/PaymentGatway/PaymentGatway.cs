using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Razorpay.Api;
using Syncfusion.EJ2.PivotView;
using userPanelOMR.model;
using userPanelOMR.Service;

namespace userPanelOMR.Controllers.PaymentGatway
{
    [ApiController]
    public class PaymentGatway : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString ;
        private readonly SignatureVerify _SignatureVerify;
        public PaymentGatway(IConfiguration configuration)
        {
            //_SignatureVerify = new SignatureVerify();
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _configuration = configuration;
        }

        [HttpPost]
        [Route("CreatePay")]
        public async Task<IActionResult> Pay([FromForm] Pg_model model)
        {
            dynamic res;
            string querry;
            try
            {
                using (var _conn = new SqlConnection(_connectionString))
                {
                    string keyId = _configuration["Razorpay:KeyId"];
                    string KeySecret = _configuration["Razorpay:KeySecret"];
                    RazorpayClient client = new RazorpayClient(keyId, KeySecret);
                    Dictionary<string, object> options = new Dictionary<string, object>(){
                        { "amount", model.amount * 100 },         // amount in paise so into Rupees
                        { "currency", "INR" },
                        { "receipt", "order_rcptid_11" },
                        { "payment_capture", 1 },
                        { "notes", new Dictionary<string, string>
                            {
                                { "UserId", model.UserId },
                                { "Package", model.Package },
                                { "Note", model.CustomNote }
                            }
                        }
                    };
                    Razorpay.Api.Order order = client.Order.Create(options);
                    res = new
                    {
                        status = true,
                        orderId = order["id"],
                        amount = order["amount"],
                        currency = order["currency"],
                        message = "Order created successfully"
                    };
                }
            }
            catch(Exception ex)
            {
                res = new
                {
                    status = false,
                    message = ex.Message,
                };
            }
            return Ok(res);
        }

        // This method is used to verify the payment signature received from Razorpay.
        // Is this method is called after the payment is completed on the client side.
        [HttpPost]
        [Route("VerifyPayment")]
        public IActionResult VerifyPayment([FromBody] RazorpayVerifyModel model)
        {
            object res;

            try
            {
                // Verify the payment signature
                var  isPaymentValid = _SignatureVerify.VerifyPayment(model.razorpay_order_id, model.razorpay_payment_id, model.razorpay_signature);
                if (isPaymentValid)
                {
                    res = new {
                        status = true,
                        message = "Payment verification successful.",
                    }; 
                }
                else
                {
                    res = new
                    {
                        status = false,
                        message = "Invalid signature. Payment verification failed."
                    };
                }
            }
            catch (Exception ex)
            {
                res = new
                {
                    status = false,
                    message = "Signature verification failed: " + ex.Message
                };
            }

            return Ok(res);
        }

        public class RazorpayVerifyModel
        {
            public string razorpay_payment_id { get; set; }
            public string razorpay_order_id { get; set; }
            public string razorpay_signature { get; set; }
        }





        //  Sql Success =
        //    CREATE TABLE PaymentFailures(
        //    FailureId INT PRIMARY KEY AUTO_INCREMENT,
        //    OrderId VARCHAR(255),
        //    PaymentId VARCHAR(255),
        //    UserId INT,
        //    Amount DECIMAL(10, 2),
        //    Currency VARCHAR(10),
        //    PaymentStatus VARCHAR(50),
        //    FailureReason VARCHAR(255),
        //    ErrorMessage TEXT,
        //    Timestamp DATETIME,
        //    PaymentMode VARCHAR(50),
        //    FailureNotes TEXT
        //    );

        //  Sql Failure =
        //    CREATE TABLE PaymentFailures(
        //    FailureId INT PRIMARY KEY AUTO_INCREMENT,
        //    OrderId VARCHAR(255),
        //    PaymentId VARCHAR(255),
        //    UserId INT,
        //    Amount DECIMAL(10, 2),
        //    Currency VARCHAR(10),
        //    PaymentStatus VARCHAR(50),
        //    FailureReason VARCHAR(255),
        //    ErrorMessage TEXT,
        //    Timestamp DATETIME,
        //    PaymentMode VARCHAR(50),
        //    FailureNotes TEXT
        //    );

    }
}
