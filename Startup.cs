using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SchoolDB.Startup))]
namespace SchoolDB
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
