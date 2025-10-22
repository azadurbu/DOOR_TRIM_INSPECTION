using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DOOR_TRIM_INSPECTION.Device.Light
{
    public class LightSerialCom
    {
        public SerialPort serialPort = new SerialPort();
        public Action<string> deleLog = null;

        //byte[] recvBuf = new byte[1024];
        public string strReceived = "";

        public LightSerialCom(string sPortName = "COM3", int sPortSpeed = 115200)
        {
            string PortName = sPortName;
            int PortSpeed = sPortSpeed;

            Init_Device(PortName, PortSpeed, 8, Parity.None, StopBits.One, 2000);

        }

        public void Terminate()
        {
            PortClose();
        }

        private void Init_Device(string strPortName, int iBundRate, int idataBit, Parity nParity, StopBits nStopBit, int iTimeOut)
        {
            try
            {
                if (serialPort.IsOpen == true) serialPort.Close();

                serialPort.ReadTimeout = iTimeOut;
                serialPort.BaudRate = iBundRate;
                serialPort.PortName = strPortName;
                serialPort.DataBits = idataBit;
                serialPort.Parity = nParity;
                serialPort.StopBits = nStopBit;

                serialPort.NewLine = "\r\n";

#if LIGHT_TEST
#else
                serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
#endif
            }
            catch (Exception ex)
            {
                //Machine.logger.Write(eLogType.LIGHT, "Failed to Init_Device. : " + ex.ToString());
            }
        }

        public bool PortOpen()
        {
            try
            {
#if LIGHT_TEST
                return true;
#else
                if (serialPort.IsOpen == true)
                    serialPort.Close();

                serialPort.Open();

                if (serialPort.IsOpen)
                {
                    serialPort.DiscardInBuffer();
                    serialPort.DiscardOutBuffer();
                    // Machine.logger.Write(eLogType.LIGHT, "Port is open.");
                    return true;
                }
                else
                {
                    //Machine.logger.Write(eLogType.LIGHT, "Port is already open.");
                    return false;
                }
#endif
            }
            catch (Exception ex)
            {

                //Machine.logger.Write(eLogType.LIGHT, "Failed to open the port. : " + ex.ToString());
                return false;
            }
        }

        private bool PortClose()
        {
            try
            {
#if LIGHT_TEST
                return true;
#else
                if (serialPort.IsOpen == true) serialPort.Close();

                if (!serialPort.IsOpen)
                {
                    serialPort.Dispose();
                    //Machine.logger.Write(eLogType.LIGHT, "Port is Close.");
                    return true;
                }
                else
                {
                    return false;
                }
#endif
            }
            catch (Exception ex)
            {
                //Machine.logger.Write(eLogType.LIGHT, "Failed to PortClose. : " + ex.ToString());
                return false;
            }
        }

        public bool IsOpen()
        {
#if LIGHT_TEST          
            return true;
#else
            return serialPort.IsOpen;
#endif
        }

        public void ResetBuffer()
        {
            strReceived = "";
            //recvBuf = new byte[1024];
            //if (!serialPort.IsOpen) return;

            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
        }

        public bool SendData(string strSendData)
        {
            if (!IsOpen())
            {
                //Machine.logger.Write(eLogType.LIGHT, string.Format("Serial Com {0} not open.", serialPort.PortName));
                return false;
            }
            strReceived = "";
#if LIGHT_TEST
#else
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();

            serialPort.Write(strSendData); // THIS LINE WAS WriteLine; DID NOT WORK
#endif
            //Machine.logger.Write(eLogType.LIGHT, string.Format("Serial Com[{0}] : Send Completed. [{1}]", serialPort.PortName, strSendData));

            return true;
        }

        public bool SendData(byte[] bySendData)
        {
            if (!serialPort.IsOpen)
                return false;

            strReceived = "";
#if LIGHT_TEST
#else
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
            serialPort.Write(bySendData, 0, bySendData.Length);
#endif
            //Machine.logger.Write(eLogType.LIGHT, string.Format("Serial Com [{0}] : Send [{1}]", serialPort.PortName, bySendData.ToString()));
            return true;
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                System.Threading.Thread.Sleep(500);
                int readcount = serialPort.BytesToRead;
                if (readcount > 0)
                {
                    string b = serialPort.ReadByte().ToString();

                    //Machine.logger.Write(eLogType.LIGHT, string.Format("Serial Com [{0}] : Received [{1}]", serialPort.PortName, b));
                    ParseReceivedData(b.ToString());
                }

            }
            catch (Exception ex)
            {

                string str = "";
                str = str + ": serialPort_DataReceived, " + ex.ToString();
                //Machine.logger.Write(eLogType.LIGHT, string.Format("Serial Com [{0}] : ERR [{1}]", serialPort.PortName, str));
            }
        }

        public static void ParseReceivedData(string strReceived)
        {
            try
            {

            }
            catch (Exception ex)
            {
                string str = "";
                str = str + ": ParseReceivedData, " + ex.ToString();
                //Machine.logger.Write(eLogType.LIGHT, str);

            }
        }

        private void ModelChange(int Model)
        {
            //레시피 변경***********************************************************************************************************************************************   
        }


        public int ReadCount()
        {
            return strReceived.Length;
        }


        public void LightOnOffEN(bool bOn,int channel, int value)
        {
            if (bOn==false)  
            {
                value = 0;
            }
            byte[] bData = new byte[22];
            int index = 0;
            bData[index++] = 0x02;        // SOH
            bData[index++] = (byte)channel.ToString()[0];        // Channel Num

            for (int i = 0; i < 6; i++)
            {
                byte[] stepVal = new byte[3];
                string hexValue = value.ToString("X3");
                byte[] asciiBytes = Encoding.ASCII.GetBytes(hexValue);
                bData[index++] = asciiBytes[0];
                bData[index++] = asciiBytes[1];
                bData[index++] = asciiBytes[2];
            }
            bData[index++] = 0xC2;       // Data (해당 Register의 data값)
            bData[index++] = 0x03;        // EOT
             
            SendData(bData);
        }
    }
}
