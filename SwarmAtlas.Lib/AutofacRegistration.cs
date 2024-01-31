using Autofac;
using SC2API.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwarmAtlas.Lib
{
    internal class AutofacRegistration
    {
        public static IContainer BuildContainer(string[] args) {
            var builder = new ContainerBuilder();
            //builder.RegisterType<CommandLineArgs>().WithParameter("args", args);
            builder.RegisterType<ExeLauncher>().AsSelf();
            builder.RegisterType<GameConfig>().AsSelf();
            builder.RegisterType<GameConnection>().AsSelf();
            builder.RegisterType<GameLauncher>().AsSelf();
            
            builder.RegisterType<SwarmAtlas>().AsSelf();
            builder.RegisterType<SwarmAtlasRunner>().AsSelf();
            builder.RegisterType<Units>().AsSelf();
            return builder.Build();
        }
    }
}
