using Application.Dtos.Request;
using Application.Dtos.Request;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;


namespace AuthMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly ICreateReservationCommand _createReservationCommand;
        private readonly IGetReservationByIdQuery _getReservationByIdQuery;
        private readonly IGetAllReservationsQuery _getAllReservationsQuery;
        private readonly IUpdateReservationCommand _updateReservationCommand;
        private readonly IDeleteReservationCommand _deleteReservationCommand;
<<<<<<< HEAD
        private readonly IVehicleService _vehicleService;
        private readonly IUserService _userService;

=======
>>>>>>> 03abab8cd72fddcaadd9a63e57cf7929ae7a05e4

        public ReservationsController(
            ICreateReservationCommand createReservationCommand,
            IGetReservationByIdQuery getReservationByIdQuery,
            IGetAllReservationsQuery getAllReservationsQuery,
            IUpdateReservationCommand updateReservationCommand,
            IDeleteReservationCommand deleteReservationCommand,
            IUserService userService, IVehicleService vehicleService)
        {
            _createReservationCommand = createReservationCommand;
            _getReservationByIdQuery = getReservationByIdQuery;
            _getAllReservationsQuery = getAllReservationsQuery;
            _updateReservationCommand = updateReservationCommand;
            _deleteReservationCommand = deleteReservationCommand;
            _userService = userService;
            _vehicleService = vehicleService;

        }

        
        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequest request)
        {
            var cliente = await _userService.GetUserByIdAsync(reservation.UserId);
            if (cliente == null) return BadRequest("Cliente inválido");

            var vehiculo = await _vehicleService.GetVehiculoByIdAsync(reservation.VehicleId);
            if (vehiculo == null) return BadRequest("Vehículo inválido");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _createReservationCommand.ExecuteAsync(request);
                return Ok(result); // Devuelve ReservationResponse
                //CreatedAtAction(nameof(GetReservationById), new { id = result.ReservationId }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} | {ex.InnerException?.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservationById(Guid id)
        {
            var reservation = await _getReservationByIdQuery.ExecuteAsync(id);
            if (reservation == null)
                return NotFound(new
                {
                    message = "No existe una reserva con ese ID. Intenta nuevamente."
                });

            return Ok(reservation);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reservations = await _getAllReservationsQuery.ExecuteAsync();
            return Ok(reservations);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(Guid id, [FromBody] UpdateReservationRequest request)
        {
            // 1) Validar existencia
            var existing = await _getReservationByIdQuery.ExecuteAsync(id);
            if (existing == null)
                return NotFound(new
                {
                    message = "No existe una reserva con ese ID. Intenta nuevamente."
                });

            // 2) Intentar actualizar
            try
            {
                await _updateReservationCommand.ExecuteAsync(id, request);
                return Ok(new
                {
                    message = "Reserva actualizada correctamente."
                });
            }
            catch (Exception ex)
            {
                // Si falla por otro motivo, devolvemos 500 con detalle
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(Guid id)
        {
            // 1) Compruebo existencia
            var existing = await _getReservationByIdQuery.ExecuteAsync(id);
            if (existing == null)
                return NotFound(new
                {
                    message = "No existe una reserva con ese ID. Intenta nuevamente."
                });

            // 2) Si existe, elimino
            await _deleteReservationCommand.ExecuteAsync(id);
            return Ok(new
            {
                message = "Reserva eliminada correctamente."
            });
        }
    }
}
