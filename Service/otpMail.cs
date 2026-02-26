using System;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace userPanelOMR.Service
{
    // Done
    public class otpMail
    {
        public readonly IConfiguration _Configuration;
        public otpMail(IConfiguration configuration)
        {
            _Configuration = configuration;
        }
        public async Task<IActionResult> sendOtp(string tooEmail)
        {
            string res = "";
            string fromEmail = _Configuration["EmailSettings:FromEmail"];
            string Password = _Configuration["EmailSettings:Password"];
            string Host = _Configuration["EmailSettings:Host"];
            int port = int.Parse(_Configuration["EmailSettings:Port"]);
            bool enableSsl = bool.Parse(_Configuration["EmailSettings:EnableSsl"]);
            string subject = "Your OTP Code";
            string otpCode = GenerateOTP();
            string body = $"Your OTP code is: {otpCode}";

            using (SmtpClient client = new SmtpClient(Host, port))
            using (MailMessage mailMessage = new MailMessage(fromEmail, tooEmail, subject, body))
            {
                client.EnableSsl = enableSsl;
                client.Credentials = new NetworkCredential(fromEmail, Password);

                try
                {
                    await client.SendMailAsync(mailMessage);
                    return new OkObjectResult(otpCode);                // HTTP 200 with OTP

                }
                catch (Exception ex)
                {
                    return new BadRequestObjectResult(ex.Message);     // HTTP 400 with error
                }
            }
        }

        // 6 digit OTP generate karne ka function
        static string GenerateOTP()
        {
            Random random = new Random();
            string otp = random.Next(100000, 999999).ToString();
            return otp;
        }

    }
}
