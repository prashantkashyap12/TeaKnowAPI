using System.Transactions;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using userPanelOMR.model.web;
using userPanelOMR.Service;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace userPanelOMR.Controllers.web
{
    public class DonateController : Controller
    {
        private readonly IConfiguration configuration; 
        private readonly string _connectionStrings;
        private readonly BlogImgSave _blogImgSave;

        public DonateController(IConfiguration configuration, BlogImgSave blogImgSave)
        {
            this.configuration = configuration;
            _connectionStrings = configuration.GetConnectionString("DefaultConnection");
            _blogImgSave = blogImgSave;
        }

        [HttpPost("AddDontLst")]
        public async Task<IActionResult> AddList([FromForm] donationList model)
        {
            dynamic res;
            var querry="";
            try {
                using (var _conn = new SqlConnection(_connectionStrings))
                {
                    await _conn.OpenAsync();
                    var imgSave = string.Empty;
                    string getMaxQuery = "SELECT MAX(DonationTran) FROM DonteLs";
                    var maxId = await _conn.ExecuteScalarAsync<int?>(getMaxQuery);
                    var Dontid = maxId == null ? 1001 : maxId.Value + 1;
                    using (var _tran = _conn.BeginTransaction())
                    {
                        try
                        {
                            //save image to file system
                            if (model.imgPath != null)
                            {
                                imgSave = await _blogImgSave.saveImg2(model.imgPath, "Donate_web");
                            }

                            var result = 0;
                            querry = $@"insert into DonteLs ([DonationTran], [ImgPath], [Categ], [Rais], [Goal], [Heading], [Para], [Progress], [link]) values('{Dontid}', '{imgSave}', '{model.categ}','{model.rais}', '{model.goal}', '{model.heading}', '{model.para}', '{model.progress}', '{model.link}');
                                        insert into DonateDetails ([DonationTran]) values('{Dontid}')";
                            result = await _conn.ExecuteAsync(querry, transaction: _tran);

                            _tran.Commit();
                            res = new
                            {
                                state = true,
                                results = result
                            };
                        }
                        catch (Exception ex)
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

        [HttpGet("getDontLst")]
        public async Task<IActionResult> ListDontLst()
         {
            dynamic result;
            try
            {
                using (var _conn = new SqlConnection(_connectionStrings))
                {
                    _conn.Open();
                    var qurry = @$"select * from DonteLs";
                    var res = await _conn.QueryAsync(qurry);
                    result = new
                    {
                        state = true,
                        res = res
                    };
                }
            }
            catch (Exception ex)
            {
                result = new
                {
                    state = false,
                    message = ex.Message
                };
            }
            return Ok(result);
        }

        [HttpDelete("DelDontLst")]
        public async Task<IActionResult> deleteBlog([FromQuery] int donationTran)
        {
            dynamic res;
            string querry;
            try
            {
                using (var connection = new SqlConnection(_connectionStrings))
                {
                    connection.Open();
                    string query = $@"DELETE FROM DonteLs WHERE DonationTran = @idd;";
                    int rowaffeact = await connection.ExecuteAsync(query, new { idd = donationTran });
                    res = new
                    {
                        state = true,
                        rowaffeact = rowaffeact,
                        message = "Donate List data deleted success"
                    };

                    if (rowaffeact > 0)
                    {
                        querry = @$"select * from DonateDetails where DonationTran = @Id;";
                        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(querry, new { Id = donationTran });
                        if (result != null)
                        {
                            querry = @$"delete from DonateDetails where DonationTran = @Id;";
                            var result2 = await connection.ExecuteAsync(querry, new { Id = donationTran });
                            res = new
                            {
                                state = true,
                                result = result,
                                massege = "DonateList and DonateDetails data deleted success"
                            };
                        }
                        else
                        {
                            res = new
                            {
                                state = true,
                                message = "record deleted from Donate List"
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

        [HttpPost("UpdateDontLst")]
        public async Task<IActionResult> DontupdateList([FromForm] donationList model)
        {
            dynamic res;
            string querry;
            string imgSave = Empty.ToString();
            try
            {
                using (var conn = new SqlConnection(_connectionStrings))
                {
                    conn.Open();
                    using(var _tran = conn.BeginTransaction())
                    {
                        try
                        {
                            var resultx = await conn.ExecuteScalarAsync<string>(@$"select ImgPath from DonteLs where DonationTran = {model.donationTran}", transaction: _tran);
                            if (model.imgPath != null)
                            {
                                _blogImgSave.DelImg(resultx??"");
                                imgSave = await _blogImgSave.saveImg2(model.imgPath, "Donate_web");
                            }
                            querry = $@"UPDATE [DonteLs] SET [ImgPath]='{imgSave}', [Categ]='{model.categ}', [Rais]='{model.rais}', [Goal]='{model.goal}', [Heading]='{model.heading}', 
                                    [Para]='{model.para}', [Progress]='{model.progress}', [link] = '{model.link}' where [DonationTran] = @Id";
                            var result = await conn.ExecuteAsync(querry, new { Id = model.donationTran }, _tran);
                            _tran.Commit();
                            res = new
                            {
                                state = true,
                                message = result
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
                    conn.Close();
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


        [HttpPost("UpdateDonateDetails")]
        public async Task<IActionResult> UpdateDonateDetails([FromForm] donationDetails model)
        {
            dynamic res;
            var query = "";
            var imgSave = string.Empty;
            using (var _conn = new SqlConnection(_connectionStrings))
            {
                _conn.Open();
                try
                {
                    //using(var _tran = _conn.BeginTransaction())
                    //{
                        try
                        {

                            string img1 = null;
                            string img1val = "";
                            string img2 = null;
                            string img2Val = "";
                            string img3 = null;
                            string img3Val = "";
                            var resultx = await _conn.QueryFirstOrDefaultAsync<dynamic>($@"select * from DonateDetails where DonationTran = {model.donationTran}");

                       

                            if (model.img != null)
                            {
                                _blogImgSave.DelImg(resultx.Img);
                                img1 = await _blogImgSave.saveImg2(model.img, "DonateDetails");
                                img1val = $"Img = '{img1}',"; 
                            }
                            if (model.smr_img1 != null)
                            {
                                _blogImgSave.DelImg(resultx.SmrImg1);
                                img2 = await _blogImgSave.saveImg2(model.smr_img1, "DonateDetails");
                                img2Val = $"SmrImg1 = '{img2}',";
                            }
                            if (model.smr_img2 != null)
                            {
                                _blogImgSave.DelImg(resultx.SmrImg2);
                                img3 = await _blogImgSave.saveImg2(model.smr_img2, "DonateDetails");
                                img3Val = $"SmrImg2 = '{img3}',";
                            }
                            query = $@"UPDATE DonateDetails SET
                                {img1val}
                                {img2Val}
                                {img3Val}
                                ColAmt = '{model.colAmt}', 
                                Raised = '{model.raised}', 
                                Progres = '{model.progres}', 
                                Notes = '{model.notes}', 
                                FixAmt1 = '{model.fix_amt1}', 
                                FixAmt2 = '{model.fix_amt2}', 
                                FixAmt3 = '{model.fix_amt3}', 
                                FixAmt4 = '{model.fix_amt4}', 
                                FixAmt5 = '{model.fix_amt5}', 
                                SumryPera = '{model.sumryPera}', 
                                SumryLi1 = '{model.sumryLi1}', 
                                SumryLi2 = '{model.sumryLi2}', 
                                SumryLi3 = '{model.sumryLi3}', 
                                SumryLi4 = '{model.sumryLi4}', 
                                SumryLi5 = '{model.sumryLi5}', 
                                SumryLi6 = '{model.sumryLi6}', 
                                SumryLi7 = '{model.sumryLi7}', 
                                SumryLi8 = '{model.sumryLi8}', 
                                SmrPra1 = '{model.smr_pra1}', 
                                SmrBlockqt = '{model.smr_blockqt}', 
                                SmrPra2 = '{model.smr_pra2}'
                                WHERE DonationTran = '{model.donationTran}';";
                            var results =  _conn.Execute(query);
                            res = new
                            {
                                state = true,
                                result = results
                            };
                        }
                        catch (Exception ex)
                        {
                            res = new
                            {
                                state = true,
                                result = ex.Message
                            };
                        }
                    //}
                }
                catch(Exception ex)
                {
                    res = new
                    {
                        state = false,
                        message = ex.Message
                    };
                }
            }
            return Ok(res);
        }


        [HttpGet("getDonateDetails")]
        public async Task<IActionResult> getDonateDetails([FromQuery] int doantTran)
        {
            dynamic res;
            string query;

            try
            {
                using(var _conn = new SqlConnection(_connectionStrings))
                {
                    query = $@"SELECT dd.Img, dd.ColAmt, dd.Raised, dd.Progres, dd.Notes, dd.FixAmt1, dd.FixAmt2, dd.FixAmt3, dd.FixAmt4, dd.FixAmt5, dd.SumryPera, 
                    dd.SumryLi1, dd.SumryLi2, dd.SumryLi3, dd.SumryLi4, dd.SumryLi5, dd.SumryLi6, dd.SumryLi7, dd.SumryLi8, dd.SmrImg1, dd.SmrImg2, 
                    dd.SmrPra1, dd.SmrBlockqt, dd.SmrPra2, dd.DonationTran FROM DonateDetails dd
                    left join DonteLs on dd.DonationTran = DonteLs.DonationTran 
                    where DonteLs.DonationTran = @donatTran";
                    var result = await _conn.QueryAsync(query, new { donatTran = doantTran});
                    
                    res = new
                    {
                        state = true,
                        result = result
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

        [HttpGet("ListDonateDetails")]
        public async Task<IActionResult> DonateDetails()
        {
            dynamic res;
            string querry;
            try
            {
                using (var conn = new SqlConnection(_connectionStrings))
                {
                    conn.Open();
                    querry = $@"select * from DonateDetails";
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
