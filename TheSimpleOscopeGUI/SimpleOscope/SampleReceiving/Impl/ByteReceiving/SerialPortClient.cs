using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope.SampleReceiving.Impl.ByteReceiving
{
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

        public void WriteString(string str)
        {
            serialPort.Write(str);
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
