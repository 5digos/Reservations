using Application.Interfaces.IServices;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Infrastructure.Services.ReservationStatusPay;

namespace Infrastructure.Services
{
    public class ReservationStatusPay : IReservationStatusPay
    {

        private readonly HttpClient _httpClient;
        private readonly IReservationRepository _reservationRepository;

        public ReservationStatusPay(HttpClient httpClient, IReservationRepository reservationRepository)
        {
            _httpClient = httpClient;
            _reservationRepository = reservationRepository;
        }

        public async Task<bool> IsReservationStatusPayValidAsync(Guid reservationStatusPayId)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationStatusPayId);
            if (reservation == null) return false;

            // Llamar al microservicio de pagos aqui
            var response = await _httpClient.GetAsync($"http://microservicio-pagos/api/payment/{reservation.ReservationId}");

            if (!response.IsSuccessStatusCode) return false;

            var content = await response.Content.ReadAsStringAsync();
            var pagoRealizado = bool.Parse(content); // true o false

            reservation.ReservationStatusId = pagoRealizado ? 2 : 3; // 2 = Aceptado, 3 = Cancelado

            await _reservationRepository.UpdateAsync(reservation);

            return true;
        }
    }
}
