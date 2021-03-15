// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Toolkit.Graph.Helpers.RoamingSettings;
using Microsoft.Toolkit.Graph.Providers;

namespace SampleTest.Samples.RoamingSettings
{
    public class RoamingSettingsViewModel : INotifyPropertyChanged
    {
        private IProvider GlobalProvider => ProviderManager.Instance.GlobalProvider;

        public event PropertyChangedEventHandler PropertyChanged;

        private string _errorText;
        public string ErrorText
        {
            get => _errorText;
            set => Set(ref _errorText, value);
        }

        private RoamingSettingsHelper _roamingSettings;

        private ObservableCollection<KeyValuePair<string, object>> _additionalData;
        public ObservableCollection<KeyValuePair<string, object>> AdditionalData
        {
            get => _additionalData;
            set => Set(ref _additionalData, value);
        }

        private string _keyInputText;
        public string KeyInputText
        {
            get => _keyInputText;
            set => Set(ref _keyInputText, value);
        }

        private string _valueInputText;
        public string ValueInputText
        {
            get => _valueInputText;
            set => Set(ref _valueInputText, value);
        }

        public RoamingSettingsViewModel()
        {
            _roamingSettings = null;
            _keyInputText = string.Empty;
            _valueInputText = string.Empty;

            ProviderManager.Instance.ProviderUpdated += (s, e) => CheckState();
            CheckState();
        }

        public void GetValue()
        {
            try
            {
                ErrorText = string.Empty;
                ValueInputText = string.Empty;

                string key = KeyInputText;
                string value = _roamingSettings.Read<string>(key);

                ValueInputText = value;
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
            }
        }

        public void SetValue()
        {
            try
            {
                ErrorText = string.Empty;

                _roamingSettings.Save(KeyInputText, ValueInputText);

                SyncRoamingSettings();
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
            }
        }

        public async void CreateCustomRoamingSettings()
        {
            try
            {
                ErrorText = string.Empty;

                await _roamingSettings.Create();
                
                AdditionalData = new ObservableCollection<KeyValuePair<string, object>>(_roamingSettings.DataStore.Settings);

                KeyInputText = string.Empty;
                ValueInputText = string.Empty;
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
            }
        }

        public async void DeleteCustomRoamingSettings()
        {
            try
            {
                ErrorText = string.Empty;

                await _roamingSettings.Delete();

                AdditionalData?.Clear();
                KeyInputText = string.Empty;
                ValueInputText = string.Empty;
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
            }
        }

        public async void SyncRoamingSettings()
        {
            try
            {
                ErrorText = string.Empty;
                AdditionalData?.Clear();

                await _roamingSettings.Sync();
                if (_roamingSettings.DataStore.Settings != null)
                {
                    AdditionalData = new ObservableCollection<KeyValuePair<string, object>>(_roamingSettings.DataStore.Settings);
                }
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
            }
        }

        private async void CheckState()
        {
            if (GlobalProvider != null && GlobalProvider.State == ProviderState.SignedIn)
            {
                await LoadState();
            }
            else
            {
                ClearState();
            }
        }

        private async Task LoadState()
        {
            try
            {
                _roamingSettings = await RoamingSettingsHelper.CreateForCurrentUser();

                KeyInputText = string.Empty;
                ValueInputText = string.Empty;
            }
            catch (Exception e)
            {
                ErrorText = e.Message;
            }
        }

        private void ClearState()
        {
            _roamingSettings = null;

            KeyInputText = string.Empty;
            ValueInputText = string.Empty;
        }

        private void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
