using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Syncfusion.EJ2.Notifications;
using userPanelOMR.model.userPanel;
using userPanelOMR.Service;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace userPanelOMR.Controllers.userpanel
{
    [ApiController]
    public class taskManager : ControllerBase
    {

        readonly private IConfiguration _configuration;
        readonly private string connectionString;
        readonly private BlogImgSave _blogImg;
        public taskManager(IConfiguration _conn, BlogImgSave blogImg = null)
        {
            _configuration = _conn;
            connectionString = _configuration.GetConnectionString("DefaultConnection");
            _blogImg = blogImg;
        }

        // Create EventTask (role based 0 / uId)
        [HttpPost("AddLiveEvent")]
        public async Task<IActionResult> ComposeEvent([FromForm] CstLiveEvent model)
        {
            dynamic res;
            string qurry = null;
            try
            {
                using (var _conn = new SqlConnection(connectionString)){
                _conn.Open();
                var isImgAdd2 = "";

                    var eventId = await _conn.ExecuteScalarAsync<int?>("select MAX(liveEventId) from LiveEvent");
                    eventId = eventId == null ? 1001 : eventId + 1; 
                    if (model.EventImg != null)
                    {
                        isImgAdd2 = await _blogImg.saveImg2(model.EventImg, "TaskManager");
                    }
                    if(model.userId.Count != 0)
                    {
                        foreach (var data in model.userId)
                        {
                            qurry = $@"insert into LiveEvent (EventName, ProjectCat, EventDescription, EventCategory, DatTim, Locat, LandMark, OrganizerName, Contact, Email, 
                            EventPurpose, ParticipantsNo, PartnerOrganizations, Resources, Comments, userId, imgPath, liveEventId ) values ('{model.EventName}', '{model.ProjectCat}', '{model.EventDescription}', '{model.EventCategory}',
                            '{model.DatTim}', '{model.Locat}', '{model.LandMark}', '{model.OrganizerName}', '{model.Contact}', '{model.Email}', '{model.EventPurpose}', '{model.ParticipantsNo}',
                            '{model.PartnerOrganizations}', '{model.Resources}', '{model.Comments}', {data.id}, '{isImgAdd2}', {eventId})";
                            var result = await _conn.ExecuteAsync(qurry);
                        }
                        res = new
                        {
                            state = true,
                            res = "Add LiveEvent Succesfully",
                        };
                    }
                    else
                    {
                        res = new
                        {
                            state = false,
                            res = "Select User then Submit",
                        };
                    }
                }
            }
            catch (Exception ex) {
                res = new
                {
                    state = false,
                    massage = ex.Message,
                };
            }
            return Ok(res);
        }

        // Get Event UserId Based.
        [HttpGet("GetAllEvent")]
        public async Task<IActionResult> GetEvent([FromQuery] string LiveEventId)
        {
            dynamic res;
            string qurry = null;
            try
            {
                using (var _conn = new SqlConnection(connectionString))
                {
                    _conn.Open();
                    if (LiveEventId != "00000")
                    {
                        qurry = $@"select * from LiveEvent left join singUps on LiveEvent.userId = singUps.userId where liveEventId = {LiveEventId}";   // Show user wise users just private / if userId is null then show all users Live Strim.
                    }
                    else
                    {
                        qurry = $@"select * from LiveEvent";   // show all users Live Strim just admin
                    }
                    var result = await _conn.QueryAsync(qurry);
                    res = new
                    {
                        state = true,
                        massage = result,
                    };
                }
            }
            catch (Exception ex)
            {
                res = new
                {
                    state = false,
                    massage = ex.Message,
                };
            }
            return Ok(res);
        }

        [HttpGet("EventUserId")]
        public async Task<IActionResult> EventUserRec ([FromQuery] string userId)
        {
            dynamic res;
            try {
                using (var _conn = new SqlConnection(connectionString))
                {
                    _conn.Open();
                    var querry = $"select * from LiveEvent where userId = {userId}";
                    var result = _conn.Query(querry).ToList();
                    res = new
                    {
                        state = true,
                        record = result
                    };
                }
            }
            catch(Exception ex)
            {
                res = new
                {
                    state = false,
                    record = ex.Message
                };
            }
            return Ok(res);
        }

        // Delete Event
        [HttpDelete("deleteEvent")]
        public async Task<IActionResult> deleteEvent([FromQuery] string LiveEventId)
        {
            dynamic res;
            string querry;
            try
            {
                using (var _conn = new SqlConnection(connectionString))
                {
                    _conn.Open();
                    var imgNme = _conn.Query($@"select top 1 imgPath from LiveEvent where liveEventId = {LiveEventId}").FirstOrDefault();
                    string imgPath = imgNme?.imgPath?.ToString(); 
                    await _blogImg.DelImg(imgPath);                  // Delete Image from LiveEvent.
                    querry = $@"delete LiveEvent where liveEventId = {LiveEventId}";   // eventId based Deleted.
                    var result = await _conn.ExecuteAsync(querry);
                    res = new
                    {
                        state = true,
                        results = result,
                        Message = "Record Delete Successfully"
                    };
                }
            }
            catch(Exception ex)
            {
                res = new
                {
                    state = false,
                    results = ex.Message
                };
            }
            return Ok(res);
        }

        // Get Update Event
        [HttpPost("updateEvent")]
        public async Task<IActionResult> updateEvent([FromBody] CstLiveEvent model)
        {
            dynamic res;
            string query;
            string isImgAdd2 = string.Empty;
            string imgCol = string.Empty;
            string eql = string.Empty;
            dynamic result3 ;
            try
            {
                using (var _conn = new SqlConnection(connectionString))
                {
                    _conn.Open();
                    string? imagePath = null;
                    if (model.EventImg != null)
                    {
                        imgCol = $",[imgPath]";
                        isImgAdd2 = await _blogImg.saveImg2(model.EventImg, "TaskManager");
                        isImgAdd2 = $"{isImgAdd2}";

                    }
                    if (model.userId.Count != 0)
                    {
                        var existingUserIds = await _conn.QueryAsync<string>("SELECT userId FROM LiveEvent WHERE liveEventId = @EventId", new { EventId = model.IdEvent });
                        var newUserIds = model.userId.Select(u => u.id).ToList();
                        
                        foreach (var data in model.userId)
                        {
                            if (newUserIds.Contains(data.id)) { 
                                var qurry = $@"update LiveEvent set EventName ='{model.EventName}', ProjectCat='{model.ProjectCat}', [EventDescription]='{model.EventDescription}', [EventCategory]='{model.EventCategory}', [DatTim]='{model.DatTim}', 
                                [Locat]='{model.Locat}',[LandMark]='{model.LandMark}',[OrganizerName]='{model.OrganizerName}',[Contact]='{model.Contact}',[Email]='{model.Email}', [EventPurpose]='{model.EventPurpose}',[ParticipantsNo]='{model.ParticipantsNo}',
                                [PartnerOrganizations]='{model.PartnerOrganizations}', [Resources]='{model.Resources}',[Comments]='{model.Comments}', [userId] = '{data.id}' {(!string.IsNullOrWhiteSpace(isImgAdd2) ? " ": $", [imgPath] = '{isImgAdd2}'")} where liveEventId = {model.IdEvent}";
                                result3 = await _conn.ExecuteAsync(qurry);
                            }
                            else
                            {
                                var eventId = await _conn.ExecuteScalarAsync<int?>("select MAX(liveEventId) from LiveEvent");
                                var qurry3 = $@"insert into LiveEvent (EventName, ProjectCat, EventDescription, EventCategory, DatTim, Locat, LandMark, OrganizerName, Contact, Email,
                                EventPurpose, ParticipantsNo, PartnerOrganizations, Resources, Comments, userId, {(!string.IsNullOrWhiteSpace(isImgAdd2) ? "": $"imgPath,")} liveEventId) values ('{model.EventName}', '{model.ProjectCat}', 
                                '{model.EventDescription}', '{model.EventCategory}', '{model.DatTim}', '{model.Locat}', '{model.LandMark}', '{model.OrganizerName}', '{model.Contact}', '{model.Email}', 
                                '{model.EventPurpose}', '{model.ParticipantsNo}', '{model.PartnerOrganizations}', '{model.Resources}', '{model.Comments}', {data.id}, 
                                {(!string.IsNullOrWhiteSpace(isImgAdd2) ? "": $"'{isImgAdd2}',")}, {eventId})";
                                var result = await _conn.ExecuteAsync(qurry3);
                            }
                        }
                        await _conn.ExecuteAsync($"DELETE FROM LiveEvent WHERE liveEventId = '{model.IdEvent}' AND EventId NOT IN {newUserIds}");
                    }
                    res = new
                    {
                        state = true,
                        results = "Record updated"
                    };
                }
            }
            catch (Exception ex)
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
