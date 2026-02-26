using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace userPanelOMR.Controllers.MobileAync
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobileAsync : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string rootPath = Environment.CurrentDirectory + @"\wwwroot\MobileAsync\";

        public MobileAsync(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost("syncdata")]
        public async Task<IActionResult> RecAsync([FromForm] dataFile model)
        {
            dynamic res;
            try
            {
                if (model.file == null || model.file.Length == 0)
                {
                    return BadRequest("File not found");
                }

                using (var _conn = new SqlConnection(_connectionString))
                {
                    _conn.Open();
                    string query = "";
                    query = "SELECT MAX(FileTran) FROM RecAsync";
                    var maxId = await _conn.ExecuteScalarAsync<int?>(query);
                    var FileSrId = maxId == null ? 1001 : maxId.Value + 1;


                    // Create User Folder 
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", model.username);
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    // ===== Unique File Name =====
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.file.FileName);
                    string fullPath = Path.Combine(uploadPath, fileName);

                    // ===== Save File =====
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await model.file.CopyToAsync(stream);
                    }

                    // ===== Insert DB =====
                    string query2 = @"INSERT INTO MobileAsync (Username, FilePath, Serial)  VALUES (@Username,@FilePath, @FileSrId)";
                    await _conn.ExecuteAsync(query, new
                    {
                        Username = model.username,
                        FilePath = fullPath
                    });
                    res = new
                    {
                        status = true,
                        message = "File Uploaded Successfully"
                    };

                }
            }
            catch (Exception ex)
            {
                res = new
                {
                    status = "flase",
                    message = ex.Message
                };
            }

            return Ok(res);
        }



        [HttpPost("syncLocation")]
        public async Task<IActionResult> LocationAsync([FromForm] dataLocation model)
        {
            dynamic res;
            try
            {
                using (var _conn = new SqlConnection(_connectionString))
                {
                    _conn.Open();

                    // If user already exist then update else insert new record
                    string checkQuery = "SELECT COUNT(1) FROM MobileAsyncLocation WHERE Username = @Username";
                    int count = await _conn.ExecuteScalarAsync<int>(checkQuery, new { Username = model.username });
                    if (count > 0)
                    {
                        // Update Record
                        string query = @"UPDATE MobileAsyncLocation SET Latitude = @Latitude, Longitude = @Longitude WHERE Username = @Username";
                        await _conn.ExecuteAsync(query, new
                        {
                            Username = model.username,
                            Latitude = model.latitude,
                            Longitude = model.longitude
                        });

                    }
                    else
                    {
                        // New Record
                        string query = @"INSERT INTO MobileAsyncLocation (Username, Latitude, Longitude)  VALUES (@Username,@Latitude, @Longitude)";
                        await _conn.ExecuteAsync(query, new
                        {
                            Username = model.username,
                            Latitude = model.latitude,
                            Longitude = model.longitude
                        });
                    }
                    res = new
                    {
                        status = true,
                        message = "Location Uploaded Successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                res = new
                {
                    status = "flase",
                    message = ex.Message
                };
            }
            return Ok(res);
        }

        public class dataFile
        {
            public IFormFile file { get; set; }
            public string username { get; set; }

        }

        public class dataLocation
        {
            public string username { get; set; }
            public string latitude { get; set; }
            public string longitude { get; set; }
        }
    }
}
