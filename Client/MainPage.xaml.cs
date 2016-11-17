using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Client.Utilities;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Client
{
    public sealed partial class MainPage
    {
        IExperimentsService experiments = new ExperimentsService();
        Color testColor;

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += async delegate {
                await ExperimentsServiceFake.Initialize();
                await experiments.Initialize();
                await ReloadControls();
            };

            LostFocus += delegate {
                Debug.WriteLine(ExperimentsServiceFake.ReportResults());
            };
        }

        private void OnPurchaseClicked(object sender, RoutedEventArgs e)
        {
            experiments.LogConversion<Color>(Experiments.PURCHASE_BUTTON_BACKGROUND_COLOR, testColor);
        }

        private async void OnReloadClicked(object sender, RoutedEventArgs e)
        {
            await ReloadControls();
        }

        private async Task ReloadControls()
        {
            await experiments.Initialize();

            testColor = experiments.Get<Color>(Experiments.PURCHASE_BUTTON_BACKGROUND_COLOR, Colors.Gray);
            btnPurchase.Background = new SolidColorBrush(testColor);
            experiments.LogView<Color>(Experiments.PURCHASE_BUTTON_BACKGROUND_COLOR, testColor);
        }
    }
}
