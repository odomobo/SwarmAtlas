using Autofac;
using SC2API.CSharp;
using SwarmAtlas.Lib.Executives;
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

            // SC2API.CSharp
            builder.RegisterType<ExeLauncher>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<GameConfig>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<GameConnection>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<GameLauncher>().AsSelf().InstancePerLifetimeScope();

            // SwarmAtlas.Lib
            builder.RegisterType<ProductionExecutive>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<GameInfo>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<SwarmAtlas>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<SwarmAtlasRunner>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<Units>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<UnitTypes>().AsSelf().InstancePerLifetimeScope();
            return builder.Build();
        }
    }
}
