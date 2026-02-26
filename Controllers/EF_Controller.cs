using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using userPanelOMR.Context;
using userPanelOMR.model;
using userPanelOMR.Service;

namespace userPanelOMR.Controllers
{
    [Route("api/User")]
    [ApiController]
    public class EF_Controller : ControllerBase
    {

        private readonly JWTContext _context;
        private readonly HashEncption _empService;
        private readonly jwtTokenGen _jwtTokenGen;
        private readonly BlogImgSave _blogImgSave;

        // Constructor - yaha DbContext inject ho raha hai
        public EF_Controller(JWTContext jwt, HashEncption empServ, jwtTokenGen jwtTokenGen, BlogImgSave blogImgSave)
        {
            _context = jwt;
            _empService = empServ;
            _jwtTokenGen = jwtTokenGen;
            _blogImgSave = blogImgSave;
        }

        // Add API - 
        [HttpGet]
        [Route("AddEmp")]
        public IActionResult Add(string name, string email, string pwd, string cont, string bankNme)
        {
            //checked validation
            if(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pwd) || string.IsNullOrEmpty(cont) || string.IsNullOrEmpty(bankNme))
            {
                return BadRequest("Please Fill Every Information");
            }
            var hashingPwd = _empService.ComputeSha256Hash(pwd);
            EmpClass newEmp = new EmpClass
            {
                EmpName = name,
                EmpEmail = email,
                password = hashingPwd,
                contact = cont,
                bankName = bankNme
            };

            // Context me add karte hain
            _context.EmployeesTab.Add(newEmp);

            // Save Context And Close
            _context.SaveChanges();

            //Main Value 
            return Ok("Add Record");
        }

        // Retireve API  (First / All Record)
        [HttpGet]
        [Route("GetList")]
        public IActionResult GetList() {

            // Get all data from EmpClass
            var empList = _context.EmployeesTab.ToList();
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

        // Delete API  -- 
        [HttpDelete]
        [Route("DeleteEmp")]
        public IActionResult delete(int id)
        {
            try
            {
                var empid = _context.EmployeesTab.Find(id);
                var empVal = empid;
                if (empid == null)
                {
                    BadRequest("Record is not avilable");
                }
                _context.EmployeesTab.Remove(empid);
                _context.SaveChanges();
                return Ok(empVal + "Has Been Removed");
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }
        }


        // Update API
        [HttpPut]
        [Route("Update")]
        public IActionResult update(int id, string name, string email, string pwd, string cont, string empBank) {
            var hashingPwd = _empService.ComputeSha256Hash(pwd);
            try
            {
                var idmain = _context.EmployeesTab.Find(id);
                if(idmain == null)
                {
                    BadRequest("Record not Find");
                }
                idmain.EmpName = name;
                idmain.EmpEmail = email;
                idmain.password = hashingPwd;
                idmain.contact = cont;
                idmain.bankName = empBank;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                BadRequest(ex.Message);
            }
            return Ok("Emp Update");
        }

        // Check does login on not
        [HttpGet]
        [Route("LoginForm")]
        public async Task<IActionResult> get(string uname, string pwd) {
            var token = string.Empty;
            //checked validation
            if (string.IsNullOrWhiteSpace(uname) || string.IsNullOrWhiteSpace(pwd))
            {
                return BadRequest("Please Fill Every Information");
            }

            var hashingPwd = _empService.ComputeSha256Hash(pwd);
            var ReturnDetails = _context.EmployeesTab.FirstOrDefault(x => x.EmpEmail == uname && x.password == hashingPwd);

            if (ReturnDetails  == null || ReturnDetails.password != hashingPwd)
            {
                return Unauthorized("Unauthorized User");
            }
            token = ""; // _jwtTokenGen.GenerateJwtToken(ReturnDetails);
            return Ok(new
            {
                message = $"Login Success",
                token = token,
            });
        }
    }
}
