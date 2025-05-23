using Application.Dtos.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class GetReservationsRequestValidator : AbstractValidator<GetReservationsRequest>
    {
        public GetReservationsRequestValidator()
        {
            RuleFor(x => x.Offset)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Offset.HasValue)
                .WithMessage("Offset debe ser un número entero >= 0.");

            RuleFor(x => x.Size)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Size.HasValue)
                .WithMessage("Size debe ser un número entero >= 0.");

            When(x => x.From.HasValue && x.To.HasValue, () =>
                RuleFor(x => x.From.Value)
                    .LessThan(x => x.To.Value)
                    .WithMessage("'From' debe ser anterior a 'To'."));
        }
    }
}
