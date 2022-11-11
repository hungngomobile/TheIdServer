﻿// Project: Aguafrommars/TheIdServer
// Copyright (c) 2022 @Olivier Lefebvre
using Aguacongas.IdentityServer.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using entity = Aguacongas.IdentityServer.Store.Entity;

namespace Aguacongas.TheIdServer.BlazorApp.Pages.User
{
    public partial class User
    {
        protected override string Expand => $"{nameof(entity.User.UserClaims)},{nameof(entity.User.UserRoles)}";

        protected override bool NonEditable => false;

        protected override string BackUrl => "users";

        protected override Task<Models.User> Create()
        {
            return Task.FromResult(new Models.User
            {
                Id = Guid.NewGuid().ToString(),
                Claims = new List<EntityNS.UserClaim>(),
                Consents = new List<EntityNS.UserConsent>(),
                Logins = new List<EntityNS.UserLogin>(),
                Roles = new List<EntityNS.Role>(),
                Tokens = new List<EntityNS.UserToken>(),
                ReferenceTokens = new List<EntityNS.ReferenceToken>(),
                RefreshTokens = new List<EntityNS.RefreshToken>(),
                BackChannelAuthenticationRequests = new List<EntityNS.BackChannelAuthenticationRequest>(),
                Sessions = new List<EntityNS.UserSession>()
            });
        }

        protected override void RemoveNavigationProperty<TEntity>(TEntity entity)
        {
            if (entity is entity.UserClaim claim)
            {
                claim.UserId = Model.Id;
            }
        }

        protected override void SanetizeEntityToSaved<TEntity>(TEntity entity)
        {
            // no navigation property
        }

        protected override async Task<Models.User> GetModelAsync()
        {
            var pageRequest = new PageRequest
            {
                Filter = $"{nameof(entity.UserClaim.UserId)} eq '{Id}'"
            };

            var model = await base.GetModelAsync();

            var userLoginStore = GetStore<entity.UserLogin>();
            var getLoginsResponse = await userLoginStore.GetAsync(pageRequest);

            var userConsentStore = GetStore<entity.UserConsent>();
            var getUserConsentsResponse = await userConsentStore.GetAsync(pageRequest);
            
            var userTokenStore = GetStore<entity.UserToken>();
            var getUserTokensResponse = await userTokenStore.GetAsync(pageRequest);

            var referenceTokenStore = GetStore<entity.ReferenceToken>();
            var getReferenceTokenResponse = await referenceTokenStore.GetAsync(pageRequest);

            var refreshTokenStore = GetStore<entity.RefreshToken>();
            var getRefreshTokenResponse = await refreshTokenStore.GetAsync(pageRequest);

            var backChannelAuthenticationRequestStore = GetStore<entity.BackChannelAuthenticationRequest>();
            var getBackChannelAuthenticationRequestResponse = await backChannelAuthenticationRequestStore.GetAsync(pageRequest);

            var sessionStore = GetStore<entity.UserSession>();
            var sessionstResponse = await sessionStore.GetAsync(pageRequest);

            model.Logins = getLoginsResponse.Items.ToList();
            model.Consents = getUserConsentsResponse.Items.ToList();
            model.Tokens = getUserTokensResponse.Items.ToList();
            model.ReferenceTokens = getReferenceTokenResponse.Items.ToList();
            model.RefreshTokens = getRefreshTokenResponse.Items.ToList();
            model.BackChannelAuthenticationRequests = getBackChannelAuthenticationRequestResponse.Items.ToList();
            model.Sessions = sessionstResponse.Items.ToList();

            var userRoles = model.UserRoles;
            if (userRoles.Any())
            {
                var roleStore = GetStore<entity.Role>();
                var rolesResponse = await roleStore.GetAsync(new PageRequest
                {
                    Filter = string.Join(" or ", userRoles.Select(r => $"{nameof(entity.Role.Id)} eq '{r.RoleId}'"))
                }).ConfigureAwait(false);
                model.Roles = rolesResponse.Items.ToList();
            }
            else
            {
                model.Roles = new List<entity.Role>();
            }

            return model;
        }

        protected async override Task<object> CreateAsync(Type entityType, object entity)
        {
            if (entity is entity.Role role)
            {
                var roleStore = GetStore<entity.Role>();
                var roleResponse = await roleStore.GetAsync(new PageRequest
                {
                    Select = "Id",
                    Take = 1,
                    Filter = $"{nameof(role.Name)} eq '{role.Name}'"
                }).ConfigureAwait(false);

                var roles = roleResponse.Items;
                if (roles.Any())
                {
                    await base.CreateAsync(typeof(entity.UserRole), new entity.UserRole
                    {
                        RoleId = roles.First().Id,
                        UserId = Model.Id
                    }).ConfigureAwait(false);
                }
                return role;
            }
            return await base.CreateAsync(entityType, entity).ConfigureAwait(false);
        }

        protected override Task<object> DeleteAsync(Type entityType, object entity)
        {
            if (entity is entity.Role role)
            {
                return base.DeleteAsync(typeof(entity.UserRole), Model.UserRoles.First(r => r.RoleId == role.Id));
            }
            return base.DeleteAsync(entityType, entity);
        }

        protected override void OnCloning()
        {
            Model.Id = Guid.NewGuid().ToString();
            Model.UserName = Localizer["Clone of {0}", Model.UserName];
        }

        protected override string GetNotiticationHeader() => Model.UserName;

        private static EntityNS.UserClaim CreateClaim()
            => new()
            {
                Issuer = ClaimsIdentity.DefaultIssuer
            };
    }
}
