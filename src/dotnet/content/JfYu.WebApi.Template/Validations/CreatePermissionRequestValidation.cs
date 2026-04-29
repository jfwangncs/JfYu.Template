using FluentValidation;
using JfYu.WebApi.Template.Model.Permission;

namespace JfYu.WebApi.Template.Validations
{
    public class CreatePermissionRequestValidation : AbstractValidator<CreatePermissionRequest>
    {
        public CreatePermissionRequestValidation()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(200);

            RuleFor(x => x.Icon)
                .MaximumLength(50);
        }
    }
}
