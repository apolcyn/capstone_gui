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
using System.ComponentModel;

namespace SimpleOscope
{
    public class PSoCCommands
    {
        public const string PSOCDisonnectRequestCommand = "#PC_REQ_DISCONNECT#";
        public const string PSOCConnectRequestCommand = "#PC_REQ_CONNECT#";
    }

    public class PSoCDisconnectRequestEventArgs : EventArgs { }

    public class WritingToClosedSerialPortEventArgs : EventArgs
    {
        public string message { get; }
        public WritingToClosedSerialPortEventArgs(string message) { this.message = message; }
    }

    public class ErrorWritingToSerialPortEventArgs : EventArgs
    {
        public string message { get; }
        public ErrorWritingToSerialPortEventArgs(string message) { this.message = message; }
    }

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

    public class PixelVoltageRelationshipChangedEventArgs
    {
        public double lowerVoltage, upperVoltage;
        public int lowerPixel, upperPixel;

        public PixelVoltageRelationshipChangedEventArgs(double lowerVoltage, int lowerPixel, 
            double upperVoltage, int upperPixel)
        {
            this.lowerVoltage = lowerVoltage;
            this.upperVoltage = upperVoltage;
            this.lowerPixel = lowerPixel;
            this.upperPixel = upperPixel;
        }
    }

    public class SampleToVoltageRelationshipChangedEventArgs
    {
        public double lowerVoltage, upperVoltage;
        public int lowerSample, upperSample;
        public SampleToVoltageRelationshipChangedEventArgs(double lowerVoltage, int lowerSample, 
            double upperVoltage, int upperSample)
        {
            this.lowerVoltage = lowerVoltage;
            this.upperVoltage = upperVoltage;
            this.lowerSample = lowerSample;
            this.upperSample = upperSample;
        }
    }

    public class SampleFrameHookChangedEventArgs
    {
        public SampleFrameHook sampleFrameHook;
        public SampleFrameHookChangedEventArgs(SampleFrameHook sampleFrameHook)
        {
            this.sampleFrameHook = sampleFrameHook;
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

        public const int NUM_PERMANENT_OSCOPE_LINES = 9;
        public const int NUM_TIME_DIVISIONS = 4;
        public const int NUM_VOLTAGE_DIVISION_LINES = 4;

        public event EventHandler<PSoCDisconnectRequestEventArgs> PSoCDisconnectRequestEvent;

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

        public event EventHandler<PixelVoltageRelationshipChangedEventArgs> PixelVoltageRelationshipChangedEvent;

        public event EventHandler<SampleToVoltageRelationshipChangedEventArgs> SampleToVoltageRelationshipChangedEvent;

        public event EventHandler<SampleFrameHookChangedEventArgs> SampleFrameHookChangedEvent;

        SerialPortClient serialPortClient;

        private LinearInterpolator linearInterpolator = new LinearInterpolator();

        private HorizontalResolutionConfiguration curHorizontalResolutionConfiguration;

        private List<Line> timeDivisionLines = new List<Line>();

        private Label voltagMeasurementCursorLabel = new Label();

        private Label triggerValueLabel = new Label();

        private AutoScaler autoScaler = null;

        public const int INITIAL_LOWER_SAMPLE = 0, INITIAL_UPPER_SAMPLE = 4095;
        public const double INITIAL_LOWER_VOLTAGE = -5.0, INITIAL_UPPER_VOLTAGE = 5.0;

        private void initializeVoltageMeasurementCursor()
        {
            this.voltage_measurement_cursor_canvas.Children.Add(voltagMeasurementCursorLabel);
        }

        private void initializeTriggerValueLabelCanvas()
        {
            this.trigger_label_canvas.Children.Add(triggerValueLabel);
        }

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
            for (int i = 0; i < NUM_VOLTAGE_DIVISION_LINES - 1; i++)
            {
                Line line = new Line();
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 1;
                voltageDivisionLines.Add(line);
                this.oscope_window_canvas.Children.Add(line);
            }
        }

