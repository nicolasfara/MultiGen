using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MultiGen
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SPI SPi = new SPI();
        AD5660_DAC Dac = new AD5660_DAC();
        AD9834 wave = new AD9834();
        

        public MainPage()
        {
            this.InitializeComponent();
            SPi.InitAll();
            //Spi.writeSpi(0x22);                
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //SPi.writeSpi(0x22, SPI.EnableChip.AD9834);
            wave.AD9834_SetFrequency(0, 1);
            //SPi.enableCs(SPI.EnableChip.AD9834);
        }
    }
}
