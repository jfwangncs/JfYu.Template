using FluentValidation;
using WebApi.Model.Permission;

namespace WebApi.Validations
{
    public class UpdatePermissionRequestValidation : AbstractValidator<UpdatePermissionRequest>
    {
        public UpdatePermissionRequestValidation()
        {
            RuleFor(x => x.Name).MaximumLength(50).When(x => x.Name != null);
            RuleFor(x => x.Description).MaximumLength(200).When(x => x.Description != null);
            RuleFor(x => x.Icon).MaximumLength(50).When(x => x.Icon != null);
        }
    }
}
