
using System;

namespace SimpleOscope
{
    /// <summary>
    /// configuration for a horizontal time/div that tells
    /// resolution manager what ADC frequency to request, and what
    /// spacing to put between samples, in order to achieve a given 
    /// time / division
    /// This should be a constant.
    /// </summary>
    public class HorizontalResolutionConfiguration
    {
        /// <summary>number of picoseconds/division on oscope</summary>
        public ulong timePerDiv
        {
            get; set;
        }
        /// <summary>the SPS in hertz needed fro PSOC to achieve this</summary>
        public ulong psocSPS
        {
            get; set;
        }
        /// <summary>the number of "imaginary, vertical lines" needed
        /// between each sample displayed on oscope.</summary>
        public uint pixelSpacing
        {
            get; set;
        }
        /// <summary>
        /// The number of samples per frame needed to achieve this time/div.
        /// </summary>
        public uint frameSize { get; set; }

        /// <summary>
        /// Width of the oscope inwodw in pixels.
        /// </summary>
        public uint oscopeWindowSize { get; set; }

        /// <summary>
        /// The number of samples that should be displayed on the oscope screen at a time.
        /// </summary>
        public uint numSamplesToDispaly { get; set; }

        private HorizontalResolutionConfiguration()
        {
        }

        public override string ToString()
        {
            return this.timePerDiv + " seconds / division";
        }

        public static HorizontalResolutionConfigurationBuilder builder()
        {
            return new HorizontalResolutionConfigurationBuilder();
        }

        public class HorizontalResolutionConfigurationBuilder
        {
            private HorizontalResolutionConfiguration config;
            private long sum = 0;

            public HorizontalResolutionConfigurationBuilder()
            {
                this.config = new HorizontalResolutionConfiguration();
            }

            public HorizontalResolutionConfigurationBuilder withTimePerDiv(ulong timePerDiv)
            {
                this.config.timePerDiv = timePerDiv;
                sum += 1;
                return this;
            }

            public HorizontalResolutionConfigurationBuilder withPsocSPS(ulong psocSPS)
            {
                this.config.psocSPS = psocSPS;
                sum += 10;
                return this;
            }

            public HorizontalResolutionConfigurationBuilder withPixelSpacing(uint pixelSpacing)
            {
                this.config.pixelSpacing = pixelSpacing;
                sum += 100;
                return this;
            }

            public HorizontalResolutionConfigurationBuilder withFrameSize(uint frameSize)
            {
                this.config.frameSize = frameSize;
                sum += 1000;
                return this;
            }

            public HorizontalResolutionConfigurationBuilder withOscopeWindowSize(uint oscopeWindowSize)
            {
                this.config.oscopeWindowSize = oscopeWindowSize;
                sum += 10000;
                return this;
            }

            public HorizontalResolutionConfigurationBuilder withNumSamplesToDisplay(uint numSamplesToDisplay)
            {
                this.config.numSamplesToDispaly = numSamplesToDisplay;
                sum += 100000;
                return this;
            }

            public HorizontalResolutionConfiguration build()
            {
                if(this.sum != 111111)
                {
                    throw new Exception("unspecified parameters");
                }
                if(this.config.oscopeWindowSize 
                    != ((this.config.numSamplesToDispaly - 1) * (this.config.pixelSpacing + 1) + 1)) {
                    throw new ArgumentException("window size doesn't fit");
                }
                if(this.config.frameSize / 2 < this.config.numSamplesToDispaly)
                {
                    throw new ArgumentException("frame size not big enough");
                }
                return config;
            }
        }
    }
}
