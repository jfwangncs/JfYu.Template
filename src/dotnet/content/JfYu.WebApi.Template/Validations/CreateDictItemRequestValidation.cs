using FluentValidation;
using JfYu.WebApi.Template.Model.DictItem;

namespace JfYu.WebApi.Template.Validations
{
    public class CreateDictItemRequestValidation : AbstractValidator<CreateDictItemRequest>
    {
        public CreateDictItemRequestValidation()
        {
            RuleFor(x => x.DictTypeId).GreaterThan(0);
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Label).NotEmpty().MaximumLength(100);
        }
    }
}
