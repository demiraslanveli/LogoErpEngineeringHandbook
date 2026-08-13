using System;
using System.Collections.Generic;

namespace LogoErp.Reference.LogoAdapter.Binding
{
    public sealed class LogoSdkBindingManifest
    {
        private readonly Dictionary<string, string> _values =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string SdkVersion { get; set; }
        public string ProductName { get; set; }
        public string Notes { get; set; }

        public void Set(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Binding key is required.", nameof(key));

            _values[key.Trim()] = value;
        }

        public bool TryGet(string key, out string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                value = null;
                return false;
            }

            return _values.TryGetValue(key.Trim(), out value);
        }

        public string GetRequired(string key)
        {
            if (!TryGet(key, out var value) || string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"Required Logo SDK binding is missing: {key}");

            return value;
        }

        public IReadOnlyDictionary<string, string> Values => _values;
    }
}
