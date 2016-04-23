// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public class DbContextServices : IDbContextServices
    {
        private IServiceProvider _scopedProvider;
        private IDbContextOptions _contextOptions;
        private ICurrentDbContext _currentContext;
        private LazyRef<IModel> _modelFromSource;
        private LazyRef<IDatabaseProviderServices> _providerServices;
        private bool _inOnModelCreating;

        public virtual IDbContextServices Initialize(
            IServiceProvider scopedProvider,
            IDbContextOptions contextOptions,
            DbContext context)
        {
            _scopedProvider = scopedProvider;
            _contextOptions = contextOptions;
            _currentContext = new CurrentDbContext(context);

            _providerServices = new LazyRef<IDatabaseProviderServices>(() =>
                _scopedProvider.GetRequiredService<IDatabaseProviderSelector>().SelectServices());

            _modelFromSource = new LazyRef<IModel>(CreateModel);

            return this;
        }

        private IModel CreateModel()
        {
            if (_inOnModelCreating)
            {
                throw new InvalidOperationException(CoreStrings.RecursiveOnModelCreating);
            }

            try
            {
                _inOnModelCreating = true;

                return _providerServices.Value.ModelSource.GetModel(
                    _currentContext.Context,
                    _providerServices.Value.ConventionSetBuilder,
                    _providerServices.Value.ModelValidator);
            }
            finally
            {
                _inOnModelCreating = false;
            }
        }

        public virtual ICurrentDbContext CurrentContext => _currentContext;

        public virtual IModel Model => CoreOptions?.Model ?? _modelFromSource.Value;

        public virtual ILoggerFactory LoggerFactory
            => CoreOptions?.LoggerFactory ?? _scopedProvider?.GetRequiredService<ILoggerFactory>();

        public virtual IMemoryCache MemoryCache
            => CoreOptions?.MemoryCache ?? _scopedProvider?.GetRequiredService<IMemoryCache>();

        private CoreOptionsExtension CoreOptions
            => _contextOptions?.FindExtension<CoreOptionsExtension>();

        public virtual IDbContextOptions ContextOptions => _contextOptions;

        public virtual IDatabaseProviderServices DatabaseProviderServices
        {
            get
            {
                Debug.Assert(
                    _providerServices != null,
                    "DbContextServices not initialized. This may mean a service is registered as Singleton when it needs to be Scoped because it depends on other Scoped services.");

                return _providerServices.Value;
            }
        }

        public virtual IServiceProvider InternalServiceProvider => _scopedProvider;
    }
}
