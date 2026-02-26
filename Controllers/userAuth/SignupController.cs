using System.IdentityModel.Tokens.Jwt;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Syncfusion.EJ2.Notifications;
using userPanelOMR.Context;
using userPanelOMR.model;
using userPanelOMR.Service;
using static System.Net.WebRequestMethods;

namespace userPanelOMR.Controllers.userAuth
{
    public class SignupController : ControllerBase
    {
        private readonly JWTContext _context;
        private readonly otpMail _otpmail;
        private readonly HashEncption _HashPwd;
        private readonly jwtTokenGen _JwtToken;

        private readonly IConfiguration _conn;
        private readonly string querryString;


        public SignupController(JWTContext jwt, otpMail otpMail, HashEncption hashPwd, jwtTokenGen JwtToken, IConfiguration conn)
        {
            _context = jwt;
            _otpmail = otpMail;
            _HashPwd = hashPwd;
            _JwtToken = JwtToken;
            _conn = conn;
            querryString = conn.GetConnectionString("DefaultConnection");
        }

        // Done -- creating user details to database and send otp to reg. email just 10 min.
        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> signup([FromBody] signUpModel model)
        {
            dynamic resp = null;
            SignUpResponse res = new SignUpResponse();
            string isExist = "";
            string querry;

            if (!string.IsNullOrEmpty(model.name) && !string.IsNullOrEmpty(model.email) && !string.IsNullOrEmpty(model.contact) && !string.IsNullOrEmpty(model.password) && !string.IsNullOrEmpty(model.role))
            {
                try
                {
                    var userNames = _context.singUps.Any(x => x.Email == model.email);
                    Console.WriteLine(userNames);
                    if (userNames != true)
                    {
                        using (var _conn = new SqlConnection(querryString))
                        {
                            _conn.Open();
                            var uid = await _conn.ExecuteScalarAsync<int?>($@"select max(userId) from singUps ");
                            var UidNo = uid == null ? 1001 : uid.Value + 1;

                            DateTime expiryTimeUtc = DateTime.UtcNow.AddMinutes(10);
                            TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                            DateTime expiryTimeIndia = TimeZoneInfo.ConvertTimeFromUtc(expiryTimeUtc, indiaTimeZone);
                            model.password = _HashPwd.ComputeSha256Hash(model.password);
                            IActionResult UserOTP = await _otpmail.sendOtp(model.email);
                            if (UserOTP is OkObjectResult GetRespon)
                            {
                                model.otp = GetRespon.Value.ToString();
                            }
                            var saveRes = _context.Add(new SingUps
                            {
                                userId = UidNo,
                                Name = model.name,
                                Email = model.email,
                                Contact = model.contact,
                                Password = model.password,
                                Otp = model.otp,
                                ExpiryDate = expiryTimeIndia,
                                IsVerified = false,
                                role = model.role
                            });
                            _context.SaveChanges();
                            var ress = _conn.Execute(@$"insert into userProfile ([userId],[isEdit]) values(@UidNo, 'false')", new { UidNo });
                            var ress2 = _conn.Execute(@$"insert into LoginTokenRec ([userId]) values(@UidNo)", new { UidNo });
                        }
                        resp = new
                        {
                            state = true,
                            Message = "Record Has been saved",
                            Otp = model.otp
                        };
                    }
                    else
                    {
                        resp = new
                        {
                            state = false,
                            message = @$"{model.email} is Already Exist, Try Diffrent Email."
                        };
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception for better debugging
                    Console.WriteLine($"Error in signup API: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    resp = new
                    {
                        state = false,
                        Message = ex.Message,
                        Otp = model.otp
                    };
                }
            }
            else
            {
                resp = new
                {
                    state = false,
                    Message = "ex.Message",
                    Otp = model.otp
                };
            }
            return Ok(resp);
        }

        // check email isExist then remove from db.
        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> deleteUser([FromQuery] string email)
        {
            dynamic res;
            try
            {
                var userDel = _context.singUps.FirstOrDefault(a=>a.Email == email);
                if (userDel!=null)
                {
                    
                    using (var _conn = new SqlConnection(querryString)) {
                        _conn.Open();
                        var querry = await _conn.ExecuteAsync($@"delete LoginTokenRec where userId = {userDel.userId}");
                        var querry2 = await _conn.ExecuteAsync($@"delete userProfile where userId={userDel.userId}");
                        _context.singUps.Remove(userDel);
                        await _context.SaveChangesAsync();
                        res = new
                        {
                            state = true,
                            message = @$"{email} is Deleted from Record."
                        };
                    }
                }
                else
                {
                    res = new
                    {
                        state = false,
                        message = $@"{email} not found."
                    };
                }
            }
            catch (Exception ex)
            {
                res = new
                {
                    state = false,
                    massage = ex.Message
                };
            }
            return Ok(res);
        }

        // Done
        [HttpPost]
        [Route("ForgetReq")]
        public async Task<IActionResult> forgetPwd1([FromBody] verifyModel model)
        {
            dynamic res;
            if (!string.IsNullOrEmpty(model.email))
            {
                try {
                    var isfound = _context.singUps.FirstOrDefault(a => a.Email == model.email);
                    if (isfound != null)
                    {
                        DateTime expiryTimeUtc = DateTime.UtcNow.AddMinutes(10);
                        TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                        DateTime expiryTimeIndia = TimeZoneInfo.ConvertTimeFromUtc(expiryTimeUtc, indiaTimeZone);
                        IActionResult UserOTP = await _otpmail.sendOtp(model.email);
                        if (UserOTP is OkObjectResult GetRespon)
                        {
                            var otp = GetRespon.Value.ToString();
                            isfound.Otp = otp;
                            isfound.ExpiryDate = expiryTimeIndia;
                            _context.SaveChanges();
                        }
                        res = new
                        {
                            state = true,
                            massage = @$"OTP Sent on {model.email}"
                        };
                    }
                    else
                    {
                        res = new
                        {
                            state = false,
                            massage = @$"{model.email} Not found."
                        };
                    }
                } 
                catch (Exception ex) {
                    res = new
                    {
                        state = false,
                        massage = ex.Message
                    };
                }
            }
            else
            {
                res = new
                {
                    state = false,
                    massage = "Format's Not Match"
                };
            }
            return Ok(res);
        }

        // Done
        [HttpPost]
        [Route("ForgetGen2")]
        public IActionResult forgetpwd2([FromBody] verifyModel model)
        {
            dynamic resp;

            if (!string.IsNullOrEmpty(model.email) || !string.IsNullOrEmpty(model.otp) || !string.IsNullOrEmpty(model.password))
            {
                try
                {
                    DateTime TimeUtc = DateTime.UtcNow;
                    TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                    DateTime TimeIndia = TimeZoneInfo.ConvertTimeFromUtc(TimeUtc, indiaTimeZone);
                    var isVAlids = _context.singUps.FirstOrDefault(s => s.Email == model.email);

                    if (isVAlids != null)
                    {
                        if (isVAlids.Otp == model.otp)
                        {
                            if (isVAlids.ExpiryDate < TimeIndia)
                            {
                                resp = new
                                {
                                    state = false,
                                    message = "OTP Expired",
                                };
                            }
                            else
                            {
                                model.password = _HashPwd.ComputeSha256Hash(model.password);
                                isVAlids.IsVerified = true;
                                isVAlids.Password = model.password;
                                _context.SaveChanges();

                                resp = new
                                {
                                    state = true,
                                    message = "User Verified",
                                };
                            }
                        }
                        else
                        {
                            resp = new
                            {
                                state = false,
                                message = "OTP Expired, Try Again",
                            };
                        }
                    }
                    else
                    {
                        resp = new
                        {
                            state = false,
                            message = "Record Not Found",
                        };
                    }
                }
                catch (Exception ex)
                {
                    resp = new
                    {
                        state = false,
                        message = ex.Message,
                    };
                }
            }
            else
            {
                resp = new
                {
                    state = false,
                    message = "Incomplete Information",
                };
            }

            return Ok(resp);  // Return Ok with the response object
        }


        // Email, OTP, Password RePwd are matched then PWD will be set as per user id 
        [HttpPost]
        [Route("Signin")]
        public async Task<IActionResult> signin([FromBody] verifyModel model) {
            SignUpResponse res = new SignUpResponse();
            string querry = string.Empty;
            dynamic resp;
            if (!string.IsNullOrEmpty(model.email) || !string.IsNullOrEmpty(model.password))
            {
                try
                {
                    model.password = _HashPwd.ComputeSha256Hash(model.password);
                    var otpRecord = _context.singUps.FirstOrDefault(o => o.Email.Trim() == model.email);
                    if(otpRecord != null && otpRecord.Email == model.email)
                    {
                        if(otpRecord.Password != model.password)
                        {
                            resp = new
                            {
                                state = false,
                                Message = "Password is not Match"
                            };
                        }
                        else
                        {
                            var isVarifyed = otpRecord.IsVerified;
                            if (!isVarifyed) {
                                resp = new {
                                    state = false,
                                    Message = "User Is Not Email Verifed",
                                };
                            }
                            else
                            {
                                var token = _JwtToken.GenerateJwtToken(otpRecord);

                                var TokenMain = token;
                                var tokenHandler = new JwtSecurityTokenHandler();
                                var jwtToken = tokenHandler.ReadJwtToken(token);

                                var EmpId = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
                                var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

                                var expiryTimeUtc1 = jwtToken.ValidFrom.ToLocalTime();
                                var expiryTimeUtc2 = jwtToken.ValidTo;
                                var expiryTimeLocal2 = expiryTimeUtc2.ToLocalTime();

                                using (var _conn = new SqlConnection(querryString))
                                {
                                    _conn.Open();
                                    var quryAdd = @$"update LoginTokenRec set token='{TokenMain}',timeot='{expiryTimeLocal2}',Role='{role}' where userId = {EmpId}";
                                    var result = await _conn.ExecuteAsync(quryAdd);
                                }

                                resp = new {
                                    Message = "Login Success",
                                    Token = token,
                                    state = true,
                                    data = otpRecord
                                };
                            }
                        }
                    }
                    else
                    {
                        resp = new
                        {
                            state = false,
                            Message = "Record Not Found, Make New Account",
                        };
                    }

                }
                catch(Exception ex)
                {
                    resp = new
                    {
                        Message = ex.Message,
                        state = false,
                    };
                }
            }
            else
            {
                resp = new
                {
                   state = false,
                   Message = "Please fill User and Password value",
                };
            }
            return Ok(resp);
        }

        // Done
        [HttpGet]
        [Route("GetList")]
        public IActionResult GetList()
        {

            // Get all data from EmpClass
            var empList = _context.singUps.ToList();
            if (empList == null)
            {
                BadRequest("No Data Found");
            }
            else if (empList.Any())
            {
                //var fs = empList.First();
                var fs = empList;
                //var fs = empList.Last();
                return Ok(fs);
            }
            return Ok(empList);
        }
    }

    public class SignUpResponse
    {
        public bool state { get; set; }
        public string Message { get; set; }
        public string Otp { get; set; }
        public string Token { get; set; }
    }
    public class signUpModel
    {
        public string date { get; set; } = "";
        public string name { get; set; } = "";
        public string email { get; set; } = "";
        public string contact { get; set; } = "";
        public string password { get; set; } = "";
        public string role { get; set; } = "";
        public string otp { get; set; } = "0000";
    }

    public class verifyModel
    {
        public string email { get; set; } = "";
        public string password { get; set; } = "";
        public string otp { get; set; } = "";
    }
}
