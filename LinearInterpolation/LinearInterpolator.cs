namespace LinearInterpolation
{
    public static class LinearInterpolator
    {
        /// <summary>
        /// This method calculates the percentage speed using a quadratic function. This calculation is used below 'lowerThreshold' 
        /// and above 'upperThreshold'. If 'actualSample' is between the thresholds, the speed is 100%.
        /// This helps you think: https://rechneronline.de/funktionsgraphen/
        /// </summary>
        /// <param name="numberOfSamples">Number of samples to which a speed is to be assigned.</param>
        /// <param name="actualSample">The sample to which a speed is assigned.</param>
        /// <param name="lowerThreshold">If 'actualSample' is greater than this value, the speed is at 100%. 
        /// 'lowerThreshold' is a percentage value.</param>
        /// <param name="upperThreshold">If 'actualSample' is less than this value, the speed is at 100%. 
        /// 'upperThreshold' is a percentage value.</param>
        /// <param name="vmin">The minimal speed in percent that could be returned.</param>
        /// <returns>The speed in percent. This speed is not below vmin.</returns>
        public static double GetQuadraticVelocityProfile(ulong numberOfSamples, 
                                                         ulong actualSample, 
                                                         ushort lowerThreshold, 
                                                         ushort upperThreshold, 
                                                         ushort vmin = 0)
        {
            if (lowerThreshold > upperThreshold ||
                upperThreshold > 100 ||
                Math.Abs(upperThreshold - lowerThreshold) < 10 || // Emotional value. However, if the thresholds are too close to each other,
                                                                  // this counteracts a smooth flow of movement.
                actualSample > numberOfSamples ||
                vmin > 100)
            {
                throw new ArgumentException("Your arguments are idiotic.");
            }

            var lowerThresholdInSamples = numberOfSamples * lowerThreshold / 100;
            var upperThresholdInSamples = numberOfSamples * upperThreshold / 100;

            // function is v(x) = ax^2 + c
            // with c = vmin and a = 1/(1-c)

            // Set minimum speed c in percent
            var c = (double)vmin / 100;

            // Set lower compression a = (1-c) / lowerThresholdInSamples^2
            double lowerCompression = (1 - c) * Math.Pow(lowerThresholdInSamples, -2);

            // Set upper compression 'a' of quadratic function
            double upperCompression = 1;
            if (numberOfSamples > upperThresholdInSamples)
            {
                upperCompression = (1 - c) * Math.Pow(Math.Abs((double)numberOfSamples - upperThresholdInSamples), -2);
            }

            if (actualSample < lowerThresholdInSamples)
            {
                var result = lowerCompression * Math.Pow(actualSample, 2) + c;
                return result;
            }
            else if (actualSample >= upperThresholdInSamples)
            {
                var result = upperCompression * Math.Pow((double)numberOfSamples - actualSample, 2) + c;
                return result;
            }

            return 1;
        }

        /// <summary>
        /// Creates an array with the size 'numberOfSamples' that assigns a value for speed and elapsed time to each contained sample.
        /// The array thus contains a discrete speed profile where all speeds between 'lowerThreshold' and 'upperThreshold'
        /// correspond to the speed 'speed'.
        ///
        /// Result is: | distance 's' | speed 'v'               | time 't'     | accumulated time 'tCum'|
        /// Units are: |s in samples  | v in samples per second | t in seconds | tCum in seconds        |
        ///
        /// See tests for example in a array of size 10 or 100.
        /// </summary>
        /// <param name="numberOfSamples"># samples</param>
        /// <param name="speed">Desired speed in 'units' per second</param>
        /// <param name="lowerThreshold">All samples that are below this percentage threshold are assigned a speed that is less than 'speed'
        /// and corresponds to a quadratic growth function.</param>
        /// <param name="upperThreshold">All samples that are above this percentage threshold are assigned a speed that is less than 'speed'
        /// and corresponds to a quadratic shrinkage function.</param>
        /// <param name="vmin">The minimal speed in percent that could be returned.</param>
        /// <returns>Array with 'numberOfSamples' tuples. A tuple contains the distance traveled in units, the speed in units/sec and the
        /// elapsed time: |distance s, speed v, time t, accumulated time tCum|.</returns>
        public static Tuple<uint, double, double, double>[] GetSampleArray(uint numberOfSamples, 
                                                                   int speed, 
                                                                   ushort lowerThreshold, 
                                                                   ushort upperThreshold, 
                                                                   ushort vmin = 0)
        {
            var result = new Tuple<uint, double, double, double>[numberOfSamples];
            double tCum = 0;
            for (uint i = 1; i <= numberOfSamples; i++)
            {
                var s = i;
                var v = LinearInterpolator.GetQuadraticVelocityProfile(numberOfSamples, i, lowerThreshold, upperThreshold, vmin) * speed;
                var t = 1 / v; // smallest step is 1. t is the time in seconds that is needed for 1 step at speed v.
                tCum += t;
                result[i-1] = new Tuple<uint, double, double, double>(s, v, t, tCum);
            }

            return result;
        }

        /// <summary>
        ///Resamples an array with variant speed and time values.
        /// In this resampling, the time values of each individual sample must be known because, as mentioned, the time grid and
        /// speed are not constant. No smoothing or windowing takes place.
        /// </summary>
        /// <param name="speedFunction">| distance 's' | speed 'v' | time 't' | accumulated time 'tCum'|</param>
        /// <param name="sampleRate">The desired sample rate in samples per second.</param>
        /// <returns></returns>
        public static Tuple<uint, double, double, double>[] Resample(Tuple<uint, double, double, double>[] speedFunction, int sampleRate)
        {
            var accumulatedTime = speedFunction[^1].Item4;
            var result = new Tuple<uint, double, double, double>[(uint)(accumulatedTime * sampleRate)];
            var timeStepWidth = 1.0 / sampleRate; // every timeStepWidth second a sample should be taken.

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = speedFunction.First(item => item.Item4 > timeStepWidth * (i + 1));
            }

            return result;
        }
    }
}
