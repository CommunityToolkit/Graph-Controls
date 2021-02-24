using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;
using Microsoft.Toolkit.Graph.RoamingSettings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SampleTest.Samples
{
    public class RoamingSettingsViewModel : INotifyPropertyChanged
    {
        private IProvider GlobalProvider => ProviderManager.Instance.GlobalProvider;

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Extension> _userExtensions;
        public ObservableCollection<Extension> UserExtensions
        {
            get => _userExtensions;
            set => Set(ref _userExtensions, value);
        }

        private int _userExtensionsSelectedIndex;
        public int UserExtensionsSelectedIndex
        {
            get => _userExtensionsSelectedIndex;
            set => Set(ref _userExtensionsSelectedIndex, value);
        }

        public Extension SelectedUserExtension
        {
            get => UserExtensionsSelectedIndex > -1 && UserExtensionsSelectedIndex <= UserExtensions?.Count
                ? UserExtensions[UserExtensionsSelectedIndex]
                : null;
        }

        private ObservableCollection<KeyValuePair<string, object>> _additionalData;
        public ObservableCollection<KeyValuePair<string, object>> AdditionalData
        {
            get => _additionalData;
            set => Set(ref _additionalData, value);
        }

        private int _additionalDataSelectedIndex;
        public int AdditionalDataSelectedIndex
        {
            get => _additionalDataSelectedIndex;
            set => Set(ref _additionalDataSelectedIndex, value);
        }

        public KeyValuePair<string, object> SelectedAdditionalData
        {
            get => AdditionalDataSelectedIndex > -1 && AdditionalDataSelectedIndex <= AdditionalData?.Count
                ? AdditionalData[AdditionalDataSelectedIndex]
                : default;
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

        private User _me;

        public RoamingSettingsViewModel()
        {
            _userExtensions = null;
            _userExtensionsSelectedIndex = -1;
            _additionalData = null;
            _additionalDataSelectedIndex = -1;
            _keyInputText = string.Empty;
            _valueInputText = string.Empty;
            _me = null;

            PropertyChanged += OnPropertyChanged;
            ProviderManager.Instance.ProviderUpdated += (s, e) => CheckState();
            CheckState();
        }

        public async void AddOrUpdateAdditionalData()
        {
            await UserExtensionsDataSource.SetValue(
                extension: SelectedUserExtension,
                userId: _me.Id,
                key: KeyInputText,
                value: ValueInputText
                );

            // Reload the Extensions to show changes.
            await LoadState();
        }

        public async void CreateCustomRoamingSettings()
        {
            var roamingSettings = new CustomRoamingSettings(_me.Id);
            await roamingSettings.Create();

            await LoadState();
        }

        public async void DeleteCustomRoamingSettings()
        {
            var roamingSettings = new CustomRoamingSettings(_me.Id);
            await roamingSettings.Delete();

            await LoadState();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case nameof(UserExtensionsSelectedIndex):
                    if (SelectedUserExtension != null)
                    {
                        var additionalData = SelectedUserExtension.AdditionalData != null
                            ? SelectedUserExtension.AdditionalData
                            : new Dictionary<string, object>();

                        AdditionalData = new ObservableCollection<KeyValuePair<string, object>>(additionalData);
                    }
                    else
                    {
                        AdditionalData.Clear();
                    }

                    KeyInputText = string.Empty;
                    ValueInputText = string.Empty;

                    break;
                case nameof(AdditionalDataSelectedIndex):
                    if (AdditionalDataSelectedIndex > -1)
                    {
                        KeyInputText = SelectedAdditionalData.Key;
                        ValueInputText = SelectedAdditionalData.Value?.ToString();
                    }
                    else
                    {
                        KeyInputText = string.Empty;
                        ValueInputText = string.Empty;
                    }
                    break;
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
            _me = await GlobalProvider.Graph.Me.Request().GetAsync();
            string userId = _me.Id;

            var userExtensions = await UserExtensionsDataSource.GetAllExtensions(userId);

            UserExtensions = new ObservableCollection<Extension>(userExtensions);
        }

        private void ClearState()
        {
            UserExtensions?.Clear();
            UserExtensionsSelectedIndex = -1;
            AdditionalData?.Clear();
            AdditionalDataSelectedIndex = -1;
            KeyInputText = string.Empty;
            ValueInputText = string.Empty;
            _me = null;
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
