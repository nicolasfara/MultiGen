using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;

namespace MultiGen
{
    class AD5660_DAC
    {
        private const string SPI_CONTROLLER_NAME = "SPI0";  /* For Raspberry Pi 2, use SPI0                             */
        private const Int32 SPI_CHIP_SELECT_LINE = 1;       /* Line 1 maps to physical pin number 24 on the Rpi2        */

        private SpiDevice SpiDac;
        private GpioController CsController;
        private GpioPin CsAD5660_A;
        private GpioPin CsAD5660_O;

        private const int CS1 = 22;
        private const int CS2 = 27;

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
                SpiDac = await SpiDevice.FromIdAsync(dis[0].Id, settings);
                if (SpiDac == null)
                {
                    Debug.WriteLine("SPI {0} initialized not completed, SPI is busy", dis[0].Id);
                    return;
                }
                Debug.WriteLine("SPI AD5660 initialize");
            }

            catch (Exception ex)
            {
                Debug.WriteLine("SPI initialization fail" + ex.Message);
                return;
            }
        }

        /* Initialize the Gpio */
        private void InitGpio()
        {
            CsController = GpioController.GetDefault();

            if (CsController == null)
            {
                throw new Exception("GPIO do not exist");
            }

            CsAD5660_A = CsController.OpenPin(CS1);
            CsAD5660_A.SetDriveMode(GpioPinDriveMode.Output);
            CsAD5660_A.Write(GpioPinValue.High);
            CsAD5660_O = CsController.OpenPin(CS2);
            CsAD5660_O.SetDriveMode(GpioPinDriveMode.Output);
            CsAD5660_O.Write(GpioPinValue.High);

            if (CsAD5660_A == null || CsAD5660_O == null)
            {
                Debug.WriteLine("Pin not open");
                return;
            }

            Debug.WriteLine("Gpio inizialized");
        }


        /***************************************************************************//**
        * @brief initialize DAC.
        *
        * @param - None.
        *
        * @return  None.    
        *******************************************************************************/
        public async Task initDac()
        {
            try
            {
                await InitSpi();
                InitGpio();
                Debug.WriteLine("AD5660 initialized");
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error initialize DAC: {0}", ex);
            }
        }


        /***************************************************************************//**
        * @brief Writes the offset.
        *
        * @param -  offset - The value of offset.
        *
        * @return  None.    
        *******************************************************************************/
        

         public void writeOffset(double offset)
        {
            try
            {
                /*ushort resolution = 65535;  //Resolution bit of AD5660 2^16
                ushort word = (ushort)((resolution / 10) * (offset + 5));   //formula to calculate a digital value for AD5660. Output +/-5V  
                byte[] wordHi = { (byte)((word & 0xFF00) >> 8) };
                byte[] wordLo = { (byte)((word & 0x00FF) >> 0) };  
                CsAD5660_O.Write(GpioPinValue.Low);
                SpiDac.Write(wordHi);
                SpiDac.Write(wordLo);
                CsAD5660_O.Write(GpioPinValue.High);
                Debug.WriteLine("AD5660 write offset complete");*/
                ushort resolution = 4095;
                ushort word = (ushort)((resolution / 10) * (offset + 5));
                word = (ushort) ((word << 2) & 0x3FFF);
                byte[] wordHi = { (byte)((word & 0xFF00) >> 8) };
                byte[] wordLo = { (byte)((word & 0x00FF) >> 8) };
                CsAD5660_O.Write(GpioPinValue.Low);
                SpiDac.Write(wordHi);
                SpiDac.Write(wordLo);
                CsAD5660_O.Write(GpioPinValue.High);
                Debug.WriteLine("AD5660 write offset complete");
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error write offset AD5660: {0}", ex.Message);
            }
        }


        /***************************************************************************//**
        * @brief Writes the value of amplitude.
        *
        * @param -  amplitude - The value amplitude.
        *
        * @return  None.    
        *******************************************************************************/
        public void writeAmplitude(double amplitude)
        {
            try
            {
                /*ushort resolution = 15728;  //correspond (in digital value) to 1.20V, max analog value for AD5660 (vref AD9834)
                ushort word = (ushort)(((1.20 - (amplitude / 10)) * resolution) / 1.20);  //formula to calculate the digital value for amplitude
                byte[] wordHi = { (byte)((word & 0xFF00) >> 8) };
                byte[] wordLo = { (byte)((word & 0x00FF) >> 8) };
                CsAD5660_A.Write(GpioPinValue.Low);
                SpiDac.Write(wordHi);
                SpiDac.Write(wordLo);
                CsAD5660_A.Write(GpioPinValue.High);
                Debug.WriteLine("AD5660 write amplitude complete");*/
                int resolution = 983;
                int word = (ushort)(((1.20 - (amplitude / 10)) * resolution) / 1.20);
                word = (word << 2) & 0x3FFF;
                byte[] wordHi = { (byte)((word & 0xFF00) >> 8) };
                byte[] wordLo = { (byte)((word & 0x00FF) >> 8) };
                CsAD5660_A.Write(GpioPinValue.Low);
                SpiDac.Write(wordHi);
                SpiDac.Write(wordLo);
                CsAD5660_A.Write(GpioPinValue.High);
                Debug.WriteLine("AD5660 write amplitude complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error write amplitude AD5660: {0}", ex.Message);
            }
        }
    }
}
