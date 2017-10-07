﻿using ModSink.Core.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace ModSink.Common.Client
{
    public class Download : IDownload
    {
        private Subject<DownloadProgress> progress = new Subject<DownloadProgress>();

        public Download(Uri source, Lazy<Stream> destination, string name)
        {
            this.Source = source;
            this.Destination = destination;
            this.Name = name;
        }

        public Lazy<Stream> Destination { get; }
        public string Name { get; }
        public IObservable<DownloadProgress> Progress => progress;
        public Uri Source { get; }
        public DownloadState State { get; private set; } = DownloadState.Queued;

        public void Start(IDownloader downloader)
        {
            if (this.State != DownloadState.Queued) throw new Exception($"State must be {DownloadState.Queued} to start a download");
            this.State = DownloadState.Downloading;
            downloader.Download(this).Subscribe(progress);
            this.Progress.Subscribe(_ => { }, _ => this.State = DownloadState.Errored, () => this.State = DownloadState.Finished);
        }
    }
}