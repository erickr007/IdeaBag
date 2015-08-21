using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(IdeaBag.Server.Core.Startup))]
namespace IdeaBag.Server.Core
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {

        }
    }
}
