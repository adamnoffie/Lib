using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Xml.Linq;
using Data;

/// <summary>
/// Utility Class to give access to a DataContext scoped within the current HttpContext 
/// </summary>
public static class DataContext
{
    public static DefaultDataContext Current
    {
        get
        {
            string key = "DefaultDataContext";
            if (HttpContext.Current.Items[key] == null)
                HttpContext.Current.Items[key] = new DefaultDataContext(
                    ConfigurationManager.ConnectionStrings[0].ConnectionString);
            return (DefaultDataContext)HttpContext.Current.Items[key];
        }
    }
}