using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO.Ports;
using System.Threading;
using System.Windows.Threading;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class FunctionGeneratorConfiguration
    {
        // First letter of configuration command for function generator
        const char FIRST_CHAR = 'D';

        const char START_COMMAND = 'A';
        const char STOP_COMMAND = 'Z';

        // Wave types and the corresponding letters to configure them
        public enum WaveType {Sine, Square, Sawtooth, Triangle, Arbitrary };
        char[] waveTypeCommands = { 'I', 'Q', 'W', 'T', 'A' };
        const char WAVE_TYPE_COMMAND = 'W';
        const WaveType DEFAULT_WAVE_TYPE = WaveType.Sine;
        WaveType waveType = DEFAULT_WAVE_TYPE;

        const decimal MIN_VPP = 0;
        const decimal MAX_VPP = 4;
        const string VPP_FORMAT_STR = "D2"; // precion of 10^-2
        const decimal DEFAULT_VPP = 0;
        decimal vpp = DEFAULT_VPP;
        const char VPP_COMMAND = 'V';

        decimal frequency;
        const int MIN_FREQ = 0;
        const int MAX_FREQ = 240000;
        const int DEFAULT_FREQ = 1;
        const char FREQUENCY_COMMAND = 'F';

        decimal vOffset;
        const decimal MIN_OFFSET = 0;
        const decimal MAX_OFFSET = 4;
        const string VOFFSET_FORMAT_STR = "D2"; // precision of 10^-2
        const decimal DEFAULT_OFFSET = 0;
        const char VOFFSET_COMMAND = 'O';

        int dutyCycle;
        const int MIN_DUTY_CYCLE = 0;
        const int MAX_DUTY_CYCLE = 100;
        const char DUTY_CYCLE_COMMAND = 'D';

        public string getConfiguration()
        {
            // start commands
            StringBuilder config = new StringBuilder();
            config.Append(FIRST_CHAR);
            config.Append(START_COMMAND);

            // wave type configuration
            config.Append(WAVE_TYPE_COMMAND);
            config.Append(waveTypeCommands[(int)getWaveType()]);

            // vpp configuration
            config.Append(VPP_COMMAND);
            config.Append(String.Format(VPP_FORMAT_STR, getVpp()));

            // frequency configuration
            config.Append(FREQUENCY_COMMAND);
            config.Append(getFrequency());

            // vOffset configuration
            config.Append(VOFFSET_COMMAND);
            config.Append(String.Format(VOFFSET_FORMAT_STR, getVOffset()));

            // duty cycle configuration
            config.Append(DUTY_CYCLE_COMMAND);
            config.Append(getDutyCycle());

            config.Append(STOP_COMMAND);

            return config.ToString();
        }

        public bool setVpp(decimal vpp)
        {
            if(vpp < MIN_VPP || vpp > MAX_VPP)
            {
                return false;
            }

            this.vpp = vpp;
            return true;
        }

        public decimal getVpp()
        {
            return vpp;
        }

        public bool setFrequency(decimal frequency)
        {
            if(frequency < MIN_FREQ || frequency > MAX_FREQ)
            {
                return false;
            }
            this.frequency = frequency;

            return true;
        }

        public decimal getFrequency()
        {
            return frequency;
        }

        public bool setVOffset(decimal vOffset)
        {
            if(vOffset < MIN_OFFSET || vOffset > MAX_OFFSET)
            {
                return false;
            }
            this.vOffset = vOffset;

            return true;
        }

        public decimal getVOffset()
        {
            return vOffset;
        }

        public void setWaveType(WaveType waveType)
        {
            this.waveType = waveType;
        }

        public WaveType getWaveType()
        {
            return waveType;
        }

        public bool setDutyCycle(int dutyCycle)
        {
            if(dutyCycle < MIN_DUTY_CYCLE || dutyCycle > MAX_DUTY_CYCLE)
            {
                return false;
            }
            this.dutyCycle = dutyCycle;
            return true;
        }      
        
        public int getDutyCycle()
        {
            return dutyCycle;
        }
    } 

    public class OscopeConfiguration
    {
        const char FIRST_CHAR = 'A';

        const char START_COMMAND = 'A';
        const char STOP_COMMAND = 'Z';

        int[] resolutionOptions = { 8, 10, 12 };
        const int DEFAULT_RESOLUTION = 8;
        int resolution = DEFAULT_RESOLUTION;
        char RESOLUTION_COMMAND = 'R';

        const int KSAMPLES_PER_SECOND_MIN = 56;
        const int KSAMPLES_PER_SECOND_MAX = 1000;
        const int KSAMPLES_PER_SECOND_DEFAULT = 56;
        int kSamplesPerSecond = KSAMPLES_PER_SECOND_DEFAULT;
        char KSAMPLES_PER_SECOND_COMMAND = 'S';

        const int SAMPLES_PER_FRAME = 1000;
        const char SAMPLES_PER_FRAME_COMMAND = 'F';

        public string getConfiguration()
        {
            StringBuilder config = new StringBuilder();

            config.Append(FIRST_CHAR);
            config.Append(START_COMMAND);

            config.Append(RESOLUTION_COMMAND);
            config.Append(resolution);

            config.Append(KSAMPLES_PER_SECOND_COMMAND);
            config.Append(kSamplesPerSecond);

            config.Append(STOP_COMMAND);

            return config.ToString();
        }

        public bool setResolution(int resolution)
        {
            if(!resolutionOptions.Contains(resolution))
            {
                return false;
            }

            this.resolution = resolution;
            return true;
        }

        public int getResolution()
        {
            return resolution;
        }

        public bool setkSamplesPerSecond(int kSamplesPerSecond)
        {
            if(kSamplesPerSecond < KSAMPLES_PER_SECOND_MIN || kSamplesPerSecond > KSAMPLES_PER_SECOND_MAX)
            {
                return false;
            }

            this.kSamplesPerSecond = kSamplesPerSecond;
            return true;
        }

        public int getKSamplesPerSecond()
        {
            return kSamplesPerSecond;
        }

    }

    public partial class MainWindow : Window
    {
        SerialPort serialPort;

        public MainWindow()
        {
            InitializeComponent();
            serialPort = new SerialPort("COM8", 115200, Parity.None, 8, StopBits.One);
            serialPort.Handshake = Handshake.None;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            serialPort.Open();
        }

        private void buttonClick(object sender, RoutedEventArgs e)
        {
            this.textBox.Clear();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // Makes sure serial port is open before trying to write
            try
            {
                if (!(serialPort.IsOpen))
                    serialPort.Open();

                String command = this.textBox.Text;
                serialPort.Write(command + "\r\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening/writing to serial port :: " + ex.Message, "Error!");
            }

        }

        // delegate is used to write to a UI control from a non-UI thread
        private delegate void SetTextDeleg(string text);

        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort.ReadLine();

            this.Dispatcher.BeginInvoke(new SetTextDeleg(si_DataReceived), new object[] { data });
        }

        private void si_DataReceived(string data) { textBox1.Text = data.Trim(); }
    }
}
