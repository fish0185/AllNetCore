// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Microsoft.AspNetCore.Authentication.Tests.OpenIdConnect
{
    /// <summary>
    /// These utilities are designed to test openidconnect related flows
    /// </summary>
    public class TestUtilities
    {
        public const string DefaultHost = @"http://localhost";

        public static IConfigurationManager<OpenIdConnectConfiguration> DefaultOpenIdConnectConfigurationManager
        {
            get
            {
                return new StaticConfigurationManager<OpenIdConnectConfiguration>(DefaultOpenIdConnectConfiguration);
            }
        }

        public static OpenIdConnectConfiguration DefaultOpenIdConnectConfiguration
        {
            get
            {
                return new OpenIdConnectConfiguration()
                {
                    AuthorizationEndpoint = @"https://login.microsoftonline.com/common/oauth2/authorize",
                    EndSessionEndpoint = @"https://login.microsoftonline.com/common/oauth2/endsessionendpoint",
                    TokenEndpoint = @"https://login.microsoftonline.com/common/oauth2/token",
                };
            }
        }
    }
}
