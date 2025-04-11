// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MudBlazor.Docs.Models;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace MudBlazor.Docs.Services
{
#nullable enable
    public class NugetApiClient
    {
        private readonly SourceRepository _repository = Repository.Factory.GetCoreV3(NuGetConstants.V3FeedUrl, FeedType.HttpV3);
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private PackageSearchResource? _packageSearch;

        public async Task<NugetPackage?> GetPackageAsync(string packageName)
        {
            try
            {
                var search = await GetPackageSearchResourceAsync();
                var result = await search.SearchAsync($"packageid:{packageName}", new SearchFilter(includePrerelease: false), skip: 0, take: 1, NullLogger.Instance, CancellationToken.None);
                return new NugetPackage { TotalDownloads = result.FirstOrDefault()?.DownloadCount };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private async Task<PackageSearchResource> GetPackageSearchResourceAsync()
        {
            if (_packageSearch != null)
            {
                return _packageSearch;
            }

            try
            {
                await _semaphore.WaitAsync();
                _packageSearch = await _repository.GetResourceAsync<PackageSearchResource>();
            }
            finally
            {
                _semaphore.Release();
            }

            return _packageSearch;
        }
    }
}
