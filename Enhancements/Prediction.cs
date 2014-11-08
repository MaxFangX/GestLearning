using System;
using System.Collections.Generic;

namespace KinectLibrary.Enhancements
{
    public class Prediction : IPrediction
    {
        /// <summary>
        /// Perdicts the next state of a hand based on previous observations. 
        /// It uses the exponential moving average of the observations for the prediction.
        /// </summary>
        /// <param name="handFrames">
        /// Observations over time. The observation order in the list is significant; 
        /// The oldest is first in the list and the newest is last.
        /// </param>
        /// <param name="weight">The weighing factor. This value must be greater than 0 and less than 1.</param>
        /// <returns>Returns a predicted hand state.</returns>
        public Hand PredictedHandState(List<Hand> handFrames, double weight)
        {
            if(weight < 0)
                throw new ArgumentOutOfRangeException("weight", "It must be greater than zero.");
            if (weight > 1)
                throw new ArgumentOutOfRangeException("weight", "It must be less than one.");

            Hand latestHand = handFrames[handFrames.Count - 1];

            Hand ema = EmaHandFrames(handFrames, weight);
            Hand futureHand = PredictedHand(latestHand, ema);

            return futureHand;
        }

        /// <summary>
        /// Calculates the exponential moving average over a specified list of hands.
        /// </summary>
        /// <param name="handFrames">
        /// Observations over time. The observation order in the list is significant; 
        /// The oldest is first in the list and the newest is last.
        /// </param>
        /// <param name="weight">The weighing factor. This value must be greater than 0 and less than 1.</param>
        /// <returns>Returns the exponential moving average of the list.</returns>
        private Hand EmaHandFrames(List<Hand> handFrames, double weight)
        {
            Hand prevEma = handFrames[0]; // EMA = Exponential Moving Average.

            for (int i = 0; i < handFrames.Count - 1; i++)
            {
                Hand currentPrediction = EmaHand(handFrames[i], prevEma, weight);
                prevEma = currentPrediction;
            }

            return prevEma; // The exponential moving average of the list.
        }

        /// <summary>
        /// Calculates the exponential moving average of two hand states.
        /// </summary>
        /// <param name="currentHand">Current hand state.</param>
        /// <param name="previousPrediction">Exponential moving average value of the previous hand state.</param>
        /// <param name="weight">The weighing factor. This value must be greater than 0 and less than 1.</param>
        /// <returns>Returns the exponential moving average of the current hand state.</returns>
        private Hand EmaHand(Hand currentHand, Hand previousPrediction, double weight)
        {
            Hand emaHand = new Hand();

            for (int j = 0; j < previousPrediction.FingerCount; j++)
            {
                emaHand.Fingers[j].Position = ExponentialMovingAverage(currentHand.Fingers[j].Position,
                                                                       previousPrediction.Fingers[j].Position, weight);

                emaHand.Fingers[j].Direction = ExponentialMovingAverage(currentHand.Fingers[j].Direction,
                                                                        previousPrediction.Fingers[j].Direction, weight);
            }

            return emaHand;
        }

        private Vector ExponentialMovingAverage(Vector currentObservation, Vector previousEma, double weight)
        {
            return weight * currentObservation + (1 - weight) * previousEma;
        }

        /// <summary>
        /// Predicts the next hand state based on the exponential moving average.
        /// </summary>
        /// <param name="currentHand">Current hand state.</param>
        /// <param name="emaHand">The the exponential moving average.</param>
        /// <returns>Returns a predicted hand state.</returns>
        private Hand PredictedHand(Hand currentHand, Hand emaHand)
        {
            Hand futureHand = new Hand();
            for (int i = 0; i < currentHand.FingerCount; i++)
            {
                futureHand.Fingers[i].Position = (currentHand.Fingers[i].Position - emaHand.Fingers[i].Position) + currentHand.Fingers[i].Position;
                futureHand.Fingers[i].Direction = (currentHand.Fingers[i].Direction - emaHand.Fingers[i].Direction) + currentHand.Fingers[i].Direction;
            }
            return futureHand;
        }
    }
}