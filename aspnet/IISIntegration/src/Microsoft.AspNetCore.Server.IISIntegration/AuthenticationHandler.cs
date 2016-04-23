// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace Microsoft.AspNetCore.Server.IISIntegration
{
    internal class AuthenticationHandler : IAuthenticationHandler
    {
        internal AuthenticationHandler(HttpContext httpContext, IISOptions options, ClaimsPrincipal user)
        {
            HttpContext = httpContext;
            User = user;
            Options = options;
        }

        internal HttpContext HttpContext { get; }

        internal IISOptions Options { get; }

        internal ClaimsPrincipal User { get; }

        internal IAuthenticationHandler PriorHandler { get; set; }

        public Task AuthenticateAsync(AuthenticateContext context)
        {
            if (ShouldHandleScheme(context.AuthenticationScheme))
            {
                if (User != null)
                {
                    context.Authenticated(User, properties: null,
                        description: Options.AuthenticationDescriptions.FirstOrDefault(descrip =>
                            string.Equals(User.Identity.AuthenticationType, descrip.AuthenticationScheme, StringComparison.Ordinal))?.Items);
                }
                else
                {
                    context.NotAuthenticated();
                }
            }

            if (PriorHandler != null)
            {
                return PriorHandler.AuthenticateAsync(context);
            }
            
            return Task.FromResult(0);
        }

        public Task ChallengeAsync(ChallengeContext context)
        {
            bool handled = false;
            if (ShouldHandleScheme(context.AuthenticationScheme))
            {
                switch (context.Behavior)
                {
                    case ChallengeBehavior.Automatic:
                        // If there is a principal already, invoke the forbidden code path
                        if (User == null)
                        {
                            goto case ChallengeBehavior.Unauthorized;
                        }
                        else
                        {
                            goto case ChallengeBehavior.Forbidden;
                        }
                    case ChallengeBehavior.Unauthorized:
                        HttpContext.Response.StatusCode = 401;
                        // We would normally set the www-authenticate header here, but IIS does that for us.
                        break;
                    case ChallengeBehavior.Forbidden:
                        HttpContext.Response.StatusCode = 403;
                        handled = true; // No other handlers need to consider this challenge.
                        break;
                }
                context.Accept();
            }

            if (!handled && PriorHandler != null)
            {
                return PriorHandler.ChallengeAsync(context);
            }
            
            return Task.FromResult(0);
        }

        public void GetDescriptions(DescribeSchemesContext context)
        {
            foreach (var description in Options.AuthenticationDescriptions)
            {
                context.Accept(description.Items);
            }

            if (PriorHandler != null)
            {
                PriorHandler.GetDescriptions(context);
            }
        }

        public Task SignInAsync(SignInContext context)
        {
            // Not supported, fall through
            if (PriorHandler != null)
            {
                return PriorHandler.SignInAsync(context);
            }
            
            return Task.FromResult(0);
        }

        public Task SignOutAsync(SignOutContext context)
        {
            // Not supported, fall through
            if (PriorHandler != null)
            {
                return PriorHandler.SignOutAsync(context);
            }
            
            return Task.FromResult(0);
        }

        private bool ShouldHandleScheme(string authenticationScheme)
        {
            if (Options.AutomaticAuthentication && string.Equals(AuthenticationManager.AutomaticScheme, authenticationScheme, StringComparison.Ordinal))
            {
                return true;
            }
            
            return Options.AuthenticationDescriptions.Any(description => string.Equals(description.AuthenticationScheme, authenticationScheme, StringComparison.Ordinal));
        }
    }
}
