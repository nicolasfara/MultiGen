using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MultiGen
{
    class AD9834
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
        private const ushort AD9834_FSEL0 = (0 << 11);
        private const ushort AD9834_FSEL1 = (1 << 11);
        private const ushort AD9834_PSEL0 = (0 << 10);
        private const ushort AD9834_PSEL1 = (1 << 10);
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


        /* Create new istance */
        MultiGen.SPI Spi = new MultiGen.SPI();

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
                Spi.enableCs(SPI.EnableChip.AD9834);
                Spi.writeSpi(regValue);
                Spi.enableCs(SPI.EnableChip.OffAll);
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
                Debug.WriteLine("Error reset AD9834: {0}", ex.Message);
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
                Debug.WriteLine("Error clear reset AD9834: {0}", ex.Message);
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
                ulong regFreq = (val * 2 ^ 28) / 50000000;
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
                Debug.WriteLine("Error write frequency register AD9834: {0}", ex.Message);
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
                AD9834_SetRegisterValue(val);
            }
            catch(Exception ex)
            {
                Debug.Write("Error setup AD9834: {0}", ex.Message);
            }

        }
    }
}
