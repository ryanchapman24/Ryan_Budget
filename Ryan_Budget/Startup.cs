using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Ryan_Budget.Startup))]
namespace Ryan_Budget
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
