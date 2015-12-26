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
using Windows.UI.Xaml.Navigation;

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
            wave.AD9834_Init();
            Dac.initDac();
            wave.AD9834_Setup(AD9834.AD9834_FSEL0, AD9834.AD9834_PSEL0, AD9834.AD9834_OUT_SINUS, 0);            
        }
    }
}
