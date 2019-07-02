﻿using System;
using System.Threading;

namespace Unity.VersionControl.Git
{
    public interface IZipHelper
    {
        bool Extract(string archive, string outFolder, CancellationToken cancellationToken,
            Func<long, long, bool> onProgress, Func<string, bool> onFilter = null);
    }
}
