using FluentValidation;
using WebApi.Model.Role;

namespace WebApi.Validations
{
    public class UpdateRoleRequestValidation : AbstractValidator<UpdateRoleRequest>
    {
        public UpdateRoleRequestValidation()
        {
            RuleFor(x => x.Name)
                .MaximumLength(50);

            RuleFor(x => x.Description)
                .MaximumLength(200);
        }
    }
}
