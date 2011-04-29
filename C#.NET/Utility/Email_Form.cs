using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

/// <summary>
/// Summary description for Email_Form
/// </summary>
public static class Email_Form
{
    /// <summary>
    /// Gets the address to email forms to
    /// </summary>
    public static string EmailFormTo
    {
        get { return ConfigurationManager.AppSettings["emailFormsTo"] as string; }
    }

    /// <summary>
    /// 0 - Title
    /// 1 - Submission Date
    /// 2 - Form Url
    /// </summary>
	public const string EmailHead = @"<html>
<body>
    <table cellpadding='3' style='width: 95%; font-size: 12pt' rules='all'>
    <colgroup><col width='25%' /><col /></colgroup>
    <tr><td colspan='2' style='padding-bottom: 20px; padding-top: 10px; font-size: 16pt'>
        {0} <br />
        <span style='font-size: 10pt'>
            Submitted: {1} <br /> Form: {2}
        </span>
    </td></tr>";

    /// <summary>
    /// 0 - Name
    /// 1 - Value
    /// </summary>
    public const string EmailRow = @"
    <tr>
        <td>{0}</td>
        <td>{1}</td>
    </tr>";

    /// <summary>
    /// 0 - Footer
    /// </summary>
    public const string EmailFoot = @"
    <tr><td colspan='2' style='padding-top: 20px; font-size: 10pt'>
        {0}
    </td></tr>
    </table>
</body>
</html>";

    /// <summary>
    /// 0 - Image src Attribute value
    /// </summary>
    public const string ImageElement = "<img alt='' style='max-width: 400px; max-height: 800px' src='{0}' />";

    public const string LinkElement = "<a href='{0}'>{1}</a>";
}
