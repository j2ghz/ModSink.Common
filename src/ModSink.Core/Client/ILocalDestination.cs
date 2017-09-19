using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ModSink.Core.Client
{
    public interface ILocalDestination
    {
        Action After { get; }
        Action Before { get; }
        Stream Stream { get; }
    }
}