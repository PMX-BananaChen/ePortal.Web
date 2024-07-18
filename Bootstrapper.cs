using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using System.Reflection;
using ePortal.CommandProcessor.Command;
using ePortal.CommandProcessor.Dispatcher;
using ePortal.Data.Infrastructure;
using ePortal.Data.Repositories;
using ePortal.Web.Core.Authentication;
using System.Web.Http;
using ePortal.Web.Mappers;
using ePortal.Domain.Handlers;
using ePortal.Models;

namespace ePortal.Web
{
    public static class Bootstrapper
    {
        public static void Run()
        {
            SetAutofacContainer();
            AutoMapperConfiguration.Configure();
        }
        private static void SetAutofacContainer()
        {
            var builder = new ContainerBuilder(); //


            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterType<DefaultCommandBus>().As<ICommandBus>().InstancePerHttpRequest();
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerHttpRequest();
            builder.RegisterType<DatabaseFactory>().As<IDatabaseFactory>().InstancePerHttpRequest();
            //builder.RegisterAssemblyTypes(typeof(CategoryRepository).Assembly)
            //.Where(t => t.Name.EndsWith("Repository"))
            //.AsImplementedInterfaces().InstancePerHttpRequest();

            builder.RegisterType<EntityRepository<ePortal.Models.SystemUser>>().As<IEntityRepository<ePortal.Models.SystemUser>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.MenuItem>>().As<IEntityRepository<ePortal.Models.MenuItem>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.AuditLink>>().As<IEntityRepository<ePortal.Models.AuditLink>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.SecurityGroup>>().As<IEntityRepository<ePortal.Models.SecurityGroup>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.MenuSecurity>>().As<IEntityRepository<ePortal.Models.MenuSecurity>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.SystemUserSecurity>>().As<IEntityRepository<ePortal.Models.SystemUserSecurity>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.ExecutableProgram>>().As<IEntityRepository<ePortal.Models.ExecutableProgram>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.Scheduling>>().As<IEntityRepository<ePortal.Models.Scheduling>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.Employee_Full>>().As<IEntityRepository<ePortal.Models.Employee_Full>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.Absence>>().As<IEntityRepository<ePortal.Models.Absence>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.Overtime>>().As<IEntityRepository<ePortal.Models.Overtime>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.AbnormalAnalysis>>().As<IEntityRepository<ePortal.Models.AbnormalAnalysis>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.AccessRecord>>().As<IEntityRepository<ePortal.Models.AccessRecord>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.Bide>>().As<IEntityRepository<ePortal.Models.Bide>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.Factory_Clocks>>().As<IEntityRepository<ePortal.Models.Factory_Clocks>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.Zone_Clocks>>().As<IEntityRepository<ePortal.Models.Zone_Clocks>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.AttendanceRecord>>().As<IEntityRepository<ePortal.Models.AttendanceRecord>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.MailingList>>().As<IEntityRepository<ePortal.Models.MailingList>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.StaffScheduling>>().As<IEntityRepository<ePortal.Models.StaffScheduling>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.FactoryScheduling>>().As<IEntityRepository<ePortal.Models.FactoryScheduling>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.AccessFormMode>>().As<IEntityRepository<ePortal.Models.AccessFormMode>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.PlantFormMode>>().As<IEntityRepository<ePortal.Models.PlantFormMode>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.OthersFormMode>>().As<IEntityRepository<ePortal.Models.OthersFormMode>>().InstancePerHttpRequest();
            builder.RegisterType<EntityRepository<ePortal.Models.HCP>>().As<IEntityRepository<ePortal.Models.HCP>>().InstancePerHttpRequest();

            var services = Assembly.Load("ePortal.Domain");
            builder.RegisterAssemblyTypes(services)
            .AsClosedTypesOf(typeof(ICommandHandler<>)).InstancePerHttpRequest();//CanAddEntityHandler

            builder.RegisterAssemblyTypes(services)
           .AsClosedTypesOf(typeof(IValidationHandler<>)).InstancePerHttpRequest();

            /*
            builder.RegisterAssemblyTypes(services)
           .AsClosedTypesOf(typeof(ICommandHandler<ePortal.Domain.Commands.CreateOrUpdateEntityCommand<ePortal.Models.FactoryScheduling>>)).InstancePerHttpRequest();
            */

            builder.RegisterAssemblyTypes(services).AssignableTo<ePortal.Models.MenuItem>();
            builder.RegisterAssemblyTypes(services).AssignableTo<ePortal.Models.SystemUser>();
            builder.RegisterAssemblyTypes(services).AssignableTo<ePortal.Models.ExecutableProgram>();
            builder.RegisterAssemblyTypes(services).AssignableTo<ePortal.Models.FactoryScheduling>();           
            
            builder.RegisterType<DefaultFormsAuthentication>().As<IFormsAuthentication>().InstancePerHttpRequest();
            builder.RegisterFilterProvider();
            IContainer container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
