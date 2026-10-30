using ApiSDD.Dtos;
using FluentValidation;

namespace ApiSDD.Validators;

/// <summary>nv-02: validator espejo de <c>PaginationQuery</c> (frontera → 422 vía middleware).</summary>
internal sealed class PaginationQueryValidator : AbstractValidator<PaginationQuery>
{
    public PaginationQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
