using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Client.Utilities;

namespace Client
{
    public sealed partial class MainPage
    {
        IExperimentsService testing = new DynamicTestingAgent();
        Color testColor;

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += async delegate {
                await testing.Initialize();
                ReloadControls();
            };
        }

        private void OnPurchaseClicked(object sender, RoutedEventArgs e)
        {
            testing.ReportColor(DynamicTestingAgent.PURCHASE_BUTTON_BACKGROUND_COLOR, testColor);
        }

        private void OnReloadClicked(object sender, RoutedEventArgs e)
        {
            ReloadControls();
        }

        private void ReloadControls()
        {
            testColor = testing.GetColor(DynamicTestingAgent.PURCHASE_BUTTON_BACKGROUND_COLOR, Colors.Gray);
            btnPurchase.Background = new SolidColorBrush(testColor);
        }
    }
}
