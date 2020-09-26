using System;
using Autofac;
using Autofac.Builder;

namespace KafkaFlow.Autofac.DependencyInjection
{
    public class AutofacDependencyConfigurator
        : IDependencyConfigurator
    {
        private readonly ContainerBuilder builder;

        public AutofacDependencyConfigurator(ContainerBuilder builder)
        {
            this.builder = builder;
        }

        public IDependencyConfigurator Add(Type serviceType, Type implementationType, InstanceLifetime lifetime)
        {
            var registrationBuilder = builder
                .RegisterType(implementationType)
                .As(serviceType);
            SetLifeTime(registrationBuilder, lifetime);
            return this;
        }

        public IDependencyConfigurator Add<TService, TImplementation>(InstanceLifetime lifetime)
            where TService : class where TImplementation : class, TService
        {
            var registrationBuilder = builder.RegisterType<TImplementation>().As<TService>();
            SetLifeTime(registrationBuilder, lifetime);
            return this;
        }

        public IDependencyConfigurator Add<TService>(InstanceLifetime lifetime) where TService : class
        {
            var registrationBuilder = builder.RegisterType<TService>();
            SetLifeTime(registrationBuilder, lifetime);
            return this;
        }

        public IDependencyConfigurator Add<TImplementation>(TImplementation service) where TImplementation : class
        {
            builder.RegisterInstance(service).SingleInstance();
            return this;
        }

        public IDependencyConfigurator Add<TImplementation>(Type serviceType,
            Func<IDependencyResolver, TImplementation> factory, InstanceLifetime lifetime)
        {
            var registrationBuilder = builder.Register((context, parameters) =>
                factory(new AutofacDependencyResolver(context.Resolve<ILifetimeScope>())));
            SetLifeTime(registrationBuilder, lifetime);

            return this;
        }


        private static void SetLifeTime<T>(
            IRegistrationBuilder<T, IConcreteActivatorData, SingleRegistrationStyle> registrationBuilder,
            InstanceLifetime lifetime)
        {
            switch (lifetime)
            {
                case InstanceLifetime.Singleton:
                    registrationBuilder.SingleInstance();
                    break;
                case InstanceLifetime.Scoped:
                    registrationBuilder.InstancePerLifetimeScope();
                    break;
                case InstanceLifetime.Transient:
                    registrationBuilder.InstancePerDependency();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }
        }
    }
}