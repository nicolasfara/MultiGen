using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using Windows.Devices.Gpio;

namespace MultiGen
{
    public class AD9834
    {
        
        /* Registers */
        private const ushort AD9834_REG_CMD = (0 << 14);
        private const ushort AD9834_REG_FREQ0 = (1 << 14);
        private const ushort AD9834_REG_FREQ1 = (2 << 14);
        private const ushort AD9834_REG_PHASE0 = (6 << 13);
        private const ushort AD9834_REG_PHASE1 = (7 << 13);

        /* Command Control Bits */
        private const ushort AD9834_B28 = (1 << 13);
        private const ushort AD9834_HLB = (1 << 12);
        public const ushort AD9834_FSEL0 = (0 << 11);
        public const ushort AD9834_FSEL1 = (1 << 11);
        public const ushort AD9834_PSEL0 = (0 << 10);
        public const ushort AD9834_PSEL1 = (1 << 10);
        private const ushort AD9834_CMD_PIN = (1 << 9);
        private const ushort AD9834_CMD_SW = (0 << 9);
        private const ushort AD9834_RESET = (1 << 8);
        private const ushort AD9834_SLEEP1 = (1 << 7);
        private const ushort AD9834_SLEEP12 = (1 << 6);
        private const ushort AD9834_OPBITEN = (1 << 5);
        private const ushort AD9834_SIGN_PIB = (1 << 4);
        private const ushort AD9834_DIV2 = (1 << 3);
        private const ushort AD9834_MODE = (1 << 1);
        public const ushort AD9834_OUT_SINUS = ((0 << 5) | (0 << 1));
        public const ushort AD9834_OUT_TRIANGLE = ((0 << 5) | (1 << 1));

        private const int CS0 = 26;
        private const int FSELECT = 5;
        private const int PSELECT = 6;
        private const int RESET = 13;
        private const int SLEEP = 19;

        private const string SPI_CONTROLLER_NAME = "SPI0";  /* For Raspberry Pi 2, use SPI0                             */
        private const Int32 SPI_CHIP_SELECT_LINE = 0;       /* Line 0 maps to physical pin number 24 on the Rpi2        */

        private SpiDevice AD9834_spi;
        private GpioController CsController;
        private GpioPin CsAD9834;
        private GpioPin PSelect;
        private GpioPin FSelect;
        private GpioPin Reset;


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
                AD9834_spi = await SpiDevice.FromIdAsync(dis[0].Id, settings);
                if (AD9834_spi == null)
                {
                    Debug.WriteLine("SPI {0} initialized not completed, SPI is busy", dis[0].Id);
                    return;
                }
                Debug.WriteLine("SPI AD9834 initialize");
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

            CsAD9834 = CsController.OpenPin(CS0);
            CsAD9834.SetDriveMode(GpioPinDriveMode.Output);
            CsAD9834.Write(GpioPinValue.High);
            PSelect = CsController.OpenPin(PSELECT);
            PSelect.SetDriveMode(GpioPinDriveMode.Output);
            PSelect.Write(GpioPinValue.Low);
            FSelect = CsController.OpenPin(FSELECT);
            FSelect.SetDriveMode(GpioPinDriveMode.Output);
            FSelect.Write(GpioPinValue.Low);
            Reset = CsController.OpenPin(RESET);
            Reset.SetDriveMode(GpioPinDriveMode.Output);
            Reset.Write(GpioPinValue.Low);

            if (CsAD9834 == null || FSelect == null || PSelect == null || Reset == null)
            {
                Debug.WriteLine("Pin not open");
                return;
            }

            Debug.WriteLine("Gpio inizialized");
        }

        /* Initialize the Gpio */
        public async Task InitAD9834()
        {
            try
            {
                await InitSpi();
                InitGpio();
                Debug.WriteLine("Initialized AD9834");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Initialization fail: " + ex.Message);
            }
        }

        public void writeSpi(ushort reg)
        {
            try
            {
                CsAD9834.Write(GpioPinValue.Low);
                byte[] word = { 0x00, 0x00 };
                word[0] = (byte)((reg & 0xFF00) >> 8);
                word[1] = (byte)((reg & 0x00FF) >> 0);
                AD9834_spi.Write(word);
                CsAD9834.Write(GpioPinValue.High);
                Debug.WriteLine("SPI write correctly");                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error write SPI: {0}", ex);
            }
        }


