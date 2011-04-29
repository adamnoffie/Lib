<%@ WebHandler Language="C#" Class="GetImage" %>
using System;
using System.Web;
using System.IO;
using System.Linq;

public class GetImage : IHttpHandler {

    /// <summary>
    /// Returns an image file stored in the FighterPictures database table
    /// </summary>
    public void ProcessRequest(HttpContext context)
    {
        try
        {
            // passed in is the id of picture to fetch
            int id;
            if (!int.TryParse(context.Request.QueryString["id"], out id))
                throw new ArgumentException("Missing 'id' query string param");

            Data.Image img = ImageController.GetByID(id);

            if (img != null)
            {
                context.Response.ContentType = img.ContentType;
                context.Response.Cache.SetCacheability(HttpCacheability.Public);
                // image will ALWAYS be the same at the given id
                context.Response.Cache.SetExpires(DateTime.UtcNow.AddYears(1));
                context.Response.BinaryWrite(img.ImageData.ToArray());
            }
            else
                throw new Exception("No image with that ID");
        }
        catch (Exception)
        {
            // if there is any issue, return a 404 File Not Found status
            context.Response.StatusCode = 404;
            context.Response.Clear();
            context.Response.Write("404 - File Not Found");
        }
    }

    public bool IsReusable
    {
        get { return false; }
    }

}