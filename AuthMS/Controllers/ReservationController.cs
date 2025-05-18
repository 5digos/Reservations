using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IReservationServices;
using Application.Interfaces.IValidator;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System;
using System.Security.Claims;
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
        private readonly IValidatorHandler<GetAvailableVehiclesRequest> _getAvailableVehiclesRequestValidator;

        public ReservationsController(
            IReservationPostService postService,
            IReservationGetService getService,
            IReservationAvailabilityService availabilityService,
            IValidatorHandler<GetAvailableVehiclesRequest> getAvailableVehiclesRequestValidator)
        {
            _postService = postService;
            _getService = getService;
            _availabilityService = availabilityService;
            _getAvailableVehiclesRequestValidator = getAvailableVehiclesRequestValidator;
        }

        ///// <summary>
        ///// Devuelve los vehículos disponibles para la sucursal y rango de fechas,
        ///// aplicando filtros y paginación.
        ///// </summary>
        //[HttpGet("available")]
        //public async Task<IActionResult> GetAvailable(
        //    [FromQuery] int branchOfficeId,
        //    [FromQuery] DateTime startTime,
        //    [FromQuery] DateTime endTime,
        //    [FromQuery] int offset = 0,
        //    [FromQuery] int size = 20,
        //    [FromQuery] int? category = null,
        //    [FromQuery] int? seatingCapacity = null,
        //    [FromQuery] int? transmissionType = null,
        //    [FromQuery] decimal? maxPrice = null,
        //    [FromQuery] string color = null,
        //    [FromQuery] string brand = null)
        //{
        //    var pagedResult = await _availabilityService.GetAvailableVehiclesAsync(
        //        branchOfficeId,
        //        startTime,
        //        endTime,
        //        offset,
        //        size,
        //        category,
        //        seatingCapacity,
        //        transmissionType,
        //        maxPrice,
        //        color,
        //        brand
        //    );

        //    Response.Headers.Add("X-Total-Count", pagedResult.TotalCount.ToString());
        //    Response.Headers.Add("X-Offset", offset.ToString());
        //    Response.Headers.Add("X-Size", size.ToString());

        //    return Ok(pagedResult.Items);
        //}


        /// <summary>
        /// Devuelve los vehículos disponibles para la sucursal y rango de fechas,
        /// aplicando filtros y paginación.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable([FromQuery] GetAvailableVehiclesRequest request)
        {
            try
            {
                await _getAvailableVehiclesRequestValidator.Validate(request);

                var paged = await _availabilityService.GetAvailableVehiclesAsync(
                    request.BranchOfficeId,
                    request.StartTime,
                    request.EndTime,
                    request.Offset,
                    request.Size,
                    request.Category,
                    request.SeatingCapacity,
                    request.TransmissionType,
                    request.MaxPrice,
                    request.Color,
                    request.Brand);

                Response.Headers.Add("X-Total-Count", paged.TotalCount.ToString());
                Response.Headers.Add("X-Offset", request.Offset.ToString());
                Response.Headers.Add("X-Size", request.Size.ToString());

                return Ok(paged.Items);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
        }

        /// <summary>
        /// Crea una nueva reserva.
        /// </summary>
        [Authorize(Policy = "ActiveUser")]
        //[Authorize]
        //[AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationRequest request)
        {
            try
            {
                //// Extraer UserId del JWT
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                    return Forbid();

                //var rawUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                // ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                //if (string.IsNullOrEmpty(rawUserId)
                //    || !int.TryParse(rawUserId, out var userId))
                //{
                //    return Forbid();
                //}

                var response = await _postService.Create(userId, request);
                return CreatedAtAction(nameof(GetById), new { id = response.ReservationId }, response);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (NotFoundException ex)
            {
                return new JsonResult(new ApiError { Message = ex.Message }) { StatusCode = 404 };
            }
            catch (InvalidValueException ex)
            {
                return Conflict(new ApiError { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        ///// <summary>
        ///// Obtiene una reserva por su ID.
        ///// </summary>
        //[Authorize(Policy = "ActiveUser")]
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetById(Guid id)
        //{
        //    try
        //    {
        //        var response = await _getService.GetById(id);
        //        //return new JsonResult(response) { StatusCode = 200 };

        //        // Extrae el userId del token
        //        var claim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "userId")?.Value;
        //        if (claim == null || !int.TryParse(claim, out var userId) || response.UserId != userId)
        //            return Forbid();             // 403 si no es suya

        //        return Ok(response);
        //    }
        //    catch (NotFoundException ex)
        //    {
        //        return NotFound(new ApiError { Message = ex.Message });
        //    }
        //}


        /// <summary>
        /// Obtiene una reserva por su ID.
        /// </summary>
        [Authorize(Policy = "ActiveUser")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var response = await _getService.GetById(id);
                //return new JsonResult(response) { StatusCode = 200 };

                var auth = await HttpContext
                    .RequestServices
                    .GetRequiredService<IAuthorizationService>()
                    .AuthorizeAsync(User, response, "SameUserPolicy");

                if (!auth.Succeeded)
                    return Forbid();

                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiError { Message = ex.Message });
            }
        }
    }
}
