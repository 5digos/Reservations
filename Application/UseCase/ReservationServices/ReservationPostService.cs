using Application.Dtos.External;
using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.Interfaces.IServices.IReservationServices;
using Application.Interfaces.IServices.IVehicleServices;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.UseCase.ReservationServices
{
    public class ReservationPostService : IReservationPostService
    {
        private readonly IReservationCommand _reservationCommand;
        private readonly IReservationEventCommand _eventCommand;
        private readonly IVehicleService _vehicleService;
        private readonly IReservationQuery _reservationQuery;
        private readonly ITimeProvider _timeProvider;
        private readonly INotificationService _notificationService;

        public ReservationPostService(
            IReservationCommand reservationCommand,
            IReservationEventCommand eventCommand,
            IVehicleService vehicleService,
            IReservationQuery reservationQuery,
            ITimeProvider timeProvider,
            INotificationService notificationService)
        {
            _reservationCommand = reservationCommand;
            _eventCommand = eventCommand;
            _vehicleService = vehicleService;
            _reservationQuery = reservationQuery;
            _timeProvider = timeProvider;
            _notificationService = notificationService;
        }

        public async Task<ReservationResponse> Create(int userId, ReservationRequest request)
        {
            // Validar Fecha de reserva
            if (request.StartTime.Date > _timeProvider.Now.Date.AddDays(1))
                throw new InvalidValueException("La fecha de retiro debe ser hoy o mañana.");

            // Validar sucursales
            var pickupBranch = await _vehicleService.GetBranchOfficeByIdAsync(request.PickupBranchOfficeId)
                ?? throw new NotFoundException("Sucursal de recogida no encontrada.");
            var dropOffBranch = await _vehicleService.GetBranchOfficeByIdAsync(request.DropOffBranchOfficeId)
                ?? throw new NotFoundException("Sucursal de devolución no encontrada.");

            // Validar vehículo
            var vehicle = await _vehicleService.GetVehicleByIdAsync(request.VehicleId)
                ?? throw new NotFoundException("Vehículo no encontrado.");
            if (vehicle.StatusId != 1)
                throw new InvalidValueException("Vehículo no disponible.");

            var rateSnapshot = vehicle.Price;

            var nextPickup = await _reservationQuery
                  .GetNextPickupBranch(request.VehicleId, request.EndTime);

            if (nextPickup.HasValue && nextPickup.Value != request.DropOffBranchOfficeId)
            {
                throw new InvalidValueException(
                    "Este vehículo tiene una reserva próxima en otra sucursal; no puede devolverse en una diferente.");
            }

            var lastDropOff = await _reservationQuery
                .GetLastReturnBranch(request.VehicleId, request.StartTime);

            if (lastDropOff.HasValue && lastDropOff.Value != request.PickupBranchOfficeId)
            {
                throw new InvalidValueException(
                    "Este vehículo no estará disponible en la sucursal de recogida al inicio de tu reserva.");
            }

            // Validar solapamiento
            if (await _reservationQuery.HasOverlap(
                request.VehicleId,
                request.StartTime,
                request.EndTime,
                bufferHours: 3))
            {
                throw new InvalidValueException("El vehículo no está disponible en el intervalo seleccionado.");
            }

            // Crear reserva
            var now = _timeProvider.Now;
            var reservation = new Reservation
            {
                ReservationId = Guid.NewGuid(),
                UserId = userId,
                VehicleId = request.VehicleId,
                PickupBranchOfficeId = request.PickupBranchOfficeId,
                DropOffBranchOfficeId = request.DropOffBranchOfficeId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = ReservationStatus.Pending,
                CreatedAt = now,
                HourlyRateSnapshot = rateSnapshot
            };
            await _reservationCommand.Add(reservation);

            // Registrar evento
            var evt = new ReservationEvent
            {
                EventId = Guid.NewGuid(),
                ReservationId = reservation.ReservationId,
                EventType = ReservationEventType.Created,
                OccurredAt = now,
                Details = "Reserva creada"
            };
            await _eventCommand.Add(evt);

            // Encolar notificación
            await _notificationService.EnqueueEvent(new NotificationEventRequest
            {
                UserId = userId,
                EventType = "ReservationCreated",
                Payload = new
                {
                    ReservationId = reservation.ReservationId,
                    PickupBranchName = pickupBranch.Name,
                    DropOffBranchName = dropOffBranch.Name,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime
                }
            });

            // Mapear respuesta
            return new ReservationResponse
            {
                ReservationId = reservation.ReservationId,
                UserId = userId,
                VehicleId = request.VehicleId,
                PickupBranchOfficeId = pickupBranch.BranchOfficeId,
                PickupBranchOfficeName = pickupBranch.Name,
                DropOffBranchOfficeId = dropOffBranch.BranchOfficeId,
                DropOffBranchOfficeName = dropOffBranch.Name,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                HourlyRateSnapshot = reservation.HourlyRateSnapshot,
                Status = reservation.Status
            };
        }


        public async Task<ReservationResponse> Confirm(int userId, Guid reservationId)
        {
            // 1) Cargo reserva
            var res = await _reservationQuery.GetById(reservationId)
                      ?? throw new NotFoundException("Reserva no encontrada.");

            // 2) Sólo el mismo usuario puede confirmarla
            if (res.UserId != userId)
                throw new UnauthorizedAccessException("No puedes confirmar esta reserva.");

            // 3) Sólo si está en estado Pending
            if (res.Status != ReservationStatus.Pending)
                throw new InvalidValueException("Solo se pueden confirmar reservas en estado Pending.");

            var pickupBranch = await _vehicleService.GetBranchOfficeByIdAsync(res.PickupBranchOfficeId)
                ?? throw new NotFoundException("Sucursal de recogida no encontrada.");
            var dropOffBranch = await _vehicleService.GetBranchOfficeByIdAsync(res.DropOffBranchOfficeId)
                ?? throw new NotFoundException("Sucursal de devolución no encontrada.");

            // 4) Cambio de estado y guardo
            res.Status = ReservationStatus.Confirmed;
            await _reservationCommand.Update(res);

            // 5) Registro evento
            var now = _timeProvider.Now;
            await _eventCommand.Add(new ReservationEvent
            {
                EventId = Guid.NewGuid(),
                ReservationId = reservationId,
                EventType = ReservationEventType.Confirmed,
                OccurredAt = now,
                Details = "Reserva confirmada"
            });

            // 6) (Opcional) Encolar notificación
            await _notificationService.EnqueueEvent(new NotificationEventRequest
            {
                UserId = userId,
                EventType = "ReservationConfirmed",
                Payload = new
                {
                    ReservationId = reservationId,
                    PickupBranchName = pickupBranch.Name,
                    DropOffBranchName = dropOffBranch.Name,
                    StartTime = res.StartTime,
                    EndTime = res.EndTime
                }
            });



            // 7) Mapear a Response
            return new ReservationResponse
            {
                ReservationId = res.ReservationId,
                UserId = res.UserId,
                VehicleId = res.VehicleId,
                PickupBranchOfficeId = res.PickupBranchOfficeId,
                PickupBranchOfficeName = pickupBranch.Name,
                DropOffBranchOfficeId = res.DropOffBranchOfficeId,
                DropOffBranchOfficeName = dropOffBranch.Name,
                StartTime = res.StartTime,
                EndTime = res.EndTime,
                HourlyRateSnapshot = res.HourlyRateSnapshot,
                Status = res.Status
            };
        }


        public async Task<ReservationResponse> Cancel(int userId, Guid reservationId)
        {
            // 1) Cargo reserva
            var res = await _reservationQuery.GetById(reservationId)
                      ?? throw new NotFoundException("Reserva no encontrada.");

            var pickupBranch = await _vehicleService.GetBranchOfficeByIdAsync(res.PickupBranchOfficeId)
                ?? throw new NotFoundException("Sucursal de recogida no encontrada.");

            var dropOffBranch = await _vehicleService.GetBranchOfficeByIdAsync(res.DropOffBranchOfficeId)
                ?? throw new NotFoundException("Sucursal de devolución no encontrada.");

            // 2) Sólo el mismo usuario puede cancelarla
            if (res.UserId != userId)
                throw new UnauthorizedAccessException("No puedes cancelar esta reserva.");

            // 3) Sólo si aún no ha empezado y faltan ≥2 horas
            var now = _timeProvider.Now;
            if (res.StartTime <= now)
                throw new InvalidValueException("No puedes cancelar reservas pasadas.");
            if (res.StartTime - now < TimeSpan.FromHours(2))
                throw new InvalidValueException("Sólo puedes cancelar hasta 2 horas antes del inicio.");

            // 4) Cambio de estado y guardo
            res.Status = ReservationStatus.Cancelled;
            await _reservationCommand.Update(res);

            // 5) Registro evento
            await _eventCommand.Add(new ReservationEvent
            {
                EventId = Guid.NewGuid(),
                ReservationId = reservationId,
                EventType = ReservationEventType.Cancelled,
                OccurredAt = now,
                Details = "Reserva cancelada por el usuario"
            });

            // 6) Encolar notificación
            await _notificationService.EnqueueEvent(new NotificationEventRequest
            {
                UserId = userId,
                EventType = "ReservationCancelled",
                Payload = new
                {
                    ReservationId = reservationId,
                    PickupBranchName = pickupBranch.Name,
                    DropOffBranchName = dropOffBranch.Name,
                    StartTime = res.StartTime,
                    EndTime = res.EndTime
                }
            });

            // 7) Mapear a DTO
            return new ReservationResponse
            {
                ReservationId = res.ReservationId,
                UserId = res.UserId,
                VehicleId = res.VehicleId,
                PickupBranchOfficeId = res.PickupBranchOfficeId,
                PickupBranchOfficeName = pickupBranch.Name,
                DropOffBranchOfficeId = res.DropOffBranchOfficeId,
                DropOffBranchOfficeName = dropOffBranch.Name,
                StartTime = res.StartTime,
                EndTime = res.EndTime,
                HourlyRateSnapshot = res.HourlyRateSnapshot,
                Status = res.Status
            };
        }


        //public async Task<ReservationResponse> Pickup(int userId, Guid reservationId, ActualPickupRequest request)
        //{
        //    // 1) Cargo reserva
        //    var res = await _reservationQuery.GetById(reservationId)
        //              ?? throw new NotFoundException("Reserva no encontrada.");

        //    // 2) Sólo el mismo usuario puede hacer el pickup
        //    if (res.UserId != userId)
        //        throw new UnauthorizedAccessException("No puedes registrar el retiro de esta reserva.");

        //    // 3) Sólo si está Confirmed
        //    if (res.Status != ReservationStatus.Confirmed)
        //        throw new InvalidValueException("Sólo las reservas confirmadas pueden iniciar el retiro.");

        //    // 4) Determino la hora de pickup
        //    var now = _timeProvider.Now;
        //    var actual = request.ActualPickupTime.HasValue
        //        ? request.ActualPickupTime.Value
        //        : now;

        //    // (Opcional: valida que actual esté dentro de un rango razonable)
        //    if (actual < res.StartTime.AddHours(-1) || actual > now.AddHours(1))
        //        throw new InvalidValueException("La hora de retiro proporcionada no es válida.");

        //    // 5) Actualizo entidad
        //    res.ActualPickupTime = actual;
        //    res.Status = ReservationStatus.InProgress;
        //    await _reservationCommand.Update(res);

        //    // 6) Registro evento
        //    await _eventCommand.Add(new ReservationEvent
        //    {
        //        EventId = Guid.NewGuid(),
        //        ReservationId = reservationId,
        //        EventType = ReservationEventType.PickedUp,
        //        OccurredAt = now,
        //        Details = "Vehiculo retirado"
        //    });

        //    // 7) Encolo notificación
        //    var pickupBranch = await _vehicleService.GetBranchOfficeByIdAsync(res.PickupBranchOfficeId)
        //                            ?? throw new NotFoundException("Sucursal de recogida no encontrada.");
        //    var dropOffBranch = await _vehicleService.GetBranchOfficeByIdAsync(res.DropOffBranchOfficeId)
        //                             ?? throw new NotFoundException("Sucursal de devolución no encontrada.");

        //    await _notificationService.EnqueueEvent(new NotificationEventRequest
        //    {
        //        UserId = userId,
        //        EventType = "ReservationPickedUp",
        //        Payload = new
        //        {
        //            ReservationId = reservationId,
        //            PickupBranchName = pickupBranch.Name,
        //            DropOffBranchName = dropOffBranch.Name,
        //            ActualPickupTime = actual,
        //            EndTime = res.EndTime
        //        }
        //    });

        //    return new ReservationResponse
        //    {
        //        ReservationId = res.ReservationId,
        //        UserId = res.UserId,
        //        VehicleId = res.VehicleId,
        //        PickupBranchOfficeId = res.PickupBranchOfficeId,
        //        PickupBranchOfficeName = pickupBranch.Name,
        //        DropOffBranchOfficeId = res.DropOffBranchOfficeId,
        //        DropOffBranchOfficeName = dropOffBranch.Name,
        //        StartTime = res.StartTime,
        //        ActualPickupTime = res.ActualPickupTime,
        //        EndTime = res.EndTime,
        //        Status = res.Status
        //    };
        //}


        public async Task<ReservationResponse> Pickup(int userId, Guid reservationId)
        {
            // 1) Cargo reserva
            var res = await _reservationQuery.GetById(reservationId)
                      ?? throw new NotFoundException("Reserva no encontrada.");

            // 2) Sólo el mismo usuario puede hacer el pickup
            if (res.UserId != userId)
                throw new UnauthorizedAccessException("No puedes registrar el retiro de esta reserva.");

            // 3) Sólo si está Confirmed
            if (res.Status != ReservationStatus.Confirmed)
                throw new InvalidValueException("Sólo las reservas confirmadas pueden iniciar el retiro.");

            // 4) Determino la hora de pickup
            var now = _timeProvider.Now;
            var actual = now;

            // (Opcional: valida que actual esté dentro de un rango razonable)
            if (actual < res.StartTime.AddHours(-1) || actual > now.AddHours(1))
                throw new InvalidValueException("La hora de retiro proporcionada no es válida.");

            // 5) Actualizo entidad
            res.ActualPickupTime = actual;
            res.Status = ReservationStatus.InProgress;
            await _reservationCommand.Update(res);

            // 6) Registro evento
            await _eventCommand.Add(new ReservationEvent
            {
                EventId = Guid.NewGuid(),
                ReservationId = reservationId,
                EventType = ReservationEventType.VehiclePickedUp,
                OccurredAt = now,
                Details = "Vehiculo retirado"
            });

            // 7) Encolo notificación
            var pickupBranch = await _vehicleService.GetBranchOfficeByIdAsync(res.PickupBranchOfficeId)
                                    ?? throw new NotFoundException("Sucursal de recogida no encontrada.");
            var dropOffBranch = await _vehicleService.GetBranchOfficeByIdAsync(res.DropOffBranchOfficeId)
                                     ?? throw new NotFoundException("Sucursal de devolución no encontrada.");

            await _notificationService.EnqueueEvent(new NotificationEventRequest
            {
                UserId = userId,
                EventType = "VehiclePickedUp",
                Payload = new
                {
                    ReservationId = reservationId,
                    PickupBranchName = pickupBranch.Name,
                    DropOffBranchName = dropOffBranch.Name,
                    ActualPickupTime = actual,
                    EndTime = res.EndTime
                }
            });

            return new ReservationResponse
            {
                ReservationId = res.ReservationId,
                UserId = res.UserId,
                VehicleId = res.VehicleId,
                PickupBranchOfficeId = res.PickupBranchOfficeId,
                PickupBranchOfficeName = pickupBranch.Name,
                DropOffBranchOfficeId = res.DropOffBranchOfficeId,
                DropOffBranchOfficeName = dropOffBranch.Name,
                StartTime = res.StartTime,
                ActualPickupTime = res.ActualPickupTime,
                EndTime = res.EndTime,
                HourlyRateSnapshot = res.HourlyRateSnapshot,
                Status = res.Status
            };
        }


        //public async Task<ReservationResponse> Return(int userId, Guid reservationId, ActualReturnRequest request)
        //{
        //    // 1) Cargo reserva
        //    var res = await _reservationQuery.GetById(reservationId)
        //              ?? throw new NotFoundException("Reserva no encontrada.");

        //    // 2) Sólo el mismo usuario puede devolver
        //    if (res.UserId != userId)
        //        throw new UnauthorizedAccessException("No puedes registrar la devolución de esta reserva.");

        //    // 3) Sólo si está en curso (InProgress)
        //    if (res.Status != ReservationStatus.InProgress)
        //        throw new InvalidValueException("Sólo las reservas en curso pueden devolverse.");

        //    // 4) Calculo hora de retorno
        //    var now = _timeProvider.Now;
        //    var actual = request.ActualReturnTime ?? now;

        //    if (res.ActualPickupTime.HasValue && actual < res.ActualPickupTime.Value)
        //        throw new InvalidValueException("La hora de devolución no puede ser anterior al retiro.");

        //    // 5) Actualizo entidad
        //    res.ActualReturnTime = actual;            
        //    await _reservationCommand.Update(res);

        //    // 6) Registro evento
        //    await _eventCommand.Add(new ReservationEvent
        //    {
        //        EventId = Guid.NewGuid(),
        //        ReservationId = reservationId,
        //        EventType = ReservationEventType.VehicleReturned,
        //        OccurredAt = now,
        //        Details = "Vehiculo devuelto"
        //    });

        //    // Actualizar la sucursal del vehículo en VehicleMS
        //    await _vehicleService.UpdateBranchOffice(res.VehicleId, res.DropOffBranchOfficeId);

        //    // 7) Encolo notificación
        //    var pickupBranch = await _vehicleService.GetBranchOfficeByIdAsync(res.PickupBranchOfficeId)
        //                            ?? throw new NotFoundException("Sucursal de recogida no encontrada.");
        //    var dropOffBranch = await _vehicleService.GetBranchOfficeByIdAsync(res.DropOffBranchOfficeId)
        //                             ?? throw new NotFoundException("Sucursal de devolución no encontrada.");

        //    await _notificationService.EnqueueEvent(new NotificationEventRequest
        //    {
        //        UserId = userId,
        //        EventType = "VehicleReturned",
        //        Payload = new
        //        {
        //            ReservationId = reservationId,
        //            PickupBranchName = pickupBranch.Name,
        //            DropOffBranchName = dropOffBranch.Name,
        //            ActualPickupTime = res.ActualPickupTime,
        //            ActualReturnTime = actual
        //        }
        //    });            

        //    return new ReservationResponse
        //    {
        //        ReservationId = res.ReservationId,
        //        UserId = res.UserId,
        //        VehicleId = res.VehicleId,
        //        PickupBranchOfficeId = res.PickupBranchOfficeId,
        //        PickupBranchOfficeName = pickupBranch.Name,
        //        DropOffBranchOfficeId = res.DropOffBranchOfficeId,
        //        DropOffBranchOfficeName = dropOffBranch.Name,
        //        StartTime = res.StartTime,                
        //        EndTime = res.EndTime,
        //        ActualPickupTime = res.ActualPickupTime,
        //        ActualReturnTime = res.ActualReturnTime,
        //        Status = res.Status
        //    };
        //}

        public async Task<ReservationResponse> Return(int userId, Guid reservationId)
        {
            // 1) Cargo reserva
            var res = await _reservationQuery.GetById(reservationId)
                      ?? throw new NotFoundException("Reserva no encontrada.");

            // 2) Sólo el mismo usuario puede devolver
            if (res.UserId != userId)
                throw new UnauthorizedAccessException("No puedes registrar la devolución de esta reserva.");

            // 3) Sólo si está en curso (InProgress)
            if (res.Status != ReservationStatus.InProgress)
                throw new InvalidValueException("Sólo las reservas en curso pueden devolverse.");

            // 4) Calculo hora de retorno
            var now = _timeProvider.Now;
            var actual = now;

            if (res.ActualPickupTime.HasValue && actual < res.ActualPickupTime.Value)
                throw new InvalidValueException("La hora de devolución no puede ser anterior al retiro.");

            // 5) Actualizo entidad
            res.ActualReturnTime = actual;
            res.Status = ReservationStatus.Completed;
            await _reservationCommand.Update(res);

            // 6) Registro evento
            await _eventCommand.Add(new ReservationEvent
            {
                EventId = Guid.NewGuid(),
                ReservationId = reservationId,
                EventType = ReservationEventType.VehicleReturned,
                OccurredAt = now,
                Details = "Vehiculo devuelto"
            });

            // Actualizar la sucursal del vehículo en VehicleMS
            await _vehicleService.UpdateBranchOffice(res.VehicleId, res.DropOffBranchOfficeId);

            // 7) Encolo notificación
            var pickupBranch = await _vehicleService.GetBranchOfficeByIdAsync(res.PickupBranchOfficeId)
                                    ?? throw new NotFoundException("Sucursal de recogida no encontrada.");
            var dropOffBranch = await _vehicleService.GetBranchOfficeByIdAsync(res.DropOffBranchOfficeId)
                                     ?? throw new NotFoundException("Sucursal de devolución no encontrada.");

            await _notificationService.EnqueueEvent(new NotificationEventRequest
            {
                UserId = userId,
                EventType = "VehicleReturned",
                Payload = new
                {
                    ReservationId = reservationId,
                    PickupBranchName = pickupBranch.Name,
                    DropOffBranchName = dropOffBranch.Name,
                    ActualPickupTime = res.ActualPickupTime,
                    ActualReturnTime = actual
                }
            });

            return new ReservationResponse
            {
                ReservationId = res.ReservationId,
                UserId = res.UserId,
                VehicleId = res.VehicleId,
                PickupBranchOfficeId = res.PickupBranchOfficeId,
                PickupBranchOfficeName = pickupBranch.Name,
                DropOffBranchOfficeId = res.DropOffBranchOfficeId,
                DropOffBranchOfficeName = dropOffBranch.Name,
                StartTime = res.StartTime,
                EndTime = res.EndTime,
                ActualPickupTime = res.ActualPickupTime,
                ActualReturnTime = res.ActualReturnTime,
                HourlyRateSnapshot = res.HourlyRateSnapshot,
                Status = res.Status
            };
        }


        public async Task<ReservationResponse> ConfirmPayment(int userId, Guid reservationId, PaymentConfirmationRequest req)
        {
            // 1) Cargo reserva
            var res = await _reservationQuery.GetById(reservationId)
                      ?? throw new NotFoundException("Reserva no encontrada.");

            // 2) Solo el dueño puede pagar
            if (res.UserId != userId)
                throw new UnauthorizedAccessException("No puedes pagar esta reserva.");

            if (res.Status != ReservationStatus.Completed)
                throw new InvalidValueException(
                    "Solo reservas con vehículo devuelto pueden ser pagadas.");

            // 4) Actualizar montos y estado
            res.OriginalCost = req.TotalAmount;
            res.LateFee = req.LateFee;
            res.Status = ReservationStatus.Paid;
            await _reservationCommand.Update(res);

            // 5) Registrar evento de pago
            var now = _timeProvider.Now;
            await _eventCommand.Add(new ReservationEvent
            {
                EventId = Guid.NewGuid(),
                ReservationId = reservationId,
                EventType = ReservationEventType.PaymentSucceeded,
                OccurredAt = now,
                Details = $"Pago de ${req.TotalAmount:0.00} vía {req.PaymentGateway}"
            });

            // 6) Encolar notificación
            var pickup = await _vehicleService.GetBranchOfficeByIdAsync(res.PickupBranchOfficeId)
                          ?? throw new NotFoundException("Sucursal de recogida no encontrada.");
            var dropoff = await _vehicleService.GetBranchOfficeByIdAsync(res.DropOffBranchOfficeId)
                          ?? throw new NotFoundException("Sucursal de devolución no encontrada.");

            await _notificationService.EnqueueEvent(new NotificationEventRequest
            {
                UserId = userId,
                EventType = "PaymentSucceeded",
                Payload = new
                {
                    ReservationId = reservationId,
                    TotalAmount = req.TotalAmount,
                    LateFee = req.LateFee,
                    PaymentGateway = req.PaymentGateway,
                    TransactionId = req.TransactionId
                }
            });


            return new ReservationResponse
            {
                ReservationId = res.ReservationId,
                UserId = res.UserId,
                VehicleId = res.VehicleId,
                PickupBranchOfficeId = res.PickupBranchOfficeId,
                PickupBranchOfficeName = pickup.Name,
                DropOffBranchOfficeId = res.DropOffBranchOfficeId,
                DropOffBranchOfficeName = dropoff.Name,
                StartTime = res.StartTime,
                EndTime = res.EndTime,
                ActualPickupTime = res.ActualPickupTime,
                ActualReturnTime = res.ActualReturnTime,
                HourlyRateSnapshot = res.HourlyRateSnapshot,
                TotalAmount = res.OriginalCost,
                LateFee = res.LateFee,
                Status = res.Status
            };
        }


        public async Task<VehicleReviewResponse> AddReview(int userId, Guid reservationId, ReviewRequest req)
        {            

            // 2) Traer la reserva
            var res = await _reservationQuery.GetById(reservationId)
                      ?? throw new NotFoundException("Reserva no encontrada.");

            // 3) Sólo el que creó puede opinar
            if (res.UserId != userId)
                throw new UnauthorizedAccessException("No puedes reseñar esta reserva.");

            // 4) Sólo si está en estado Paid
            if (res.Status != ReservationStatus.Paid)
                throw new InvalidValueException("Solo reservas pagadas pueden reseñarse.");

            // 5) Construir DTO para el microservicio de vehículos
            var vehicleReviewReq = new VehicleReviewRequest
            {
                ReservationId = reservationId,
                Rating = req.Rating,
                Comment = req.Comment
            };

            // 6) Llamar a VehicleMS
            var reviewResp = await _vehicleService.AddReviewAsync(res.VehicleId, vehicleReviewReq);

            return reviewResp;
        }
    }
}
