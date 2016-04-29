// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public static class ForwardingProxy
    {
        public static T Unwrap<T>([NotNull] object target)
            where T : class
        {
#if NET451
            return target as T ?? new ForwardingProxy<T>(target).GetTransparentProxy();
#else
            return (T)target;
#endif
        }
    }
}
