using Microsoft.AspNetCore.Http;
using Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.IServices;


namespace Template.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    { 
        private readonly IReservationService _reservationService;
        //definir controladores de reserva
        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpPost]
        //Crea la reserva en una lista de tareas
        public async Task<IActionResult> Create([FromBody] ReservationDto dto)
        {
            var result = await _reservationService.CreateReservationAsync(dto);
            return Ok(result);
        }
    }
}
