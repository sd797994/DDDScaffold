using ApplicationService.ApplicationServiceImpl;
using Autofac;
using Infrastructure;
using Infrastructure.DataBase;
using Infrastructure.EfDataAccess;

namespace WebApi
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Common.GetProjectAssembliesArray()).Where(x => !Common.IsSystemType(x))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterType<UnitofWorkManager<MySqlEfContext>>().As<IUnitofWork>().InstancePerLifetimeScope();
        }
    }
}
