/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.MongoDB.DataAccess.Generic;
using MDD4All.SpecIF.DataModels.RightsManagement;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using System.Threading;
using System.Threading.Tasks;

namespace MDD4All.SpecIF.Microservice.RightsManagement
{
	public class SpecIfApiRoleStore : IRoleStore<ApplicationRole>
	{
		private const string SPECIF_ADMIN_DATABASE_NAME = "specifAdmin";

		private MongoDBDataAccessor<ApplicationRole> _roleMongoDbAccessor;

		public SpecIfApiRoleStore(string connectionString)
		{
			_roleMongoDbAccessor = new MongoDBDataAccessor<ApplicationRole>(connectionString, SPECIF_ADMIN_DATABASE_NAME);
		}

		public async Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
		{
			await Task.Run(() => _roleMongoDbAccessor.Add(role));

			return IdentityResult.Success;
		}

		public Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
		{
			return Task.FromResult(IdentityResult.Success);
		}

		public void Dispose()
		{
			
		}

		public async Task<ApplicationRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
		{
			return await Task.FromResult(_roleMongoDbAccessor.GetItemById(roleId));
		}

		public async Task<ApplicationRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
		{
			BsonDocument filter = new BsonDocument()
			{
				{  "normalizedName", normalizedRoleName }
			};

			return await Task.FromResult(_roleMongoDbAccessor.GetItemByFilter(filter.ToJson()));
		}

		public Task<string> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
		{
			return Task.FromResult(role.NormalizedName);
		}

		public Task<string> GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
		{
			return Task.FromResult(role.Id.ToString());
		}

		public Task<string> GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
		{
			return Task.FromResult(role.Name);
		}

		public Task SetNormalizedRoleNameAsync(ApplicationRole role, string normalizedName, CancellationToken cancellationToken)
		{
			role.NormalizedName = normalizedName;
			return Task.FromResult(0);
		}

		public Task SetRoleNameAsync(ApplicationRole role, string roleName, CancellationToken cancellationToken)
		{
			role.Name = roleName;
			return Task.FromResult(0);
		}

		public async Task<IdentityResult> UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
		{
			await Task.Run(() => _roleMongoDbAccessor.Update(role, role.Id));
			return IdentityResult.Success;
		}
	}
}
