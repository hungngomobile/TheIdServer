﻿using Aguacongas.IdentityServer.Store;
using Aguacongas.IdentityServer.Store.Entity;
using Aguacongas.TheIdServer.BlazorApp.Infrastructure.Services;
using Aguacongas.TheIdServer.BlazorApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.TheIdServer.BlazorApp.Pages
{
    [Authorize(Policy = "Is4-Reader")]
    public abstract class EntitiesModel<T> : ComponentBase, IDisposable where T: class
    {
        private PageRequest _pageRequest;
        private CancellationTokenSource _cancellationTokenSource;

        [Inject]
        protected IAdminStore<T> AdminStore { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected IStringLocalizerAsync<EntitiesModel<T>> Localizer { get; set; }

        protected IEnumerable<T> EntityList { get; private set; }

        public GridState GridState { get; } = new GridState();

        protected abstract string SelectProperties { get; }

        protected virtual string Expand { get; }

        protected override async Task OnInitializedAsync()
        {
            Localizer.OnResourceReady = () => InvokeAsync(StateHasChanged);
            await base.OnInitializedAsync().ConfigureAwait(false);
            _pageRequest = new PageRequest
            {
                Select = SelectProperties,
                Expand = Expand,
                Take = 10
            };
            await GetEntityList(_pageRequest)
                .ConfigureAwait(false);

            GridState.OnHeaderClicked += async e =>
            {
                _pageRequest.OrderBy = e.OrderBy;
                await GetEntityList(_pageRequest)
                    .ConfigureAwait(false);
                StateHasChanged();
            };
        }

        protected Task OnFilterChanged(string filter)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
                       
            return Task.Delay(500, token)
                .ContinueWith(async task =>
                {
                    if (task.IsCanceled)
                    {
                        return;
                    }

                    var propertyArray = SelectProperties.Split(',');
                    var expressionArray = new string[propertyArray.Length];
                    for (int i = 0; i < propertyArray.Length; i++)
                    {
                        expressionArray[i] = $"contains({propertyArray[i]},'{filter.Replace("'", "''")}')";
                    }
                    _pageRequest.Filter = string.Join(" or ", expressionArray);

                    var page = await AdminStore.GetAsync(_pageRequest, token)
                                .ConfigureAwait(false);

                    EntityList = page.Items;

                    await InvokeAsync(() => StateHasChanged())
                        .ConfigureAwait(false);
                }, TaskScheduler.Default);
        }

        [SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "Url")]
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Never null")]
        protected virtual void OnRowClicked(T entity)
        {
            if (!(entity is IEntityId entityWithId))
            {
                throw new InvalidOperationException($"The identity type {typeof(T).Name} is not a 'IEntityId', override this method to navigate to the identity page.");
            }
            NavigationManager.NavigateTo($"{typeof(T).Name.ToLower()}/{entityWithId.Id}");
        }


        protected virtual string LocalizeEntityProperty<TEntityResource>(ILocalizable<TEntityResource> entity, string value, EntityResourceKind kind) where TEntityResource: IEntityResource
        {
            return entity.Resources.FirstOrDefault(r => r.ResourceKind == kind && r.CultureId == CultureInfo.CurrentCulture.Name)?.Value ?? value;
        }

        private async Task GetEntityList(PageRequest pageRequest)
        {
            var page = await AdminStore.GetAsync(pageRequest)
                            .ConfigureAwait(false);
            EntityList = page.Items;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _cancellationTokenSource?.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}