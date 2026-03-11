using FluentValidation;
using WebApi.Model.Request;

namespace WebApi.Validations
{
    public class CreateUserRequestValidation : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidation()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Password)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.NickName)
                .MaximumLength(50);

            RuleFor(x => x.RealName)
                .MaximumLength(50);

            RuleFor(x => x.Phone)
                .MaximumLength(50);
        }
    }
}
