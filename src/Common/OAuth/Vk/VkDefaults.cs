using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllSub.OAuth.Vk
{
    public static class VkDefaults
    {
        /// <summary>
        /// The default scheme for Vk authentication. 
        /// </summary>
        public const string AuthenticationScheme = "Vk";

        /// <summary>
        /// The default display name for VK authentication. 
        /// </summary>
        public static readonly string DisplayName = "VKontakte";

        /// <summary>
        /// The default endpoint used to perform Vk authentication.
        /// </summary>
        /// <remarks>
        /// For more details about this endpoint, see <see href="https://developers.google.com/identity/protocols/oauth2/web-server#httprest"/>.
        /// </remarks>
        public static readonly string AuthorizationEndpoint = "https://api.vkontakte.ru/oauth/authorize";

        /// <summary>
        /// The OAuth endpoint used to exchange access tokens.
        /// </summary>
        public static readonly string TokenEndpoint = "https://api.vkontakte.ru/oauth/access_token";

        /// <summary>
        /// The Vk endpoint that is used to gather additional user information.
        /// </summary>
        /// <remarks>
        /// For more details about this endpoint, see <see href="https://dev.vk.com/method/users.get"/>.
        /// </remarks>
        public static readonly string UserInformationEndpoint = "https://api.vk.com/method/users.get";
    }
}
