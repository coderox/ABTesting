using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

namespace Client.Utilities
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
            Displayed = 1;
        }
    }

    public interface IExperimentsService
    {
        Task Initialize();
        bool GetBool(string key, bool defaultValue);
        void ReportBool(string key, bool value);

        Color GetColor(string key, Color defaultValue);
        void ReportColor(string key, Color value);
    }

    public class DynamicTestingAgent : IExperimentsService
    {
        public const string PURCHASE_BUTTON_BACKGROUND_COLOR = "PurchaseButtonBackgroundColor";
        public const string ANOTHER_TEST = "AnotherTest";

        private const double RANDOMIZATION_THRESHOLD = 0.1;

        Random random = new Random();
        Dictionary<string, List<TestOption>> tests { get; set; }

        public DynamicTestingAgent()
        {
            tests = new Dictionary<string, List<TestOption>>();
        }

        public Task Initialize()
        {
            var colorTests = new List<TestOption> {
                new TestOption(Colors.Green.ToString()),
                new TestOption(Colors.Red.ToString()),
                new TestOption(Colors.Blue.ToString())
            };
            tests.Add(PURCHASE_BUTTON_BACKGROUND_COLOR, colorTests);

            var boolTests = new List<TestOption> {
                new TestOption(true.ToString()),
                new TestOption(false.ToString())
            };
            tests.Add(ANOTHER_TEST, boolTests);

            return Task.CompletedTask;
        }

        private string Choose(string key)
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

            choice.Displayed++;
            return choice.Value;
        }

        private void Report(string key, string value)
        {
            if (!tests.ContainsKey(key)) {
                return;
            }
            var currentTests = tests[key];
            var choice = currentTests.FirstOrDefault(t => t.Value == value);
            if (choice != null) {
                choice.Selected++;
            }
        }

        #region Boolean

        public bool GetBool(string key, bool defaultValue = false)
        {
            var value = Choose(key);
            bool result;
            if (bool.TryParse(value, out result)) {
                return result;
            }
            return defaultValue;
        }

        public void ReportBool(string key, bool value)
        {
            Report(key, value.ToString());
        }

        #endregion

        #region Color

        public Color GetColor(string key, Color defaultColor)
        {
            var testColor = Choose(key);
            if (string.IsNullOrEmpty(testColor)) {
                return defaultColor;
            }
            return GetColorFromHex(testColor);
        }

        public void ReportColor(string key, Color value)
        {
            Report(key, value.ToString());
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
    }
}
