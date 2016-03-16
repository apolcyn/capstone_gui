using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope
{
    /* An object that contains all of the configuration relevant to the function generator.
    Always have a configuration that has been send and is currently being used, as well as a "next configuration", 
    which s being built up. */
    public class FunctionGeneratorConfiguration
    {
        // First letter of configuration command for function generator
        const char FIRST_CHAR = 'D';

        const char START_COMMAND = 'A';
        const char STOP_COMMAND = 'Z';

        public WaveType waveType;

        public class WaveType
        {
            public enum WaveName { Sine, Square, Sawtooth, Triangle, Arbitrary, NotSelected };
            private char[] waveNameCommands = { 'I', 'Q', 'W', 'T', 'A', '\0' };

            public char waveLetter {get;}
            public WaveName name;

            public override string ToString()
            {
                return this.name.ToString();
            }

            public WaveType(WaveName name)
            {
                this.waveLetter = waveNameCommands[(int)name];
                this.name = name;
            }
        }

        // Wave types and the corresponding letters to configure them
        const char WAVE_TYPE_COMMAND = 'W';
        WaveType DEFAULT_WAVE_TYPE = new WaveType(WaveType.WaveName.Sine);

        public decimal vpp { get; set; }

        public const decimal MIN_VPP = 0;
        public const decimal MAX_VPP = 4;
        public const string VPP_FORMAT_STR = "F2"; // precion of 10^-2
        public const decimal DEFAULT_VPP = 0;
        const char VPP_COMMAND = 'V';

        public int frequency { get; set; }

        public const int MIN_FREQ = 0;
        public const int MAX_FREQ = 240000;
        const int DEFAULT_FREQ = 1000;
        public const char FREQUENCY_COMMAND = 'F';

        public decimal vOffset { get; set; }

        public const decimal MIN_OFFSET = 0;
        public const decimal MAX_OFFSET = 4;
        public const string VOFFSET_FORMAT_STR = "F2"; // precision of 10^-2
        const decimal DEFAULT_OFFSET = 0;
        public const char VOFFSET_COMMAND = 'O';

        public int dutyCycle { get; set; }

        const int MIN_DUTY_CYCLE = 0;
        const int MAX_DUTY_CYCLE = 100;
        const int DEFAULT_DUTY_CYCLE = 50;
        const char DUTY_CYCLE_COMMAND = 'D';

        public FunctionGeneratorConfiguration()
        {
            dutyCycle = DEFAULT_DUTY_CYCLE;
            frequency = DEFAULT_FREQ;
            vOffset = DEFAULT_OFFSET;
            vpp = DEFAULT_VPP;
            waveType = DEFAULT_WAVE_TYPE;
        }

        public string getConfiguration()
        {
            // start commands
            StringBuilder config = new StringBuilder();
            config.Append(FIRST_CHAR);

            // wave type configuration
            config.Append(WAVE_TYPE_COMMAND);
            config.Append(waveType.waveLetter);

            // vpp configuration
            config.Append(VPP_COMMAND);
            config.Append(vpp.ToString(VPP_FORMAT_STR));

            // frequency configuration
            config.Append(FREQUENCY_COMMAND);
            config.Append(frequency);

            // vOffset configuration
            config.Append(VOFFSET_COMMAND);
            config.Append(vOffset.ToString(VOFFSET_FORMAT_STR));

            // duty cycle configuration
            config.Append(DUTY_CYCLE_COMMAND);
            config.Append(dutyCycle);

            return config.ToString();
        }

        public bool setVpp(decimal vpp)
        {
            if (vpp < MIN_VPP || vpp > MAX_VPP)
            {
                return false;
            }

            this.vpp = vpp;
            return true;
        }

        public bool setFrequency(int frequency)
        {
            if (frequency < MIN_FREQ || frequency > MAX_FREQ)
            {
                return false;
            }
            this.frequency = frequency;

            return true;
        }

        public bool setVOffset(decimal vOffset)
        {
            if (vOffset < MIN_OFFSET || vOffset > MAX_OFFSET)
            {
                return false;
            }
            this.vOffset = vOffset;

            return true;
        }

        public bool setDutyCycle(int dutyCycle)
        {
            if (dutyCycle < MIN_DUTY_CYCLE || dutyCycle > MAX_DUTY_CYCLE)
            {
                return false;
            }
            this.dutyCycle = dutyCycle;
            return true;
        }

        public char getWaveLetter()
        {
            return this.waveType.waveLetter;
        }
    }
}
