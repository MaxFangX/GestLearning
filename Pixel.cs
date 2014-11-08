namespace KinectLibrary
{
    /// <summary>
    /// Pixel status.
    /// </summary>
    public enum Pixel
    {
        /// <summary>
        /// Pixel is not i the range interval.
        /// </summary>
        OutOfRange = 0,

        /// <summary>
        /// Pixel is in the range interval.
        /// </summary>
        InRange = 1,

        /// <summary>
        /// Pixel is not defined.
        /// </summary>
        Undefined = 2,
    }
}