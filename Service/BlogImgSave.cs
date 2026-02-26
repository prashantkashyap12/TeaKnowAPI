using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.Notifications;
using userPanelOMR.model.web;

namespace userPanelOMR.Service
{
    public class BlogImgSave
    {

        private readonly IWebHostEnvironment _webHostEnv;
        private readonly IWebHostEnvironment _evn;
        public BlogImgSave(IWebHostEnvironment webHostEnv, IWebHostEnvironment evn)
        {
            _webHostEnv = webHostEnv;
            _evn = evn;
        }
        public async Task<string> DelImg(string imgPath)
        {
            dynamic res;
            string FolderPath2 = string.Empty;
            try
                {
                if (imgPath == null || imgPath == "")
                {
                    return "value is empty";
                }
                var ext = Path.GetExtension(imgPath).ToLower();
                var allowExt = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                if (!allowExt.Contains(ext))
                {
                    res = "Your Image is not .jpg / .jpeg / .png / .pdf";
                }
                else
                {
                    var fileDel = Path.Combine(_evn.ContentRootPath,  imgPath);
                    if (File.Exists(fileDel))
                    {
                        File.Delete(fileDel);
                        res = "Image Deleted Successfully";
                    }
                    else
                    {
                        res =  "Image Not Founded";
                    }
                }
            }
            catch(Exception ex)
            {
                res = ex.Message;
            }
            return res;
        }


        public async Task<string> saveImg2(IFormFile imgPath, String FolderPath)
        {
            dynamic res;
            string FolderPath2 = string.Empty;
            string folderPathMs = FolderPath;
            string fileName = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(FolderPath))
                {
                    return "Data is not exist.";
                }
                var ext = Path.GetExtension(imgPath.FileName).ToLower();
                var allowExt = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                if (!allowExt.Contains(ext))
                {
                    return "Your Image is not .jpg / .jpeg / .png / .pdf";
                }
                FolderPath = Path.Combine(_evn.ContentRootPath, "WebImg", FolderPath);
                if (!Directory.Exists(FolderPath))
                {
                    Directory.CreateDirectory(FolderPath);
                }
                var timeDat = DateTime.Now.ToString("yyyyMMddHHmm");
                fileName = timeDat +"_"+ Guid.NewGuid()+ ext;

                var imgPath2 = Path.Combine(FolderPath, fileName);
                using (var stream = new FileStream(imgPath2, FileMode.Create))
                {
                    await imgPath.CopyToAsync(stream);
                }
                FolderPath2 = "Image Saved Successfully";
            }

            catch (Exception ex)
            {
                FolderPath2 = ex.Message;
            }
            var savePath = "" ;
            savePath = "";
            savePath = "WebImg/" + folderPathMs + "/" + fileName;
            return savePath;
        }


    }
}
