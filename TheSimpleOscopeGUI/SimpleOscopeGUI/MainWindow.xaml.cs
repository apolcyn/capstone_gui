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
using System.IO;
using SimpleOscope.SampleReceiving.Impl;
using SimpleOscope.SampleReceiving;
using SimpleOscope.SampleReceiving.Impl;
using SimpleOscope.SampleReceiving.Impl;
using SimpleOscope.SampleReceiving.Impl;
using SimpleOscope.SampleReceiving.Impl;
using SimpleOscope.SampleReceiving.Impl;
using System.Collections.ObjectModel;

namespace SimpleOscope
{
    public class OscopeWidthChangedEventArgs : EventArgs
    {
        public int newWidth { get; }
        public OscopeWidthChangedEventArgs(int newWidth) { this.newWidth = newWidth; }
    }

    public class OscopeHeightChangedEventArgs : EventArgs
    {
        public int newHeight { get; }
        public OscopeHeightChangedEventArgs(int newHeight) { this.newHeight = newHeight; }
    }

    public class MaxSampleSizeChangedEventArgs : EventArgs
    {
        public double maxSampleSize { get; }
        public MaxSampleSizeChangedEventArgs(double maxSampleSize) { this.maxSampleSize = maxSampleSize; }
    }

    public class SampleScalerChangedEventArgs : EventArgs
    {
        public double sampleScaler { get; }
        public SampleScalerChangedEventArgs(double sampleScaler) { this.sampleScaler = sampleScaler; }
    }

    public class SampleOffsetChangedEventArgs : EventArgs
    {
        public double sampleOffset { get; }
        public SampleOffsetChangedEventArgs(double sampleOffset) { this.sampleOffset = sampleOffset; }
    }

    public class NumSamplesToDisplayChangedEventArgs : EventArgs
    {
        public uint numSamples { get; }
        public NumSamplesToDisplayChangedEventArgs(uint numSamples) { this.numSamples = numSamples; }
    }

    public class SampleSpacingChangedEventArgs : EventArgs
    {
        public uint sampleSpacing { get; }
        public SampleSpacingChangedEventArgs(uint sampleSpacing) { this.sampleSpacing = sampleSpacing; }
    }

    public class TriggerHorizontalPositionChangedEventArgs : EventArgs
    {
        public uint triggerHorizontalPosition { get; }
        public TriggerHorizontalPositionChangedEventArgs(uint triggerHorizontalPosition)
        {
            this.triggerHorizontalPosition = triggerHorizontalPosition;
        }
    }

    public class SampleFrameSizeChangedEventArgs : EventArgs
    {
        public uint sampleFrameSize { get; }
        public SampleFrameSizeChangedEventArgs(uint sampleFrameSize)
        {
            this.sampleFrameSize = sampleFrameSize;
        }
    }

    public class TriggerLevelChangedEventArgs : EventArgs
    {
        public int triggerLevel { get; }
        public TriggerLevelChangedEventArgs(int triggerLevel) { this.triggerLevel = triggerLevel; }
    }

    public class COMPortSelectedEventArgs : EventArgs
    {
        public string fullComportName { get; }
        public COMPortSelectedEventArgs(string fullComportName) { this.fullComportName = fullComportName; }
    }

    public class HorizontalResolutionConfigChangedEventArgs
    {
        public HorizontalResolutionConfiguration config;
        public HorizontalResolutionConfigChangedEventArgs(HorizontalResolutionConfiguration config)
        {
            this.config = config;
        }
    }

    /* Main window object, contains references to all of the visual components on the GUI display.
     * Manages adjustments of oscillscope horizontal and vertical resolution and trigger level
     * and type. 
     * Determines when to send configuration update commands to PSOC.
     */
    public partial class MainWindow : Window
    {
        private FunctionGeneratorConfiguration curFunctionGeneratorConfiguration = new FunctionGeneratorConfiguration();
        private FunctionGeneratorConfiguration nextFunctionGeneratorConfiguration = new FunctionGeneratorConfiguration();