        private List<HorizontalResolutionConfiguration> horizontalConfigurations
            = new List<HorizontalResolutionConfiguration>();

        private void createHorizontalResolutionConfigs()
        {
            this.horizontalConfigurations.Add(HorizontalResolutionConfiguration.builder()
                 .withFrameSize(800)
                 .withNumSamplesToDisplay(400)
                 .withOscopeWindowSize(400)
                 .withPixelSpacing(0)
                 .withPsocSPS(50)
                 .withTimePerDiv(1000000)
                 .build());

            this.horizontalConfigurations.Add(HorizontalResolutionConfiguration.builder()
                 .withFrameSize(800)
                 .withNumSamplesToDisplay(400)
                 .withOscopeWindowSize(400)
                 .withPixelSpacing(0)
                 .withPsocSPS(250)
                 .withTimePerDiv(200000)
                 .build());

            this.horizontalConfigurations.Add(HorizontalResolutionConfiguration.builder()
                 .withFrameSize(800)
                 .withNumSamplesToDisplay(400)
                 .withOscopeWindowSize(400)
                 .withPixelSpacing(0)
                 .withPsocSPS(1000)
                 .withTimePerDiv(50000)
                 .build());

            this.horizontalConfigurations.Add(HorizontalResolutionConfiguration.builder()
                 .withFrameSize(800)
                 .withNumSamplesToDisplay(400)
                 .withOscopeWindowSize(400)
                 .withPixelSpacing(0)
                 .withPsocSPS(50000)
                 .withTimePerDiv(1000)
                 .build());

            this.horizontalConfigurations.Add(HorizontalResolutionConfiguration.builder()
                 .withFrameSize(800)
                 .withNumSamplesToDisplay(400)
                 .withOscopeWindowSize(400)
                 .withPixelSpacing(0)
                 .withPsocSPS(250000)
                 .withTimePerDiv(200)
                 .build());

            this.horizontalConfigurations.Add(HorizontalResolutionConfiguration.builder()
                 .withFrameSize(800)
                 .withNumSamplesToDisplay(200)
                 .withOscopeWindowSize(399)
                 .withPixelSpacing(1)
                 .withPsocSPS(1000000)
                 .withTimePerDiv(50)
                 .build());
        }

        private void setupWaveOptionDropDown()
        {
            this.DAC_wave_type_list.Items.Add(
                new FunctionGeneratorConfiguration.WaveType(
                    FunctionGeneratorConfiguration.WaveType.WaveName.NotSelected));

            this.DAC_wave_type_list.Items.Add(
                new FunctionGeneratorConfiguration.WaveType(
                    FunctionGeneratorConfiguration.WaveType.WaveName.Sine));

            this.DAC_wave_type_list.Items.Add(
                new FunctionGeneratorConfiguration.WaveType(
                    FunctionGeneratorConfiguration.WaveType.WaveName.Square));

            this.DAC_wave_type_list.Items.Add(
                new FunctionGeneratorConfiguration.WaveType(
                    FunctionGeneratorConfiguration.WaveType.WaveName.Sawtooth));

            this.DAC_wave_type_list.Items.Add(
                new FunctionGeneratorConfiguration.WaveType(
                    FunctionGeneratorConfiguration.WaveType.WaveName.Triangle));

            this.DAC_wave_type_list.Items.Add(
                new FunctionGeneratorConfiguration.WaveType(
                    FunctionGeneratorConfiguration.WaveType.WaveName.Arbitrary));
        }

        private bool connected = false;

