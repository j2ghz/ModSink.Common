using ModSink.Core.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ModSink.Common.Client
{
    internal class LocalDestination : ILocalDestination
    {
        private readonly Lazy<Stream> stream;

        public LocalDestination(Action before, Lazy<Stream> stream, Action after)
        {
            this.Before = before;
            this.stream = stream;
            this.After = after;
        }

        public Action After { get; }

        public Action Before { get; }

        public Stream Stream => this.stream.Value;
    }
}