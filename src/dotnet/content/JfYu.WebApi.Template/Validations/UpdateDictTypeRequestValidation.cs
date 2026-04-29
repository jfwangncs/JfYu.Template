using FluentValidation;
using JfYu.WebApi.Template.Model.DictType;

namespace JfYu.WebApi.Template.Validations
{
    public class UpdateDictTypeRequestValidation : AbstractValidator<UpdateDictTypeRequest>
    {
        public UpdateDictTypeRequestValidation()
        {
            RuleFor(x => x.Name).MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(500);
        }
    }
}
