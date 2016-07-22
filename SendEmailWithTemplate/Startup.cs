using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SendEmailWithTemplate.Startup))]
namespace SendEmailWithTemplate
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
