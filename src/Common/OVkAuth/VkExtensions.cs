using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllSub.OVkAuth
{
    /// <summary>
    /// Extension methods to configure Vk OAuth authentication.
    /// </summary>
    public static class VkExtensions
    {
        /// <summary>
        /// Adds Vk OAuth-based authentication to <see cref="AuthenticationBuilder"/> using the default scheme.
        /// The default scheme is specified by <see cref="VkDefaults.AuthenticationScheme"/>.
        /// <para>
        /// Vk authentication allows application users to sign in with their Vk account.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
        public static AuthenticationBuilder AddVk(this AuthenticationBuilder builder)
            => builder.AddVk(VkDefaults.AuthenticationScheme, _ => { });

        /// <summary>
        /// Adds Vk OAuth-based authentication to <see cref="AuthenticationBuilder"/> using the default scheme.
        /// The default scheme is specified by <see cref="VkDefaults.AuthenticationScheme"/>.
        /// <para>
        /// Vk authentication allows application users to sign in with their Vk account.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <param name="configureOptions">A delegate to configure <see cref="VkOptions"/>.</param>
        /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
        public static AuthenticationBuilder AddVk(this AuthenticationBuilder builder, Action<VkOptions> configureOptions)
            => builder.AddVk(VkDefaults.AuthenticationScheme, configureOptions);

        /// <summary>
        /// Adds Vk OAuth-based authentication to <see cref="AuthenticationBuilder"/> using the default scheme.
        /// The default scheme is specified by <see cref="VkDefaults.AuthenticationScheme"/>.
        /// <para>
        /// Vk authentication allows application users to sign in with their Vk account.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <param name="authenticationScheme">The authentication scheme.</param>
        /// <param name="configureOptions">A delegate to configure <see cref="VkOptions"/>.</param>
        /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
        public static AuthenticationBuilder AddVk(this AuthenticationBuilder builder, string authenticationScheme, Action<VkOptions> configureOptions)
            => builder.AddVk(authenticationScheme, VkDefaults.DisplayName, configureOptions);

        /// <summary>
        /// Adds Vk OAuth-based authentication to <see cref="AuthenticationBuilder"/> using the default scheme.
        /// The default scheme is specified by <see cref="VkDefaults.AuthenticationScheme"/>.
        /// <para>
        /// Vk authentication allows application users to sign in with their Vk account.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
        /// <param name="authenticationScheme">The authentication scheme.</param>
        /// <param name="displayName">A display name for the authentication handler.</param>
        /// <param name="configureOptions">A delegate to configure <see cref="VkOptions"/>.</param>
        /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
        public static AuthenticationBuilder AddVk(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<VkOptions> configureOptions)
            => builder.AddOAuth<VkOptions, VKAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
    }
}
