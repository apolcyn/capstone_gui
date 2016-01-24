
namespace SimpleOscope
{
    /// <summary>
    /// configuration for a horizontal time/div that tells
    /// resolution manager what ADC frequency to request, and what
    /// spacing to put between samples, in order to achieve a given 
    /// time / division
    /// This should be a constant.
    /// </summary>
    public struct HorizontalResolutionConfiguration
    {
        /// <summary>number of picoseconds/division on oscope</summary>
        public ulong timePerDiv
        {
            get;
        }
        /// <summary>the SPS in hertz needed fro PSOC to achieve this</summary>
        public ulong psocSPS
        {
            get;
        }
        /// <summary>the number of "imaginary, vertical lines" needed
        /// between each sample displayed on oscope.</summary>
        public uint pixelSpacing
        {
            get;
        }
        /// <summary>
        /// The number of samples per frame needed to achieve this time/div.
        /// </summary>
        public uint frameSize
        {
            get;
        }

        /// <summary>
        /// Creates configuration constants to use for scaling horizontal resolution
        /// </summary>
        /// <param name="timePerDiv">time/div in picoseconds</param>
        /// <param name="psocSPS">samples per second in Hz</param>
        /// <param name="pixelSpacing">number of pixels - 1 between sample pixels</param>
        /// <param name="frameSize">number of samples per frame needed</param>
        public HorizontalResolutionConfiguration(ulong timePerDiv
            , ulong psocSPS, uint pixelSpacing, uint frameSize)
        {
            this.timePerDiv = timePerDiv;
            this.psocSPS = psocSPS;
            this.pixelSpacing = pixelSpacing;
            this.frameSize = frameSize;
        }
    }
}
