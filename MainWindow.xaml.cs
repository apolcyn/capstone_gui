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
    /// 

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

        public const decimal MIN_VPP = 0;
        public const decimal MAX_VPP = 4;
        public const string VPP_FORMAT_STR = "F2"; // precion of 10^-2
        public const decimal DEFAULT_VPP = 0;
        decimal vpp = DEFAULT_VPP;
        const char VPP_COMMAND = 'V';

        int frequency;
        public const int MIN_FREQ = 0;
        public const int MAX_FREQ = 240000;
        const int DEFAULT_FREQ = 1;
        public const char FREQUENCY_COMMAND = 'F';

        decimal vOffset;
        public const decimal MIN_OFFSET = 0;
        public const decimal MAX_OFFSET = 4;
        public const string VOFFSET_FORMAT_STR = "F2"; // precision of 10^-2
        const decimal DEFAULT_OFFSET = 0;
        public const char VOFFSET_COMMAND = 'O';

        int dutyCycle;
        const int MIN_DUTY_CYCLE = 0;
        const int MAX_DUTY_CYCLE = 100;
        const char DUTY_CYCLE_COMMAND = 'D';

        public string getConfiguration()
        {
            // start commands
            StringBuilder config = new StringBuilder();
            config.Append(FIRST_CHAR);

            // wave type configuration
            config.Append(WAVE_TYPE_COMMAND);
            config.Append(waveTypeCommands[(int)getWaveType()]);

            // vpp configuration
            config.Append(VPP_COMMAND);
            config.Append(getVpp().ToString(VPP_FORMAT_STR));

            // frequency configuration
            config.Append(FREQUENCY_COMMAND);
            config.Append(getFrequency());

            // vOffset configuration
            config.Append(VOFFSET_COMMAND);
            config.Append(getVOffset().ToString(VOFFSET_FORMAT_STR));

            // duty cycle configuration
            config.Append(DUTY_CYCLE_COMMAND);
            config.Append(getDutyCycle());

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

        public bool setFrequency(int frequency)
        {
            if(frequency < MIN_FREQ || frequency > MAX_FREQ)
            {
                return false;
            }
            this.frequency = frequency;

            return true;
        }

        public int getFrequency()
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

            config.Append(RESOLUTION_COMMAND);
            config.Append(getResolution());

            config.Append(KSAMPLES_PER_SECOND_COMMAND);
            config.Append(getKSamplesPerSecond());

            return config.ToString();
        }

        public bool setResolution(int resolutionIndex)
        {
            if(resolutionIndex < 0 || resolutionIndex > resolutionOptions.Length)
            {
                return false;
            }

            this.resolution = resolutionOptions[resolutionIndex];
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

        const char COMMAND_OUTER_CHAR = '#';

        private FunctionGeneratorConfiguration curFunctionGeneratorConfiguration = new FunctionGeneratorConfiguration();
        private FunctionGeneratorConfiguration nextFunctionGeneratorConfiguration = new FunctionGeneratorConfiguration();

        private OscopeConfiguration curOscopeConfiguration = new OscopeConfiguration();
        private OscopeConfiguration nextOscopeConfiguration = new OscopeConfiguration();

        public MainWindow()
        {
            InitializeComponent();
            /*serialPort = new SerialPort("COM8", 115200, Parity.None, 8, StopBits.One);
            serialPort.Handshake = Handshake.None;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            serialPort.Open();*/
        }

      /*  private void buttonClick(object sender, RoutedEventArgs e)
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

        private void si_DataReceived(string data) { textBox1.Text = data.Trim(); }*/

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void DAC_vpp_updated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            nextFunctionGeneratorConfiguration.setVpp((decimal)this.DAC_vpp_slider.Value);
            this.DAC_vpp_slider.Value = (double)nextFunctionGeneratorConfiguration.getVpp();
            this.Vpp_text_display.Text 
                = nextFunctionGeneratorConfiguration.getVpp().ToString(FunctionGeneratorConfiguration.VPP_FORMAT_STR);
        }

        private void DAC_voffset_updated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            nextFunctionGeneratorConfiguration.setVOffset((decimal)this.DAC_voffset_slider.Value);
            this.DAC_voffset_slider.Value = (double)nextFunctionGeneratorConfiguration.getVOffset();
            this.Voffset_text_dispaly.Text
                = nextFunctionGeneratorConfiguration.getVOffset().ToString(FunctionGeneratorConfiguration.VOFFSET_FORMAT_STR);
        }

        private void DAC_duty_cycle_updated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            nextFunctionGeneratorConfiguration.setDutyCycle((int)this.DAC_duty_cycle.Value);
            this.DAC_duty_cycle.Value = nextFunctionGeneratorConfiguration.getDutyCycle();
            this.duty_cycle_text_dispaly.Text = nextFunctionGeneratorConfiguration.getDutyCycle() + "%";
        }

        private void DAC_frequency_slider_updated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            nextFunctionGeneratorConfiguration.setFrequency((int)this.DAC_frequency_slider.Value);
            this.DAC_frequency_slider.Value = nextFunctionGeneratorConfiguration.getFrequency();
            this.DAC_frequency_text_display.Text
                = nextFunctionGeneratorConfiguration.getFrequency().ToString();
        }

        private void DAC_wavetype_selected(object sender, SelectionChangedEventArgs e)
        {
            nextFunctionGeneratorConfiguration
                .setWaveType((FunctionGeneratorConfiguration.WaveType)this.DAC_wave_type_list.SelectedIndex);
        }

        private void DAC_start_btn_click(object sender, RoutedEventArgs e)
        {
            this.DAC_config_command.Clear();
            this.DAC_config_command.Text = nextFunctionGeneratorConfiguration.getConfiguration();
        }

        private void oscope_ksamples_updated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.oscope_ksamples_text_display != null)
            {
                nextOscopeConfiguration.setkSamplesPerSecond((int)this.oscope_ksamples_slider.Value);
                this.oscope_ksamples_slider.Value = nextOscopeConfiguration.getKSamplesPerSecond();
                this.oscope_ksamples_text_display.Text = nextOscopeConfiguration.getKSamplesPerSecond().ToString() + " K/sec";
            }
        }

        private void start_oscope_btn_click(object sender, RoutedEventArgs e)
        {
            this.oscope_configuration_display.Text = nextOscopeConfiguration.getConfiguration();
        }

        private string FullPSoCCommand(string configuration)
        {
            return "#" + configuration + "#";
        }

        private void oscope_resolution_updated(object sender, SelectionChangedEventArgs e)
        {
            nextOscopeConfiguration.setResolution(this.oscope_resolution_dropdown.SelectedIndex);
        }

    }
}
