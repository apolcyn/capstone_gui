using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving.Impl.ByteReceiving
{
    /// <summary>
    /// Interfaces with the USB to the PSOC.
    /// </summary>
    public class SerialPortClient
    {
        private SerialPort serialPort { get; set; }
        private ByteReceiver byteReceiver { get; set; }

        public SerialPortClient(SerialPort serialPort, ByteReceiver byteReceiver)
        {
            this.serialPort = serialPort;
            this.byteReceiver = byteReceiver;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
        }

        public SerialPortClient(ByteReceiver byteReceiver, MainWindow mainWindow)
        {
            this.byteReceiver = byteReceiver;
            mainWindow.COMPortSelectedEvent += COMPortSelected;
        }

        private void COMPortSelected(object sender, COMPortSelectedEventArgs args)
        {
            if(this.serialPort != null)
            {
                this.serialPort.Close();
            }
            this.serialPort = new SerialPort(args.fullComportName, 57600, Parity.None, 8, StopBits.One);
            this.serialPort.Handshake = Handshake.None;
            this.serialPort.Open();
            this.serialPort.DataReceived += sp_DataReceived;
        }

        /// <summary>
        /// Writes the string as is to the PSOC.
        /// </summary>
        public void WriteString(string str)
        {
            serialPort.Write(str);
        }

        /// <summary>
        /// Writes a command to the PSOC and checks to make sure
        /// that it is wrapped in '#' characters
        /// </summary>
        public void SendPsocCommand(string str)
        {
            if(str[0] != '#' || str[str.Length - 1] != '#')
            {
                throw new Exception("incorrectoly formatted string: " + str);
            }
            WriteString(str);
        }

        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while(serialPort.BytesToRead > 0)
            {
                byteReceiver.byteReceived((byte)serialPort.ReadByte());
            }
        }
    }
}
