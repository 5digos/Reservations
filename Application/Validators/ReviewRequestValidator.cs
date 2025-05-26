using Application.Dtos.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class ReviewRequestValidator : AbstractValidator<ReviewRequest>
    {
        public ReviewRequestValidator()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("La calificación debe estar entre 1 y 5.");
            RuleFor(x => x.Comment)
                .MaximumLength(500)
                .WithMessage("El comentario no puede exceder 500 caracteres.");
        }
    }
}
