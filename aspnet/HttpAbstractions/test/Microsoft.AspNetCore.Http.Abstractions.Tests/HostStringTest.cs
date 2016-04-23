﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Http
{
    public class HostStringTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void CtorThrows_IfPortIsNotGreaterThanZero(int port)
        {
            // Act and Assert
            ExceptionAssert.ThrowsArgumentOutOfRange(() => new HostString("localhost", port), "port", "The value must be greater than zero.");
        }

        [Theory]
        [InlineData("localhost", "localhost")]
        [InlineData("1.2.3.4", "1.2.3.4")]
        [InlineData("[2001:db8:a0b:12f0::1]", "[2001:db8:a0b:12f0::1]")]
        [InlineData("本地主機", "本地主機")]
        [InlineData("localhost:5000", "localhost")]
        [InlineData("1.2.3.4:5000", "1.2.3.4")]
        [InlineData("[2001:db8:a0b:12f0::1]:5000", "[2001:db8:a0b:12f0::1]")]
        [InlineData("本地主機:5000", "本地主機")]
        public void Domain_ExtractsHostFromValue(string sourceValue, string expectedDomain)
        {
            // Arrange
            var hostString = new HostString(sourceValue);

            // Act
            var result = hostString.Host;

            // Assert
            Assert.Equal(expectedDomain, result);
        }

        [Theory]
        [InlineData("localhost", null)]
        [InlineData("1.2.3.4", null)]
        [InlineData("[2001:db8:a0b:12f0::1]", null)]
        [InlineData("本地主機", null)]
        [InlineData("localhost:5000", 5000)]
        [InlineData("1.2.3.4:5000", 5000)]
        [InlineData("[2001:db8:a0b:12f0::1]:5000", 5000)]
        [InlineData("本地主機:5000", 5000)]
        public void Port_ExtractsPortFromValue(string sourceValue, int? expectedPort)
        {
            // Arrange
            var hostString = new HostString(sourceValue);

            // Act
            var result = hostString.Port;

            // Assert
            Assert.Equal(expectedPort, result);
        }

        [Theory]
        [InlineData("localhost:BLAH")]
        public void Port_ExtractsInvalidPortFromValue(string sourceValue)
        {
            // Arrange
            var hostString = new HostString(sourceValue);

            // Act
            var result = hostString.Port;

            // Assert
            Assert.Equal(null, result);
        }

        [Theory]
        [InlineData("localhost", 5000, "localhost", 5000)]
        [InlineData("1.2.3.4", 5000, "1.2.3.4", 5000)]
        [InlineData("[2001:db8:a0b:12f0::1]", 5000, "[2001:db8:a0b:12f0::1]", 5000)]
        [InlineData("2001:db8:a0b:12f0::1", 5000, "[2001:db8:a0b:12f0::1]", 5000)]
        [InlineData("本地主機", 5000, "本地主機", 5000)]
        public void Ctor_CreatesFromHostAndPort(string sourceHost, int sourcePort, string expectedHost, int expectedPort)
        {
            // Arrange
            var hostString = new HostString(sourceHost, sourcePort);

            // Act
            var host = hostString.Host;
            var port = hostString.Port;

            // Assert
            Assert.Equal(expectedHost, host);
            Assert.Equal(expectedPort, port);
        }
    }
}
