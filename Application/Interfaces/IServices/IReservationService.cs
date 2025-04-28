using Application.Dtos.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.IServices;
using Domain.Entities;

namespace Application.Interfaces.IServices
{
    interface IReservationService
    {
        Task<Reservation> CreateReservationAsync(ReservationDto dto);
    }
}
