using MDD4All.MongoDB.DataAccess.Generic;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using MDD4All.SpecIF.DataModels.RightsManagement;

namespace MDD4All.SpecIf.Microservice.RightsManagement
{
	public class SpecIfApiUserStore : IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>, IUserRoleStore<ApplicationUser>
	{
		private const string SPECIF_ADMIN_DATABASE_NAME = "specifAdmin";

		private MongoDBDataAccessor<ApplicationUser> _userMongoDbAccessor;

		public SpecIfApiUserStore(string connectionString)
		{
			_userMongoDbAccessor = new MongoDBDataAccessor<ApplicationUser>(connectionString, SPECIF_ADMIN_DATABASE_NAME);
		}

		public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
		{
			user.Roles.Add(roleName);
			await Task.Run(() => _userMongoDbAccessor.Update(user, user.Id));
		}

		public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
		{
			await Task.Run(() => _userMongoDbAccessor.Add(user));

			return IdentityResult.Success;
		}

		public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
		{
            IdentityResult result = IdentityResult.Success;

            bool deleteResult = _userMongoDbAccessor.Delete(user.Id);

            if(!deleteResult)
            {
                result = IdentityResult.Failed();
            }

            return Task.FromResult(result);
		}

		public void Dispose()
		{
			
		}

		public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
		{
			return await Task.FromResult(_userMongoDbAccessor.GetItemById(userId));
		}

		public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
		{
			BsonDocument filter = new BsonDocument()
			{
				{  "normalizedUserName", normalizedUserName }
			};

			return await Task.FromResult(_userMongoDbAccessor.GetItemByFilter(filter.ToJson()));
		}

        public async Task<List<ApplicationUser>> GetAllUsers()
        {
            return await Task.FromResult(_userMongoDbAccessor.GetItems().ToList());
        }

		public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.NormalizedUserName);
		}

		public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.PasswordHash);
		}

		public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        { 
			return await Task.FromResult(user.Roles);
		}

		public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.Id.ToString());
		}

		public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.UserName);
		}

		public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
		{
			return await Task.FromResult(_userMongoDbAccessor.GetItems().Where(user => user.Roles.Contains(roleName)).ToList());
		}

		public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.PasswordHash != null);
		}

		public Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.Roles.Contains(roleName));
		}

		public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
		{
			user.Roles.Remove(roleName);
			await Task.Run(() => _userMongoDbAccessor.Update(user, user.Id));

		}

		public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
		{
			user.NormalizedUserName = normalizedName;
			return Task.FromResult(0);
		}

		public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
		{
			user.PasswordHash = passwordHash;
			return Task.FromResult(0);
		}

		public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
		{
			user.UserName = userName;
			return Task.FromResult(0);
		}

		public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
		{
			await Task.Run(() => _userMongoDbAccessor.Update(user, user.Id));
			return IdentityResult.Success;
		}
	}
}
