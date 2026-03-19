using FluentValidation;
using WebApi.Model.Role;

namespace WebApi.Validations
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