        private OscopeConfiguration curOscopeConfiguration = new OscopeConfiguration();
        private OscopeConfiguration nextOscopeConfiguration = new OscopeConfiguration();

        public const int NUM_PERMANENT_OSCOPE_LINES = 8;
        public const int NUM_TIME_DIVISIONS = 4;
        public const int NUM_VOLTAGE_DIVISION_LINES = 4;

        /// <summary>
        /// Raised when the oscope width is set to new value.
        /// </summary>
        public event EventHandler<OscopeWidthChangedEventArgs> OscopeWidthChangedEvent;

        /// <summary>
        /// Raised when the oscope height is set to new value.
        /// </summary>
        public event EventHandler<OscopeHeightChangedEventArgs> OscopeHeightChangedEvent;

        /// <summary>
        /// Raised when the max sample size is set.
        /// </summary>
        public event EventHandler<MaxSampleSizeChangedEventArgs> MaxSampleSizeChangedEvent;

        /// <summary>
        /// Raised when the scaling applied to samples is set.
        /// </summary>
        public event EventHandler<SampleScalerChangedEventArgs> SampleScalerChangedEvent;

        /// <summary>
        /// Raised when the final offset applied to samples is set.
        /// </summary>
        public event EventHandler<SampleOffsetChangedEventArgs> SampleOffsetChangedEvent;

        public event EventHandler<TriggerHorizontalPositionChangedEventArgs> TriggerHorizontalPositionChangedEvent;

        public event EventHandler<TriggerLevelChangedEventArgs> TriggerLevelChangedEvent;

        public event EventHandler<COMPortSelectedEventArgs> COMPortSelectedEvent;

        public event EventHandler<HorizontalResolutionConfigChangedEventArgs> HorizonalResolutionConfigChangedEvent;

        SerialPortClient serialPortClient;

        private HorizontalResolutionConfiguration curHorizontalResolutionConfiguration;

        private List<Line> timeDivisionLines = new List<Line>();

        private void setupTimeDivisionLines()
        {
            for (int i = 0; i < NUM_TIME_DIVISIONS - 1; i++)
            {
                Line line = new Line();
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 1;
                timeDivisionLines.Add(line);
                this.oscope_window_canvas.Children.Add(line);
            }
        }

        private List<Line> voltageDivisionLines = new List<Line>();

        private void setupVoltageDivisionLines()
        {
            for(int i = 0; i < NUM_VOLTAGE_DIVISION_LINES - 1; i++)
            {
                Line line = new Line();
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 1;
                voltageDivisionLines.Add(line);
                this.oscope_window_canvas.Children.Add(line);
            }
        }

        private void createHorizontalResolutionConfigs()
        {
            this.timePerDivisionSelector.Items.Add(HorizontalResolutionConfiguration.builder()
                .withFrameSize(200)
                .withNumSamplesToDisplay(100)
                .withOscopeWindowSize(397)
                .withPixelSpacing(3)
                .withPsocSPS(0)
                .withTimePerDiv(0)
                .build());
        }

