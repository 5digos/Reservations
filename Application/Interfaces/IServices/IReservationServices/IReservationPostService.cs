using Application.Dtos.External;
using Application.Dtos.Request;
using Application.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IServices.IReservationServices
{
    public interface IReservationPostService
    {
        Task<ReservationResponse> Create(int userId, ReservationRequest request);
        Task<ReservationResponse> Confirm(int userId, Guid reservationId);
        Task<ReservationResponse> Cancel(int userId, Guid reservationId);
        //Task<ReservationResponse> Pickup(int userId, Guid reservationId, ActualPickupRequest request);
        Task<ReservationResponse> Pickup(int userId, Guid reservationId);
        //Task<ReservationResponse> Return(int userId, Guid reservationId, ActualReturnRequest request);
        Task<ReservationResponse> Return(int userId, Guid reservationId);
        Task<ReservationResponse> ConfirmPayment(int userId, Guid reservationId, PaymentConfirmationRequest req);
        Task<VehicleReviewResponse> AddReview(int userId, Guid reservationId, ReviewRequest req);

    }
}
