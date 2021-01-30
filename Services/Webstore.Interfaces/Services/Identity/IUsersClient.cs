using Microsoft.AspNetCore.Identity;
using WebStore.Domain.Entities.Identity;

namespace Webstore.Interfaces.Services.Identity
{
    public interface IUsersClient :
    IUserRoleStore<User>,
            IUserPasswordStore<User>,
            IUserEmailStore<User>,
            IUserPhoneNumberStore<User>,
            IUserTwoFactorStore<User>,
            IUserLoginStore<User>,
            IUserClaimStore<User>
    {
    }
}