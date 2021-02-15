using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace SchoolDB.Controllers
{
    public class Connection
    {
        public string ConnectionString
        {
           // get => WebConfigurationManager.ConnectionStrings["School_db"].ConnectionString;
            get => WebConfigurationManager.ConnectionStrings["SchoolContext"].ConnectionString;
        }
    }
}