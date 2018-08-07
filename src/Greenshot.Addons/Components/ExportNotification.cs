﻿#region Greenshot GNU General Public License

// Greenshot - a free and open source screenshot tool
// Copyright (C) 2007-2018 Thomas Braun, Jens Klingen, Robin Krom
// 
// For more information see: http://getgreenshot.org/
// The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 1 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using Autofac.Features.OwnedInstances;
using Caliburn.Micro;
using Dapplo.Log;
using Greenshot.Addons.Core;
using Greenshot.Addons.Interfaces;
using Greenshot.Addons.ViewModels;

namespace Greenshot.Addons.Components
{
    /// <summary>
    /// This is to notify the user of exports
    /// </summary>
    public class ExportNotification
    {
        private static readonly LogSource Log = new LogSource();
        private readonly ICoreConfiguration _coreConfiguration;
        private readonly IEventAggregator _eventAggregator;
        private readonly Func<IDestination, ExportInformation, Owned<ExportNotificationViewModel>> _toastFactory;

        public ExportNotification(
            ICoreConfiguration coreConfiguration,
            IEventAggregator eventAggregator,
            Func<IDestination, ExportInformation, Owned<ExportNotificationViewModel>> toastFactory)
        {
            _coreConfiguration = coreConfiguration;
            _eventAggregator = eventAggregator;
            _toastFactory = toastFactory;
        }

        /// <summary>
        /// This takes care of creating the toast view model, publishing it, and disposing afterwards
        /// </summary>
        /// <param name="source">IDestination</param>
        /// <param name="exportInformation">ExportInformation</param>
        public void NotifyOfExport(IDestination source, ExportInformation exportInformation)
        {
            if (exportInformation == null || !_coreConfiguration.ShowTrayNotification || !exportInformation.ExportMade)
            {
                Log.Info().WriteLine("No notification due to ShowTrayNotification = {0} - or export made = {1}", _coreConfiguration.ShowTrayNotification, exportInformation?.ExportMade);
                return;
            }
            // Create the ViewModel "part"
            var message = _toastFactory(source, exportInformation);
            // Prepare to dispose the view model parts automatically if it's finished
            void DisposeHandler(object sender, DeactivationEventArgs args)
            {
                message.Value.Deactivated -= DisposeHandler;
                message.Dispose();
            }

            message.Value.Deactivated += DisposeHandler;

            // Show the ViewModel as toast 
            _eventAggregator.PublishOnCurrentThread(message.Value);
        }

    }
}