        private void dumpFramesToFileButton_Click(object sender, RoutedEventArgs e)
        {
            DumpSamplesWindow dumpSamplesWindow 
                = new DumpSamplesWindow(
                    this.curHorizontalResolutionConfiguration.psocSPS + "sps.txt");

            if(dumpSamplesWindow.ShowDialog() == true)
            {
                string fileName = dumpSamplesWindow.Answer();
                this.fileDataDumper = new DataDumper(fileName);

                SampleFrameHookChangedEvent(this
                    , new SampleFrameHookChangedEventArgs(fileDataDumper.dumpNewFrame));
            }

            dumpSamplesWindow.Close();
        }

        private void rawSamplesMode_Click(object sender, RoutedEventArgs e)
        {
            SampleToVoltageRelationshipChangedEvent(this
                , new SampleToVoltageRelationshipChangedEventArgs(0, 0, 1, 1));
            PixelVoltageRelationshipChangedEvent(this
                , new PixelVoltageRelationshipChangedEventArgs(0, 0, 1, 1));
            TriggerLevelChangedEvent(this
                , new TriggerLevelChangedEventArgs(INITIAL_UPPER_SAMPLE / 2));
        }

        private void updateVoltageCursorLabelAfterPixelToVoltageUpdated(object sender
            , PixelVoltageRelationshipUpdatedEventArgs args)
        {
            updateVoltageMeasurementCursorDisplayText();
        }

        private void updateVoltageMeasurementCursorDisplayText()
        {
            double newLabelVerticalPosition = this.voltage_measurement_cursor_canvas.Height
                - this.voltage_measurement_slider.Value;
            Canvas.SetTop(this.voltagMeasurementCursorLabel, newLabelVerticalPosition);
            voltagMeasurementCursorLabel.Content = String.Format("{0} V"
                , linearInterpolator.pixelToVoltage((int)this.voltage_measurement_slider.Value).ToString("##.##"));

        }

        private void voltage_measurement_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            updateVoltageMeasurementCursorDisplayText();
        }

