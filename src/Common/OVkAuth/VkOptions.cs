using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AllSub.OVkAuth
{
    public class VkOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new <see cref="GoogleOptions"/>.
        /// </summary>
        public VkOptions()
        {
            ClaimsIssuer = "VK";

            CallbackPath = new PathString("/signin-vkontakte");
            AuthorizationEndpoint = VkDefaults.AuthorizationEndpoint;
            TokenEndpoint = VkDefaults.TokenEndpoint;
            UserInformationEndpoint = VkDefaults.UserInformationEndpoint;
            
            Scope.Add("openid");
            Scope.Add("profile");
            Scope.Add("email");

            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            ClaimActions.MapJsonKey(ClaimTypes.GivenName, "first_name");
            ClaimActions.MapJsonKey(ClaimTypes.Surname, "last_name");
            ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
            ApiVersion = "5.131";
        }
        
        public string ApiVersion { get; set; }
    }
}
