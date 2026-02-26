using System;
using System.Security.Cryptography;
using System.Text;

namespace userPanelOMR.Service
{
    public class SignatureVerify
    {
        private readonly IConfiguration _configuration;

        public SignatureVerify(IConfiguration configuration) {
            _configuration = configuration;
        }

        // This method verifies the payment signature received from Razorpay only
        public bool VerifyPayment(string orderId, string paymentId, string signature)
        {
            try
            {
                var keyId = _configuration["Razorpay:KeyId"];
                string keySecret = _configuration["Razorpay:KeySecret"];
                // Create the string to be signed
                string stringToSign = orderId + "|" + paymentId;

                // Generate the signature using the Razorpay secret key
                string generatedSignature = GenerateSignature(stringToSign, keySecret);

                // Compare the generated signature with the signature received from Razorpay
                return generatedSignature.Equals(signature); // Here is the comparison
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error verifying payment: " + ex.Message);
                return false;
            }
        }

        private string GenerateSignature(string stringToSign, string secretKey)
        {
            // Generate the HMAC SHA256 signature bit convert to string and return it
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
                // Convert the byte array to a hexadecimal string and return it along with replcing "-" with empty string
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
