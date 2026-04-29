using FluentValidation;
using JfYu.WebApi.Template.Model.DictItem;

namespace JfYu.WebApi.Template.Validations
{
    public class UpdateDictItemRequestValidation : AbstractValidator<UpdateDictItemRequest>
    {
        public UpdateDictItemRequestValidation()
        {
            RuleFor(x => x.Label).MaximumLength(100);
        }
    }
}
