using FluentValidation;
using JfYu.WebApi.Template.Model.Role;

namespace JfYu.WebApi.Template.Validations
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
