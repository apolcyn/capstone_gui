﻿using System;
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
using System.IO;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    /* An object that contains all of the configuration relevant to the function generator.
    Always have a configuration that has been send and is currently being used, as well as a "next configuration", 
    which s being built up. */
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

    /* An object that contains all of the configuration relevant to the oscope.
    Always have a configuration that has been send and is currently being used, as well as a "next configuration", 
    which s being built up. */
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

        const int MIN_TRIGGER_LEVEL = 0;
        const int MAX_TRIGGER_LEVEL = 300;
        int triggerLevel;

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

        public bool setTriggerLevel(int triggerLevel)
        {
            if(triggerLevel > MAX_TRIGGER_LEVEL || triggerLevel < MIN_TRIGGER_LEVEL)
            {
                return false;
            }
            this.triggerLevel = triggerLevel;
            return true;
        }

        public int getTriggerLevel()
        {
            return this.triggerLevel;
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

    /* An object that receives bytes and turns them into samples when they're ready.
    Can add bytes from UART to it and get the next sample when its ready.*/
    public class SampleReceiver
    {
        private ushort numBytesInNext;
        private const int NUM_BYTES_PER_SAMPLE = 2;
        private ushort sampleBuf;

        public bool nextSampleReady()
        {
            return numBytesInNext == NUM_BYTES_PER_SAMPLE;
        }
        
        public ushort nextSample()
        {
            if(numBytesInNext != NUM_BYTES_PER_SAMPLE)
            {
                throw new Exception("illegal state. trying to get next sample when next sample not ready");
            }
            numBytesInNext = 0;
            ushort val = sampleBuf;
            sampleBuf = 0;
            val = (ushort)(val / 4095.0 * 300);
            return val;
        }

        public void addReceivedByte(byte newByte)
        {
            if(numBytesInNext == NUM_BYTES_PER_SAMPLE)
            {
                throw new Exception("illegal state. overwriting received data");
            }
            sampleBuf = (ushort)(newByte + (sampleBuf << 8));
            numBytesInNext++;
        }
    }

    public interface SampleDisplayer
    {
        void newSamplesReceived(UInt16[] samplesBuffer, int numSamplesReceived);
    }

    /* An object that receives frames of samples from PSoC over UART. This object keeps track of 
    the state of the sample frames, and parses the UART bytes into a list of samples to display.
    When it's received a full frame of samples, it calls a callback method to display all of the samples
    on the main window display. */
    public class DataReceiver {
        private SerialPort serialPort;
        private int numSamplesExpected;
        private int numSamplesReceived;
        private UInt16[] samplesBuffer = new UInt16[10000];
        private byte[] numSamplesHeader = new byte[100];
        private int numSamplesHeaderIndex;
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        private Queue<byte> topSerialBuffer = new Queue<byte>();

        private enum ReceiveState { NOT_STARTED, FINDING_NUM_SAMPLES, RECEIVING_SAMPLES, GETTING_HEADER_F};
        private ReceiveState curState = ReceiveState.NOT_STARTED, nextState;

        private SampleDisplayer sampleDisplayer;
        private SampleReceiver samplesReceiver = new SampleReceiver();

        public DataReceiver(SampleDisplayer sampleDisplayer)
        {
            this.sampleDisplayer = sampleDisplayer;
            serialPort = new SerialPort("COM10", 57600, Parity.None, 8, StopBits.One);
            serialPort.Handshake = Handshake.None;
            serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            serialPort.Open();
        }

        public int getExpectedSamples(byte[] numSamplesHeader, int len)
        {
            if(len <= 0)
            {
                throw new Exception("invalid argument, passed a length of " + len);
            }

            int total = 0;
            int i = 0;
            while(i < len)
            {
                total *= 10;
                if(numSamplesHeader[i] < '0' || numSamplesHeader[i] > '9')
                {
                    throw new Exception("parsing the num samples count. got an unexpected character");
                }
                total += numSamplesHeader[i++] - '0';
            }

            return total;
        }

        public void byteReceived(byte newByte)
        {
            switch(curState)
            {
                /* TODO: Note that checking for first byte of 'F' leaves open possibility of reading
                a random number whos value happens to equal ASCII 'F' value, and then thinking that it's
                a start, when its not. Could fix this with a slightly longer special character sequence for starts. */
                case ReceiveState.NOT_STARTED:
                    if(newByte == '#')
                    {
                        nextState = ReceiveState.GETTING_HEADER_F;
                        stopwatch.Restart();
                    }
                    break;

                case ReceiveState.GETTING_HEADER_F:
                    if(newByte == 'F')
                    {
                        numSamplesHeaderIndex = 0;
                        nextState = ReceiveState.FINDING_NUM_SAMPLES;
                    }
                    else
                    {
                        throw new Exception("invalid state. expecting header 'F' but got:" + newByte);
                    }
                    break;

                case ReceiveState.FINDING_NUM_SAMPLES:
                    if(newByte >= '0' && newByte <= '9')
                    {
                        numSamplesHeader[numSamplesHeaderIndex++] = newByte;
                    }
                    else if(newByte == 'D')
                    {
                        if(numSamplesHeaderIndex == 0)
                        {
                            throw new Exception("ending the num samples header, but haven't got the number yet");
                        }
                        else
                        {
                            numSamplesReceived = 0;
                            numSamplesExpected = getExpectedSamples(numSamplesHeader, numSamplesHeaderIndex);
                            nextState = ReceiveState.RECEIVING_SAMPLES;
                        }
                    }
                    else
                    {
                        throw new Exception("received invalid byte. filling in the header, but received =" + newByte + "=");
                    }
                    break;

                case ReceiveState.RECEIVING_SAMPLES:
                    samplesReceiver.addReceivedByte(newByte);
                    if(samplesReceiver.nextSampleReady())
                    {
                        samplesBuffer[numSamplesReceived++] = samplesReceiver.nextSample();
                    }
                    if(numSamplesReceived == numSamplesExpected)
                    {
                        sampleDisplayer.newSamplesReceived(samplesBuffer, numSamplesReceived);
                        numSamplesExpected = 0;
                        numSamplesReceived = 0;
                        numSamplesHeaderIndex = 0;
                        nextState = ReceiveState.NOT_STARTED;
                        stopwatch.Stop();
                        log(stopwatch.ElapsedMilliseconds.ToString());
                    }
                    break;

                default:
                    throw new Exception("in an invalid state");
                    break;
            }

            curState = nextState;
        }

        void log(String str)
        {
            using (System.IO.StreamWriter w = File.AppendText("data_receiver_log" + DateTime.Now.Hour + ".txt"))
            {
                w.WriteLineAsync(str);
            }
        }

        /* An arbitrary byte or set of bytes is ready */
        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int newVal = 0;
            for(int i = 0; i < serialPort.BytesToRead && ((newVal = serialPort.ReadByte()) != -1); i++)
            {
                byteReceived((byte)newVal);
            }

            if(newVal == -1)
            {
                throw new Exception("end of UART stream seems to have been reached. strange");
            }
        }

        public void sendPsoCCommand(String command)
        {
            // Makes sure serial port is open before trying to write
            try
            {
                if (!(serialPort.IsOpen))
                    serialPort.Open();

                serialPort.Write(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening/writing to serial port :: " + ex.Message, "Error!");
            }
        }
    }

    public interface TriggerType
    {
        bool trigger(ushort firstSample, ushort secondSample, int trigger);
    }

    public class RisingEdgeTrigger : TriggerType
    {
        public bool trigger(ushort firstSample, ushort secondSample, int trigger)
        {
            return firstSample < trigger && secondSample >= trigger;
        }
    }

    public class FallingEdgeTrigger : TriggerType
    {
        public bool trigger(ushort firstSample, ushort secondSample, int trigger)
        {
            return firstSample > trigger && secondSample <= trigger;
        }
    }

    /* Main window object, contains references to all of the visual components on the GUI display. */
    public partial class MainWindow : Window, SampleDisplayer
    {
        DataReceiver dataReceiver;

        const char COMMAND_OUTER_CHAR = '#';

        private FunctionGeneratorConfiguration curFunctionGeneratorConfiguration = new FunctionGeneratorConfiguration();
        private FunctionGeneratorConfiguration nextFunctionGeneratorConfiguration = new FunctionGeneratorConfiguration();

        private OscopeConfiguration curOscopeConfiguration = new OscopeConfiguration();
        private OscopeConfiguration nextOscopeConfiguration = new OscopeConfiguration();

        private const int SAMPLES_PER_WINDOW = 100;
        private Queue<ushort> oscopeSamples = new Queue<ushort>(SAMPLES_PER_WINDOW);
        private ushort[] samplesDisplayBuffer = new ushort[SAMPLES_PER_WINDOW];

        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        private RisingEdgeTrigger risingEdgeTrigger = new RisingEdgeTrigger();
        private FallingEdgeTrigger fallingEdgeTrigger = new FallingEdgeTrigger();
        private TriggerType curTrigger;

        private int verticalTriggerIndex;

        public MainWindow()
        {
            InitializeComponent();
            dataReceiver = new DataReceiver(this);
            curTrigger = risingEdgeTrigger;
        }

        public void newSamplesReceived(ushort[] samplesBuf, int numSamples)
        {
            this.Dispatcher.BeginInvoke(new SetDeleg(si_DataReceived), new object[] { samplesBuf, numSamples });
        }

        // delegate is used to write to a UI control from a non-UI thread
        public delegate void SetDeleg(ushort[] nums, int numSamples);

        private void si_DataReceived(ushort[] data, int numSamples) {
            enqueueNewSamples(data, 0, numSamples);
        }

        // event handler for DAC vpp slider updates
        private void DAC_vpp_updated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            update_DAC_vpp((decimal)this.DAC_vpp_slider.Value);
        }

        // updates value of DAC vpp
        private void update_DAC_vpp(decimal newVpp)
        {
            nextFunctionGeneratorConfiguration.setVpp(newVpp);
            this.DAC_vpp_slider.Value = (double)nextFunctionGeneratorConfiguration.getVpp();
            if (this.Vpp_text_display != null)
            {
                this.Vpp_text_display.Text
                    = nextFunctionGeneratorConfiguration.getVpp().ToString(FunctionGeneratorConfiguration.VPP_FORMAT_STR);
            }
        }

        // event handler for DAC voffset slider updates
        private void DAC_voffset_updated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            update_DAC_voffset((decimal)this.DAC_voffset_slider.Value);
        }

        // updates DAC voffset value
        private void update_DAC_voffset(decimal newOffset)
        {
            nextFunctionGeneratorConfiguration.setVOffset(newOffset);
            this.DAC_voffset_slider.Value = (double)nextFunctionGeneratorConfiguration.getVOffset();
            if (this.Voffset_text_display != null)
            {
                this.Voffset_text_display.Text
                    = nextFunctionGeneratorConfiguration.getVOffset().ToString(FunctionGeneratorConfiguration.VOFFSET_FORMAT_STR);
            }
        }

        // event handler for DAC duty cycle slider updates
        private void DAC_duty_cycle_updated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            update_DAC_duty_cycle((int)this.DAC_duty_cycle.Value);
        }

        // updates actual duty cycle for DAC waveform
        private void update_DAC_duty_cycle(int newDutyCycle)
        {
            nextFunctionGeneratorConfiguration.setDutyCycle(newDutyCycle);
            this.DAC_duty_cycle.Value = nextFunctionGeneratorConfiguration.getDutyCycle();
            if (this.duty_cycle_text_display != null)
            {
                this.duty_cycle_text_display.Text = nextFunctionGeneratorConfiguration.getDutyCycle().ToString();
            }
        }

        // event handler for DAC frequenecy slider udpates
        private void DAC_frequency_slider_updated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            update_DAC_frequency((int)this.DAC_frequency_slider.Value);
        }

        // updates the actual frequency of the DAC wave
        private void update_DAC_frequency(int newFreq)
        {
            nextFunctionGeneratorConfiguration.setFrequency(newFreq);
            this.DAC_frequency_slider.Value = nextFunctionGeneratorConfiguration.getFrequency();
            if (this.DAC_frequency_text_display != null)
            {
                this.DAC_frequency_text_display.Text
                    = nextFunctionGeneratorConfiguration.getFrequency().ToString();
            }
        }

        private void DAC_wavetype_selected(object sender, SelectionChangedEventArgs e)
        {
            nextFunctionGeneratorConfiguration
                .setWaveType((FunctionGeneratorConfiguration.WaveType)this.DAC_wave_type_list.SelectedIndex);
        }

        private void DAC_start_btn_click(object sender, RoutedEventArgs e)
        {
            dataReceiver.sendPsoCCommand("#DA#");
        }

        // event handler for samples/second slider update
        private void oscope_ksamples_updated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            update_oscope_ksamples((int)this.oscope_ksamples_slider.Value);
        }

        // updates actual oscope ksamples/sec
        private void update_oscope_ksamples(int val)
        {
            if (this.oscope_ksamples_text_display != null)
            {
                nextOscopeConfiguration.setkSamplesPerSecond(val);
                this.oscope_ksamples_slider.Value = nextOscopeConfiguration.getKSamplesPerSecond();
                this.oscope_ksamples_text_display.Text = nextOscopeConfiguration.getKSamplesPerSecond().ToString();
            }
        }

        private void start_oscope_btn_click(object sender, RoutedEventArgs e)
        {
            this.oscope_configuration_display.Text = nextOscopeConfiguration.getConfiguration();
            //dataReceiver.sendPsoCCommand(nextOscopeConfiguration.getConfiguration());
            dataReceiver.sendPsoCCommand("#AA#");
        }

        /* Finds the index in the new samples buffer that sets off the trigger, if there are any samples that do. */
        private int findTriggerStartIndex(ushort[] newSamples, int startIndex, int numSamples)
        {
            int triggerLevel = nextOscopeConfiguration.getTriggerLevel();
            int count = 0;

            for(int i = startIndex; i < numSamples - 1; i++)
            {
                if(curTrigger.trigger(newSamples[i], newSamples[i + 1], triggerLevel) && (++count == 2))
                {
                    return i + 1;
                }
            }

            return -1;
        }

        int min(int a, int b)
        {
            if(a < b)
            {
                return a;
            }
            return b;
        }

        /* Adds in new samples and redraws the current display frame. Assumes that
        the nuber of samples given to it is at least the number of samples needed to fill the window. */
        private void enqueueNewSamples(ushort[] newSamples, int startIndex, int numSamples)
        {
            stopwatch.Restart();
            int triggerIndex = findTriggerStartIndex(newSamples, startIndex, numSamples);

            /* If we've found a trigger index, draw the wave starting at the closest possible point up to the trigger */
            if(triggerIndex != -1)
            {
                int k = 0;
                int i = min(triggerIndex, (startIndex + numSamples) - SAMPLES_PER_WINDOW);
                i = Math.Max(triggerIndex - verticalTriggerIndex, startIndex);

                while(k < SAMPLES_PER_WINDOW)
                {
                    samplesDisplayBuffer[k++] = newSamples[i++];
                }

                drawOscopeLineGroup(samplesDisplayBuffer, k);
            }
            stopwatch.Stop();
        }

        /* Draws lines on the canvas display between a list of points. */
        private void drawOscopeLineGroup(ushort[] vals, int numVals)
        {
            if(numVals != SAMPLES_PER_WINDOW)
            {
                throw new Exception("trying to add " + numVals + " samples but need " + SAMPLES_PER_WINDOW
                    + " samples per window"); 
            }

            int spacing = (int) this.oscope_window_canvas.Width / (SAMPLES_PER_WINDOW - 1);
            int curX = 0;
            clearOscopeCanvas();

            for(int i = 0; i < numVals - 1; i++)
            {
                drawOscopeLine(curX, (int)this.oscope_window_canvas.Height - vals[i]
                    , curX + spacing, (int)this.oscope_window_canvas.Height - vals[i + 1]);
                curX += spacing;
            }
        }

        private void clearOscopeCanvas()
        {
            this.oscope_window_canvas.Children.RemoveRange(2, this.oscope_window_canvas.Children.Count - 1);
        }

        /* Draws a line on the canvas between two points. */
        private void drawOscopeLine(int prevX, int prevY, int curX, int curY)
        {
            Line myLine = new Line();
            myLine.Stroke = System.Windows.Media.Brushes.Gold;
            myLine.X1 = prevX;
            myLine.X2 = curX;
            myLine.Y1 = prevY;
            myLine.Y2 = curY;
            myLine.HorizontalAlignment = HorizontalAlignment.Left;
            myLine.VerticalAlignment = VerticalAlignment.Center;
            myLine.StrokeThickness = 4;
            this.oscope_window_canvas.Children.Add(myLine);
        }

        private string FullPSoCCommand(string configuration)
        {
            return "#" + configuration + "#";
        }

        private void oscope_resolution_updated(object sender, SelectionChangedEventArgs e)
        {
            nextOscopeConfiguration.setResolution(this.oscope_resolution_dropdown.SelectedIndex);
        }

        private void DAC_Frequency_update_btn_click(object sender, RoutedEventArgs e)
        {
            string[] vals = this.DAC_frequency_text_display.Text.Split();
            update_DAC_frequency(int.Parse(vals[0]));
        }

        private void Vpp_update_btn_Click(object sender, RoutedEventArgs e)
        {
            update_DAC_vpp(decimal.Parse(this.Vpp_text_display.Text.Split()[0]));
        }

        private void Voffset_update_btn_Click(object sender, RoutedEventArgs e)
        {
            update_DAC_voffset(decimal.Parse(this.Voffset_text_display.Text.Split()[0]));
        }

        private void duty_cycle_update_btn_Click(object sender, RoutedEventArgs e)
        {
            update_DAC_duty_cycle(int.Parse(this.duty_cycle_text_display.Text.Split()[0]));
        }

        private void oscope_ksamples_update_btn_Click(object sender, RoutedEventArgs e)
        {
            update_oscope_ksamples(int.Parse(this.oscope_ksamples_text_display.Text.Split()[0]));
        }

        private void update_oscope_trigger_level(int triggerLevel)
        {
            nextOscopeConfiguration.setTriggerLevel((int)this.trigger_slider_button.Maximum - triggerLevel);
            this.trigger_slider_button.Value = this.trigger_slider_button.Maximum - nextOscopeConfiguration.getTriggerLevel();
        }

        private void trigger_slider_button_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            update_oscope_trigger_level((int)this.trigger_slider_button.Value);
        }



        private void log(String str)
        {
            using (StreamWriter w = File.AppendText("main_window_log-" + DateTime.Now.Hour + ".txt"))
            {
                w.WriteLineAsync(str);
            }
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.trigger_selection.SelectedIndex == 0)
            {
                curTrigger = risingEdgeTrigger;
            }
            else if(this.trigger_selection.SelectedIndex == 1)
            {
                curTrigger = fallingEdgeTrigger;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.DAC_config_command.Clear();
            this.DAC_config_command.Text = nextFunctionGeneratorConfiguration.getConfiguration();
            dataReceiver.sendPsoCCommand("#" + nextFunctionGeneratorConfiguration.getConfiguration() + "#");
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            dataReceiver.sendPsoCCommand("#PC_REQ_CONNECT#");
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            dataReceiver.sendPsoCCommand("#DZ#");
        }

        private void vertical_trigger_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.verticalTriggerIndex = (int)this.vertical_trigger_slider.Value / 4;
        }
    }
}