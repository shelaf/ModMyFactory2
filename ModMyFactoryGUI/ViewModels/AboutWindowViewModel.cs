//  Copyright (C) 2020-2021 Mathis Rech
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.

using Avalonia.Utilities;
using ModMyFactoryGUI.Localization;
using ModMyFactoryGUI.MVVM;
using ModMyFactoryGUI.Update;
using ModMyFactoryGUI.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace ModMyFactoryGUI.ViewModels
{
    internal sealed class AboutWindowViewModel : ViewModelBase<AboutWindow>, IWeakSubscriber<EventArgs>
    {
        public string Author => "Mathis Rech";

        public TagVersion GUIVersion => VersionStatistics.AppVersion;

        public IEnumerable<AssemblyVersionViewModel> AssemblyVersions { get; }

        public ICommand CloseCommand { get; }

        public string Changelog { get; }

        public AttributionViewModel Attributions { get; }

        public AboutWindowViewModel()
        {
            WeakSubscriptionManager.Subscribe(App.Current.Locales, nameof(LocaleManager.UICultureChanged), this);
            AssemblyVersions = VersionStatistics.LoadedAssemblyVersions.Select(kvp => new AssemblyVersionViewModel(kvp.Key, kvp.Value.ToString()));
            CloseCommand = ReactiveCommand.Create(() => AttachedView!.Close());

            var changelogFile = new FileInfo(Path.Combine(Program.ApplicationDirectory.FullName, "Changelog.md"));
            Changelog = changelogFile.Exists ? File.ReadAllText(changelogFile.FullName) : string.Empty;
            
            Attributions = new AttributionViewModel();
        }

        private void UICultureChangedHandler(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(GUIVersion));
        }

        void IWeakSubscriber<EventArgs>.OnEvent(object sender, EventArgs e) => UICultureChangedHandler(sender, e);
    }
}
