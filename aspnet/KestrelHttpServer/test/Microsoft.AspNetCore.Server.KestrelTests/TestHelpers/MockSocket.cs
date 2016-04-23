﻿using System;
using Microsoft.AspNetCore.Server.Kestrel.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Networking;

namespace Microsoft.AspNetCore.Server.KestrelTests.TestHelpers
{
    class MockSocket : UvStreamHandle
    {
        public MockSocket(Libuv uv, int threadId, IKestrelTrace logger) : base(logger)
        {
            CreateMemory(uv, threadId, IntPtr.Size);
        }

        protected override bool ReleaseHandle()
        {
            DestroyMemory(handle);
            handle = IntPtr.Zero;
            return true;
        }
    }
}
