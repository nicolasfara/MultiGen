using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MultiGen
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        AD5660_DAC Dac = new AD5660_DAC();
        AD9834 wave = new AD9834();
        

        public MainPage()
        {
            this.InitializeComponent();            
            wave.InitAD9834().GetAwaiter();
            Dac.initDac().GetAwaiter();            
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            wave.AD9834_SetFrequency(0, 1);
            wave.AD9834_SetPhase(0, 180);
            wave.AD9834_Setup(0, 0, AD9834.AD9834_OUT_SINUS, 0);
            Dac.writeAmplitude(1.2);
            Dac.writeOffset(0.4);          
        }
    }
}
