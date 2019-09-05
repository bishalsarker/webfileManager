using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using fileBrowser.Models;

namespace fileBrowser.Controllers
{
    public class HomeController : Controller
    {
        // GET: /Home/
        public ActionResult Index()
        {
            return View();
        }


        //API
        [HttpPost]
        public JsonResult UploadFiles(HttpPostedFileBase[] files, string loc)
        {
            string msg = "";

            loc = getPath(loc);

            foreach (HttpPostedFileBase file in files)
            { 
                if (file != null)
                {
                    var InputFileName = Path.GetFileName(file.FileName);
                    string path = "";
                    if (loc == "/")
                    {
                        path = Path.Combine(Server.MapPath("~/App_Data" + "/" + InputFileName));
                    }
                    else
                    {
                        path = Path.Combine(Server.MapPath("~/App_Data" + loc + "/" + InputFileName));
                    }
                    file.SaveAs(path);
                    msg = "{'status': '200', 'loc': '"+path+"'}";
                }
                else
                {
                    msg = "{'status': '500'}";
                }

            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }  
        public JsonResult getDir(string folder, string currPage)
        {
            string path = "";
            DirInfoModel dim = new DirInfoModel();

            string curr = getPath(currPage);

            if (folder == "" || folder == null)
            {
                path = Server.MapPath("~/App_Data" + curr);
                dim.curr = "/";
            }
            else
            {
                if (curr == "/")
                {
                    path = Server.MapPath("~/App_Data" + curr + folder);
                    dim.curr = curr + folder;
                }
                else
                {
                    path = Server.MapPath("~/App_Data" + curr + "/" + folder);
                    dim.curr = curr + "/" + folder;
                }
            }

            DirInfoModel dim2 = new DirInfoModel();
            dim2 = getDirDetails(path);

            dim.status = dim2.status;
            dim.folders = dim2.folders;
            dim.files = dim2.files;

            if (dim2.status == "500")
            {
                dim.curr = null;
            }
            return Json(dim, JsonRequestBehavior.AllowGet);
            
        }
        public JsonResult getPrevDir(string currPage)
        {
            string path = "";
            DirInfoModel dim = new DirInfoModel();

            string curr = getPrevPath(currPage);

            path = Server.MapPath("~/App_Data" + curr);

            DirInfoModel dim2 = new DirInfoModel();
            dim2 = getDirDetails(path);

            dim.status = dim2.status;
            dim.folders = dim2.folders;
            dim.files = dim2.files;
            dim.curr = curr;

            if (dim2.status == "500")
            {
                dim.curr = null;
            }
            return Json(dim, JsonRequestBehavior.AllowGet);

        }
        public JsonResult refreshDir(string currPage)
        {
            string path = "";
            DirInfoModel dim = new DirInfoModel();

            string curr = getPath(currPage);

            path = Server.MapPath("~/App_Data" + curr);

            DirInfoModel dim2 = new DirInfoModel();
            dim2 = getDirDetails(path);

            dim.status = dim2.status;
            dim.folders = dim2.folders;
            dim.files = dim2.files;
            dim.curr = curr;

            if (dim2.status == "500")
            {
                dim.curr = null;
            }

            return Json(dim, JsonRequestBehavior.AllowGet);

        }
        public JsonResult createDir(string folderName, string location)
        {
            String path = "";
            String fName = "";
            String msg = "";

            string loc = getPath(location);

            if (folderName == "" || folderName == null)
            {
                fName = "Unnamed Folder";
            }
            else
            {
                fName = folderName;
            }

            if (loc == "/")
            {
                path = Server.MapPath("~/App_Data" + "/" );
            }
            else
            {
                path = Server.MapPath("~/App_Data" + loc);
            }

            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                dir.CreateSubdirectory(fName);
                msg = "{'status': '200'}";
            }
            catch (Exception e)
            {
                msg = "{'status': '500'}";
            }

            return Json(msg, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult delete(string names, string location)
        {
            var d = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<ListModel> list = d.Deserialize<List<ListModel>>(names);

            string curr = getPath(location);
            string msg = "";

            foreach (ListModel l in list)
            {
                string path = Server.MapPath("~/App_Data" + curr + "/" + l.name);

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    msg = "200";
                }
                else if (Directory.Exists(path))
                {
                    System.IO.Directory.Delete(path, true);
                    msg = "200";
                }
                else
                {
                    msg = "500";
                }
            }

            return Json("{'status': '" + msg + "'}", JsonRequestBehavior.AllowGet);
        }
        public JsonResult getUsedSpace()
        {
            String path = Server.MapPath("~/App_Data");
            DirectoryInfo di = new DirectoryInfo(path);
            long val = di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
            string usage = convertFileSize(val);
            return Json(usage, JsonRequestBehavior.AllowGet);
        }

        //Functions
        private DirInfoModel getDirDetails(string path)
        {
            DirInfoModel dim = new DirInfoModel();
            try
            {
                List<FileInfoModel> fileInfo = new List<FileInfoModel>();
                List<FolderInfoModel> folderInfo = new List<FolderInfoModel>();
                DirectoryInfo dir = new DirectoryInfo(path);
                DirectoryInfo[] subDir = dir.GetDirectories();
                FileInfo[] files = dir.GetFiles();

                foreach (DirectoryInfo d in subDir)
                {
                    folderInfo.Add(new FolderInfoModel
                    {
                        folderName = d.Name
                    });
                }
                foreach (FileInfo f in files)
                {
                    

                    fileInfo.Add(new FileInfoModel
                    {
                        fileName = f.Name,
                        size = convertFileSize(f.Length)
                    });
                }

                if (fileInfo.Count() < 1)
                {
                    dim.files = null;
                }
                else
                {
                    dim.files = fileInfo;
                }

                if (folderInfo.Count() < 1)
                {
                    dim.folders = null;
                }
                else
                {
                    dim.folders = folderInfo;
                }

                dim.status = "200";
                return dim;
            }
            catch (Exception e)
            {
                dim.status = "500";
                dim.files = null;
                dim.folders = null;
                return dim;
            }
        }

        //Utilities
        private string getPath(string p)
        {
            string[] pSplit = p.Split('/');
            string lis = "";
            int i = 1;
            while (i < pSplit.Length)
            {
                string t = pSplit[i].Trim();
                lis = lis + "/" + t;
                i++;
            }

            if (lis.Trim() == "" || lis.Trim() == null || lis.Trim() == "/")
            {
                lis = "/";
            }

            return lis;
        }
        public string getPrevPath(string p)
        {
            string[] pSplit = p.Split('/');
            string lis = "";
            int i = 1;
            while (i < pSplit.Length-1)
            {
                string t = pSplit[i].Trim();
                lis = lis + "/" + t;
                i++;
            }

            if (lis.Trim() == "" || lis.Trim() == null || lis.Trim() == "/")
            {
                lis = "/";
            }

            return lis;
        }
        private string convertFileSize(long val)
        {
            double v = val;
            if (v < 1048576.00)
            {
                v = (v / 1024);
                return v.ToString("0.00") + " kb";
            }
            else
            {
                v = (v / 1024) / 1024;
                return v.ToString("0.00") + " mb";
            }
        }

	}
}