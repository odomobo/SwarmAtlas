using Autofac;
using SC2API.CSharp;
using SwarmAtlas.Lib.Executives;
using SwarmAtlas.Gui;
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
            builder.RegisterType<GameLauncher>().AsSelf().InstancePerLifetimeScope();

            // SwarmAtlas.Lib
            builder.RegisterType<ProductionExecutive>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<SceneBuilder>().AsSelf().InstancePerLifetimeScope();
            bool realRenderer = true; // TODO: take this as param I guess
            if (realRenderer)
            {
                builder.RegisterType<Renderer>().As<IRenderer>().InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<DummyRenderer>().As<IRenderer>().InstancePerLifetimeScope();
            }

            builder.RegisterType<GameInfo>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<SwarmAtlas>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<SwarmAtlasRunner>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<Units>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<UnitTypes>().AsSelf().InstancePerLifetimeScope();
            return builder.Build();
        }
    }
}
