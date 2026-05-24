using System;
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels
{
    public class ScoreboardPlayerActionVM : ViewModel
    {
        private readonly Action<string> _executeAction;
        private string _definition;
        private string _value;
        private bool _isEnabled;

        public ScoreboardPlayerActionVM(string id, string definition, string value, bool isEnabled, Action<string> executeAction)
        {
            Id = id;
            Definition = definition;
            Value = value;
            IsEnabled = isEnabled;
            _executeAction = executeAction;
        }

        public string Id { get; }

        [DataSourceProperty]
        public string Definition
        {
            get => _definition;
            set
            {
                if (_definition != value)
                {
                    _definition = value;
                    OnPropertyChangedWithValue(value, "Definition");
                }
            }
        }

        [DataSourceProperty]
        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChangedWithValue(value, "Value");
                }
            }
        }

        [DataSourceProperty]
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChangedWithValue(value, "IsEnabled");
                }
            }
        }

        private void ExecuteAction()
        {
            if (IsEnabled)
                _executeAction?.Invoke(Id);
        }
    }
}
