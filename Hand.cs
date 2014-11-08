using System.Collections.Generic;
using System.Linq;

namespace KinectLibrary
{
    public class Hand
    {
        /// <summary>
        /// Returns a vector value which represents a finger that is not on the hand.
        /// </summary>
        public static readonly Vector FingerNotFound = Vector.Thousand;

        private readonly List<Fingertip> fingers;

        public Hand(IEnumerable<Fingertip> fingers)
        {
            this.fingers = new List<Fingertip>(5);
            this.fingers.AddRange(fingers);
            //InitalizeFingerList();
        }

        public Hand()
        {
            fingers = new List<Fingertip>(5);
            InitalizeFingerList();
        }

        private void InitalizeFingerList()
        {
            for (int i = fingers.Count; i < 5; i++)
                fingers.Add(new Fingertip {Direction = FingerNotFound, Position = FingerNotFound});
        }

        public List<Fingertip> Fingers { get { return fingers; } }

        public Fingertip Thumb
        {
            get { return FindFinger(Finger.ThumbID); }
            set { fingers[Finger.ThumbID] = value; }
        }

        public Fingertip IndexFinger
        {
            get { return FindFinger(Finger.IndexFingerID); }
            set { fingers[Finger.IndexFingerID] = value; }
        }

        public Fingertip MiddleFinger
        {
            get { return FindFinger(Finger.MiddleFingerID); }
            set { fingers[Finger.MiddleFingerID] = value; }
        }

        public Fingertip RingFinger
        {
            get { return FindFinger(Finger.RingFingerID); }
            set { fingers[Finger.RingFingerID] = value; }
        }

        public Fingertip LittleFinger
        {
            get { return FindFinger(Finger.LittleFingerID); }
            set { fingers[Finger.LittleFingerID] = value; }
        }
        
        private Fingertip FindFinger(int fingerID)
        {
            if (FingerCount <= fingerID)
                return null;

            return fingers[fingerID];
        }

        public bool HasFinger(int fingerID)
        {
            return FindFinger(fingerID) != null && FindFinger(fingerID).Position != Hand.FingerNotFound;
        }

        public static bool HasEqualFingers(Hand handOne, Hand handTwo)
        {
            if (handOne.FingerCount != handTwo.FingerCount)
                return false;

            for (int i = 0; i < Finger.FingerCount; i++)
            {
                if (handOne.HasFinger(i) != handTwo.HasFinger(i))
                    return false;
            }

            return true;
        }

        public static bool HasEqualFingers(Hand handOne, Hand handTwo, out List<int> missingFingerID)
        {
            missingFingerID = new List<int>(5);

            for (int i = 0; i < Finger.FingerCount; i++)
            {
                if (handOne.HasFinger(i) != handTwo.HasFinger(i))
                {
                    missingFingerID.Add(i);
                }
            }

            return missingFingerID.Count == 0;
        }

        public int FingerCount
        {
            get
            {
                return Fingers.Count(fingertip => fingertip.Position.X != FingerNotFound.X);
            }
        }

        public static int MaxFingers { get { return 5; } } // Max fingers per hand.

    }

    /// <summary>
    /// Finger indexes.
    /// </summary>
    public static class Finger
    {
        public const int ThumbID        = 0;
        public const int IndexFingerID  = 1;
        public const int MiddleFingerID = 2;
        public const int RingFingerID   = 3;
        public const int LittleFingerID = 4;
        public const int FingerCount    = 5;
    }
}