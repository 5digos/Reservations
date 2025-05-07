using Application.Dtos.Request;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IReservationServices;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;


namespace AuthMS.Controllers
{    
    [ApiController]
    [Route("api/v1/[controller]")]    
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationPostService _postService;
        private readonly IReservationGetService _getService;
        private readonly IReservationAvailabilityService _availabilityService;

        public ReservationsController(
            IReservationPostService postService,
            IReservationGetService getService,
            IReservationAvailabilityService availabilityService)
        {
            _postService = postService;
            _getService = getService;
            _availabilityService = availabilityService;
        }

        /// <summary>
        /// Devuelve los vehículos disponibles para la sucursal y rango de fechas,
        /// aplicando filtros y paginación.
        /// </summary>
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable(
            [FromQuery] int branchOfficeId,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime,
            [FromQuery] int offset = 0,
            [FromQuery] int size = 20,
            [FromQuery] int? category = null,
            [FromQuery] int? seatingCapacity = null,
            [FromQuery] int? transmissionType = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string color = null,
            [FromQuery] string brand = null)
        {
            var pagedResult = await _availabilityService.GetAvailableVehiclesAsync(
                branchOfficeId,
                startTime,
                endTime,
                offset,
                size,
                category,
                seatingCapacity,
                transmissionType,
                maxPrice,
                color,
                brand
            );

            Response.Headers.Add("X-Total-Count", pagedResult.TotalCount.ToString());
            Response.Headers.Add("X-Offset", offset.ToString());
            Response.Headers.Add("X-Size", size.ToString());

            return Ok(pagedResult.Items);
        }

        /// <summary>
        /// Crea una nueva reserva.
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ReservationRequest request)
        {
            // Extraer UserId del JWT
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "userId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Forbid();

            var response = await _postService.Create(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = response.ReservationId }, response);
        }

        /// <summary>
        /// Obtiene una reserva por su ID.
        /// </summary>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _getService.GetById(id);
            if (response == null)
                return NotFound();
            return Ok(response);
        }
    }
}