        public MainWindow()
        {
            InitializeComponent();

            Array.ForEach<string>(SerialPort.GetPortNames(), name => this.COM_port_comboBox.Items.Add(name));

            // Initialize PSOC sample receiving chain.
            OscopeWindowClient oscopeWindowClient  = new OscopeWindowClientImpl(this.oscope_window_canvas, this);
            SampleFrameDisplayerImpl sampleFrameDisplayer 
                = SampleFrameDisplayerImpl.newSampleFrameDisplayerImpl(oscopeWindowClient, this);
            SampleFrameAssembler sampleFrameAssembler = new SampleFrameAssemblerImpl(sampleFrameDisplayer);
            SampleAssembler sampleAssembler = HighByteFirstSampleAssemblerImpl.newHighByteFirstSampleAssemblerImpl(sampleFrameAssembler, this);
            ByteReceiverImpl byteReceiver = new ByteReceiverImpl(sampleAssembler, sampleFrameAssembler);
            serialPortClient = SerialPortClient.newSerialPortClient(byteReceiver, this);
            if (COMPortSelectedEvent == null) throw new Exception("didnt register");
            byteReceiver.PsocReadyEvent += PSOC_ready;

            HorizonalResolutionConfigChangedEvent += oscopeHorizontalResolutionConfigurationChanged;
            HorizonalResolutionConfigChangedEvent += updateHorizontalTriggeringSelector;

            setupTimeDivisionLines();
            setupVoltageDivisionLines();

            createHorizontalResolutionConfigs();

            HorizontalResolutionConfiguration defaultConfig
                = (HorizontalResolutionConfiguration)this.timePerDivisionSelector.Items[0];
            HorizonalResolutionConfigChangedEvent(this
                , new HorizontalResolutionConfigChangedEventArgs(defaultConfig));

            this.oscope_window_canvas.SizeChanged += oscopeActualSizeChanged;
            this.oscope_window_canvas.SizeChanged += updateTimeDivisionLines;
            this.oscope_window_canvas.SizeChanged += updateVoltageDivisionLines;

            TriggerLevelChangedEvent(this, new TriggerLevelChangedEventArgs(0));
            TriggerHorizontalPositionChangedEvent(this, new TriggerHorizontalPositionChangedEventArgs(0));
            OscopeHeightChangedEvent(this, new OscopeHeightChangedEventArgs(
                    (int)this.oscope_window_canvas.Height));
            MaxSampleSizeChangedEvent(this, new MaxSampleSizeChangedEventArgs(
                    4095));
            SampleScalerChangedEvent(this, new SampleScalerChangedEventArgs(
                    1));
            SampleOffsetChangedEvent(this, new SampleOffsetChangedEventArgs(
                    0.0));
        }
    }

    public partial class MainWindow
    {

        private void voltageOffsetSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SampleOffsetChangedEvent(this, new SampleOffsetChangedEventArgs(e.NewValue));
        }

