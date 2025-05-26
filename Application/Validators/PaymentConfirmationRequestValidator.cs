using Application.Dtos.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class PaymentConfirmationRequestValidator : AbstractValidator<PaymentConfirmationRequest>
    {
        public PaymentConfirmationRequestValidator()
        {
            // TotalAmount obligatorio y > 0
            RuleFor(x => x.TotalAmount)
                .GreaterThan(0m)
                .WithMessage("El monto total debe ser mayor que 0.");

            // LateFee ≥ 0 y ≤ TotalAmount
            RuleFor(x => x.LateFee)
                .GreaterThanOrEqualTo(0m)
                .WithMessage("La multa no puede ser negativa.")
                .LessThanOrEqualTo(x => x.TotalAmount)
                .WithMessage("La multa no puede exceder el monto total.");

            // PaymentGateway obligatorio, no vacío
            RuleFor(x => x.PaymentGateway)
                .NotEmpty()
                .WithMessage("El nombre de la pasarela de pago es obligatorio.")
                .MaximumLength(50)
                .WithMessage("El nombre de la pasarela no puede exceder 50 caracteres.");

            // TransactionId obligatorio, no vacío
            RuleFor(x => x.TransactionId)
                .NotEmpty()
                .WithMessage("El identificador de transacción es obligatorio.")
                .MaximumLength(100)
                .WithMessage("El identificador de transacción no puede exceder 100 caracteres.");
        }
    }
}
