using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.IO;
using Data;
using System.Collections.Generic;

public partial class Admin_PhotosAdd : System.Web.UI.Page
{
    public const string savePathUrl = "~/Admin/uploads/";
    string savePath;
    string[] files;


    protected void Page_Load(object sender, EventArgs e)
    {
        savePath = Server.MapPath(savePathUrl);

        if (!Page.IsPostBack)
        {
            files = Directory.GetFiles(savePath);
            if (files.Length > 0)
            {
                // Processing images already uploaded to "uploads" folder. 
                mview.SetActiveView(vwAdd);
                rptFiles.DataSource = files;
                rptFiles.DataBind();

                // see if there are existing albums we can add these photos too, and update UI accordingly
                var albums = PhotoRepository.GetAllAlbums(false).OrderBy(pa => pa.Name);
                if (albums.Count() > 0)
                {
                    spanAlbumSelect.Visible = true;
                    ddAlbums.DataSource = albums;
                    ddAlbums.DataTextField = "Name";
                    ddAlbums.DataValueField = "PhotoAlbumID";
                    ddAlbums.DataBind();
                }
            }
        }
    }

    /// <summary>
    /// BIND - a file in the upload directory to rptFiles
    /// </summary>
    protected void rptFiles_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        string file = (string)e.Item.DataItem;
        System.Web.UI.WebControls.Image img = (System.Web.UI.WebControls.Image)e.Item.FindControl("img");
        HtmlAnchor lnkDel = (HtmlAnchor)e.Item.FindControl("lnkDel");
        TextBox txtName = (TextBox)e.Item.FindControl("txtName");
        //TextBox txtDesc = (TextBox)e.Item.FindControl("txtDesc");
        HiddenField hdnName = (HiddenField)e.Item.FindControl("hdnName");
        HiddenField hdnDel = (HiddenField)e.Item.FindControl("hdnDel");

        txtName.Text = Path.GetFileNameWithoutExtension(file);
        string filename = Path.GetFileName(file);
        img.ImageUrl = savePathUrl + filename;
        hdnName.Value = filename;
        lnkDel.HRef = "javascript:del('" + hdnDel.ClientID + "')";
    }

    /// <summary>
    /// EVENT - clicked submit
    /// </summary>
    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        DateTime now = DateTime.UtcNow;

        Dictionary<string, Photo> photos = new Dictionary<string, Photo>();
        foreach (RepeaterItem item in rptFiles.Items)
        {
            TextBox txtName = (TextBox)item.FindControl("txtName");
            TextBox txtDesc = (TextBox)item.FindControl("txtDesc");
            HiddenField hdnName = (HiddenField)item.FindControl("hdnName");
            HiddenField hdnDel = (HiddenField)item.FindControl("hdnDel");

            if (string.IsNullOrEmpty(hdnDel.Value)) // did we not remove it?
            {
                Photo p = new Photo();
                p.IsEnabled = true;     // init
                p.DateCreated = now;    // init
                p.Name = string.IsNullOrEmpty(txtName.Text) ?
                    Path.GetFileNameWithoutExtension(hdnName.Value) : txtName.Text;
                p.Description = txtDesc.Text;
                photos.Add(Path.Combine(savePath, hdnName.Value), p);
            }
        }

        if (radAlbumNew.Checked)
        {
            string albumName = string.IsNullOrEmpty(txtAlbumName.Text) ?
                now.ToLocalTime().ToShortDateString() : txtAlbumName.Text;
            PhotoRepository.CreatePhotos(photos, albumName); // adding to NEW album
        }
        else
            PhotoRepository.CreatePhotos(photos, int.Parse(ddAlbums.SelectedValue)); // adding to existing album
            
        ClearUploadsDirectory();
        Response.Redirect("~/Admin/PhotoAlbums.aspx");
    }

    /// <summary>
    /// EVENT - clicked cancel
    /// </summary>
    protected void btnCancel_Click(object sender, EventArgs e)
    {
        // remove all of the temp. uploaded files, and redirect
        ClearUploadsDirectory();
        Response.Redirect(Request.RawUrl);
    }

    /// <summary>
    /// Clears all of the files out of the /uploads directory
    /// </summary>
    protected void ClearUploadsDirectory()
    {
        files = Directory.GetFiles(savePath);
        foreach (string file in files)
            File.Delete(file);
    }


}
