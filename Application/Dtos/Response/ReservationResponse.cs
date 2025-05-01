using System;

namespace Application.Dtos.Response
{
    public class ReservationResponse
    {
        public Guid ReservationId { get; set; }
        public Guid VehicleId { get; set; }
        public int UserId { get; set; }
        public int PickUpBranchOfficeId { get; set; }
        public int DropOffBranchOfficeId { get; set; }
        public DateOnly Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int ReservationStatusId { get; set; }
        
    }
}
