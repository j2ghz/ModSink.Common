﻿using Microsoft.Reactive.Testing;
using ModSink.Common;
using ModSink.Core;
using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using Xunit;
using FluentAssertions;

namespace Modsink.Common.Tests
{
    public class HashingTest
    {
        [Fact]
        public async System.Threading.Tasks.Task GetHashOfEmptyAsync()
        {
            var hashF = new XXHash64();
            var hashing = new Hashing(hashF);
            var stream = new MemoryStream(new byte[] { });
            var hash = await hashing.GetFileHash(stream, CancellationToken.None);
            hash.ShouldBeEquivalentTo(hashF.HashOfEmpty);
        }
    }
}