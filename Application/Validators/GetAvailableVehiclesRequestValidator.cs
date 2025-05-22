using Application.Dtos.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class GetAvailableVehiclesRequestValidator : AbstractValidator<GetAvailableVehiclesRequest>
    {
        public GetAvailableVehiclesRequestValidator()
        {
            RuleFor(x => x.PickupBranchOfficeId)
                .GreaterThan(0).WithMessage("La sucursal de recogida es obligatoria y debe ser mayor a 0.");

            RuleFor(x => x.DropOffBranchOfficeId)
                .GreaterThan(0).WithMessage("La sucursal de devolucion es obligatoria y debe ser mayor a 0.");

            RuleFor(x => x.StartTime)
                .GreaterThan(_ => DateTime.Now)
                .WithMessage("La fecha y hora de inicio deben ser posteriores a la fecha y hora actual.")
                .LessThan(x => x.EndTime)
                .WithMessage("La fecha de inicio debe ser anterior a la fecha de fin.");

            RuleFor(x => x.EndTime)                
                .GreaterThan(x => x.StartTime)
                .WithMessage("La fecha de fin debe ser posterior a la fecha de inicio.");

            RuleFor(x => x.Offset)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Offset.HasValue)
                .WithMessage("El valor 'offset' debe ser un numero entero no negativo.");

            RuleFor(x => x.Size)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Size.HasValue)
                .WithMessage("El valor 'size' debe ser un numero entero no negativo.");

            When(x => x.Category.HasValue, () =>
                RuleFor(x => x.Category.Value)
                    .GreaterThan(0)
                    .WithMessage("El filtro de categoría, si se especifica, debe ser mayor a 0."));

            When(x => x.SeatingCapacity.HasValue, () =>
                RuleFor(x => x.SeatingCapacity.Value)
                    .GreaterThan(0)
                    .WithMessage("La capacidad, si se especifica, debe ser mayor a 0."));

            When(x => x.TransmissionType.HasValue, () =>
                RuleFor(x => x.TransmissionType.Value)
                    .GreaterThan(0)
                    .WithMessage("El tipo de transmisión, si se especifica, debe ser mayor a 0."));

            When(x => x.MaxPrice.HasValue, () =>
                RuleFor(x => x.MaxPrice.Value)
                    .GreaterThan(0)
                    .WithMessage("El precio máximo, si se especifica, debe ser mayor a 0."));

            When(x => !string.IsNullOrEmpty(x.Color), () =>
                RuleFor(x => x.Color)
                    .MaximumLength(50)
                    .WithMessage("El color no puede exceder 50 caracteres."));

            When(x => !string.IsNullOrEmpty(x.Brand), () =>
                RuleFor(x => x.Brand)
                    .MaximumLength(100)
                    .WithMessage("La marca no puede exceder 100 caracteres."));
        }
    }
}
