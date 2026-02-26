using System.Transactions;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using userPanelOMR.model.web;
using userPanelOMR.Service;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace userPanelOMR.Controllers.web
{
    public class ProjectController : Controller
    {

        public readonly IConfiguration configuration;
        public readonly string _connectionStrings;
        private readonly BlogImgSave _blogImgSave;

        public ProjectController( IConfiguration configuration, BlogImgSave blogSave)
        {
            _connectionStrings = configuration.GetConnectionString("DefaultConnection");
            _blogImgSave = blogSave;
        }

        [HttpPost("AddProjLs")]
        public async Task<IActionResult> AddProjlist([FromForm] projectList model)
        {
            var qurry="";
            dynamic res;
            string imgSave = string.Empty;
            try
            {
                using (var _conn = new SqlConnection(_connectionStrings))
                {
                    string getMaxQuery = "SELECT MAX(ProjTran) FROM ProjectList";
                    var maxId = await _conn.ExecuteScalarAsync<int?>(getMaxQuery);
                    var projectid = maxId == null ? 1001 : maxId.Value + 1;

                    if (model.Imgpath != null)
                    {
                        imgSave = await _blogImgSave.saveImg2(model.Imgpath, "Project_web");
                    }
                    qurry = @$"Insert into ProjectList ([ProjTran], [Head], [Pera], [Imgpath], [Path]) values({projectid}, '{model.head}', '{model.pera}', '{imgSave}', '{model.path}');
                               Insert into ProjectDetails (ProjTran) values({projectid})";
                    var results = await _conn.ExecuteAsync(qurry);

                    res = new
                    {
                        state=true,
                        result = results
                    };                
                }
            }
            catch (Exception ex)
            {
                res = new
                {
                    state=false,
                    message = ex.Message
                };
            }
            return Ok(res);
        }

        [HttpGet("getProjLs")]
        public async Task<IActionResult> ListProjlist()
        {
            dynamic result;
            try
            {
                using (var _conn = new SqlConnection(_connectionStrings))
                {
                    var qurry = @$"select * from ProjectList";
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

        [HttpDelete("delProjectLs")]
        public async Task<IActionResult> deleteBlog([FromQuery] int projectTran)
        {
            dynamic res;
            string querry;
            var result4 = "";
            try
            {
                using (var connection = new SqlConnection(_connectionStrings))
                {
                    connection.Open();

                    string getImgPath2 = $@"select Imgpath from ProjectList where ProjTran = @ProjId2";
                    result4 = await connection.ExecuteScalarAsync<string>(getImgPath2, new { ProjId2 = projectTran });
                    if (!string.IsNullOrEmpty(result4))
                    {
                        await _blogImgSave.DelImg(result4);
                    }

                    string query = $@"DELETE FROM ProjectList WHERE ProjTran = @idd;";
                    int rowaffeact = await connection.ExecuteAsync(query, new { idd = projectTran });
                    res = new
                    {
                        state = true,
                        rowaffeact = rowaffeact,
                        message = "ProjectList data deleted success"
                    };

                    if (rowaffeact > 0)
                    {
                        // Add Mathord Get path and delete if exists
                        querry = @$"select * from ProjectDetails where ProjTran = @Id;";
                        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(querry, new { Id = projectTran });
                        if (result != null)
                        {
                            if (!string.IsNullOrEmpty(result.Img1))
                            {
                                await _blogImgSave.DelImg(result.Img1.ToString());
                            }
                            if (!string.IsNullOrEmpty(result.Img2))
                            {
                                await _blogImgSave.DelImg(result.Img2.ToString());
                            }
                            if (!string.IsNullOrEmpty(result.Img3))
                            {
                                await _blogImgSave.DelImg(result.Img3.ToString());
                            }
                            querry = @$"delete from ProjectDetails where ProjTran = @Id;";
                            var result2 = await connection.ExecuteAsync(querry, new { Id = projectTran });
                            res = new
                            {
                                state = true,
                                result = result,
                                massege = "ProjectList and ProjectDetails data deleted success"
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

        [HttpPost("updateProjectLs")]
        public async Task<IActionResult> updateProjLs([FromForm] projectList model)
        {
            dynamic res;
            string query;
            string imgSave = null;
            var result2 = "";
            try
            {
                using(var _conn = new SqlConnection(_connectionStrings))
                {
                    if (model.Imgpath != null)
                    {
                        string getImgPath = $@"select Imgpath from ProjectList where ProjTran = @ProjId2";
                        result2 = await _conn.ExecuteScalarAsync<string>(getImgPath, new { ProjId2 = model.projectTran });
                        if (!string.IsNullOrEmpty(result2))
                        {
                            await _blogImgSave.DelImg(result2);
                        }
                        imgSave = await _blogImgSave.saveImg2(model.Imgpath, "Project_web");
                    }
                    query = $@"Update ProjectList Set [Head]='{model.head}', [Pera]='{model.pera}', [Imgpath]='{imgSave}', [Path]='{model.path}' Where ProjTran = @projId";
                    var result = await _conn.ExecuteAsync(query, new  { projId = model.projectTran});
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
                    state = false,
                    results = ex.Message
                };
            }
            return Ok(res);
        }

        [HttpPost("updateProjectDtls")]
        public async Task<IActionResult> updateProjDetls([FromForm] priojectDetails model)
        {
            dynamic res;
            string query;
            string imgSave = null;
            try
            {
                using(var _conn = new SqlConnection(_connectionStrings))
                {
                    _conn.OpenAsync();
                    using (var _tran = _conn.BeginTransaction())
                    {
                        try
                        {
                            var results = 0;
                            var projTran = model.projDetails2[0].projectTran;

                            query = $@"select * from ProjectDetails where ProjTran= @projT";
                            var result2 = await _conn.QueryFirstOrDefaultAsync(query, new { projT = model.projDetails2[0].projectTran}, transaction: _tran);
                            Console.WriteLine(result2);
                            var img1="";
                            var img2 = "";
                            var img3 = "";
                            var projT = 0;
                            foreach (var img in model.projDetails1)
                            {
                                if (img.img1 != null)
                                {
                                    await _blogImgSave.DelImg(result2.Img1);
                                    img1 = await _blogImgSave.saveImg2(img.img1, "ProjectDetails");
                                }if (img.img2 !=null)
                                {
                                    await _blogImgSave.DelImg(result2.Img2);
                                    img2 = await _blogImgSave.saveImg2(img.img2, "ProjectDetails");
                                }if (img.img3 != null)
                                {
                                    await _blogImgSave.DelImg(result2.Img3);
                                    img3 = await _blogImgSave.saveImg2(img.img3, "ProjectDetails");
                                }
                                projT = img.projectTran; 
                            }
                            Console.WriteLine("Next Step");
                            query = $@"update ProjectDetails set [Img1]=@img1A, [Img2]=@img2A, [Img3]=@img3A where ProjTran = @projectTran";
                            results = await _conn.ExecuteAsync(query, new { img1A = img1, img2A = img2, img3A = img3, projectTran = projT},  transaction: _tran);
                            
                            foreach (var cont in model.projDetails2)
                            {
                                query = $@"update ProjectDetails set [Head1]=@head1, [Pera1]=@pera1, [Pera11]=@pera11, [Head2]=@head2, [Pera2]=@pera2, [Head3]=@head3, [Pera3]=@pera3 where ProjTran = @projectTran";
                                results = await _conn.ExecuteAsync(query, cont, transaction: _tran);
                                query = null;
                            }
                            foreach (var into in model.projDetails3)
                            {
                                query = $@"update ProjectDetails set [Cat]=@cat, [Auth]=@auth, [Tag]=@tag, [Cost]=@cost, [Date]=@date where ProjTran = @projectTran";
                                results = await _conn.ExecuteAsync(query, into, transaction:  _tran);
                            }

                            _tran.Commit();
                            res = new
                            {
                                state = true,
                                result = results,
                            };

                        }
                        catch (Exception ex)
                        {
                            _tran.Rollback();
                            res = new
                            {
                                state = false,
                                result = ex.Message
                            };
                        }
                    }
                    _conn.CloseAsync();
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

        [HttpGet("GetDetailsProjId")]
        public async Task<IActionResult> updateProjTran([FromQuery] int ProjTran)
        {
            dynamic res;
            string query = null;
            try
            {
                using (var _conn = new SqlConnection(_connectionStrings))
                {
                    query = @$"SELECT pro.ProjTran, projLs.Head, pro.Head1, pro.Pera1, pro.Pera11, pro.Head2, pro.Pera2, pro.Head3, pro.Pera3, pro.Img1, pro.Img2, pro.Img3, pro.Cat, pro.Auth, pro.Tag, pro.Cost, pro.Date
                    FROM ProjectDetails pro
                    left join ProjectList projLs on
                    projLs.ProjTran = pro.ProjTran
                    where pro.ProjTran = @proTran";
                    var result = await _conn.QueryAsync(query, new { proTran = ProjTran });

                    res = new
                    {
                        state = true,
                        results= result
                    };
                }
            }
            catch (Exception ex)
            {
                res = new
                {
                    state = false,
                    results = ex.Message
                };

            }
            return Ok(res);

        }

        [HttpGet("ListProjectDtls")]
        public async Task<IActionResult> ProjectDetails()
        {
            dynamic res;
            string querry;
            try
            {
                using (var conn = new SqlConnection(_connectionStrings))
                {
                    conn.Open();
                    querry = $@"select * from ProjectDetails";
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
