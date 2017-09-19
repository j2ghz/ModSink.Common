using ModSink.Core.Client;
using System;
using System.Collections.Generic;
using System.Text;
using ModSink.Core.Models.Repo;
using System.IO;
using System.Reactive.Disposables;

namespace ModSink.Common.Client
{
    public class LocalRepoManager : ILocalRepoManager
    {
        private readonly DirectoryInfo localDir;
        private readonly Uri localPath;

        public LocalRepoManager(Uri localPath)
        {
            this.localPath = localPath;
            this.localDir = new DirectoryInfo(localPath.LocalPath);
        }

        public void Delete(HashValue hash)
        {
            GetFileInfo(hash).Delete();
        }

        public Uri GetFileUri(HashValue hash, bool temp = false)
        {
            return new Uri(this.localPath, hash.ToString() + (temp ? ".tmp" : ""));
        }

        public bool IsFileAvailable(HashValue hash)
        {
            return GetFileInfo(hash).Exists;
        }

        public Stream Read(HashValue hash)
        {
            var uri = GetFileUri(hash);
            var file = new FileInfo(uri.LocalPath);
            return file.Open(FileMode.Open, FileAccess.Read);
        }

        public ILocalDestination Write(HashValue hash)
        {
            var tempUri = GetFileUri(hash, true);
            var uri = GetFileUri(hash);
            var file = new FileInfo(tempUri.LocalPath);
            var after = new Action(() =>
            {
                File.Move(tempUri.LocalPath, uri.LocalPath);
            });
            var stream = new Lazy<Stream>(() => file.Open(FileMode.Create, FileAccess.Write));
            return new LocalDestination(() => { }, stream, after);
        }

        private FileInfo GetFileInfo(HashValue hash)
        {
            return new FileInfo(GetFileUri(hash).LocalPath);
        }
    }
}