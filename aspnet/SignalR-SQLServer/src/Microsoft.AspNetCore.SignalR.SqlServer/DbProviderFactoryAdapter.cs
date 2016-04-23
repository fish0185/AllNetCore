// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;

namespace Microsoft.AspNetCore.SignalR.SqlServer
{
    internal class DbProviderFactoryAdapter : IDbProviderFactory
    {
        private readonly DbProviderFactory _dbProviderFactory;

        public DbProviderFactoryAdapter(DbProviderFactory dbProviderFactory)
        {
            _dbProviderFactory = dbProviderFactory;
        }

#if NET451
        public IDbConnection CreateConnection()
#else
        public DbConnection CreateConnection()
#endif
        {
            return _dbProviderFactory.CreateConnection();
        }

        public DbParameter CreateParameter()
        {
            return _dbProviderFactory.CreateParameter();
        }
    }
}
