using System.Collections.Generic;

namespace KinectLibrary.Enhancements
{
    public interface IPrediction
    {
        /// <summary>
        /// Perdicts the next state of a hand based on previous observations.
        /// </summary>
        /// <param name="handFrames">
        /// Observations over time. The observation order in the list is significant; 
        /// The oldest is first in the list and the newest is last.
        /// </param>
        /// <param name="weight">The weighing factor. This value must be greater than 0 and less than 1.</param>
        /// <returns>Returns a predicted hand state.</returns>
        Hand PredictedHandState(List<Hand> handFrames, double weight);
    }
}