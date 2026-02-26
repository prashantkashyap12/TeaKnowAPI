using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using userPanelOMR.Context;
using userPanelOMR.model;
using userPanelOMR.Service;

namespace userPanelOMR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CREDController : ControllerBase
    {
        private readonly JWTContext _jwtContext;
        public CREDController(JWTContext jwtContext)
        {
            _jwtContext = jwtContext;
        }

        // Done   ---   @save into webHoting rootPath.
        [HttpPost]
        [Route("FileSave")]
        public async Task <IActionResult> Post(IFormFile file, string name, string contact, string address)
        {
            if ((file== null || file.Length == 0 )&&(name==null||name.Length==0) && (contact==null||contact.Length==0) && (address==null||address.Length==0))
            {
                BadRequest("Detils Not properly");
            }
            var fileExt = Path.GetExtension(file.FileName).ToLower();
            var allowExt = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            if (!allowExt.Contains(fileExt))
            {
                return BadRequest("File Not proper format");
            }
            string filename = Path.GetFileName(file.FileName);
            var guid = Guid.NewGuid().ToString();
            var fileName = guid+"_"+filename;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "img");

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            string res = "";
            var savePath = Path.Combine(filePath, fileName);
            try
            {
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                   await file.CopyToAsync(stream);
                }
                var save = _jwtContext.Add(new DynamicForm
                {
                    UName = name,
                    UCont = contact,
                    UAdd = address,
                    Furl = savePath,
                });
                _jwtContext.SaveChanges();
                // System ERROR couldn't proper created on this path then deleted.
                var fileInfo = new FileInfo(savePath);
                if(fileInfo.Length == 0)
                {
                    System.IO.File.Delete(savePath);  // Role Back 
                    return BadRequest("file way saved but 0 byte delete");
                }
                res = fileInfo.Length + " FileSave";
            }
            catch(Exception ex)
            {
              res = ex.Message;
            }
            return Ok("res");
        }

        // Done   ---   @Delete perticular file from server's img path.
        [HttpDelete]
        [Route("FileDel")]
        public IActionResult Delete(string filePath)
        {
            dynamic res;
            try
            {
                //string filePath = @"D:\Prashant_Devloper\Web_Api\JWT\omrScanner\Uploads\img\28_04_2025_15_05_22_Untitled design.jpg";
                string fileNames = Path.GetFileName(filePath);
                var RootPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "img");
                var delPath = Path.Combine(RootPath, fileNames);
                if (System.IO.File.Exists(delPath))
                {
                    // delete from localRoot Location
                    System.IO.File.Delete(delPath);
                    Console.WriteLine("File deleted successfully.");

                    // delete form db
                    var delRes = _jwtContext.DynamicForm.FirstOrDefault(o => o.Furl == filePath);
                    if (delRes != null)
                    {
                        var IsDel = _jwtContext.DynamicForm.Remove(delRes);
                        _jwtContext.SaveChanges();
                        res = new
                        {
                            state = true,
                            message = "Record Has been Removed from Db"
                        };
                    }
                }
            }
            catch(Exception ex)
            {
                res = new {
                    state = false,
                    message = ex.Message
                };
            }
            return Ok("ERROR");
        }

        // When User want to show the information according to role then
        [Authorize(Roles = "User")]
        [HttpGet]
        [Route("UserIDE")]
        public IActionResult Get(string id)
        {
            return Ok("Welcome User.");
        }

        // When Admin want to show the information according to role then
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("AdminIDE")]
        public IActionResult Get(int id)
        {
            return Ok("UserIDE");
        }
    }
}
