﻿using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Humanizer.Bytes;
using ModSink.Common.Client;
using ModSink.Core.Client;

namespace Modsink.Common.Tests
{
    internal class MockDownloader : IDownloader
    {
        public IObservable<DownloadProgress> Download(Uri source, Stream destination, string name)
        {
            return Download(new Download(source, new Lazy<Task<Stream>>(() => Task.Run(() => destination)), name));
        }


        public IObservable<DownloadProgress> Download(IDownload download)
        {
            var progress = Observable.Create<DownloadProgress>(observer =>
            {
                //Get response
                observer.OnNext(new DownloadProgress(
                    ByteSize.FromBytes(0),
                    ByteSize.FromBytes(0),
                    DownloadProgress.TransferState.AwaitingResponse));

                //Read response
                observer.OnNext(new DownloadProgress(
                    ByteSize.FromBytes(0),
                    ByteSize.FromBytes(0),
                    DownloadProgress.TransferState.ReadingResponse));

                observer.OnNext(new DownloadProgress(
                    ByteSize.FromBytes(1),
                    ByteSize.FromBytes(0),
                    DownloadProgress.TransferState.ReadingResponse));

                //Download
                observer.OnNext(new DownloadProgress(
                    ByteSize.FromBytes(1),
                    ByteSize.FromBytes(0),
                    DownloadProgress.TransferState.Downloading));

                //Finish
                observer.OnNext(new DownloadProgress(
                    ByteSize.FromBytes(1),
                    ByteSize.FromBytes(1),
                    DownloadProgress.TransferState.Finished));

                observer.OnCompleted();
                return Disposable.Empty;
            }).Publish();

            progress.Connect();
            return progress;
        }
    }
}