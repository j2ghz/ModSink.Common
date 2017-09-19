﻿using ModSink.Common;
using ModSink.Common.Client;
using ModSink.Core.Client;
using ModSink.Core.Models.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Modsink.Common.Tests.Client
{
    public class LocalRepoManagerTest
    {
        private ILocalRepoManager manager = new LocalRepoManager(new Uri(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())));

        [Fact]
        public void WriteReadAndDelete()
        {
            var hash = new XXHash64().HashOfEmpty;
            this.manager.IsFileAvailable(hash).Should().Be(false);
            Assert.Throws<FileNotFoundException>(() => this.manager.Read(hash));
            var dest = this.manager.Write(hash);
            dest.Before();
            using (var stream = this.manager.Write(hash).Stream)
            {
                stream.WriteByte(0xff);
            }
            dest.After();
            this.manager.IsFileAvailable(hash).Should().Be(true);
            using (var stream = this.manager.Read(hash))
            {
                stream.ReadByte().Should().Be(0xff);
                stream.ReadByte().Should().Be(-1);
            }
            this.manager.Delete(hash);
            this.manager.IsFileAvailable(hash).Should().Be(false);
            Assert.Throws<FileNotFoundException>(() => this.manager.Read(hash));
        }
    }
}