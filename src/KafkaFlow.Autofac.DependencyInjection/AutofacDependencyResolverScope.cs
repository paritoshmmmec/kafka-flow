﻿using System;
using System.Collections.Generic;
using System.Text;
using Autofac;

namespace KafkaFlow.Autofac.DependencyInjection
{
    internal class AutofacDependencyResolverScope : IDependencyResolverScope
    {
        private readonly ILifetimeScope scope;

        public AutofacDependencyResolverScope(ILifetimeScope scope)
        {
            this.scope = scope;
            this.Resolver = new AutofacDependencyResolver(scope);
        }

        public void Dispose() => this.scope.Dispose();

        public IDependencyResolver Resolver { get; }
    }
}
