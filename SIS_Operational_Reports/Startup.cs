
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SIS_Operational_Reports.Startup))]
namespace SIS_Operational_Reports
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
