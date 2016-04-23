// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
#if !NETCORE50
using Microsoft.AspNetCore.Hosting;
#endif

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class StartupInvoker
    {
        private readonly Type _startupType;
        private readonly string _environment;
        private readonly string _startupProjectDir;

        public StartupInvoker(
            [NotNull] Assembly startupAssembly,
            [CanBeNull] string environment,
            [NotNull] string startupProjectDir)
        {
            Check.NotNull(startupAssembly, nameof(startupAssembly));
            Check.NotEmpty(startupProjectDir, nameof(startupProjectDir));

            _environment = !string.IsNullOrEmpty(environment)
                ? environment
                : "Development";

            _startupProjectDir = startupProjectDir;

            _startupType = startupAssembly.DefinedTypes.Where(t => t.Name == "Startup" + _environment)
                .Concat(startupAssembly.DefinedTypes.Where(t => t.Name == "Startup"))
                .Concat(startupAssembly.DefinedTypes.Where(t => t.Name == "Program"))
                .Concat(startupAssembly.DefinedTypes.Where(t => t.Name == "App"))
                .Select(t => t.AsType())
                .FirstOrDefault();
        }

        public virtual IServiceProvider ConfigureServices()
        {
            var services = ConfigureHostServices(new ServiceCollection());

            return Invoke(
                _startupType,
                new[] { "ConfigureServices", "Configure" + _environment + "Services" },
                services) as IServiceProvider
                   ?? services.BuildServiceProvider();
        }

        public virtual IServiceCollection ConfigureDesignTimeServices([NotNull] IServiceCollection services)
            => ConfigureDesignTimeServices(_startupType, services);

        public virtual IServiceCollection ConfigureDesignTimeServices([CanBeNull] Type type, [NotNull] IServiceCollection services)
        {
            Invoke(type, new[] { "ConfigureDesignTimeServices" }, services);
            return services;
        }

        private object Invoke(Type type, string[] methodNames, IServiceCollection services)
        {
            if (type == null)
            {
                return null;
            }

            MethodInfo method = null;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < methodNames.Length; i++)
            {
                method = type.GetTypeInfo().GetDeclaredMethod(methodNames[i]);
                if (method != null)
                {
                    break;
                }
            }

            if (method == null)
            {
                return null;
            }

            var instance = !method.IsStatic
                ? ActivatorUtilities.GetServiceOrCreateInstance(GetHostServices(), type)
                : null;

            var parameters = method.GetParameters();
            var arguments = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;
                arguments[i] = parameterType == typeof(IServiceCollection)
                    ? services
                    : ActivatorUtilities.GetServiceOrCreateInstance(GetHostServices(), parameterType);
            }

            return method.Invoke(instance, arguments);
        }

        private void ConfigureAspNetHostServices(IServiceCollection services)
        {
            // This implementation is very intentional.
            //  1. This is not compiled on NETCORE50
            //  2. This is in a separate method.
            //
            // This prevents powershell commands from loading Microsoft.AspNetCore.Hosting.Abstractions
            // when executing on a UWP project.

            // TODO create a better abstraction for startup services on different project models
#if !NETCORE50
            services.AddSingleton<IHostingEnvironment>(
                new HostingEnvironment
                {
                    ContentRootPath = _startupProjectDir,
                    EnvironmentName = _environment
                });

            if (PlatformServices.Default != null)
            {
                if (PlatformServices.Default.Application != null)
                {
                    services.AddSingleton<IApplicationEnvironment>(
                        new ApplicationEnvironment(PlatformServices.Default.Application)
                        {
                            ApplicationBasePath = _startupProjectDir
                        });
                }

                if (PlatformServices.Default.Runtime != null)
                {
                    services.AddSingleton(PlatformServices.Default.Runtime);
                }
            }
#endif
        }

        protected virtual IServiceCollection ConfigureHostServices([NotNull] IServiceCollection services)
        {
            if (!DesignTimeEnvironment.IsUwp())
            {
                ConfigureAspNetHostServices(services);
            }

            services.AddLogging();
            services.AddOptions();

            return services;
        }

        private IServiceProvider GetHostServices()
            => ConfigureHostServices(new ServiceCollection()).BuildServiceProvider();
    }
}
