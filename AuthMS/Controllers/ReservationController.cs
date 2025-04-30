using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
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

        public ReservationsController(
            ICreateReservationCommand createReservationCommand,
            IGetReservationByIdQuery getReservationByIdQuery)
        {
            _createReservationCommand = createReservationCommand;
            _getReservationByIdQuery = getReservationByIdQuery;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] Reservation reservation)
        {
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

    }
}
