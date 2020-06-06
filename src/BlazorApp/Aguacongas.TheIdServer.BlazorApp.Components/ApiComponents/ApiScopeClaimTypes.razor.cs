﻿using Aguacongas.TheIdServer.BlazorApp.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Entity = Aguacongas.IdentityServer.Store.Entity;

namespace Aguacongas.TheIdServer.BlazorApp.Components.ApiComponents
{
    public partial class ApiScopeClaimTypes
    {
        private IEnumerable<Entity.ApiScopeClaim> Claims => Model.ApiScopeClaims.Where(c => c.Type != null && c.Type.Contains(HandleModificationState.FilterTerm));

        private Entity.ApiScopeClaim _claim = new Entity.ApiScopeClaim();

        [Parameter]
        public Entity.ApiScope Model { get; set; }

        [CascadingParameter]
        public HandleModificationState HandleModificationState { get; set; }

        protected override void OnInitialized()
        {
            _claim.ApiScpope = Model;
            HandleModificationState.OnFilterChange += HandleModificationState_OnFilterChange;
            HandleModificationState.OnStateChange += HandleModificationState_OnStateChange;
            base.OnInitialized();
        }

        private void HandleModificationState_OnStateChange(ModificationKind kind, object entity)
        {
            if (entity is Entity.ApiScopeClaim)
            {
                StateHasChanged();
            }
        }

        private void HandleModificationState_OnFilterChange(string obj)
        {
            StateHasChanged();
        }

        private void OnDeleteClaimClicked(Entity.ApiScopeClaim claim)
        {
            Model.ApiScopeClaims.Remove(claim);
            HandleModificationState.EntityDeleted(claim);
        }

        private void OnClaimValueChanged(Entity.ApiScopeClaim claim)
        {
            Model.ApiScopeClaims.Add(claim);
            _claim = new Entity.ApiScopeClaim { ApiScpope = Model };
            claim.Id = Guid.NewGuid().ToString();
            HandleModificationState.EntityCreated(claim);
        }
    }
}