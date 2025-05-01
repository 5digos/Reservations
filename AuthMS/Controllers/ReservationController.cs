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
        private readonly IVehicleService _vehicleService;
        private readonly IUserService _userService;


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
        public async Task<IActionResult> CreateReservation([FromBody] Reservation reservation)
        {
            var cliente = await _userService.GetUserByIdAsync(reservation.UserId);
            if (cliente == null) return BadRequest("Cliente inválido");

            var vehiculo = await _vehicleService.GetVehiculoByIdAsync(reservation.VehicleId);
            if (vehiculo == null) return BadRequest("Vehículo inválido");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _createReservationCommand.ExecuteAsync(reservation);
                return Ok(new { message = "Reservation created successfully." });
            }
            catch (Exception ex)
            {
                // Para depuración, luego podés usar logger
                return StatusCode(500, $"Internal server error: {ex.Message} | {ex.InnerException?.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservationById(Guid id)
        {
            var reservation = await _getReservationByIdQuery.ExecuteAsync(id);
            if (reservation == null)
                return NotFound();

            return Ok(reservation);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reservations = await _getAllReservationsQuery.ExecuteAsync();
            return Ok(reservations);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(Guid id, [FromBody] Reservation updatedReservation)
        {
            try
            {
                await _updateReservationCommand.ExecuteAsync(id, updatedReservation);
                return Ok(new { message = "Reservation updated successfully." });
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(Guid id)
        {
            try
            {
                await _deleteReservationCommand.ExecuteAsync(id);
                return Ok(new { message = "Reservation deleted successfully." });
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }



    }
}
