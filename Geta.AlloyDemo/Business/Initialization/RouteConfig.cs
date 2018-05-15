using System;
using System.Linq;
using System.Web.Http;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace AlloyDemoKit.Business.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class RouteConfig : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            GlobalConfiguration.Configure(ConfigWebApiRoutes);
        }

        private void ConfigWebApiRoutes(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
        }

        public void Uninitialize(InitializationEngine context)
        {
            //Add uninitialization logic
        }
    }
}