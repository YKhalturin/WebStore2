using Microsoft.AspNetCore.Identity;
using WebStore.Domain.Entities.Identity;

namespace Webstore.Interfaces.Services.Identity
{
    public interface IRolesClient : IRoleStore<Role> { }
}