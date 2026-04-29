using FluentValidation;
using JfYu.WebApi.Template.Model.Permission;

namespace JfYu.WebApi.Template.Validations
{
    public class UpdatePermissionRequestValidation : AbstractValidator<UpdatePermissionRequest>
    {
        public UpdatePermissionRequestValidation()
        {
            RuleFor(x => x.Name)
                .MaximumLength(50);

            RuleFor(x => x.Description)
                .MaximumLength(200);

            RuleFor(x => x.Icon)
                .MaximumLength(50);
        }
    }
}
