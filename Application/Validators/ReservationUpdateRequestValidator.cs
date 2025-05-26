using Application.Dtos.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class ReservationUpdateRequestValidator : AbstractValidator<ReservationUpdateRequest>
    {
        public ReservationUpdateRequestValidator()
        {
            RuleFor(x => x.PickupBranchOfficeId)
                .GreaterThan(0)
                .WithMessage("La sucursal de recogida es obligatoria.");

            RuleFor(x => x.DropOffBranchOfficeId)
                .GreaterThan(0)
                .WithMessage("La sucursal de devolución es obligatoria.");

            RuleFor(x => x.StartTime)
              .Must(dt => dt.Minute == 0 && dt.Second == 0)
              .WithMessage("La hora de inicio debe ser un número entero de hora (sin minutos ni segundos).")
              .LessThan(x => x.EndTime)
              .WithMessage("StartTime debe ser anterior a EndTime.")
              .GreaterThan(_ => DateTime.Now)
              .WithMessage("La fecha y hora de inicio deben ser mayores que la fecha y hora actual.");

            RuleFor(x => x.EndTime)
              .Must(dt => dt.Minute == 0 && dt.Second == 0)
              .WithMessage("La hora de fin debe ser un número entero de hora (sin minutos ni segundos).")
              .GreaterThan(x => x.StartTime)
              .WithMessage("EndTime debe ser posterior a StartTime.");
        }
    }
}
