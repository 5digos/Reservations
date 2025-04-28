using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.ICommand
{
    public interface IVehicleCommand
    {
        void Execute();
        void Undo();
        void Redo();
        void Save();
        void Load();
        void Delete();
        void Update();
        void GetById();
        void GetAll();
        void GetByBranchOfficeId();
        void GetByReservationId();
        void GetByUserId();
        void GetByVehicleId();
        void GetByReservationStatusId();
        void GetByStartTime();
        void GetByEndTime();
        void GetByPickUpBranchOfficeId();
        void GetByDropOffBranchOfficeId();
        void GetByVehicleTypeId();
        void GetByVehicleModelId();
        void GetByVehicleModelStatusId();
        void GetByVehicleBrandId();
        void GetByVehicleColorId();
        void GetByVehicleYear();
        void GetByVehiclePrice();
    }
}
