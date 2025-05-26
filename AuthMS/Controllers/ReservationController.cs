using Application.Dtos.External;
using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IReservationServices;
using Application.Interfaces.IValidator;
using Azure;
using Azure.Core;
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
        private readonly IReservationPutService _putService;
        private readonly IReservationAvailabilityService _availabilityService;
        private readonly IValidatorHandler<GetAvailableVehiclesRequest> _getAvailableVehiclesRequestValidator;
        private readonly IValidatorHandler<GetReservationsRequest> _getReservationsRequestValidator;


        public ReservationsController(
            IReservationPostService postService,
            IReservationGetService getService,
            IReservationPutService putService,
            IReservationAvailabilityService availabilityService,
            IValidatorHandler<GetAvailableVehiclesRequest> getAvailableVehiclesRequestValidator,
            IValidatorHandler<GetReservationsRequest> getReservationsRequestValidator)
        {
            _postService = postService;
            _getService = getService;
            _putService = putService;
            _availabilityService = availabilityService;
            _getAvailableVehiclesRequestValidator = getAvailableVehiclesRequestValidator;
            _getReservationsRequestValidator = getReservationsRequestValidator;

        }


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
                    request.PickupBranchOfficeId,
                    request.DropOffBranchOfficeId,
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
            catch (InvalidValueException ex)
            {
                return Conflict(new ApiError { Message = ex.Message });
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


        /// <summary>
        /// Obtiene las reservas del usuario, con filtros opcionales y paginación.
        /// </summary>
        [Authorize(Policy = "ActiveUser")]
        [HttpGet]
        public async Task<IActionResult> GetReservations([FromQuery] GetReservationsRequest request)
        {
            try
            {
                await _getReservationsRequestValidator.Validate(request);

                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                    return Forbid();

                var result = await _getService.GetReservationsAsync(
                    userId,
                    request.Status,
                    request.From,
                    request.To,
                    request.Offset,
                    request.Size);

                Response.Headers.Add("offset", (request.Offset).ToString());
                Response.Headers.Add("size", (request.Size).ToString());
                Response.Headers.Add("totalCount", result.TotalCount.ToString());

                return Ok(result.Items);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }

        }


        /// <summary>
        /// Modifica sucursales y fechas de una reserva existente.
        /// </summary>
        [Authorize(Policy = "ActiveUser")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ReservationResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Update(Guid id, [FromBody] ReservationUpdateRequest request)
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "UserId");
            if (claim == null || !int.TryParse(claim.Value, out var userId))
                return Forbid();

            try
            {
                var updated = await _putService.Update(userId, id, request);
                return Ok(updated);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiError { Message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidValueException ex)
            {
                return Conflict(new ApiError { Message = ex.Message });
            }
        }


        /// <summary>
        /// Confirma una reserva existente.
        /// </summary>
        [Authorize(Policy = "ActiveUser")]
        [HttpPost("{id}/confirm")]
        [ProducesResponseType(typeof(ReservationResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> ConfirmReservation(Guid id)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Forbid();

            try
            {
                var resp = await _postService.Confirm(userId, id);
                return Ok(resp);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiError { Message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidValueException ex)
            {
                return Conflict(new ApiError { Message = ex.Message });
            }
        }


        /// <summary>
        /// Cancela una reserva existente.
        /// </summary>
        [Authorize(Policy = "ActiveUser")]
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(ReservationResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "UserId");
            if (claim == null || !int.TryParse(claim.Value, out var userId))
                return Forbid();

            try
            {
                var response = await _postService.Cancel(userId, id);
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiError { Message = ex.Message });
            }
            catch (InvalidValueException ex)
            {
                return Conflict(new ApiError { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
        }


        ///// <summary>
        ///// Recoge un vehículo de una reserva existente.
        ///// </summary>
        //[Authorize(Policy = "ActiveUser")]
        //[HttpPost("{id}/pickup")]
        //[ProducesResponseType(typeof(ReservationResponse), 200)]
        //[ProducesResponseType(typeof(ApiError), 409)]
        //[ProducesResponseType(404)]
        //[ProducesResponseType(403)]
        //public async Task<IActionResult> Pickup(Guid id, [FromBody] ActualPickupRequest request)
        //{
        //    var claim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "UserId");
        //    if (claim == null || !int.TryParse(claim.Value, out var userId))
        //        return Forbid();

        //    try
        //    {
        //        var resp = await _postService.Pickup(userId, id, request);
        //        return Ok(resp);
        //    }
        //    catch (NotFoundException ex)
        //    {
        //        return NotFound(new ApiError { Message = ex.Message });
        //    }
        //    catch (InvalidValueException ex)
        //    {
        //        return Conflict(new ApiError { Message = ex.Message });
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        return Forbid();
        //    }
        //}


        /// <summary>
        /// Recoge un vehículo de una reserva existente.
        /// </summary>
        [Authorize(Policy = "ActiveUser")]
        [HttpPost("{id}/pickup")]
        [ProducesResponseType(typeof(ReservationResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 409)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Pickup(Guid id)
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "UserId");
            if (claim == null || !int.TryParse(claim.Value, out var userId))
                return Forbid();

            try
            {
                var resp = await _postService.Pickup(userId, id);
                return Ok(resp);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiError { Message = ex.Message });
            }
            catch (InvalidValueException ex)
            {
                return Conflict(new ApiError { Message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }


        ///// <summary>
        ///// Devuelve un vehículo de una reserva existente.
        ///// </summary>
        //[Authorize(Policy = "ActiveUser")]
        //[HttpPost("{id}/return")]
        //[ProducesResponseType(typeof(ReservationResponse), 200)]
        //[ProducesResponseType(typeof(ApiError), 409)]
        //[ProducesResponseType(404)]
        //[ProducesResponseType(403)]
        //public async Task<IActionResult> Return(Guid id, [FromBody] ActualReturnRequest request)
        //{
        //    var claim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "UserId");
        //    if (claim == null || !int.TryParse(claim.Value, out var userId))
        //        return Forbid();

        //    try
        //    {
        //        var resp = await _postService.Return(userId, id, request);
        //        return Ok(resp);
        //    }
        //    catch (NotFoundException ex)
        //    {
        //        return NotFound(new ApiError { Message = ex.Message });
        //    }
        //    catch (InvalidValueException ex)
        //    {
        //        return Conflict(new ApiError { Message = ex.Message });
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        return Forbid();
        //    }
        //}


        /// <summary>
        /// Confirmar retorno de vehículo al finalizar una reserva
        /// </summary>
        [Authorize(Policy = "ActiveUser")]
        [HttpPost("{id}/return")]
        [ProducesResponseType(typeof(ReservationResponse), 200)]
        [ProducesResponseType(typeof(ApiError), 409)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> Return(Guid id)
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "UserId");
            if (claim == null || !int.TryParse(claim.Value, out var userId))
                return Forbid();

            try
            {
                var resp = await _postService.Return(userId, id);
                return Ok(resp);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiError { Message = ex.Message });
            }
            catch (InvalidValueException ex)
            {
                return Conflict(new ApiError { Message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }


        /// <summary>
        /// Confirma el pago de una reserva existente.
        /// </summary>
        [Authorize(Policy = "ActiveUser")]
        [HttpPost("{id}/payment")]
        [ProducesResponseType(typeof(ReservationResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> ConfirmPayment(Guid id, [FromBody] PaymentConfirmationRequest request)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Forbid();

            try
            {
                var resp = await _postService.ConfirmPayment(userId, id, request);
                return Ok(resp);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiError { Message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidValueException ex)
            {
                return Conflict(new ApiError { Message = ex.Message });
            }
        }


        /// <summary>
        /// Agrega una reseña a un vehículo.
        /// </summary>
        [Authorize(Policy = "ActiveUser")]
        [HttpPost("{id}/reviews")]
        [ProducesResponseType(typeof(VehicleReviewResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> AddReview(Guid id, [FromBody] ReviewRequest req)
        {
            // extraigo userId del token
            var userId = int.Parse(User.Claims.First(c => c.Type == "sub" || c.Type == "UserId").Value);

            try
            {
                var result = await _postService.AddReview(userId, id, req);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new ApiError { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (InvalidValueException ex)
            {
                return Conflict(new ApiError { Message = ex.Message });
            }
        }

    }
}
