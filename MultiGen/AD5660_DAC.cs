using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MultiGen;

namespace MultiGen
{
    class AD5660_DAC
    {
        MultiGen.SPI Dac = new MultiGen.SPI();

        /***************************************************************************//**
        * @brief initialize DAC.
        *
        * @param - None.
        *
        * @return  None.    
        *******************************************************************************/
        public void initDac()
        {
            try
            {
                writeAmplitude(1);
                writeOffset(0);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error initialize DAC: {0}", ex.Message);
            }
        }


        /***************************************************************************//**
        * @brief Writes the offset.
        *
        * @param -  offset - The value of offset.
        *
        * @return  None.    
        *******************************************************************************/
        public void writeOffset(ushort offset)
        {
            try
            {
                ushort resolution = 65535;  //Resolution bit of AD5660 2^16
                ushort word = (ushort)((resolution / 10) * (offset + 5));   //formula to calculate a digital value for AD5660. Output +/-5V           
                Dac.enableCs(SPI.EnableChip.Offset);
                Dac.writeSpi(word);
                Dac.enableCs(SPI.EnableChip.OffAll);
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
        public void writeAmplitude(ushort amplitude)
        {
            try
            {
                ushort resolution = 15728;  //correspond (in digital value) to 1.20V, max analog value for AD5660 (vref AD9834)
                ushort word = (ushort)(((1.20 - (amplitude / 10)) * resolution) / 1.20);  //formula to calculate the digital value for amplitude
                Dac.enableCs(SPI.EnableChip.Amplitude);
                Dac.writeSpi(word);
                Dac.enableCs(SPI.EnableChip.OffAll);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error write amplitude AD5660: {0}", ex.Message);
            }
        }
    }
}
