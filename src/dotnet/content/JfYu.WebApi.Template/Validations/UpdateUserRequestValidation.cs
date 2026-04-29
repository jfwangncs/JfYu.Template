using FluentValidation;
using JfYu.WebApi.Template.Model.Request;

namespace JfYu.WebApi.Template.Validations
{
    public class UpdateUserRequestValidation : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidation()
        {
            RuleFor(x => x.NickName)
                .MaximumLength(50);

            RuleFor(x => x.RealName)
                .MaximumLength(50);

            RuleFor(x => x.Phone)
                .MaximumLength(50);
        }
    }
}
