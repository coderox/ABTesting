using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;

namespace Client.Utilities
{
    public class ExperimentsService : IExperimentsService
    {
        Dictionary<string, string> variations;
        public Task Initialize()
        {
            variations = ExperimentsServiceFake.GetVariations();
            return Task.CompletedTask;
        }

        private string Choose(string key)
        {
            return variations.ContainsKey(key) ? variations[key] : string.Empty;
        }

        #region Boolean

        private bool GetBool(string key, bool defaultValue = false)
        {
            var value = Choose(key);
            bool result;
            if (bool.TryParse(value, out result)) {
                return result;
            }
            return defaultValue;
        }

        #endregion

        #region Color

        private Color GetColor(string key, Color defaultColor)
        {
            var testColor = Choose(key);
            if (string.IsNullOrEmpty(testColor)) {
                return defaultColor;
            }
            return GetColorFromHex(testColor);
        }

        private Color GetColorFromHex(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            return Color.FromArgb(a, r, g, b);
        }
        #endregion

        public T Get<T>(string key, T defaultValue)
        {
            try {
                switch (typeof(T).Name) {
                    case nameof(Boolean):
                        return (T)(object)GetBool(key);
                    case nameof(Color):
                        Color defaultColor = (Color)(object)defaultValue;
                        return (T)(object)GetColor(key, defaultColor);
                    case nameof(String):
                        return (T)(object)Choose(key);
                    default:
                        return defaultValue;
                }
            } catch {
                return defaultValue;
            }
        }

        public void LogConversion<T>(string key, T value)
        {
            ExperimentsServiceFake.LogConversion(key, value);
        }

        public void LogView<T>(string key, T value)
        {
            ExperimentsServiceFake.LogView(key, value);
        }
    }
}
