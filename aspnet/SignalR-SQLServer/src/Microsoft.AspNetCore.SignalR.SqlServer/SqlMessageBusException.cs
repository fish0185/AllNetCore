// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.SignalR.SqlServer
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="Should never have inner exceptions")]
#if NET451
    [Serializable]
#endif
    public class SqlMessageBusException : Exception
    {
        public SqlMessageBusException(string message)
            : base(message)
        {

        }
    }
}
