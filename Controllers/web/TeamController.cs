using System.Transactions;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using userPanelOMR.model.web;
using userPanelOMR.Service;

namespace userPanelOMR.Controllers.web
{
    public class TeamController : Controller
    {
        private readonly string _connectionStrings;
        private readonly string _env;
        private readonly BlogImgSave _blogImgSave;
        //private readonly imgSave = await _blogImgSave.saveImg2(model.Imgpath, "ProjectList");

        public TeamController(IConfiguration connectionStrings, IHostEnvironment env, BlogImgSave blogImgSave)
        {
            _connectionStrings = connectionStrings.GetConnectionString("DefaultConnection");
            _env = env.ContentRootPath;
            _blogImgSave = blogImgSave;
        }

        [HttpPost("AddTeam")]
        public async Task<IActionResult> AddTeams([FromForm] TeamModel model)
        {
            dynamic res;
            string query;
            var result = 0;
            string imgSave = null;
            try
            {
                using (var _conn = new SqlConnection(_connectionStrings))
                {
                    _conn.Open();
                    using (var _tran = _conn.BeginTransaction())
                    {
                        try
                        {
                            string getMaxQuery = "SELECT MAX(TeamTran) FROM TeamRes";
                            var maxId = await _conn.ExecuteScalarAsync<int?>(getMaxQuery, transaction: _tran);
                            var Teamid = maxId == null ? 1001 : maxId.Value + 1;

                            if (model.imgPath != null)
                            {
                                imgSave = await _blogImgSave.saveImg2(model.imgPath, "Team_web");
                            }
                            query = $@"Insert into TeamRes ([TeamTran], [ImgPath], [Name], [Position]) values({Teamid}, '{imgSave}', '{model.name}', '{model.position}')";
                            result = await _conn.ExecuteAsync(query, transaction: _tran);

                           
                            _tran.Commit();
                            res = new
                            {
                                state = true,
                                results = result
                            };
                        }
                        catch(Exception ex)
                        {
                            _tran.Rollback();
                            res = new
                            {
                                state = false,
                                message = ex.Message
                            };
                        }
                    }
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

        [HttpGet("GetTeamList")]
        public async Task<IActionResult> allRec()
        {
            dynamic res;
            string query = "";
            try
            {
                using (var _conn = new SqlConnection(_connectionStrings))
                {
                    _conn.Open();
                    query = $@"select * from TeamRes";
                    var result = await _conn.QueryAsync(query);
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
                    res = ex.Message
                };
            }
            return Ok(res);
        }

        [HttpDelete("DeleteTeam2")]
        public async Task<IActionResult> TeamDelete([FromQuery] int teamTranId)
        {
            dynamic res;
            string query;
            var result=0;
            try
            {
                using (var _conn = new SqlConnection(_connectionStrings))
                {
                    _conn.Open();

                    var resultx = await _conn.QueryFirstOrDefaultAsync<string>($@"select ImgPath from TeamRes where TeamTran = {teamTranId}");
                    if(resultx != null)
                    {
                        await _blogImgSave.DelImg(resultx);
                    }
                    query = $@"delete from TeamRes where TeamTran = {teamTranId}";
                    result = await _conn.ExecuteAsync(query);
                    res = new
                    {
                        status = true,
                        message = result
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

        [HttpPost("TeamUpdate")]
        public async Task<IActionResult> updateTeam([FromForm] TeamModel model)
        {
            dynamic res;
            string querry;
            string imgSave = null;
            try
            {
                using (var _conn = new SqlConnection(_connectionStrings))
                {
                    _conn.Open();

                    //var resultx = await _conn.QueryFirstOrDefault($@"select ImgPath from TeamRes where TeamTran = {model.TeamTran}");
                    var resultx = await _conn.QueryFirstOrDefaultAsync<string>(
                    "SELECT ImgPath FROM TeamRes WHERE TeamTran = @TeamTran",
                    new { TeamTran = model.TeamTran });
                    if (model.imgPath != null)
                    {
                        await _blogImgSave.DelImg(resultx ?? "");
                        imgSave = await _blogImgSave.saveImg2(model.imgPath, "Team_web");
                        querry = $@"update TeamRes set [Name] = '{model.name}', [ImgPath]='{imgSave}', [Position]= '{model.position}' where TeamTran = {model.TeamTran}";
                    }
                    else
                    {
                        querry = $@"update TeamRes set [Name] = '{model.name}', [Position]= '{model.position}' where TeamTran = {model.TeamTran}";

                    }
                    var result = await _conn.ExecuteAsync(querry);
                    res = new
                    {
                        status = true,
                        results = result
                    };
                }
            }catch(Exception ex)
            {
                res = new
                {
                    status = false,
                    message = ex.Message
                };
            }
            return Ok(res);
        }
    }
}
