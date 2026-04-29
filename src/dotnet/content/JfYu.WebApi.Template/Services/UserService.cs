using JfYu.Data.Context;
using JfYu.Data.Extension;
using JfYu.Data.Model;
using JfYu.Data.Service;
//#if (EnableWeChat)
using JfYu.WeChat;
//#endif
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using JfYu.WebApi.Template.Constants;
using JfYu.WebApi.Template.Entity;
using JfYu.WebApi.Template.Exceptions;
using JfYu.WebApi.Template.Model;
using JfYu.WebApi.Template.Model.Request;
using JfYu.WebApi.Template.Model.User;
using JfYu.WebApi.Template.Services.Interfaces;
//#if (EnableJWTRedis)
using JfYu.Redis.Interface;
using JfYu.WebApi.Template.Options;
using Microsoft.Extensions.Options;
//#endif

namespace JfYu.WebApi.Template.Services
{
    public class UserService(AppDbContext context, ReadonlyDBContext<AppDbContext> readonlyDBContext
        //#if (EnableWeChat)
        , IMiniProgram miniProgram
        //#endif
        //#if (EnableJWTRedis)
        , IRedisService redisService
        , IOptions<JwtSettings> jwtSettings
        , ILogger<UserService> logger
        //#endif
        )
        : Service<User, AppDbContext>(context, readonlyDBContext), IUserService
    {
        //#if (EnableWeChat)
        private readonly IMiniProgram _miniProgram = miniProgram;
        //#endif
        //#if (EnableJWTRedis)
        private readonly IRedisService _redisService = redisService;
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;
        private readonly ILogger<UserService> _logger = logger;
        //#endif

        public async Task<User?> LoginAsync(LoginRequest login)
        {
            var user = login.Platform switch
            {
                //#if (EnableWeChat)
                Platform.Wechat => await LoginWechatAsync(login),
                //#endif
                Platform.Web => await LoginWebAsync(login),
                _ => throw new BusinessException(ErrorCode.UnsupportedLoginMethod)
            };

            user.LastLoginTime = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        //#if (EnableWeChat)
        private async Task<User> LoginWechatAsync(LoginRequest login)
        {
            var authSession = await _miniProgram.LoginAsync(login.Code!);

            if (authSession == null || !string.IsNullOrEmpty(authSession.ErrorMessage))
                throw new BusinessException(ErrorCode.UserNotFound);

            return new User
            {
                UserName = "",
                Phone = login.Phone,
                OpenId = authSession.OpenId,
                SessionKey = authSession.SessionKey,
            };
        }
        //#endif

        private async Task<User> LoginWebAsync(LoginRequest login)
        {
            var user = await GetOneAsync(q => q.UserName == login.UserName)
                ?? throw new BusinessException(ErrorCode.UserNotFound);

            if (user.Status != (int)DataStatus.Active)
                throw new BusinessException(ErrorCode.AccountDisabled);

            if (string.IsNullOrEmpty(user.Password))
                throw new BusinessException(ErrorCode.PasswordNotSet);

            if (!BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
                throw new BusinessException(ErrorCode.InvalidCredentials);

            return user;
        }

        public async Task<PagedData<User>> GetPagedAsync(QueryRequest query)
        {
            var q = _readonlyContext.Users.Include(q => q.Roles).ThenInclude(q => q.Permissions).AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchKey))
            {
                q = q.Where(u =>
                    (u.UserName != null && u.UserName.Contains(query.SearchKey)) ||
                    (u.NickName != null && u.NickName.Contains(query.SearchKey)) ||
                    (u.RealName != null && u.RealName.Contains(query.SearchKey)) ||
                    (u.Phone != null && u.Phone.Contains(query.SearchKey)));
            }

            if (query.Status.HasValue)
                q = q.Where(u => u.Status == query.Status.Value);

            if (query.StartTime.HasValue)
                q = q.Where(u => u.CreatedTime >= query.StartTime.Value);

            if (query.EndTime.HasValue)
                q = q.Where(u => u.CreatedTime <= query.EndTime.Value);

            return await q.ToPagedAsync(query.PageIndex, query.PageSize);

        }

        public override async Task<User?> GetOneAsync(Expression<Func<User, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            predicate ??= q => true;
            return await _readonlyContext.Users.Include(q => q.Roles).ThenInclude(q => q.Permissions).FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<bool> UpdateAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            var tracked = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
                ?? throw new BusinessException(ErrorCode.UserNotFound);

            request.Adapt(tracked);

            await SyncRolesAsync(tracked, request.RoleIds, cancellationToken);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            //#if (EnableJWTRedis)
            await SyncJwtCacheAsync(userId, request);
            //#endif

            return result;
        }

        private async Task SyncRolesAsync(User tracked, List<int>? roleIds, CancellationToken cancellationToken)
        {
            if (roleIds == null)
                return;

            tracked.Roles.Clear();
            if (roleIds.Count == 0)
                return;

            var roles = await _context.Roles
                .Where(r => roleIds.Contains(r.Id))
                .ToListAsync(cancellationToken);
            foreach (var r in roles)
                tracked.Roles.Add(r);
        }

        //#if (EnableJWTRedis)
        private async Task SyncJwtCacheAsync(int userId, UpdateUserRequest request)
        {
            // Invalidate permission cache when roles change
            if (request.RoleIds != null)
                await _redisService.RemoveAsync(string.Format(RedisKey.UserPermission, userId));

            if (!request.Status.HasValue)
                return;

            var blacklistKey = string.Format(RedisKey.UserBlacklist, userId);
            if (request.Status.Value != (int)DataStatus.Active)
                await TrySetBlacklistAsync(userId, blacklistKey);
            else
                await TryRemoveBlacklistAsync(userId, blacklistKey);
        }

        private async Task TrySetBlacklistAsync(int userId, string blacklistKey)
        {
            try
            {
                await _redisService.Database.StringSetAsync(blacklistKey, "1", TimeSpan.FromMinutes(_jwtSettings.Expires));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set JWT blacklist for user {UserId}", userId);
            }
        }

        private async Task TryRemoveBlacklistAsync(int userId, string blacklistKey)
        {
            try
            {
                await _redisService.RemoveAsync(blacklistKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove JWT blacklist for user {UserId}", userId);
            }
        }
        //#endif

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null)
                throw new BusinessException(ErrorCode.UserNotFound);

            if (string.IsNullOrEmpty(user.Password) || !BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password))
                throw new BusinessException(ErrorCode.InvalidOldPassword);

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }

        public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null)
                throw new BusinessException(ErrorCode.UserNotFound);

            if (request.NickName != null) user.NickName = request.NickName;
            if (request.RealName != null) user.RealName = request.RealName;
            if (request.Phone != null) user.Phone = request.Phone;
            if (request.Avatar != null) user.Avatar = request.Avatar;
            if (request.Gender.HasValue) user.Gender = request.Gender.Value;
            if (request.Province != null) user.Province = request.Province;
            if (request.City != null) user.City = request.City;
            if (request.Country != null) user.Country = request.Country;

            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
