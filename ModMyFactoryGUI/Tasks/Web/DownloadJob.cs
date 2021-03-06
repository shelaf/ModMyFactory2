//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using ModMyFactory.BaseTypes;
using ModMyFactory.WebApi;
using ModMyFactory.WebApi.Factorio;
using ModMyFactory.WebApi.Mods;
using ModMyFactoryGUI.Helpers;
using ModMyFactoryGUI.ViewModels;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ModMyFactoryGUI.Tasks.Web
{
    internal abstract class DownloadJob : IJob
    {
        public event EventHandler? Completed;

        public FileInfo? File { get; private set; }

        public abstract string Description { get; }

        public Progress<double> Progress { get; } = new Progress<double>();

        public bool Success { get; private set; }

        protected abstract Task<FileInfo> DownloadFile(CancellationToken cancellationToken, IProgress<double> progress);

        protected virtual void OnCompleted(EventArgs e)
            => Completed?.Invoke(this, e);

        public async Task Run(CancellationToken cancellationToken)
        {
            try
            {
                File = await DownloadFile(cancellationToken, Progress);
                Success = true;
            }
            catch (ApiException ex)
            {
                Success = false;
                await MessageHelper.ShowMessageForApiException(ex);
            }

            OnCompleted(EventArgs.Empty);
        }
    }

    internal class DownloadModReleaseJob : DownloadJob
    {
        private readonly string _username, _token;

        public ModReleaseInfo Release { get; }

        public override string Description { get; }

        public DownloadModReleaseJob(ModReleaseInfo release, string modDisplayName, string username, string token)
            => (Release, Description, _username, _token) = (release, modDisplayName, username, token);

        protected override Task<FileInfo> DownloadFile(CancellationToken cancellationToken, IProgress<double> progress)
        {
            var dir = Program.Locations.GetModDir(Release.Info.FactorioVersion);
            string fileName = Path.Combine(dir.FullName, Release.FileName);
            return ModApi.DownloadModReleaseAsync(Release, _username, _token, fileName, cancellationToken, progress);
        }
    }

    internal sealed class UpdateModJob : DownloadModReleaseJob
    {
        public ModUpdateInfo Info { get; }

        public bool Replace { get; }

        public bool RemoveOld { get; }

        public UpdateModJob(ModUpdateInfo info, bool replace, bool removeOld, string username, string token)
            : base(info.Release, info.Family.DisplayName, username, token)
            => (Info, Replace, RemoveOld) = (info, replace, removeOld);
    }

    internal sealed class DownloadFactorioJob : DownloadJob
    {
        private readonly string _username, _token;
        private readonly AccurateVersion _version;
        private readonly FactorioBuild _build;
        private readonly Platform _platform;

        public override string Description => $"Factorio {_version}";

        public DownloadFactorioJob(string username, string token, AccurateVersion version, FactorioBuild build, Platform platform)
        {
            (_username, _token) = (username, token);
            (_version, _build, _platform) = (version, build, platform);
        }

        protected override Task<FileInfo> DownloadFile(CancellationToken cancellationToken, IProgress<double> progress)
        {
            var dir = Program.TemporaryDirectory;
            string fileName = Path.Combine(dir.FullName, $"Factorio_{_version}.tmp"); // Dummy extension because it could be either ZIP or TAR.GZ
            return DownloadApi.DownloadReleaseAsync(_version, _build, _platform, _username, _token, fileName, cancellationToken, progress);
        }
    }
}
