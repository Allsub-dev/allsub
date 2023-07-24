using AllSub.WebMVC.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AllSub.WebMVC.Services
{
    public interface IUserExternalInfoManager
    {
        /// <summary>
        /// User is just added, adding values
        /// </summary>
        /// <param name="user"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        Task AddLoginInfo(ApplicationUser user, ExternalLoginInfo info);

        /// <summary>
        /// User is signed in at this point, check if we have to update values
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        Task CheckLoginInfo(ClaimsPrincipal principal, ExternalLoginInfo info);

        /// <summary>
        /// If user is signed in we add the values, otherwithe it should be handled in another method
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        Task SaveTicket(TicketReceivedContext ctx);
    }
}
