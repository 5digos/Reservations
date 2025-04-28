using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.ICommand
{
    public interface IUserCommand
    {
        Task<bool> CreateUser(string email, string password, string firstName, string lastName);
        Task<bool> UpdateUser(int userId, string email, string password, string firstName, string lastName);
        Task<bool> DeleteUser(int userId);
        Task<bool> GetUserById(int userId);
        Task<bool> GetAllUsers();
    }
    
    
}
