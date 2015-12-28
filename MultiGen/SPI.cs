using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using Windows.Devices.Gpio;
using MultiGen;


namespace MultiGen
{
    public class SPI
    {
        private const string SPI_CONTROLLER_NAME = "SPI0";  /* For Raspberry Pi 2, use SPI0                             */
        private const Int32 SPI_CHIP_SELECT_LINE = 0;       /* Line 0 maps to physical pin number 24 on the Rpi2        */

        private SpiDevice SpiMultiGen;
        private GpioController CsController;
        private GpioPin CsAD9834;
        private GpioPin CsAD5660_A;
        private GpioPin CsAD5660_O;
        private GpioPin PSelect;
        private GpioPin FSelect;
        private GpioPin Reset;

        private const int CS0 = 26;
        private const int CS1 = 22;
        private const int CS2 = 27;
        private const int FSELECT = 5;
        private const int PSELECT = 6;
        private const int RESET = 13;
        private const int SLEEP = 19;

        public enum EnableChip
        {
            AD9834,
            Offset,
            Amplitude,
            OffAll,
        };

        public enum enableRegister
        {
            FselecOn = 1,
            FselectOff = 0,
            PselectOn = 1,
            PselectOff = 0,
        };

        /* Initialize the SPI bus */
        private async Task InitSpi()
        {
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 1000000;
                settings.Mode = SpiMode.Mode0;

                string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
                var dis = await DeviceInformation.FindAllAsync(spiAqs);
                SpiMultiGen = await SpiDevice.FromIdAsync(dis[0].Id, settings);
                if(SpiMultiGen == null)
                {
                    Debug.WriteLine("SPI {0} initialized not completed, SPI is busy", dis[0].Id);
                    return;
                }
                Debug.WriteLine("SPI {0} initialize", dis[0].Id);
            }

            catch(Exception ex)
            {
                Debug.WriteLine("SPI initialization fail"+ ex.Message);
                return;
            }
        }

        /* Initialize the Gpio */
        private void InitGpio()
        {
            CsController = GpioController.GetDefault();

            if(CsController == null)
            {
                throw new Exception("GPIO do not exist");
            }

            CsAD9834 = CsController.OpenPin(CS0);
            CsAD9834.SetDriveMode(GpioPinDriveMode.Output);
            CsAD9834.Write(GpioPinValue.High);            
            CsAD5660_A = CsController.OpenPin(CS1);
            CsAD5660_A.SetDriveMode(GpioPinDriveMode.Output);
            CsAD5660_A.Write(GpioPinValue.High);            
            CsAD5660_O = CsController.OpenPin(CS2);
            CsAD5660_O.SetDriveMode(GpioPinDriveMode.Output);
            CsAD5660_O.Write(GpioPinValue.High);
            PSelect = CsController.OpenPin(PSELECT);
            PSelect.SetDriveMode(GpioPinDriveMode.Output);
            PSelect.Write(GpioPinValue.Low);
            FSelect = CsController.OpenPin(FSELECT);
            FSelect.SetDriveMode(GpioPinDriveMode.Output);
            FSelect.Write(GpioPinValue.Low);
            Reset = CsController.OpenPin(RESET);
            Reset.SetDriveMode(GpioPinDriveMode.Output);
            Reset.Write(GpioPinValue.Low);

            if(CsAD5660_A == null || CsAD5660_O == null || CsAD9834 == null || FSelect == null || PSelect == null || Reset == null)
            {
                Debug.WriteLine("Pin not open");
                return;
            }

            Debug.WriteLine("Gpio inizialized");
        }

        /* Initialize the Gpio */
        public async void InitAll()
        {
            try
            {
                await InitSpi();
                InitGpio();                
                Debug.WriteLine("Inizialized");                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Initialization fail: "+ ex.Message);
            }
        }

        /* Manage CS */
        public void enableCs(EnableChip cs)
        {
            switch (cs)
            {
                case EnableChip.AD9834:
                    CsAD9834.Write(GpioPinValue.Low);
                    CsAD5660_A.Write(GpioPinValue.High);
                    CsAD5660_O.Write(GpioPinValue.High);
                    Debug.WriteLine("OK");
                    break;
                case EnableChip.Amplitude:
                    CsAD9834.Write(GpioPinValue.High);
                    CsAD5660_A.Write(GpioPinValue.Low);
                    CsAD5660_O.Write(GpioPinValue.High);
                    break;
                case EnableChip.Offset:
                    CsAD9834.Write(GpioPinValue.High);
                    CsAD5660_A.Write(GpioPinValue.High);
                    CsAD5660_O.Write(GpioPinValue.Low);
                    break;
                case EnableChip.OffAll:
                    CsAD9834.Write(GpioPinValue.High);
                    CsAD5660_A.Write(GpioPinValue.High);
                    CsAD5660_O.Write(GpioPinValue.High);
                    break;
                default:
                    Debug.WriteLine("Error input function");
                    break;

            }
        }

        /* Write data on SPI */
        public void writeSpi(ushort reg, EnableChip cs)
        {
            try
            {
                enableCs(cs);
                byte[] word = { 0x00, 0x00 };
                word[0] = (byte)((reg & 0xFF00) >> 8);
                word[1] = (byte)((reg & 0x00FF) >> 0);
                SpiMultiGen.Write(word);
                Debug.WriteLine("SPI write correctly");
                enableCs(EnableChip.OffAll);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error write SPI: {0}", ex);
            }
        }

        /* Write on phase register */
        public void writePSELEC(enableRegister select)
        {
            if ((int)select == 1)
            {
                PSelect.Write(GpioPinValue.High);
            }
            else
            {
                PSelect.Write(GpioPinValue.Low);
            }
        }

        /*Write on frequency register */
        public void writeFSELEC(enableRegister select)
        {
            if ((int)select == 1)
            {
                FSelect.Write(GpioPinValue.High);
            }
            else
            {
                FSelect.Write(GpioPinValue.Low);
            }
        }

    }
}
