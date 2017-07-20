using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace JsonService
{
    public partial class AppRelease : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string versionNum = TextBox1.Text;
            string fileName = FileUpload1.FileName;
            string path = @"F:\PrecompiledWeb\PrecompiledWeb\WebUI\EditionURL\app.apk";
            if (versionNum.IndexOf(".") <= 0)
                Response.Write("<script language=javascript>alert('版本号格式有误,请输入正确版本号如(4.1)')</script>");
            if (fileName.ToLower().LastIndexOf(".apk") != fileName.Length - 4)
                Response.Write("<script language=javascript>alert('选择文件格式有误，请选择APK文件')</script>");
            else if (versionNum.IndexOf(".") > 0 && fileName.ToLower().LastIndexOf(".apk") == fileName.Length - 4)
            {
                try
                {
                    if (File.Exists(path))
                        File.Delete(path);

                    Stream fileStream = FileUpload1.PostedFile.InputStream;
                    byte[] bytes = new byte[fileStream.Length];
                    fileStream.Read(bytes, 0, bytes.Length);
                    fileStream.Seek(0, SeekOrigin.Begin);
                    if (File.Exists(path))
                        File.Delete(path);
                    FileStream fs = new FileStream(path , FileMode.Create);
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(bytes);
                    bw.Close();
                    fs.Close();

                    StreamWriter sw = new StreamWriter(path.Replace(".apk",".txt"),false);
                    sw.WriteLine(versionNum);
                    sw.Flush();
                    sw.Close();

                    Response.Write("<script language=javascript>alert('上传成功')</script>");
                }
                catch (Exception ex)
                {
                    Response.Write("<script language=javascript>alert('上传失败')</script>");
                }
            }
        }
    }
}