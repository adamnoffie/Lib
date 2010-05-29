using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Xml.Linq;

namespace Data
{
    public partial class DefaultDataContext
    {
        [System.Data.Linq.Mapping.Function(Name = "NEWID", IsComposable = true)]
        public Guid Random()
        {
            // this code is not actually executed, it simply provides a way to access 
            // T-SQL "NEWID()" function from Linq to SQL
            throw new NotImplementedException();
        }
    } 
}