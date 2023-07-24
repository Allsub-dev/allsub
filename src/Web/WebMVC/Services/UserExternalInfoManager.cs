using AllSub.CommonCore.Constants;
using AllSub.WebMVC.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AllSub.WebMVC.Services
{
    public class UserExternalInfoManager : IUserExternalInfoManager
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _userDbContext;

        public UserExternalInfoManager(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext userDbContext)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userDbContext = userDbContext;
        }

        public async Task AddLoginInfo(ApplicationUser user, ExternalLoginInfo info)
        {
            // User is just added, add/update values
            var props = new Dictionary<string, string>();
            string? accessToken = info.AuthenticationTokens?.FirstOrDefault(t => t.Name == "access_token")?.Value;
            string? userid = info.Principal.HasClaim(c => c.Type == ClaimTypes.NameIdentifier) ? info.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value  : null;

            AddProperties(info.LoginProvider, accessToken, userid, props);
            await SaveProperties(user, props);
        }

        private void AddProperties(string loginProvider, string? accessToken, string? userId, Dictionary<string, string> props)
        {
            switch (loginProvider)
            {
                case PropertyKeys.Vk.Key:
                    if (accessToken != null)
                    {
                        props.Add(PropertyKeys.Vk.AccessTokenKey, accessToken);
                    }
                    if (userId != null)
                    {
                        props.Add(PropertyKeys.Vk.UseerIdKey, userId);
                    }
                    break;
                case PropertyKeys.Google.Key:
                    if (accessToken != null)
                    {
                        props.Add(PropertyKeys.Google.AccessTokenKey, accessToken);
                    }
                    if (userId != null)
                    {
                        props.Add(PropertyKeys.Google.UseerIdKey, userId);
                    }
                    break;
            }
        }

        public async Task CheckLoginInfo(ClaimsPrincipal principal, ExternalLoginInfo info)
        {
            // User is signed in at this point - update values
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user != null)
            {
                await AddLoginInfo(user, info);
            }
        }

        public async Task SaveTicket(TicketReceivedContext ctx)
        {
            var user = await GetUser(ctx);
            if (user != null)
            {
                var props = new Dictionary<string, string>();
                string? accessToken = ctx.Properties?.GetTokenValue("access_token");
                string? userid = ctx.Principal?.HasClaim(c => c.Type == ClaimTypes.NameIdentifier) == true ? ctx.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;

                AddProperties(ctx.Scheme.Name, accessToken, userid, props);
                await SaveProperties(user, props);
            }
        }

        private async Task<ApplicationUser?> GetUser(TicketReceivedContext ctx)
        {
            string? email = ctx.Principal?.HasClaim(c => c.Type == ClaimTypes.Email) == true ? ctx.Principal.FindFirst(ClaimTypes.Email)?.Value : null;
            if (!string.IsNullOrWhiteSpace(email))
            {
                return await _userManager.FindByEmailAsync(email);
            }
            
            return null;
        }

        private async Task SaveProperties(ApplicationUser user, Dictionary<string, string> properties)
        {
            var saveUser = false;
            await _userDbContext.Entry(user).Collection(x => x.UserProperties).LoadAsync();
            foreach (var prop in properties)
            {
                var userProperty = user.UserProperties.FirstOrDefault(c => c.Key == prop.Key);
                if (userProperty == null)
                {
                    user.UserProperties.Add(new UserProperty { Key = prop.Key, Value = prop.Value, UserId = user.Id });
                    saveUser = true;
                }
                else if (userProperty.Value != prop.Value)
                {
                    userProperty.Value = prop.Value;
                    saveUser = true;
                }
            }

            if (saveUser)
            {
                await _userManager.UpdateAsync(user);
            }
        }
    }
}
