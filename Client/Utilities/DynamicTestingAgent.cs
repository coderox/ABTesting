﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Client.Utilities
{
    public interface IExperimentsService
    {
        Task Initialize();
        T Get<T>(string key, T defaultValue);
        void LogView<T>(string key, T value);
        void LogConversion<T>(string key, T value);
    }

    public static class Experiments
    {
        public const string PURCHASE_BUTTON_BACKGROUND_COLOR = "PurchaseButtonBackgroundColor";
        public const string ANOTHER_TEST = "AnotherTest";
        public const string STRING_TEST = "AndAStringTest";
    }

    public class ExperimentsService : IExperimentsService
    {
        Dictionary<string, string> variations;
        public async Task Initialize()
        {
            variations = DynamicTestingAgent.GetVariations();
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
            DynamicTestingAgent.LogConversion(key, value);
        }

        public void LogView<T>(string key, T value)
        {
            DynamicTestingAgent.LogView(key, value);
        }
    }

    public static class DynamicTestingAgent
    {
        public class TestOption
        {
            public string Value { get; private set; }
            public int Displayed { get; set; }
            public int Selected { get; set; }
            public float ChanceOfWorking
            {
                get
                {
                    if (Displayed == 0)
                        return 0;
                    return (float)Selected / (float)Displayed;
                }
            }
            public TestOption(string value)
            {
                Value = value;
            }
        }

        private const double RANDOMIZATION_THRESHOLD = 0.1;

        static Random random = new Random();
        static Dictionary<string, List<TestOption>> tests { get; set; }

        static DynamicTestingAgent()
        {
            tests = new Dictionary<string, List<TestOption>>();
        }

        public static Task Initialize()
        {
            tests.Clear();

            tests.Add(Experiments.PURCHASE_BUTTON_BACKGROUND_COLOR, new List<TestOption> {
                new TestOption(Colors.Green.ToString()),
                new TestOption(Colors.Red.ToString()),
                new TestOption(Colors.Blue.ToString())
            });

            tests.Add(Experiments.ANOTHER_TEST, new List<TestOption> {
                new TestOption(true.ToString()),
                new TestOption(false.ToString())
            });

            tests.Add(Experiments.STRING_TEST, new List<TestOption> {
                new TestOption("Microsoft"),
                new TestOption("Google"),
                new TestOption("Facebook"),
                new TestOption("Apple")
            });

            return Task.CompletedTask;
        }

        public static Dictionary<string, string> GetVariations()
        {
            var variations = new Dictionary<string, string>();
            foreach (var test in tests) {
                variations.Add(test.Key, Choose(test.Key));
            }
            return variations;
        }

        private static string Choose(string key)
        {
            if (!tests.ContainsKey(key)) {
                return string.Empty;
            }
            var currentTests = tests[key];
            var numChoices = currentTests.Count;

            if (numChoices > 1 && random.NextDouble() < RANDOMIZATION_THRESHOLD) {
                // choose random value
                int index = random.Next(numChoices);
                return currentTests[index].Value;
            }

            var choice = currentTests.OrderByDescending(test => test.ChanceOfWorking).First();

            return choice.Value;
        }

        private static void Log(string key, string value, bool isConversion = false)
        {
            if (!tests.ContainsKey(key)) {
                return;
            }
            var currentTests = tests[key];
            var choice = currentTests.FirstOrDefault(t => t.Value == value);
            if (choice != null) {
                if (isConversion) {
                    choice.Selected++;
                } else {
                    choice.Displayed++;
                }
            }
        }



        public static void LogView<T>(string key, T value)
        {
            try {
                Log(key, value.ToString());
            } catch {

            }
        }

        public static void LogConversion<T>(string key, T value)
        {
            try {
                Log(key, value.ToString(), true);
            } catch {

            }
        }

        public static string ReportResults()
        {
            var sb = new StringBuilder();
            foreach (var entry in tests) {
                sb.AppendLine(entry.Key);
                foreach (var item in entry.Value) {
                    sb.AppendLine($"Choice: {item.Value} rate: {item.ChanceOfWorking}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
