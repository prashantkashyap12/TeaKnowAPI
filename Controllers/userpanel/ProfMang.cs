using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Syncfusion.EJ2.Notifications;
using userPanelOMR.model.userPanel;
using userPanelOMR.Service;

namespace userPanelOMR.Controllers.userpanel
{
    [ApiController]
    public class ProfMang : ControllerBase
    {

        readonly public IConfiguration Configuration;
        readonly string querryString;
        readonly public BlogImgSave blogImg;
        public ProfMang(IConfiguration _conn, BlogImgSave _blogImg) {
        
            Configuration = _conn;
            querryString = _conn.GetConnectionString("DefaultConnection");
            blogImg = _blogImg;
        }

        // post to profile
        [HttpPost("ProfileRec")] // Update profile details
        public async Task<IActionResult> ProfileRec([FromForm] ProfileUpdate model)
        {
            string qurry = null;
            dynamic res;

            try
            {
                using (var _conn = new SqlConnection(querryString))
                {
                    _conn.Open();
                    var imgPath = Empty.ToString();
                    var resultx = await _conn.ExecuteScalarAsync<string>($@"select ProfileImg from userProfile where userId = {model.userId}");
                    if (model.ProfileImg != null)
                    {
                        await blogImg.DelImg(resultx);
                        imgPath = await blogImg.saveImg2(model.ProfileImg, "userImg");
                        imgPath = $"[ProfileImg]='{imgPath}'";
                    }
                    qurry = $@"update userProfile set [Father_Name] = '{model.Father_Name}', [DOB]='{model.DOB}', [Gender] = '{model.Gender}' , [MaritalStatus]='{model.MaritalStatus}' ,
                    [Education]='{model.Education}', [Profession]='{model.Profession}', [whatApp]='{model.whatApp}', [AddressFull]='{model.AddressFull}', [Member_Type]='{model.Member_Type}', 
                    [join_cat]='{model.join_cat}', [Already_Join]='{model.Already_Join}', [designation]='{model.designation}', {imgPath} where userId = {model.userId}
";                  var result = await _conn.ExecuteAsync(qurry);
                    res = new
                    {
                        state = true,
                        results = result,
                        massege = "profile is Updated"
                    };
                }
            }
            catch(Exception ex)
            {   
                res = new
                {
                    state = true,
                    massege = ex.Message
                };
            }
            return Ok(res);
        }

        // get from sign+profile
        [HttpGet("GTProfUpdate")]  // Get profile details using userId or not
        public async Task<IActionResult> GetProUp([FromQuery] int userId)
        {
            string qurry = null;
            dynamic res;
            try
            {
                using (var _conn = new SqlConnection(querryString))
                {
                    _conn.Open();
                 

                    if (userId == 0)
                    {
                        qurry = $@"SELECT sig.userId, sig.Name, sig.Email, sig.Password, sig.Contact, sig.role, sig.ExpiryDate, sig.IsVerified, sig.userId, Urprof.userId, Urprof.Father_Name, Urprof.DOB, Urprof.Gender, Urprof.MaritalStatus,
                    Urprof.Education, Urprof.Profession,Urprof.ProfileImg, Urprof.isEdit, Urprof.whatApp, Urprof.AddressFull, Urprof.Member_Type, Urprof.join_cat, Urprof.Already_Join, Urprof.designation, Urprof.isEdit FROM userProfile Urprof LEFT JOIN singUps sig ON sig.userId = Urprof.userId";
                    }
                    else
                    {
                        qurry = $@"SELECT sig.userId, sig.Name, sig.Email, sig.Password, sig.Contact, sig.role, sig.ExpiryDate, sig.IsVerified, sig.userId, Urprof.userId, Urprof.Father_Name, Urprof.DOB, Urprof.Gender, Urprof.MaritalStatus,
                    Urprof.Education, Urprof.Profession,Urprof.ProfileImg, Urprof.isEdit, Urprof.whatApp, Urprof.AddressFull, Urprof.Member_Type, Urprof.join_cat, Urprof.Already_Join, Urprof.designation, Urprof.isEdit FROM userProfile Urprof LEFT JOIN singUps sig ON sig.userId = Urprof.userId where Urprof.userId = {userId};";
                    }

                    var result = _conn.Query(qurry).ToList();
                    res = new
                    {
                        state = true,
                        results = result
                    };
                }
            }
            catch (Exception ex)
            {
                res = new
                {
                    state = true,
                    massege = ex.Message
                };
            }
            return Ok(res);
        }

        [HttpPost("updateRec")] // Set user permission 
        public async Task<IActionResult> updateProfile([FromBody] ProfIsEdit model)
        {
            var query = "";
            dynamic res;

            try
            {
                using(var _conn = new SqlConnection(querryString))
                {
                    query = $@"update userProfile SET [isEdit]='{model.isDocView}', [designation]='{model.designation}' where userId = {model.uid}";
                    var result = await _conn.ExecuteAsync(query);
                    res = new
                    {
                        state = true,
                        message = result
                    };
                }
            }
            catch(Exception ex)
            {
                res = new
                {
                    state = false,
                    message = ex.Message
                };
            }
            return Ok(res);
        }
    }
}



