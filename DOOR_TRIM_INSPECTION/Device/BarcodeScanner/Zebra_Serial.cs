using Google.Protobuf.WellKnownTypes;
using System.IO.Ports;
using System.Text;

namespace DOOR_TRIM_INSPECTION.Device.BarcodeScanner
{
    public class Zebra_Serial
    {
        private SerialPort _serialPort = new SerialPort();

        private string _barcode = "";
        public string Barcode { get { return _barcode; } set{ _barcode = value;  } }

        public Zebra_Serial(string ComPort, int baudRate = 9600, int dataBit = 8, Parity parity = Parity.None, StopBits stopBit = StopBits.One, int timeOut = 2000)
        {
            if (_serialPort.IsOpen == true)
                _serialPort.Close();

            _serialPort.PortName = ComPort;
            _serialPort.BaudRate = baudRate;
            _serialPort.DataBits = dataBit;
            _serialPort.Parity = parity;
            _serialPort.StopBits = stopBit;
            _serialPort.ReadTimeout = timeOut;
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
            _serialPort.NewLine = "\r\n";
        }

        public bool PortOpen()
        {
            try
            {
                if (_serialPort.IsOpen == true)
                    _serialPort.Close();

                _serialPort.Open();
                if (_serialPort.IsOpen)
                {
                    ResetBuffer();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch { return false; }
        }
        
        public void ClearBarcodeData()
        {
            _barcode = null;
        }

        private void ResetBuffer()
        {
            _barcode = "";
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int readcount = _serialPort.BytesToRead;
            if (readcount > 2)
            {
                string data = _serialPort.ReadLine();
                _barcode = ParsingRecvData(data);
            }
        }

        private string ParsingRecvData(string data)
        {
            string ParseData = data;

            // Parsing
            

            return ParseData;
        }
    }
}