        private void time_per_division_selection_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            HorizontalResolutionConfiguration config
                = (HorizontalResolutionConfiguration)this.horizontalConfigurations[(int)this.time_per_division_selection_slider.Value];
            HorizonalResolutionConfigChangedEvent(this
                , new HorizontalResolutionConfigChangedEventArgs(config));
        }

        private void updateTimePerDivisionTextDisplay(object sender, HorizontalResolutionConfigChangedEventArgs args)
        {
            string[] timePrefixes = { "micro", "milli", "" };
            ulong divisor = 1000;
            int i;

            if(args.config.timePerDiv < 0 || args.config.timePerDiv > 1000000000)
            {
                throw new ArgumentException(
                    "expected time per div to be between 1 and 1000000000 microseconds");
            }

            for (i = 0; i < 4 && args.config.timePerDiv / divisor > 0; divisor *= 1000, i++)
                ;

            this.time_per_division_display.Content = String.Format("{0} {1}"
                , args.config.timePerDiv / (divisor / 1000), timePrefixes[i] + "second(s)");
        }

        private DataDumper fileDataDumper;

        private ObservableCollection<MenuItem> validCOMPorts = new ObservableCollection<MenuItem>();

        private void psoc_connect_button_click(object sender, RoutedEventArgs e)
        {
            serialPortClient.SendPsocCommand(PSoCCommands.PSOCConnectRequestCommand);
        }

        private void psoc_disconnect_button_click(object sender, RoutedEventArgs e)
        {
            PSoCDisconnectRequestEvent(this, new PSoCDisconnectRequestEventArgs());
        }

        private void initializeCOMPortDropDown()
        {
            this.com_port_selection.ItemsSource = validCOMPorts;
        }

        private PSOCMessageLogger psocMessageLogger;

        public const string MESSAGE_LOG_DIR = "./message_logs/";

        private void triggerFound(object sender, TriggerFoundEventArgs args)
        {
            this.autoScaler.IncrementTriggerCount();
        }

        private void frameAssembled(object sender, FrameAssembledEventArgs args)
        {
            this.autoScaler.IncrementFrameNumber();
        }

        private delegate void HorizontalConfigChangedDelegateOnUIThreadDelegate(int newHorizontalConfigIndex);

        private void horizontalConfigChangedeOnUIThread(int newHorizontalConfigIndex)
        {
            HorizonalResolutionConfigChangedEvent(this,
                        new HorizontalResolutionConfigChangedEventArgs(
                            horizontalConfigurations[newHorizontalConfigIndex]));
        }

        private void horizontalConfigChangedDelegate(int newHorizontalConfigIndex)
        {
            this.Dispatcher.BeginInvoke(
                new HorizontalConfigChangedDelegateOnUIThreadDelegate(horizontalConfigChangedeOnUIThread)
                , new object[] {newHorizontalConfigIndex});
        }

        private delegate void TriggerLevelChangedUIThreadDelegate(int newTriggerLevel);

        private void triggerLevelChangedUIThread(int newTriggerLevel)
        {
            TriggerLevelChangedEvent(this
                , new TriggerLevelChangedEventArgs(newTriggerLevel));
        }

        private void triggerLevelChangedDelegate(int newTriggerLevel)
        {
            this.Dispatcher.BeginInvoke(new TriggerLevelChangedUIThreadDelegate(triggerLevelChangedUIThread)
                , new object[] { newTriggerLevel });
        }

        private delegate void AutoScalingCompleteUIThreadDelegate(int bestTriggerLevel, int bestConfigIndex);

        private void autoScalingCompleteDelegateUIThread(int bestTriggerLevel, int bestConfigIndex)
        {
            horizontalConfigChangedDelegate(bestConfigIndex);
            triggerLevelChangedDelegate(bestTriggerLevel);
            this.sampleFrameDisplayer.TriggerFoundEvent -= this.triggerFound;
            this.sampleFrameDisplayer.FrameAssembledEvent -= this.frameAssembled;
            this.autoScaler = null;
            this.trigger_slider_button.Value = bestTriggerLevel;
            this.time_per_division_selection_slider.Value = bestConfigIndex;
        }

        private void autoScalingCompleteDelegate(int bestTriggerLevel, int bestConfigIndex)
        {
            this.Dispatcher.BeginInvoke(new AutoScalingCompleteDelegate(autoScalingCompleteDelegateUIThread)
                , new object[] { bestTriggerLevel, bestConfigIndex });
        }

        private void auto_scale_button_Click(object sender, RoutedEventArgs e)
        {
            List<int> triggersToTry = new List<int>();
            for(int i = (int)this.trigger_slider_button.Minimum
                ; i < (int)this.trigger_slider_button.Maximum; i += 50)
            {
                triggersToTry.Add(i);
            }

            List<int> configIndices = new List<int>(new int[] { 2, 3, 4 });

            int idealAveNumTriggers = 3;

            SampleScalerChangedEvent(this, new SampleScalerChangedEventArgs(1));
            SampleOffsetChangedEvent(this, new SampleOffsetChangedEventArgs(0.0));

            this.autoScaler = new AutoScaler(configIndices
                , triggerLevelChangedDelegate
                , horizontalConfigChangedDelegate
                , autoScalingCompleteDelegate
                , triggersToTry
                , idealAveNumTriggers);

            this.sampleFrameDisplayer.TriggerFoundEvent += this.triggerFound;
            this.sampleFrameDisplayer.FrameAssembledEvent += this.frameAssembled;
        }

        private void initializePSOCMessageLogger()
        {
            Directory.CreateDirectory(MESSAGE_LOG_DIR);
            string fileName = MESSAGE_LOG_DIR + "psoc-message-log-last-run" + System.DateTime.Now.Hour + ".txt";
            this.psocMessageLogger = new PSOCMessageLogger(fileName);
        }

        private SampleFrameDisplayerImpl sampleFrameDisplayer;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize PSOC sample receiving chain.
            OscopeWindowClient oscopeWindowClient  = new OscopeWindowClientImpl(this.oscope_window_canvas, this);
            this.sampleFrameDisplayer 
                = SampleFrameDisplayerImpl.newSampleFrameDisplayerImpl(oscopeWindowClient, this);
            SampleFrameAssembler sampleFrameAssembler = new SampleFrameAssemblerImpl(sampleFrameDisplayer);
            SampleAssembler sampleAssembler = HighByteFirstSampleAssemblerImpl.newHighByteFirstSampleAssemblerImpl(sampleFrameAssembler
                , this, this.linearInterpolator);
            ByteReceiverImpl byteReceiver = new ByteReceiverImpl(sampleAssembler, sampleFrameAssembler);
            serialPortClient = SerialPortClient.newSerialPortClient(byteReceiver, this);
            if (COMPortSelectedEvent == null) throw new Exception("didnt register");
            byteReceiver.PsocReadyEvent += PSOC_ready;

            HorizonalResolutionConfigChangedEvent += oscopeHorizontalResolutionConfigurationChanged;
            HorizonalResolutionConfigChangedEvent += updateHorizontalTriggeringSelector;
            HorizonalResolutionConfigChangedEvent += commandPSOCForNewSamplesPerFrame;
            HorizonalResolutionConfigChangedEvent += updateTimePerDivisionTextDisplay;

            initializeVoltageMeasurementCursor();
            initializeTriggerValueLabelCanvas();
            initializeCOMPortDropDown();
            initializePSOCMessageLogger();

            PixelVoltageRelationshipChangedEvent += linearInterpolator.pixelVoltageRelationshipChanged;

            SampleToVoltageRelationshipChangedEvent += linearInterpolator.sampleToVoltageRelationShipChanged;

            linearInterpolator.PixelVoltageRelationshipUpdatedEvent += updateVoltageOffsetDisplay;
            linearInterpolator.PixelVoltageRelationshipUpdatedEvent += updateTriggerCursorLabelFromPixelToVoltageUpdated;
            linearInterpolator.PixelVoltageRelationshipUpdatedEvent += updateVoltageCursorLabelAfterPixelToVoltageUpdated;
            linearInterpolator.PixelVoltageRelationshipUpdatedEvent += updateVoltagePerDivisionDisplay;
            SampleToVoltageRelationshipChangedEvent(this
                , new SampleToVoltageRelationshipChangedEventArgs(INITIAL_LOWER_VOLTAGE
                , INITIAL_LOWER_SAMPLE, INITIAL_UPPER_VOLTAGE, INITIAL_UPPER_SAMPLE));

            setupTimeDivisionLines();
            setupVoltageDivisionLines();

            createHorizontalResolutionConfigs();
            setupWaveOptionDropDown();

            serialPortClient.WroteCommandEvent += wroteCommandToPsoc;
            serialPortClient.ErrorWritingToSerialPortEvent += errorWritingToSerialPort;
            serialPortClient.WritingToClosedSerialPortEvent += writingToClosedSerialPort;

            HorizontalResolutionConfiguration defaultConfig
                = (HorizontalResolutionConfiguration)this.horizontalConfigurations[0];
            HorizonalResolutionConfigChangedEvent(this
                , new HorizontalResolutionConfigChangedEventArgs(defaultConfig));

            this.oscope_window_canvas.SizeChanged += oscopeActualSizeChanged;
            this.oscope_window_canvas.SizeChanged += updateTimeDivisionLines;
            this.oscope_window_canvas.SizeChanged += updateVoltageDivisionLines;
            this.oscope_window_canvas.SizeChanged += updateVoltagePixelRelationship;

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
        private void errorWritingToSerialPort(object sender, ErrorWritingToSerialPortEventArgs args)
        {
            MessageBox.Show("Error writing to serial Port. Error Message: {0}", args.message);
        }

        private void writingToClosedSerialPort(object sender, WritingToClosedSerialPortEventArgs args)
        {
            MessageBox.Show("PSoC disconnected. Writing to closed serial Port. Error Message: {0}"
                , args.message);
        }

        private void updateVoltageOffsetDisplay(object sender, PixelVoltageRelationshipUpdatedEventArgs args)
        {
            double newVoltageAtWindowBottom = linearInterpolator.pixelToVoltage((int)(this.oscope_window_canvas.ActualHeight / 2));

            this.voltage_at_window_bottom_display.Content = String.Format("{0} V"
                , newVoltageAtWindowBottom.ToString("##.##"));
        }

        private void updateVoltagePerDivisionDisplay(object sender, PixelVoltageRelationshipUpdatedEventArgs args)
        {
            double newVoltsPerDivision = linearInterpolator
                .pixelToVoltage((int)(this.oscope_window_canvas.Height / NUM_VOLTAGE_DIVISION_LINES))
                - linearInterpolator.pixelToVoltage(0);
            this.voltsPerDivisionDisplay.Content = String.Format("{0} V"
                , String.Format(newVoltsPerDivision.ToString("##.##"))); 
        }

        private void wroteCommandToPsoc(object sender, WroteCommandEventArgs args)
        {
            this.psocMessageLogger.appendMessage(args.command);
        }


        public void scanForCOMPorts(object sender, System.EventArgs args)
        {
            this.validCOMPorts.Clear();
            foreach(string name in SerialPort.GetPortNames())
            {
                MenuItem newMenuItem = new MenuItem();
                newMenuItem.Header = name;
                newMenuItem.Click += (o, e) => COMPortSelectedEvent(this
                    , new COMPortSelectedEventArgs(name));
                this.validCOMPorts.Add(newMenuItem);
            }
        }

        private void commandPSOCForNewSamplesPerFrame(object sender
            , HorizontalResolutionConfigChangedEventArgs args)
        {
            if (connected)
            {
                serialPortClient.SendPsocCommand(String.Format("#AS{0}#", args.config.psocSPS));
                Thread.Sleep(200);
                serialPortClient.SendPsocCommand(String.Format("#AA#"));
            }
        }

        private void updateVoltagePixelRelationship(object sender, SizeChangedEventArgs args)
        {
            PixelVoltageRelationshipChangedEvent(this
                , new PixelVoltageRelationshipChangedEventArgs(INITIAL_LOWER_VOLTAGE, 0
                , INITIAL_UPPER_VOLTAGE, (int)args.NewSize.Height));
        }

        private void voltageOffsetSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PixelVoltageRelationshipChangedEvent != null)
            {
                double voltagePerDivisionScaler = this.voltsPerDivisionSlider.Value;
                double voltageDisplayOffset = e.NewValue;
                double newLowerVoltage = (INITIAL_LOWER_VOLTAGE + voltageDisplayOffset)
                    * voltagePerDivisionScaler;
                double newUpperVoltage = (INITIAL_UPPER_VOLTAGE + voltageDisplayOffset)
                    * voltagePerDivisionScaler;
                int lowerPixel = 0;
                int upperPixel = (int)this.oscope_window_canvas.Height;

                PixelVoltageRelationshipChangedEvent(this
                    , new PixelVoltageRelationshipChangedEventArgs(newLowerVoltage, lowerPixel
                    , newUpperVoltage, upperPixel));
            }
        }

        private void PSOC_ready(object sender, PsocReadyEventArgs args)
        {
            MessageBox.Show("PSOC device connected.");
            connected = true;
        }

        private void voltsPerDivisionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PixelVoltageRelationshipChangedEvent != null)
            {
                double voltagePerDivisionScaler = e.NewValue;
                double voltageDisplayOffset = this.voltageOffsetSlider.Value;
                double newLowerVoltage = (INITIAL_LOWER_VOLTAGE + voltageDisplayOffset)
                    * voltagePerDivisionScaler;
                double newUpperVoltage = (INITIAL_UPPER_VOLTAGE + voltageDisplayOffset)
                    * voltagePerDivisionScaler;
                int lowerPixel = 0;
                int upperPixel = (int)(this.oscope_window_canvas.Height);

                PixelVoltageRelationshipChangedEvent(this
                    , new PixelVoltageRelationshipChangedEventArgs(newLowerVoltage, lowerPixel
                    , newUpperVoltage, upperPixel));
            }
        }

        private void updateTimeDivisionLines(object sender, SizeChangedEventArgs args)
        {
            int count = 1;
            foreach(Line line in timeDivisionLines)
            {
                line.X1 = line.X2 = Math.Ceiling(args.NewSize.Width / NUM_TIME_DIVISIONS) * count;
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
            nextFunctionGeneratorConfiguration.waveType 
                = ((FunctionGeneratorConfiguration.WaveType)this.DAC_wave_type_list.SelectedItem);
            if (serialPortClient != null  && nextFunctionGeneratorConfiguration.waveType.waveLetter != '\0')
            {
                string temp = String.Format("#DW{0}#"
                    , nextFunctionGeneratorConfiguration.getWaveLetter());
                serialPortClient.SendPsocCommand(temp);
            }
        }

        private void DAC_start_btn_click(object sender, RoutedEventArgs e)
        {
            serialPortClient.SendPsocCommand("#DA#");
        }

        private void start_oscope_btn_click(object sender, RoutedEventArgs e)
        {
            serialPortClient.SendPsocCommand("#AA#");
        }

        private void DAC_Frequency_update_btn_click(object sender, RoutedEventArgs e)
        {
            string[] vals = this.DAC_frequency_text_display.Text.Split();
            update_DAC_frequency(int.Parse(vals[0]));
            if (serialPortClient != null)
            {
                serialPortClient.SendPsocCommand(String.Format("#DF{0}#"
                , nextFunctionGeneratorConfiguration.frequency));
            }
        }

        private void Vpp_update_btn_Click(object sender, RoutedEventArgs e)
        {
            update_DAC_vpp(decimal.Parse(this.Vpp_text_display.Text.Split()[0]));
            if (serialPortClient != null)
            {
                serialPortClient.SendPsocCommand(String.Format("#DV{0}#"
                , nextFunctionGeneratorConfiguration.vpp));
            }
        }

        private void Voffset_update_btn_Click(object sender, RoutedEventArgs e)
        {
            update_DAC_voffset(decimal.Parse(this.Voffset_text_display.Text.Split()[0]));
        }

        private void duty_cycle_update_btn_Click(object sender, RoutedEventArgs e)
        {
            update_DAC_duty_cycle(int.Parse(this.duty_cycle_text_display.Text.Split()[0]));
            if (serialPortClient != null)
            {
                serialPortClient.SendPsocCommand(String.Format("#DD{0}#"
                , nextFunctionGeneratorConfiguration.dutyCycle));
            }
        }

        private void updateTriggerCursorLabelFromPixelToVoltageUpdated(object sender
            , PixelVoltageRelationshipUpdatedEventArgs args)
        {
            updateTriggerValueLabel();
        }

        private void updateTriggerValueLabel()
        {
            double newVerticalPosition = this.trigger_label_canvas.Height 
                - this.trigger_slider_button.Value;
            Canvas.SetTop(this.triggerValueLabel, newVerticalPosition);
            triggerValueLabel.Content = String.Format("{0} V"
                , linearInterpolator.pixelToVoltage((int)this.trigger_slider_button.Value).ToString("##.##"));
        }

        private void trigger_slider_button_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            updateTriggerValueLabel();

            if (TriggerLevelChangedEvent != null)
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

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            serialPortClient.SendPsocCommand("#" + nextFunctionGeneratorConfiguration.getConfiguration() + "#");
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

        private void window_closing(object sender, CancelEventArgs e)
        {
            PSoCDisconnectRequestEvent(this, new PSoCDisconnectRequestEventArgs());
        }
    }
}
