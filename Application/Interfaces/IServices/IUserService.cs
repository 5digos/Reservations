using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.Request;

namespace Application.Interfaces.IServices
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(int id);
    }
}
