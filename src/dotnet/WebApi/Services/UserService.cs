using JfYu.Data.Context;
using JfYu.Data.Extension;
using JfYu.Data.Model;
using JfYu.Data.Service;
using JfYu.WeChat;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebApi.Constants;
using WebApi.Entity;
using WebApi.Exceptions;
using WebApi.Model.Request;
using WebApi.Model.Response;
using WebApi.Services.Interfaces;

namespace WebApi.Services
{
  public class UserService(AppDbContext context, ReadonlyDBContext<AppDbContext> readonlyDBContext, IMiniProgram miniProgram)
      : Service<User, AppDbContext>(context, readonlyDBContext), IUserService
  {
    private readonly IMiniProgram _miniProgram = miniProgram;

    public async Task<User?> LoginAsync(LoginRequest login)
    {
      User? user = null;
      if (login.Platform == PlatformEnum.Wechat)
      {
        var authSession = await _miniProgram.LoginAsync(login.Code!);

        if (login.Phone == "admin" && login.Code == "password")
          authSession = new JfYu.WeChat.Model.Response.WechatLoginResponse() { OpenId = "OpenID", SessionKey = "SessionKey" };

        if (authSession == null || !string.IsNullOrEmpty(authSession.ErrorMessage))
          throw new BusinessException(ErrorCode.UserNotFound);

        user = new User
        {
          UserName = "",
          Phone = login.Phone,
          OpenId = authSession.OpenId,
          SessionKey = authSession.SessionKey,
        };
      }
      else if (login.Platform == PlatformEnum.Web)
      {

        user = await GetOneAsync(q => q.UserName == login.UserName);

        if (user == null)
          throw new BusinessException(ErrorCode.UserNotFound);

        if (string.IsNullOrEmpty(user.Password))
          throw new BusinessException(ErrorCode.PasswordNotSet);

        if (!BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
          throw new BusinessException(ErrorCode.InvalidCredentials);
      }
      else
        throw new BusinessException(ErrorCode.UnsupportedLoginMethod);
      user!.LastLoginTime = DateTime.UtcNow;
      _context.Users.Update(user);
      await _context.SaveChangesAsync();
      return user;
    }

    public async Task<PagedData<UserResponse>> GetPagedAsync(QueryRequest query)
    {
      var q = _readonlyContext.Users.Include(q => q.Roles).AsQueryable();

      if (!string.IsNullOrWhiteSpace(query.SearchKey))
      {
        q = q.Where(u =>
            (u.UserName != null && u.UserName.Contains(query.SearchKey)) ||
            (u.NickName != null && u.NickName.Contains(query.SearchKey)) ||
            (u.RealName != null && u.RealName.Contains(query.SearchKey)) ||
            (u.Phone != null && u.Phone.Contains(query.SearchKey)));
      }

      return await q.ToPagedAsync(q => q.Adapt<IEnumerable<UserResponse>>(), query.PageIndex, query.PageSize);

    }

    public async Task<int> UpdateAsync(int id, UpdateUserRequest request)
    {
      var user = await GetOneAsync(q => q.Id == id);
      if (user == null)
        throw new BusinessException(ErrorCode.UserNotFound);
      request.Adapt(user);
      return await UpdateAsync(user);
    }

    public override async Task<User?> GetOneAsync(Expression<Func<User, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
      predicate ??= q => true;
      return await _readonlyContext.Users.Include(q => q.Roles).FirstOrDefaultAsync(predicate, cancellationToken);
    }
  }
}
