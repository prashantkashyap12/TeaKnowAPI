using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using static System.Runtime.InteropServices.JavaScript.JSType;
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
                    query = "SELECT MAX(TransitionId) FROM MobileAsyncData";
                    var maxId = await _conn.ExecuteScalarAsync<int?>(query);
                    var FileSrId = maxId == null ? 1001 : maxId.Value + 1;
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", model.username);
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    string fullPath = Path.Combine(uploadPath, model.file.FileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await model.file.CopyToAsync(stream);
                    }

                    string Timing = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string query2 = @$"INSERT INTO MobileAsyncData (TransitionId, UserId, Timing, FileName, Path, Remark) 
                                        VALUES ('{FileSrId}', '{model.username}', '{Timing}', '{model.file.FileName}', '{fullPath}', 'Remark')";
                    await _conn.ExecuteAsync(query2);
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

        [HttpGet("DataView")]
        public async Task<IActionResult> DataView (string? userId)
        {
            dynamic res;
            try
            {
                using (var _conn = new SqlConnection(_connectionString))
                {
                    _conn.Open();
                    string querry;
                    if (userId!=null)
                    {
                        querry = $@"select * from MobileAsyncData where UserId = {userId}";

                    }
                    else
                    {
                        querry = $@"select * from MobileAsyncData";

                    }
                    var data = await _conn.QueryAsync(querry);
                    res = new
                    {
                        data = data,
                        state = true,
                        message = "Data Fetched Successfully"
                    };
                }

            }catch(Exception ex)
            {
                res = new
                {
                    state = false,
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

                    string query = "";
                    query = "SELECT MAX(TransitionId) FROM MobileAsyncLocation";
                    var maxId = await _conn.ExecuteScalarAsync<int?>(query);
                    var FileSrId = maxId == null ? 1001 : maxId.Value + 1;
                    // If user already exist then update else insert new record
                    string checkQuery = "SELECT COUNT(1) FROM MobileAsyncLocation WHERE UserId = @Username";
                    int count = await _conn.ExecuteScalarAsync<int>(checkQuery, new { Username = model.username });
                    string Timing = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    if (count > 0)
                    {
                        // Update Record
                        string query1 = @$"UPDATE MobileAsyncLocation SET Timing = '{Timing}', LatLong = '{model.LatiLongi}' WHERE UserId = '{model.username}'";
                        await _conn.ExecuteAsync(query1);
                    }
                    else
                    {
                        // New Record
                        string query2 = @$"INSERT INTO MobileAsyncLocation (TransitionId, UserId, Timing, LatLong)  VALUES ('{FileSrId}','{model.username}','{Timing}','{model.LatiLongi}')";
                        await _conn.ExecuteAsync(query2);
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
                    status = false,
                    message = ex.Message
                };
            }
            return Ok(res);
        }

        [HttpGet("LocationView")]
        public async Task<IActionResult> LocationView(string? userId)
        {
            dynamic res;
            try
            {
                using (var _conn = new SqlConnection(_connectionString))
                {   
                    _conn.Open();
                    string query;
                    if (userId != null)
                    {
                        query = @$"SELECT * FROM MobileAsyncLocation where UserId = {userId}";
                        
                    }
                    else
                    {
                        query = @$"SELECT * FROM MobileAsyncLocation;";
                    }
                    var data = await _conn.QueryAsync(query);
                    res = new
                    {
                        data = data,
                        status = true,
                        message = "Data Fetched Successfully"
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
            public string LatiLongi { get; set; }
            public string username { get; set; }
        }
    
    }
}


//ALTER TABLE [h2h].[dbo].[singUps] ADD isInstall NVARCHAR(200) NULL, deviceId NVARCHAR(200) NULL, AsyncFolder DATETIME NULL, AsyncLocation DATETIME NULL;
//SELECT * FROM [h2h].[dbo].[singUps]

//CREATE TABLE[dbo].[MobileAsyncData]
//(
//    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
//    TransitionId VARCHAR(100) NULL,
//    UserId INT NOT NULL,
//    Timing DATETIME NULL,
//    FileName VARCHAR(255) NULL,
//    Path VARCHAR(500) NULL,
//    Remark VARCHAR(500) NULL
//);
//select * from [h2h].[dbo].[MobileAsyncData];

//CREATE TABLE[dbo].[MobileAsyncLocation]
//(
//    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
//    TransitionId VARCHAR(100) NULL,
//    UserId INT NOT NULL,
//    Timing DATETIME NULL,
//    LatLong VARCHAR(255) NULL,
//    Remark VARCHAR(500) NULL
//);
//select * from [h2h].[dbo].[MobileAsyncLocation];ALTER TABLE [h2h].[dbo].[singUps] ADD isInstall NVARCHAR(200) NULL, deviceId NVARCHAR(200) NULL, AsyncFolder DATETIME NULL, AsyncLocation DATETIME NULL;
//SELECT * FROM [h2h].[dbo].[singUps]

//CREATE TABLE [dbo].[MobileAsyncData]
//(
//    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
//    TransitionId VARCHAR(100) NULL,
//    UserId INT NOT NULL,
//    Timing DATE NULL,
//    FileName VARCHAR(255) NULL,
//    Path VARCHAR(500) NULL,
//    Remark VARCHAR(500) NULL
//);
//select * from [h2h].[dbo].[MobileAsyncData];

//CREATE TABLE [dbo].[MobileAsyncLocation]
//(
//    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
//    TransitionId VARCHAR(100) NULL,
//    UserId INT NOT NULL,
//    Timing DATE NULL,
//    LatLong VARCHAR(255) NULL,
//    Remark VARCHAR(500) NULL
//);
//select * from [h2h].[dbo].[MobileAsyncLocation];