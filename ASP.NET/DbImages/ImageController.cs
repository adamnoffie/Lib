using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Xml.Linq;
using Data;
using System.Collections.Generic;


/// <summary>
/// Represents the ImageTypes lookup table in the database
/// <remarks>DO NOT CHANGE unless you know what you are doing.</remarks>
/// </summary>
public enum ImageTypes : int
{
    GalleryPhoto = 100,
    GalleryPhoto_Thumb = 110,
    Banner = 400,
    PageBox = 1200,
    PrimaryDomain = 7000,
    PrimaryDomain_Thumb = 7010
} 

/// <summary>
/// Images table controller class
/// </summary>
public static class ImageController
{
    public static DefaultDataContext db { get { return DataContext.Current; } }

    public const string HANDLER_URL = "~/GetImage.ashx?id=";

    /// <summary>
    /// Returns all of the images related to a particular object, specified by type and id
    /// </summary>
    /// <param name="type"></param>
    /// <param name="relatedID"></param>
    /// <returns></returns>
    public static IQueryable<Image> GetAllForRelatedObject(ImageTypes type, int relatedID)
    {
        return GetAllForRelatedObject((int)type, relatedID);
    }

    /// <summary>
    /// Returns all of the images related to a particular object, specified by type and id
    /// </summary>
    /// <param name="typeID"></param>
    /// <param name="relatedID"></param>
    /// <returns></returns>
    public static IQueryable<Image> GetAllForRelatedObject(int typeID, int relatedID)
    {
        return from img in db.Images
               where img.ImageTypeID == typeID && img.RelatedID == relatedID
               select img;
    }

    /// <summary>
    /// Returns a ASP.NET relative URL for an object image
    /// <example>"~/GetImage.ashx?id=[id]"</example>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetImageUrl(ImageTypes type, int id)
    {
        int imgID = GetAllForRelatedObject(type, id).Select(i => i.ImageID).FirstOrDefault();
        return HANDLER_URL + imgID;
    }

    /// <summary>
    /// Fetch image
    /// </summary>
    /// <param name="imageID"></param>
    /// <returns></returns>
    public static Image GetByID(int imageID)
    {
        return db.Images.Where(i => i.ImageID == imageID).SingleOrDefault();
    }


}
