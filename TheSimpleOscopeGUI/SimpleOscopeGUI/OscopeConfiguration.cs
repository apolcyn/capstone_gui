using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOscope
{
    /* An object that contains all of the configuration relevant to the oscope.
    Always have a configuration that has been send and is currently being used, as well as a "next configuration", 
    which s being built up. */
    public class OscopeConfiguration
    {
        const char FIRST_CHAR = 'A';

        const char START_COMMAND = 'A';
        const char STOP_COMMAND = 'Z';

        public int resolution { get; set; }

        int[] resolutionOptions = { 8, 10, 12 };
        const int DEFAULT_RESOLUTION = 8;
        char RESOLUTION_COMMAND = 'R';

        public int kSamplesPerSecond { get; set; }

        const int KSAMPLES_PER_SECOND_MIN = 56;
        const int KSAMPLES_PER_SECOND_MAX = 1000;
        const int KSAMPLES_PER_SECOND_DEFAULT = 56;
        const char KSAMPLES_PER_SECOND_COMMAND = 'S';

        const int SAMPLES_PER_FRAME = 1000;
        const char SAMPLES_PER_FRAME_COMMAND = 'F';

        public int triggerLevel { get; set; }

        const int MIN_TRIGGER_LEVEL = 0;
        const int MAX_TRIGGER_LEVEL = 300;
        const int DEFAULT_TRIGGER_LEVEL = 150;

        public OscopeConfiguration()
        {
            kSamplesPerSecond = KSAMPLES_PER_SECOND_DEFAULT;
            resolution = DEFAULT_RESOLUTION;
            triggerLevel = DEFAULT_TRIGGER_LEVEL;
        }

        public string getConfiguration()
        {
            StringBuilder config = new StringBuilder();

            config.Append(FIRST_CHAR);

            config.Append(RESOLUTION_COMMAND);
            config.Append(resolution);

            config.Append(KSAMPLES_PER_SECOND_COMMAND);
            config.Append(kSamplesPerSecond);

            return config.ToString();
        }

        public bool setResolution(int resolutionIndex)
        {
            if (resolutionIndex < 0 || resolutionIndex > resolutionOptions.Length)
            {
                return false;
            }

            this.resolution = resolutionOptions[resolutionIndex];
            return true;
        }

        public bool setTriggerLevel(int triggerLevel)
        {
            if (triggerLevel > MAX_TRIGGER_LEVEL || triggerLevel < MIN_TRIGGER_LEVEL)
            {
                return false;
            }
            this.triggerLevel = triggerLevel;
            return true;
        }

        public bool setkSamplesPerSecond(int kSamplesPerSecond)
        {
            if (kSamplesPerSecond < KSAMPLES_PER_SECOND_MIN || kSamplesPerSecond > KSAMPLES_PER_SECOND_MAX)
            {
                return false;
            }

            this.kSamplesPerSecond = kSamplesPerSecond;
            return true;
        }
    }
}