        /***************************************************************************//**
         * @brief Writes the value to a register.
         *
         * @param -  regValue - The value to write to the register.
         *
         * @return  None.    
        *******************************************************************************/
        private void AD9834_SetRegisterValue(ushort regValue)
        {
            try
            {
                byte[] data = { 0x03, 0x00, 0x00 };

                data[1] = (byte)((regValue & 0xFF00) >> 8);
                data[2] = (byte)((regValue & 0x00FF) >> 0);
                CsAD9834.Write(GpioPinValue.Low);
                AD9834_spi.Write(data);
                CsAD9834.Write(GpioPinValue.High);      
                Debug.WriteLine("Write register complete");
            } 
            catch(Exception ex)
            {
                Debug.WriteLine("Error write register AD9834: {0}", ex.Message);
            }        
        }


        /***************************************************************************//**
         * @brief Initializes the SPI communication peripheral and resets the part.
         *
         * @return None.
        *******************************************************************************/
        public void AD9834_Init()
        {
            try
            {
                AD9834_SetRegisterValue(AD9834_REG_CMD | AD9834_RESET);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error initialized AD9834: {0}", ex.Message);
            }
        }


        /***************************************************************************//**
         * @brief Sets the Reset bit of the AD9834.
         *
         * @return None.
        *******************************************************************************/
        public void AD9834_Reset()
        {
            try
            {
                AD9834_SetRegisterValue(AD9834_REG_CMD | AD9834_RESET);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error reset AD9834: {0}", ex);
            }
        }


        /***************************************************************************//**
         * @brief Clears the Reset bit of the AD9834.
         *
         * @return None.
        *******************************************************************************/
        public void AD9834_ClearReset()
        {
            try
            {
                AD9834_SetRegisterValue(AD9834_REG_CMD);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error clear reset AD9834: {0}", ex);
            }
        }


        /***************************************************************************//**
         * @brief Writes to the frequency registers.
         *
         * @param -  reg - Frequence register to be written to.
         * @param -  val - The value to be written.
         *
         * @return  None.    
        *******************************************************************************/
        public void AD9834_SetFrequency(ushort reg, ulong val)
        {            
            try
            {                
                ulong regFreq = (val * 268435456) / 50000000;
                ushort freqHi = reg;
                ushort freqLo = reg;
                freqHi = (ushort)(freqHi | (regFreq & 0xFFFC000) >> 14);
                freqLo = (ushort)(freqLo | (regFreq & 0x3FFF));
                AD9834_SetRegisterValue(AD9834_B28);
                AD9834_SetRegisterValue(freqLo);
                AD9834_SetRegisterValue(freqHi);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error write frequency register AD9834: {0}", ex);
            }
        }


        /***************************************************************************//**
         * @brief Writes to the phase registers.
         *
         * @param -  reg - Phase register to be written to.
         * @param -  val - The value to be written.
         *
         * @return  None.    
        *******************************************************************************/
        public void AD9834_SetPhase(ushort reg, ulong val)
        {
            try
            {
                ulong regPhase = (val * 4096) / 360;
                ushort phase = reg;
                phase = (ushort)(phase | regPhase);
                AD9834_SetRegisterValue(phase);
            }
            catch(Exception ex)
            {
                Debug.Write("Error write frequency register AD9834: {0}", ex.Message);
            }
        }


        /***************************************************************************//**
        * @brief Selects the Frequency,Phase and Waveform type.
        *
        * @param -  freq  - Frequency register used.
        * @param -  phase - Phase register used.
        * @param -  type  - Type of waveform to be output.
        *
        * @return  None.    
        *******************************************************************************/
        public void AD9834_Setup(ushort freq, ushort phase, ushort type, ushort commandType)
        {
            try
            {
                ushort val = 0;
                val = (ushort)(freq | phase | type | commandType);
                if(commandType == 1)
                {
                    if(freq == 1)
                    {
                        FSelect.Write(GpioPinValue.High);
                    }
                    else
                    {
                        FSelect.Write(GpioPinValue.Low);
                    }
                    if(phase == 1)
                    {
                        PSelect.Write(GpioPinValue.High);
                    }
                    else
                    {
                        PSelect.Write(GpioPinValue.Low);
                    }
                }
                AD9834_SetRegisterValue(val);
            }
            catch(Exception ex)
            {
                Debug.Write("Error setup AD9834: {0}", ex.Message);
            }

        }
    }
}
