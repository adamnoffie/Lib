<%@ WebHandler Language="C#" Class="UploadifyImages" %>

using System;
using System.Web;
using System.Drawing;
using System.IO;
using System.Configuration;

/// <summary>
/// Generic handler for the jQuery uploadify plug-in
/// </summary>
public class UploadifyImages : IHttpHandler 
{
    const string savePathUrl = "~/Admin/uploads/";
        
    public void ProcessRequest (HttpContext context) 
    {
        bool imageScaled = false;
        context.Response.ContentType = "text/plain";
        context.Response.Expires = -1;

        Image upImg;

        try
        {
            HttpPostedFile file = context.Request.Files["Filedata"];

            // test to make sure it was an image uploaded, will throw exception if not
            try
            {
                // convert file uplaoded to image, and scale down if appropriate
                upImg = Image.FromStream(file.InputStream);
                int maxDimension = PhotoRepository.Settings.Photo_MaxDimension;
                if (upImg.Width > maxDimension || upImg.Height > maxDimension)
                {
                    upImg = Utility.Imaging.ScaleImage(upImg, maxDimension);
                    imageScaled = true;
                }
            }
            catch (Exception)
            {
                throw new Exception("Not a valid image file");
            }

            string savePath = context.Server.MapPath(savePathUrl);
            string filename = file.FileName;

            // determine the file name to save as, even if the file already exists (we'll just append index)
            int i = 1;
            string filepath = Path.Combine(savePath, filename);
            while (File.Exists(filepath))
            {
                string ext = Path.GetExtension(file.FileName);
                filename = Path.GetFileNameWithoutExtension(file.FileName) + i.ToString() + ext;
                i++;
                filepath = Path.Combine(savePath, filename);
            }

            if (imageScaled)
                Utility.Imaging.SaveJpeg(filepath, upImg, 95);
            else
                file.SaveAs(filepath);

            context.Response.Write(Utility.Web.ResolveServerUrl(savePathUrl + filename, false));
            context.Response.StatusCode = 201;  // 201 = Successfully Created
        }
        catch (Exception ex)
        {
            context.Response.Write("Error: " + ex.Message);
        }
    }
 
    public bool IsReusable {
        get { return false; }
    }
}