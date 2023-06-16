using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AllSub.OAuth.Vk
{
    public class VkOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new <see cref="VkOptions"/>.
        /// </summary>
        public VkOptions()
        {

            CallbackPath = new PathString("/signin-vkontakte");
            AuthorizationEndpoint = VkDefaults.AuthorizationEndpoint;
            TokenEndpoint = VkDefaults.TokenEndpoint;
            UserInformationEndpoint = VkDefaults.UserInformationEndpoint;

            Scope.Add("profile");
            Scope.Add("email");

            ApiVersion = "5.131";

            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "user_id");
            ClaimActions.MapJsonKey(ClaimTypes.Name, "email");
            ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
            // TODO: actually we can't read the next claims for bad user data format returned from VK getUser
            //ClaimActions.MapJsonKey(ClaimTypes.GivenName, "first_name");
            //ClaimActions.MapJsonKey(ClaimTypes.Surname, "last_name");
        }

        public string ApiVersion { get; set; }
    }
}
