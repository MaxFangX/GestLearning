//------------------------------------------------------------------------------
// <copyright file="GestureResultView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.DiscreteGestureBasics
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Collections.Generic;

    /// <summary>
    /// Stores discrete gesture results for the GestureDetector.
    /// Properties are stored/updated for display in the UI.
    /// </summary>
    public sealed class GestureResultView : INotifyPropertyChanged
    {
        /// <summary> Image to show when the 'detected' property is true for a tracked body </summary>
        private readonly ImageSource seatedImage = new BitmapImage(new Uri(@"Images\Seated.png", UriKind.Relative));

        /// <summary> Image to show when the 'detected' property is false for a tracked body </summary>
        private readonly ImageSource notSeatedImage = new BitmapImage(new Uri(@"Images\NotSeated.png", UriKind.Relative));

        /// <summary> Image to show when the body associated with the GestureResultView object is not being tracked </summary>
        private readonly ImageSource notTrackedImage = new BitmapImage(new Uri(@"Images\NotTracked.png", UriKind.Relative));

        /// <summary> Array of brush colors to use for a tracked body; array position corresponds to the body colors used in the KinectBodyView class </summary>
        private readonly Brush[] trackedColors = new Brush[] { Brushes.Red, Brushes.Orange, Brushes.Green, Brushes.Blue, Brushes.Indigo, Brushes.Violet };

        /// <summary> Brush color to use as background in the UI </summary>
        private Brush bodyColor = Brushes.Gray;

        /// <summary> The body index (0-5) associated with the current gesture detector </summary>
        private int bodyIndex = 0;

        /// <summary> Current confidence value reported by the discrete gesture </summary>
        private float confidence = 0.0f;

        private string output = "unchanged value";

        /// <summary> True, if the discrete gesture is currently being detected </summary>
        private bool detected = false;
        private Dictionary<string, bool> detectedDictionary = new Dictionary<string, bool>();
        private void UpdateDetectedDictionary(string name, bool detected)
        {
            if (detectedDictionary.ContainsKey(name))
            {
                detectedDictionary[name] = detected;
            }
            else
            {
                detectedDictionary.Add(name, detected);
            }
        }

        /// <summary> Image to display in UI which corresponds to tracking/detection state </summary>
        private ImageSource imageSource = null;
        
        /// <summary> True, if the body is currently being tracked </summary>
        private bool isTracked = false;

        //Confidence Dictionary
        private Dictionary<string, float> confidenceDictionary = new Dictionary<string, float>();

        private void UpdateDictionary(string name, float confidence)
        {
            if (confidenceDictionary.ContainsKey(name))
            {
                confidenceDictionary[name] = confidence;
            }
            else
            {
                confidenceDictionary.Add(name, confidence);
            }
        }

        //Hardcoded
        private readonly string haltName = "Halt";
        private readonly string hiName = "Hi";
        private readonly string weName = "We";
        private readonly string loveName = "Love";
        private readonly string youName = "You";
        private readonly string byeName = "Bye";
        private readonly string hackSCName = "HackSC";

        /// <summary>
        /// Initializes a new instance of the GestureResultView class and sets initial property values
        /// </summary>
        /// <param name="bodyIndex">Body Index associated with the current gesture detector</param>
        /// <param name="isTracked">True, if the body is currently tracked</param>
        /// <param name="detected">True, if the gesture is currently detected for the associated body</param>
        /// <param name="confidence">Confidence value for detection of the 'Seated' gesture</param>
        public GestureResultView(int bodyIndex, bool isTracked, bool detected, float confidence)
        {
            this.BodyIndex = bodyIndex;
            this.IsTracked = isTracked;
            this.Detected = detected;
            this.Confidence = confidence;
            this.ImageSource = this.notTrackedImage;
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> 
        /// Gets the body index associated with the current gesture detector result 
        /// </summary>
        public int BodyIndex
        {
            get
            {
                return this.bodyIndex;
            }

            private set
            {
                if (this.bodyIndex != value)
                {
                    this.bodyIndex = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets the body color corresponding to the body index for the result
        /// </summary>
        public Brush BodyColor
        {
            get
            {
                return this.bodyColor;
            }

            private set
            {
                if (this.bodyColor != value)
                {
                    this.bodyColor = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets a value indicating whether or not the body associated with the gesture detector is currently being tracked 
        /// </summary>
        public bool IsTracked 
        {
            get
            {
                return this.isTracked;
            }

            private set
            {
                if (this.IsTracked != value)
                {
                    this.isTracked = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public string Output
        {
            get
            {
                return this.output;
            }
            private set
            {
                if (this.output != value)
                {
                    this.output = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets a value indicating whether or not the discrete gesture has been detected
        /// </summary>
        public bool Detected 
        {
            get
            {
                return this.detected;
            }

            private set
            {
                if (this.detected != value)
                {
                    this.detected = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool HaltDetected
        {
            get
            {
                if (detectedDictionary.ContainsKey(haltName))
                {
                    return detectedDictionary[haltName];
                }
                return false;
            }

            private set
            {
                if (this.detectedDictionary[haltName] != value)
                {
                    UpdateDetectedDictionary(haltName, value);
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool HiDetected
        {
            get
            {
                if (detectedDictionary.ContainsKey(hiName))
                {
                    return detectedDictionary[hiName];
                }
                return false;
            }

            private set
            {
                if (this.detectedDictionary[hiName] != value)
                {
                    UpdateDetectedDictionary(hiName, value);
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool WeDetected
        {
            get
            {
                if (detectedDictionary.ContainsKey(weName))
                {
                    return detectedDictionary[weName];
                }
                return false;
            }

            private set
            {
                if (this.detectedDictionary[weName] != value)
                {
                    UpdateDetectedDictionary(weName, value);
                    this.NotifyPropertyChanged();
                }
            }
        }
        public bool LoveDetected
        {
            get
            {
                if (detectedDictionary.ContainsKey(loveName))
                {
                    return detectedDictionary[loveName];
                }
                return false;
            }

            private set
            {
                if (this.detectedDictionary[loveName] != value)
                {
                    UpdateDetectedDictionary(loveName, value);
                    this.NotifyPropertyChanged();
                }
            }
        }
        public bool YouDetected
        {
            get
            {
                if (detectedDictionary.ContainsKey(youName))
                {
                    return detectedDictionary[youName];
                }
                return false;
            }

            private set
            {
                if (this.detectedDictionary[youName] != value)
                {
                    UpdateDetectedDictionary(youName, value);
                    this.NotifyPropertyChanged();
                }
            }
        }
        public bool ByeDetected
        {
            get
            {
                if (detectedDictionary.ContainsKey(byeName))
                {
                    return detectedDictionary[byeName];
                }
                return false;
            }

            private set
            {
                if (this.detectedDictionary[byeName] != value)
                {
                    UpdateDetectedDictionary(byeName, value);
                    this.NotifyPropertyChanged();
                }
            }
        }
        public bool HackSCDetected
        {
            get
            {
                if (detectedDictionary.ContainsKey(hackSCName))
                {
                    return detectedDictionary[hackSCName];
                }
                return false;
            }

            private set
            {
                if (this.detectedDictionary[hackSCName] != value)
                {
                    UpdateDetectedDictionary(hackSCName, value);
                    this.NotifyPropertyChanged();
                }
            }
        }



        /// <summary> 
        /// Gets a float value which indicates the detector's confidence that the gesture is occurring for the associated body 
        /// </summary>
        public float Confidence
        {
            get
            {
                return this.confidence;
            }

            private set
            {
                if (this.confidence != value)
                {
                    this.confidence = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public float HaltConfidence
        {
            get
            {
                if (this.confidenceDictionary.ContainsKey(haltName))
                {
                    return this.confidenceDictionary[haltName];
                }
                return 0.0f;
            }

            private set
            {
                if (this.confidenceDictionary[haltName] != value)
                {
                    this.confidenceDictionary[haltName] = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public float HiConfidence
        {
            get
            {
                if (this.confidenceDictionary.ContainsKey(hiName))
                {
                    return this.confidenceDictionary[hiName];
                }
                return 0.0f;
            }

            private set
            {
                if (this.confidenceDictionary[hiName] != value)
                {
                    this.confidenceDictionary[hiName] = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public float WeConfidence
        {
            get
            {
                if (this.confidenceDictionary.ContainsKey(weName))
                {
                    return this.confidenceDictionary[weName];
                }
                return 0.0f;
            }

            private set
            {
                if (this.confidenceDictionary[weName] != value)
                {
                    this.confidenceDictionary[weName] = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public float LoveConfidence
        {
            get
            {
                if (this.confidenceDictionary.ContainsKey(loveName))
                {
                    return this.confidenceDictionary[loveName];
                }
                return 0.0f;
            }

            private set
            {
                if (this.confidenceDictionary[loveName] != value)
                {
                    this.confidenceDictionary[loveName] = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public float YouConfidence
        {
            get
            {
                if (this.confidenceDictionary.ContainsKey(youName))
                {
                    return this.confidenceDictionary[youName];
                }
                return 0.0f;
            }

            private set
            {
                if (this.confidenceDictionary[youName] != value)
                {
                    this.confidenceDictionary[youName] = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public float ByeConfidence
        {
            get
            {
                if (this.confidenceDictionary.ContainsKey(byeName))
                {
                    return this.confidenceDictionary[byeName];
                }
                return 0.0f;
            }

            private set
            {
                if (this.confidenceDictionary[byeName] != value)
                {
                    this.confidenceDictionary[byeName] = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public float HackSCConfidence
        {
            get
            {
                if (this.confidenceDictionary.ContainsKey(hackSCName))
                {
                    return this.confidenceDictionary[hackSCName];
                }
                return 0.0f;
            }

            private set
            {
                if (this.confidenceDictionary[hackSCName] != value)
                {
                    this.confidenceDictionary[hackSCName] = value;
                    this.NotifyPropertyChanged();
                }
            }
        }


        /// <summary> 
        /// Gets an image for display in the UI which represents the current gesture result for the associated body 
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.imageSource;
            }

            private set
            {
                if (this.ImageSource != value)
                {
                    this.imageSource = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Updates the values associated with the discrete gesture detection result
        /// </summary>
        /// <param name="isBodyTrackingIdValid">True, if the body associated with the GestureResultView object is still being tracked</param>
        /// <param name="isGestureDetected">True, if the discrete gesture is currently detected for the associated body</param>
        /// <param name="detectionConfidence">Confidence value for detection of the discrete gesture</param>
        public void UpdateGestureResult(bool isBodyTrackingIdValid, string gestureName, bool isGestureDetected, float detectionConfidence)
        {
            this.IsTracked = isBodyTrackingIdValid;
            this.Confidence = 0.0f;

            if (!this.IsTracked)
            {
                //this.ImageSource = this.notTrackedImage;
                this.Detected = false;
                this.BodyColor = Brushes.Gray;
            }
            else
            {
                this.Detected = isGestureDetected;
                UpdateDetectedDictionary(gestureName, isGestureDetected);
                this.BodyColor = this.trackedColors[this.BodyIndex];
                UpdateDictionary(gestureName, detectionConfidence);

                Random random = new Random();
                int randomNumber = random.Next(75, 95);
                this.output = "Grade: " + randomNumber + "\n";
                foreach (string entry in detectedDictionary.Keys)
                {
                    if (this.detectedDictionary[entry] == true)
                    {
                        this.output += gestureName + "\n";
                    }
                }

                //if (this.Detected)
                //{
                //    this.ImageSource = this.seatedImage;
                //}
                //else
                //{
                //    this.ImageSource = this.notSeatedImage;
                //}
            }
        }

        /// <summary>
        /// Notifies UI that a property has changed
        /// </summary>
        /// <param name="propertyName">Name of property that has changed</param> 
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
