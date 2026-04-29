using FluentValidation;
using JfYu.WebApi.Template.Model.Role;

namespace JfYu.WebApi.Template.Validations
{
    public class CreateRoleRequestValidation : AbstractValidator<CreateRoleRequest>
    {
        public CreateRoleRequestValidation()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Description)
                .MaximumLength(200);
        }
    }
}
