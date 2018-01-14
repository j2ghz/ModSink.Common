using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using DynamicData;
using ModSink.Core.Client;
using ModSink.Core.Models.Repo;
using ReactiveUI;

namespace ModSink.Common.Client
{
    public class ClientService : ReactiveObject, IClientService
    {
        private readonly SourceList<Repo> repos = new SourceList<Repo>();

        public ClientService(IDownloadService downloadService, ILocalStorageService localStorageService,
            IDownloader downloader, IFormatter serializationFormatter)
        {
            DownloadService = downloadService;
            LocalStorageService = localStorageService;
            Downloader = downloader;
            SerializationFormatter = serializationFormatter;
        }

        public IDownloader Downloader { get; }
        public IFormatter SerializationFormatter { get; }
        public IDownloadService DownloadService { get; }
        public ILocalStorageService LocalStorageService { get; }
        public IObservableList<Modpack> Modpacks => Repos.Connect().TransformMany(r => r.Modpacks).AsObservableList();
        public IObservableList<Repo> Repos => repos.AsObservableList();

        public async Task DownloadMissingFiles(Modpack modpack)
        {
            await Task.WhenAll(
                modpack.Mods
                    .SelectMany(m => m.Mod.Files)
                    .Select(fh => Task.Factory.StartNew(async () =>
                    {
                        if (!await LocalStorageService.IsFileAvailable(fh.Value))
                            DownloadService.Add(new Download(GetDownloadUri(fh.Value),
                                new Lazy<Task<Stream>>(async () => await LocalStorageService.Write(fh.Value)),
                                fh.Value.ToString()));
                    })));
        }

        public Uri GetDownloadUri(HashValue hash)
        {
            foreach (var repo in Repos.Items)
                if (repo.Files.TryGetValue(hash, out var relativeUri))
                    return new Uri(repo.BaseUri, relativeUri);
            throw new KeyNotFoundException($"Key {hash} was not found in a Files dictionary of any Repo");
        }

        public IObservable<DownloadProgress> LoadRepo(Uri uri)
        {
            var obs = Observable.Create<DownloadProgress>(async o =>
            {
                var tempFile = Path.GetTempFileName();
                var stream = new FileStream(tempFile, FileMode.Create);
                var progress = Downloader.Download(uri, stream, "Repo");
                progress.Subscribe(o.OnNext, o.OnError, () => { });
                await progress;
                stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read);
                var repo = (Repo) SerializationFormatter.Deserialize(stream);
                repo.BaseUri = new Uri(uri, ".");
                repos.Add(repo);
                o.OnCompleted();
            }).Publish();
            obs.Connect();
            return obs;
        }
    }
}