        private void timePerDivisionSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HorizontalResolutionConfiguration config
                = (HorizontalResolutionConfiguration)this.timePerDivisionSelector.SelectedItem;
            oscopeHorizontalResolutionConfigurationChanged(this
                , new HorizontalResolutionConfigChangedEventArgs(config));
        }

        private void PSOC_ready(object sender, PsocReadyEventArgs args)
        {
            MessageBox.Show("PSOC device connected.");
        }

        private void voltsPerDivisionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SampleScalerChangedEvent != null)
            {
                SampleScalerChangedEvent(this, new SampleScalerChangedEventArgs(e.NewValue));
            }
        }

        private void updateTimeDivisionLines(object sender, SizeChangedEventArgs args)
        {
            int count = 1;
            foreach(Line line in timeDivisionLines)
            {
                line.X1 = line.X2 = (args.NewSize.Width / NUM_TIME_DIVISIONS) * count;
                line.Y1 = 0;
                line.Y2 = args.NewSize.Height;
                count++;
            }
        }

        private void updateVoltageDivisionLines(object sender, SizeChangedEventArgs args)
        {
            int count = 1;
            foreach(Line line in voltageDivisionLines)
            {
                line.X1 = 0;
                line.X2 = args.NewSize.Width;
                line.Y1 = line.Y2 = (args.NewSize.Height / NUM_VOLTAGE_DIVISION_LINES) * count;
                count++;
            }
        }

        private void COM_port_selection_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1)
            {
                throw new Exception(String.Format("selected number of items is {0}", e.AddedItems.Count));
            }
            COMPortSelectedEvent(this, new COMPortSelectedEventArgs(e.AddedItems[0].ToString()));
        }

        /// <summary>
        ///  event handler for DAC vpp slider updates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DAC_vpp_updated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            update_DAC_vpp((decimal)this.DAC_vpp_slider.Value);
        }

        private void oscopeActualSizeChanged(object sender
            , SizeChangedEventArgs args)
        {
            if(curHorizontalResolutionConfiguration.oscopeWindowSize != args.NewSize.Width)
            {
                throw new Exception(String.Format("what the heck. wanted oscope width to be {0}"
                    + " but it came out to be {1}"
                    , curHorizontalResolutionConfiguration.oscopeWindowSize
                    , args.NewSize.Width));
            }
        }

        private void updateHorizontalTriggeringSelector(object sender
            , HorizontalResolutionConfigChangedEventArgs args)
        {
            this.vertical_trigger_slider.Width = args.config.oscopeWindowSize;
            this.vertical_trigger_slider.Maximum = args.config.oscopeWindowSize;
        }
        
        private void oscopeHorizontalResolutionConfigurationChanged(object sender
            , HorizontalResolutionConfigChangedEventArgs args)
        {
            this.curHorizontalResolutionConfiguration = args.config;
        }

        // updates value of DAC vpp
        private void update_DAC_vpp(decimal newVpp)
        {
            nextFunctionGeneratorConfiguration.setVpp(newVpp);
            this.DAC_vpp_slider.Value = (double)nextFunctionGeneratorConfiguration.vpp;
            if (this.Vpp_text_display != null)
            {
                this.Vpp_text_display.Text
                    = nextFunctionGeneratorConfiguration.vpp.ToString(FunctionGeneratorConfiguration.VPP_FORMAT_STR);
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
            this.DAC_voffset_slider.Value = (double)nextFunctionGeneratorConfiguration.vOffset;
            if (this.Voffset_text_display != null)
            {
                this.Voffset_text_display.Text
                    = nextFunctionGeneratorConfiguration.vOffset.ToString(FunctionGeneratorConfiguration.VOFFSET_FORMAT_STR);
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
            this.DAC_duty_cycle.Value = nextFunctionGeneratorConfiguration.dutyCycle;
            if (this.duty_cycle_text_display != null)
            {
                this.duty_cycle_text_display.Text = nextFunctionGeneratorConfiguration.dutyCycle.ToString();
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
            this.DAC_frequency_slider.Value = nextFunctionGeneratorConfiguration.frequency;
            if (this.DAC_frequency_text_display != null)
            {
                this.DAC_frequency_text_display.Text
                    = nextFunctionGeneratorConfiguration.frequency.ToString();
            }
        }

        private void DAC_wavetype_selected(object sender, SelectionChangedEventArgs e)
        {
            nextFunctionGeneratorConfiguration.waveType = ((FunctionGeneratorConfiguration.WaveType)this.DAC_wave_type_list.SelectedIndex);
        }

        private void DAC_start_btn_click(object sender, RoutedEventArgs e)
        {
            serialPortClient.SendPsocCommand("#DA#");
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
                this.oscope_ksamples_slider.Value = nextOscopeConfiguration.kSamplesPerSecond;
                this.oscope_ksamples_text_display.Text = nextOscopeConfiguration.kSamplesPerSecond.ToString();
            }
        }

        private void start_oscope_btn_click(object sender, RoutedEventArgs e)
        {
            this.oscope_configuration_display.Text = nextOscopeConfiguration.getConfiguration();
            serialPortClient.SendPsocCommand("#AA#");
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

        private void trigger_slider_button_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(TriggerLevelChangedEvent != null)
            {
                TriggerLevelChangedEvent(this
                    , new TriggerLevelChangedEventArgs((int)(this.trigger_slider_button.Value)));

            }
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
            if (this.trigger_selection.SelectedIndex == 0)
            {
                //throw new NotImplementedException();
            }
            else if(this.trigger_selection.SelectedIndex == 1)
            {
                //throw new NotImplementedException();
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.DAC_config_command.Clear();
            this.DAC_config_command.Text = nextFunctionGeneratorConfiguration.getConfiguration();
            serialPortClient.SendPsocCommand("#" + nextFunctionGeneratorConfiguration.getConfiguration() + "#");
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            serialPortClient.SendPsocCommand("#PC_REQ_CONNECT#");
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            serialPortClient.SendPsocCommand("#DZ#");
        }

        private void vertical_trigger_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TriggerHorizontalPositionChangedEvent(this
                , new TriggerHorizontalPositionChangedEventArgs((uint)e.NewValue));
        }
    }
}
