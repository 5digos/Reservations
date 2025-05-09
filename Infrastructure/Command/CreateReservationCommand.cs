using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Dtos.Request;
using Application.Dtos.Response;
using Application.Interfaces.ICommand;
using Application.Interfaces.IServices;
using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Infrastructure.Command
{
    public class CreateReservationCommand : ICreateReservationCommand
    {
        private readonly IReservationRepository _repository;
        private readonly IVehicleService _vehicleService;
        private readonly IUserService _userService;
        private readonly IBranchOfficeService _branchOfficeService;

        public CreateReservationCommand(IReservationRepository repository, IVehicleService vehicleService, IUserService userService, IBranchOfficeService branchOfficeService)
        {
            _repository = repository;
            _vehicleService = vehicleService;
            _userService = userService;
            _branchOfficeService = branchOfficeService;
        }

        public async Task<ReservationResponse> ExecuteAsync(CreateReservationRequest request)
        {
            if (!await _vehicleService.ExistsAsync(request.VehicleId))
                throw new Exception("El vehículo no existe.");

            if (!await _userService.IsUserValidAsync(request.UserId))
                throw new Exception("El Usuario no existe");
            if (!await _branchOfficeService.IsBranchOfficeValidAsync(request.PickUpBranchOfficeId))
                throw new Exception("ID Sucursal inicio no valida");

            if (!await _branchOfficeService.IsBranchOfficeValidAsync(request.DropOffBranchOfficeId))
                throw new Exception("ID Sucursal fin no valida");


            var reservation = new Reservation
            {
                ReservationId = Guid.NewGuid(),
                VehicleId = request.VehicleId,
                UserId = request.UserId,
                PickUpBranchOfficeId = request.PickUpBranchOfficeId,
                DropOffBranchOfficeId = request.DropOffBranchOfficeId,
                Date = request.Date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                ReservationStatusId = 1, // Pendientew
                
            };

            await _repository.AddAsync(reservation);

            return new ReservationResponse
            {
                ReservationId = reservation.ReservationId,
                VehicleId = reservation.VehicleId,
                UserId = reservation.UserId,
                PickUpBranchOfficeId = reservation.PickUpBranchOfficeId,
                DropOffBranchOfficeId = reservation.DropOffBranchOfficeId,
                Date = reservation.Date,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                ReservationStatusId = reservation.ReservationStatusId,
                
            };
        }
    }
}
