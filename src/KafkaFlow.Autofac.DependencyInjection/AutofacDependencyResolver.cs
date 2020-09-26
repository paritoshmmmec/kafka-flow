using System;
using Autofac;

namespace KafkaFlow.Autofac.DependencyInjection
{
    public class AutofacDependencyResolver : IDependencyResolver
    {
        private readonly ILifetimeScope scope;

        public AutofacDependencyResolver(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        public IDependencyResolverScope CreateScope()
        {
            return new AutofacDependencyResolverScope(this.scope.BeginLifetimeScope());
        }

        public object Resolve(Type type) => this.scope.Resolve(type);
    }
}