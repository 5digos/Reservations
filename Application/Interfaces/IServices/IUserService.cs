
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.Request;

﻿using System.Threading.Tasks;


namespace Application.Interfaces.IServices
{
    public interface IUserService
    {

        Task<UserDto?> GetUserByIdAsync(int id);

        Task<bool> IsUserValidAsync(int userId);

    }
}
