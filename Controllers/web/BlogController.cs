using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using userPanelOMR.model.web;
using userPanelOMR.Service;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace userPanelOMR.Controllers.web
{
    public class BlogController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly string _connectionStrings;
        private readonly string _root = Environment.CurrentDirectory + "\\wFileManager";
        private readonly BlogImgSave _blogImgSave;

        public BlogController(IConfiguration configuration, BlogImgSave blogImgSave)
        {
            this.configuration = configuration;
            _connectionStrings = configuration.GetConnectionString("DefaultConnection");
            _blogImgSave = blogImgSave;
        }

        // Create Blog - short info  5 input on baseed tranId
        [HttpPost]
        [Route("blogAdd")]
        public async Task<IActionResult> blogAdd([FromForm] blogList model)
        {
            dynamic res;
            string querry;
            var result = "";
            var imgpath = string.Empty;
            var imgSave = string.Empty;
            try {
                using (var connection = new SqlConnection(_connectionStrings))
                {
                    connection.Open();

                    string getMaxQuery = "SELECT MAX(blogTran) FROM BlogList";
                    var maxId = await connection.ExecuteScalarAsync<int?>(getMaxQuery);
                    var blogid = maxId == null ? 1001 : maxId.Value + 1;

                    //save image to file system
                    if (model.imgPath != null)
                    {
                        imgSave = await _blogImgSave.saveImg2(model.imgPath, "Blog_web");
                        imgpath = $"[imgPath],";
                    }

                    // check insert data into database
                    var querry2 = @$"Insert into BlogList ([blogTran], {imgpath} [date], [rights], [type], [link], [linkText1], [linkText2]) Values('{blogid}', '{imgSave}', '{model.date}', '{model.rights}', '{model.type}', '{model.link}', '{model.linkText1}', '{model.linkText2}'); 
                                     Insert into BlogDetails ([blogTran]) Values('{blogid}')";
                    var result2 = await connection.ExecuteAsync(querry2);
                    res = new
                    {
                        state = true,
                        message1 = "Blog Added Successfully",
                        message2 = imgSave
                    };
                }
            }
            catch (Exception ex) {
                res = new {
                    state = false,
                    message = ex.Message
                };
            }
            return Ok(res);
        }
        
        // Show blog - short info 5 input All. - ngOnInit()
        [HttpGet("blogList")]
        public async Task<IActionResult> getRes()
        {
            dynamic res;
            string querry;
            try
            {
                using (var connection = new SqlConnection(_connectionStrings))
                {
                    connection.Open();
                    querry = $"select * from blogList";
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

        // Delete Blog - short info 5 input on baseed tranId
        [HttpDelete("blogDel")]
        public async Task<IActionResult> deleteBlog([FromQuery] string BlogTran)
        {
            dynamic res;
            string querry;
            try {
                using (var connection = new SqlConnection(_connectionStrings))
                {
                    connection.Open();

                    var queryDelFind = $@"select * from blogList where blogTran = @blgId";
                    var blogLsDel = connection.QueryFirstOrDefault(queryDelFind, new { blgId = BlogTran });

                    if (!string.IsNullOrEmpty(blogLsDel.imgPath))
                    {
                        _blogImgSave.DelImg(blogLsDel.imgPath.ToString());
                    }
                    string query = $@"DELETE FROM blogList WHERE blogTran = @idd;";
                    int rowaffeact = await connection.ExecuteAsync(query, new {idd = BlogTran});
                    res = new
                    {
                        state = true,
                        rowaffeact = rowaffeact,
                        message = "blogList data deleted success"
                    };


                    if (rowaffeact > 0)
                    {
                        querry = @$"select * from BlogDetails where blogTran = @Id;";
                        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(querry, new { Id =  BlogTran});

                        if (!string.IsNullOrEmpty(result.Img))
                        {
                            _blogImgSave.DelImg(result.Img.ToString());
                        }
                        if (!string.IsNullOrEmpty(result.BlogImg1))
                        {
                            _blogImgSave.DelImg(result.BlogImg1.ToString());
                        }
                        if (!string.IsNullOrEmpty(result.BlogImg2))
                        {
                            _blogImgSave.DelImg(result.BlogImg2.ToString());
                        }
                        if (result != null)
                        {
                            querry = @$"delete from blogDetails where blogTran = @Id;";
                            var result2 = await connection.ExecuteAsync(querry, new { Id = BlogTran});
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
            catch (Exception ex) {
                res = new
                {
                    state = false,
                    message = ex.Message
                };
            }
            return Ok(res);
        }

        // Update Blog - short info 5 input on baseed tranId
        [HttpPost("blogUpdate")]
        public async Task<IActionResult> update([FromForm] blogList model)
        {
            dynamic res;
            string querry;
            var imgSave = string.Empty;
            try
            {
                using (var conn = new SqlConnection(_connectionStrings))
                {
                    conn.Open();
                    var oldImagePath = "";
                    //save image to file system
                    if (model.imgPath != null)
                    {
                        string getImagePathQuery = "SELECT imgPath FROM BlogList WHERE blogTran = @blogTran";
                        oldImagePath = await conn.ExecuteScalarAsync<string>(getImagePathQuery, new { blogTran = model.blogid });
                        if (!string.IsNullOrEmpty(oldImagePath))
                        {
                            await _blogImgSave.DelImg(oldImagePath);
                        }
                        imgSave = await _blogImgSave.saveImg2(model.imgPath, "Blog_web");
                        querry = $@"UPDATE [BlogList] SET [imgPath] = '{imgSave}', [date]='{model.date}', [rights]='{model.rights}', [type]='{model.type}', [link]='{model.link}', [linkText1]='{model.linkText1}', [linkText2]='{model.linkText2}' where blogTran = @Id";
                    }
                    else
                    {
                        querry = $@"UPDATE [BlogList] SET [date]='{model.date}', [rights]='{model.rights}', [type]='{model.type}', [link]='{model.link}', [linkText1]='{model.linkText1}', [linkText2]='{model.linkText2}' where blogTran = @Id";
                    }
                    var result = await conn.ExecuteAsync(querry, new { Id = model.blogid });
                    var dsresult = result > 0 ? "Data updated successfully" : "No data found to update";
                    res = new
                    {
                        state = true,
                        message = dsresult
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

        // Create Blog - full info 10 input on baseed tranId.  We will passing model, with blogTran. as where blogTran is mathed then update row.
        [HttpPost("blogDetUpdate")]
        public async Task<IActionResult> blogDetAdd([FromForm] blogDetails model)
        {
            dynamic res;
            string query;
            var imgSave1Col = string.Empty;
            var imgSave1 = string.Empty;

            var imgSave2Col = string.Empty;
            var imgSave2 = string.Empty;

            var imgSave3Col = string.Empty;
            var imgSave3 = string.Empty;
            try
            {
                using (var conn = new SqlConnection(_connectionStrings))
                {
                    await conn.OpenAsync();

                    var queryColumns = new List<string>
                    {
                        "Rights = @vRight",
                        "type = @vType",
                        "Head = @vhead",
                        "pera1 = @vpera1",
                        "pera2 = @vPera2",
                        "blockquate = @vBlock",
                        "pera3 = @vpera3",
                        "pera4 = @vpera4",
                        "fbLink = @vfbLink",
                        "twLink = @vtwLink",
                        "linkLink = @vLink",
                        "instLink = @vInstalLink"
                    };

                    var parameters = new DynamicParameters(new
                    {
                        vRight = model.rigths,
                        vType = model.type,
                        vhead = model.head1,
                        vpera1 = model.pera1,
                        vPera2 = model.pera2,
                        vBlock = model.blockquate,
                        vpera3 = model.pera3,
                        vpera4 = model.pera4,
                        vfbLink = model.fbLink,
                        vtwLink = model.twLink,
                        vLink = model.linkLink,
                        vInstalLink = model.instLink,
                        vblogTn = model.blogTran
                    });

                    // Get existing record
                    var querry2 = @"SELECT * FROM BlogDetails WHERE blogTran = @Id";
                    var result = await conn.QueryFirstOrDefaultAsync<dynamic>(querry2, new { Id = model.blogTran });

                    // Conditional image fields
                    if (model.img != null)
                    {
                        _blogImgSave.DelImg(result.BlogImg2);
                        var savedPath = await _blogImgSave.saveImg2(model.img, "BlogDetails");
                        queryColumns.Add("img = @vImg");
                        parameters.Add("@vImg", savedPath);
                    }

                    if (model.blogImg1 != null)
                    {
                        _blogImgSave.DelImg(result.Img);
                        var savedPath = await _blogImgSave.saveImg2(model.blogImg1, "BlogDetails");
                        queryColumns.Add("blogImg1 = @vBlogImg1");
                        parameters.Add("@vBlogImg1", savedPath);
                    }

                    if (model.blogImg2 != null)
                    {
                        _blogImgSave.DelImg(result.BlogImg1);
                        var savedPath = await _blogImgSave.saveImg2(model.blogImg2, "BlogDetails");
                        queryColumns.Add("blogImg2 = @vBlogImg2");
                        parameters.Add("@vBlogImg2", savedPath);
                    }

                    var setClause = string.Join(", ", queryColumns);

                    var finalQuery = $@" UPDATE blogDetails 
        SET {setClause}
        WHERE blogTran = @vblogTn";

                    var updatedCount = await conn.ExecuteAsync(finalQuery, parameters);

                    res = new
                    {
                        state = true,
                        insertedId = updatedCount,
                        message = "Data updated into DB successfully"
                    };
                }
            }
            catch (Exception ex)
            {
                res = new
                {
                    state = false,
                    message = ex.Message,
                    detail = ex.StackTrace
                };
            }
            return Ok(res);
        }

        // Show blog - full info 10 on the based of tranId. - ngOnInit() 
        [HttpGet("blogDetRec")]
        public async Task<IActionResult> blogDetList([FromQuery] string blogTran)
        {
            dynamic res;
            string querry;
            try
            {
                using (var conn = new SqlConnection(_connectionStrings))
                {
                    conn.Open();
                    querry = $@"SELECT 
                        bds.blogTran,
                        blogLs.linkText1, 
                        bds.img, 
                        bds.Rights, bds.type, bds.Head, bds.Pera1, bds.Pera2, 
                        bds.Blockquate, bds.Pera3, bds.BlogImg1, bds.BlogImg2, bds.Pera4, bds.FbLink, 
                        bds.TwLink, bds.LinkLink, bds.InstLink 
                        FROM BlogDetails bds 
                        left join blogList blogLs 
                        on bds.blogTran = blogLs.blogTran 
                        where blogLs.blogTran = @blogTran";
                    var result = await conn.QueryAsync(querry, new { blogTran });
                    res = new
                    {
                        state = true,   
                        result = result

                    };
                }
            } catch (Exception ex)
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
