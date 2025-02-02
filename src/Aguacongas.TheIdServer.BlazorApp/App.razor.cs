﻿// Project: Aguafrommars/TheIdServer
// Copyright (c) 2023 @Olivier Lefebvre
using Aguacongas.TheIdServer.BlazorApp.Services;
using Microsoft.AspNetCore.Components.Routing;
using System.Reflection;

namespace Aguacongas.TheIdServer.BlazorApp
{
    public partial class App
    {
        private readonly List<Assembly> _lazyLoadedAssemblies = new List<Assembly>
        {
            typeof(Pages.Index).Assembly
        };

        private readonly string[] _pageKindList = new []
        {
            "Api",
            "ApiScope",
            "Client",
            "Culture",
            "ExternalProvider",
            "Identities",
            "Identity",
            "Import",
            "Key",
            "Role",
            "User",
            "RelyingParties",
            "RelyingParty",
            "Settings"
        };

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(false);
            await _themeService.InitAsync().ConfigureAwait(false);
        }

    private Task OnNavigateAsync(NavigationContext args)
        {
            var path = args.Path.Split("/")[0];
            if (path == "protectresource")
            {
                return LoadAssemblyAsync("Aguacongas.TheIdServer.BlazorApp.Pages.Api.dll");
            }

            if (path == "identityresource")
            {
                return LoadAssemblyAsync("Aguacongas.TheIdServer.BlazorApp.Pages.Identity.dll");
            }

            var pageKind = _pageKindList.FirstOrDefault(k => path == $"{k.ToLower()}s");
            if (pageKind != null)
            {
                return LoadAssemblyAsync($"Aguacongas.TheIdServer.BlazorApp.Pages.{pageKind}s.dll");
            }

            pageKind = _pageKindList.FirstOrDefault(k => path == k.ToLower());
            return pageKind != null ? LoadAssemblyAsync($"Aguacongas.TheIdServer.BlazorApp.Pages.{pageKind}.dll") : Task.CompletedTask;
        }

        private async Task LoadAssemblyAsync(string assemblyName)
        {
            _logger.LogDebug("LoadAssemblyAsync {AssemblyName}", assemblyName);
            var assemblies = await _assemblyLoader.LoadAssembliesAsync(
                new[] { assemblyName }).ConfigureAwait(false);
            _lazyLoadedAssemblies.AddRange(assemblies.Where(a => !_lazyLoadedAssemblies.Any(l => l.FullName == a.FullName)));
        }
    }
}
