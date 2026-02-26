using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using userPanelOMR.model.web;
using userPanelOMR.Service;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace userPanelOMR.Controllers.web
{
    public class EventController : Controller
    {

        private readonly string connString;
        private readonly BlogImgSave _blogImgSave;

        public EventController(IConfiguration _connString, BlogImgSave blogImgSave)
        {
            connString = _connString.GetConnectionString("DefaultConnection");
            _blogImgSave = blogImgSave;
        }


        [HttpPost("AddEventLs")]
        public async Task<IActionResult> eventList([FromForm] eventList model)
        {
            dynamic res;
            string query;
            var result = 0;
            string imgSave = null;
            try
            {
                using (var connection = new SqlConnection(connString))
                {
                    connection.Open();
                    query = "SELECT MAX(EventTran) FROM EventList";
                    var maxId = await connection.ExecuteScalarAsync<int?>(query);
                    var eventId = maxId == null ? 1001 : maxId.Value + 1;
                    query = null;

                    if (model.imgPath != null)
                    {
                        imgSave = await _blogImgSave.saveImg2(model.imgPath, "Event_web");
                    }

                    query = @$"Insert into EventList ([EventTran], [ImgPath], [Date], [Heading], [Para], [Place], [Address], [Link], [LinkText]) values({eventId}, '{imgSave}','{model.date}','{model.heading}','{model.pera}', '{model.place}', '{model.address}', '{model.link}', '{model.linkText}');
                            Insert into EventDetails (EventTran) values({eventId});";
                    result = await connection.ExecuteAsync(query);

                    res = new
                    {
                        state = true,
                        res = result
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

        [HttpGet("ViewEventLs")]
        public async Task<IActionResult> getRes()
        {
            dynamic res;
            string querry;
            try
            {
                using (var connection = new SqlConnection(connString))
                {
                    connection.Open();
                    querry = $"select * from EventList";
                    var result = connection.Query(querry).ToList();
                    res = new
                    {
                        state = true,
                        result = result
                    };
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                res = new
                {
                    state = false,
                    result = ex.Message
                };
            }

            return Ok(res);
        }

        [HttpDelete("DelEventLs")]
        public async Task<IActionResult> deleteEvent([FromQuery] string eventTran)

        {
            dynamic res;
            string querry;
            try
            {
                using (var connection = new SqlConnection(connString))
                {
                    connection.Open();

                    var resultx = connection.QueryFirstOrDefault(@$"select * from EventList where EventTran = {eventTran}");
                    await _blogImgSave.DelImg(resultx.ImgPath ?? "");
                    string query = $@"DELETE FROM EventList WHERE EventTran = @id;";
                    int rowaffeact = await connection.ExecuteAsync(query, new { id = eventTran });
                    res = new
                    {
                        state = true,
                        rowaffeact = rowaffeact,
                        message = "blogList data deleted success"
                    };

                    if (rowaffeact > 0)
                    {
                        querry = @$"select * from EventDetails where EventTran = @Id;";
                        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(querry, new { Id = eventTran });
                        if (result != null)
                        {
                            await _blogImgSave.DelImg(result.ImgPath ?? "");
                            querry = @$"delete from EventDetails where EventTran = @Id;";
                            var result2 = await connection.ExecuteAsync(querry, new { Id = eventTran });
                            res = new
                            {
                                state = true,
                                result = result,
                                massege = "blogList and BlogDetails data deleted success"
                            };
                        }
                        else
                        {
                            res = new
                            {
                                state = true,
                                message = "record deleted from blog List"
                            };
                        }
                    }
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

        [HttpPost("updatEventLs")]
        public async Task<IActionResult> update([FromForm] eventList model)
        {
            dynamic res;
            string querry;
            string imgSave = null;
            try
            {
                using (var conn = new SqlConnection(connString))
                {
                    conn.Open();
                    var resultx = await conn.ExecuteScalarAsync<string>($@"select ImgPath from EventList where EventTran = {model.eventTran} ");
                    if (model.imgPath != null)
                    {
                        await _blogImgSave.DelImg(resultx ?? "");
                        imgSave = await _blogImgSave.saveImg2(model.imgPath, "Event_web");
                    }
                    querry = $@"UPDATE [EventList] SET [ImgPath]='{imgSave}', [Date]='{model.date}', [Heading]='{model.heading}', [Para]='{model.pera}', 
                    [Place]='{model.place}', [Address]='{model.address}', [Link]='{model.link}', [LinkText]='{model.linkText}' where EventTran = @Id";   // make query to update all fields
                    var result = await conn.ExecuteAsync(querry, new { Id = model.eventTran });

                    res = new
                    {
                        state = true,
                        message = result
                    };
                }
            }
            catch (Exception ex)
            {
                res = new
                {
                    state = false,
                    message = ex.Message,
                };
            }

            return Ok(res);
        }

        [HttpGet("getEventDetilsId")]
        public async Task<IActionResult> getEvtDetlId([FromQuery] int eventTrn)
        {
            dynamic res;
            string query = null;
            try {
                using (var _conn = new SqlConnection(connString))
                {
                    query = $@"SELECT evtDet.EventTran, evtls.Heading, evtDet.ImgPath, evtDet.Dedicate, evtDet.Head1, evtDet.Pera11, evtDet.Pera12, evtDet.Head2, evtDet.Pera22, evtDet.Li1, evtDet.Li2, evtDet.Li3, evtDet.Li4, evtDet.Li5, evtDet.OtherHead, evtDet.Parti1, evtDet.Parti12, evtDet.Parti13, evtDet.Parti14, evtDet.Parti15, evtDet.Parti16, evtDet.Parti17 
                    FROM EventDetails evtDet 
                    left join EventList evtls 
                    on evtls.EventTran =  evtDet.EventTran 
                    where evtDet.EventTran = @evetTrn;";
                    var result = await _conn.QueryAsync(query, new { evetTrn = eventTrn });
                    res = new
                    {
                        state = true,
                        results = result
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

        [HttpPost("updateEventDtls")]
        public async Task<IActionResult> eventDetail([FromForm] eventDetails model)
        {
            var query="";
            dynamic result;
            var imgSave = string.Empty;
            try
            {
                using (var _conn = new SqlConnection(connString))
                {
                    var resultx = await _conn.ExecuteScalarAsync<string>(@$"select ImgPath from EventDetails where EventTran = {model.eventTran}");
                    if (model.img != null)
                    {
                        await _blogImgSave.DelImg(resultx ?? "");
                        imgSave = await _blogImgSave.saveImg2(model.img, "EventDetails");
                    }
                    query = $@"update EventDetails set [ImgPath]='{imgSave}', [Dedicate]='{model.dadicat}', [Head1]='{model.head1}', [Pera11]='{model.pera11}', [Pera12]='{model.pera12}', [Head2]='{model.head2}', [Pera22]='{model.pera22}', 
                    [Li1]='{model.li1}', [Li2]='{model.li2}', [Li3]='{model.li3}', [Li4]='{model.li4}', [Li5]='{model.li5}', [OtherHead]='{model.otherHead}', [Parti1]='{model.parti1}', [Parti12]='{model.parti12}', [Parti13]='{model.parti13}', 
                    [Parti14]='{model.parti14}', [Parti15]='{model.parti15}', [Parti16]='{model.parti16}', [Parti17]='{model.parti17}'  where EventTran = @Id";
                    var res = await _conn.ExecuteAsync(query, new { Id =  model.eventTran});

                    result = new
                    {
                        result = res,
                        status = true
                    };
                }
            }catch(Exception ex)
            {
                result = new
                {
                    status = false,
                    message = ex.Message
                };
            }
            return Ok(result);
        }


        [HttpGet("EventDtls")]
        public async Task<IActionResult> eventDetList()
        {
            dynamic res;
            string querry;
            try
            {
                using (var conn = new SqlConnection(connString))
                {
                    conn.Open();
                    querry = $@"select * from EventDetails";
                    var result = await conn.QueryAsync(querry);
                    res = new
                    {
                        List = result,
                        state = true,
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
