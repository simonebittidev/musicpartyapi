//using EasyCaching.Core;
//using Microsoft.Extensions.Caching.Memory;
//using SpotifyAPIs.Entities;

//namespace SpotifyAPIs
//{
//    public sealed class CacheWorker : BackgroundService
//    {
//        private readonly ILogger<CacheWorker> _logger;

//        private readonly TimeSpan _updateInterval = TimeSpan.FromHours(3);

//        private bool _isCacheInitialized = false;



//        public CacheWorker(
//            ILogger<CacheWorker> logger,
//            HttpClient httpClient,
//            CacheSignal<Photo> cacheSignal,
//            IMemoryCache cache) 
//            {
//                _logger = logger;
//            }

//        public override async Task StartAsync(CancellationToken cancellationToken)
//        {
//            await cacheSignal.WaitAsync();
//            await base.StartAsync(cancellationToken);
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                logger.LogInformation("Updating cache.");

//                try
//                {
//                    Photo[]? photos =
//                        await httpClient.GetFromJsonAsync<Photo[]>(
//                            Url, stoppingToken);

//                    if (photos is { Length: > 0 })
//                    {
//                        cache.Set("Photos", photos);
//                        logger.LogInformation(
//                            "Cache updated with {Count:#,#} photos.", photos.Length);
//                    }
//                    else
//                    {
//                        logger.LogWarning(
//                            "Unable to fetch photos to update cache.");
//                    }
//                }
//                finally
//                {
//                    if (!_isCacheInitialized)
//                    {
//                        cacheSignal.Release();
//                        _isCacheInitialized = true;
//                    }
//                }

//                try
//                {
//                    logger.LogInformation(
//                        "Will attempt to update the cache in {Hours} hours from now.",
//                        _updateInterval.Hours);

//                    await Task.Delay(_updateInterval, stoppingToken);
//                }
//                catch (OperationCanceledException)
//                {
//                    logger.LogWarning("Cancellation acknowledged: shutting down.");
//                    break;
//                }
//            }
//        }
//    }
//}

