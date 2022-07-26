using LinearInterpolation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinearInterpolationTests
{
    [TestClass]
    public class LinearInterpolatorTests
    {
        [TestMethod]
        public void TestQuadraticVelocity()
        {
            var epsilon = 0.00001;

            Assert.IsTrue(Math.Abs(LinearInterpolator.GetQuadraticVelocityProfile(200, 30, 15, 85) - 1) < epsilon);     // Limit value for 100% speed
            Assert.IsTrue(Math.Abs(LinearInterpolator.GetQuadraticVelocityProfile(200, 0, 15, 85) - 0) < epsilon);      // First sample means speed = 0
            Assert.IsTrue(Math.Abs(LinearInterpolator.GetQuadraticVelocityProfile(200, 15, 15, 85) - 0.25) < epsilon);  // 50% to limit means 25% speed
            Assert.IsTrue(Math.Abs(LinearInterpolator.GetQuadraticVelocityProfile(100, 0, 0, 100) - 1) < epsilon);      // lower threshold 0 means speed 100% from first sample
            Assert.IsTrue(Math.Abs(LinearInterpolator.GetQuadraticVelocityProfile(100, 1, 0, 100) - 1) < epsilon);      // still speed 100% at second sample
            Assert.IsTrue(Math.Abs(LinearInterpolator.GetQuadraticVelocityProfile(100, 0, 1, 100) - 0) < epsilon);      // lower threshold 1 means speed 0 at first sample
            Assert.IsTrue(Math.Abs(LinearInterpolator.GetQuadraticVelocityProfile(100, 1, 1, 100)- 1) < epsilon);       // and speed 100% at second sample
            Assert.IsTrue(Math.Abs(LinearInterpolator.GetQuadraticVelocityProfile(100, 99, 15, 100)- 1) < epsilon);     // upper threshold 100, speed still 100% at sample 99
            Assert.IsTrue(Math.Abs(LinearInterpolator.GetQuadraticVelocityProfile(100, 100, 15, 100)- 0) < epsilon);    // upper threshold 100, speed 0 at last sample
            Assert.IsTrue(Math.Abs(LinearInterpolator.GetQuadraticVelocityProfile(100, 99, 15, 99)- 1) < epsilon);      // upper threshold 99, speed 100% at sample 99
            Assert.IsTrue(Math.Abs(LinearInterpolator.GetQuadraticVelocityProfile(100, 100, 15, 99)- 0) < epsilon);     // upper threshold 99, speed 0 at last sample

            Assert.ThrowsException<ArgumentException>(() => LinearInterpolator.GetQuadraticVelocityProfile(100, 49, 50, 50));// exc, threshold too close
            Assert.IsTrue(Math.Abs(LinearInterpolator.GetQuadraticVelocityProfile(100, 49, 10, 50)- 1) < epsilon);          // 1 sample before upper threshold, speed 100%
            Assert.IsTrue(Math.Abs(LinearInterpolator.GetQuadraticVelocityProfile(100, 50, 10, 50)- 1) < epsilon);          // actual sample on upper threshold, speed 100%
            Assert.IsTrue(Math.Abs(LinearInterpolator.GetQuadraticVelocityProfile(100, 51, 10, 50) - 0.9604) < epsilon);    // 1 sample beyond upper threshold speed < 100%

            Assert.ThrowsException<ArgumentException>(() => LinearInterpolator.GetQuadraticVelocityProfile(200, 66, 17, 15));
            Assert.ThrowsException<ArgumentException>(() => LinearInterpolator.GetQuadraticVelocityProfile(200, 15, 15, 15));
            Assert.ThrowsException<ArgumentException>(() => LinearInterpolator.GetQuadraticVelocityProfile(200, 201, 15, 15));        
        }

        [TestMethod]
        public void TestQuadraticVelocityWithVmin()
        {
            var epsilon = 0.00001;
            var result = LinearInterpolator.GetQuadraticVelocityProfile(100, 0, 15, 85, 15);
            Assert.IsTrue(Math.Abs(result - 0.15) < epsilon);

            result = LinearInterpolator.GetQuadraticVelocityProfile(100, 15, 15, 85, 15);
            Assert.IsTrue(Math.Abs(result - 1) < epsilon);

            result = LinearInterpolator.GetQuadraticVelocityProfile(100, 85, 15, 85, 15);
            Assert.IsTrue(Math.Abs(result - 1) < epsilon);

            result = LinearInterpolator.GetQuadraticVelocityProfile(100, 99, 15, 85, 15);
            Assert.IsTrue(Math.Abs(result - 0.1537776) < epsilon);

            result = LinearInterpolator.GetQuadraticVelocityProfile(100, 100, 15, 85, 15);
            Assert.IsTrue(Math.Abs(result - 0.15) < epsilon);

        }

        /// <summary>
        /// Test for simple discrete linear movement array.
        /// </summary>
        [TestMethod]
        public void TestGetSampleArray()
        {
            var epsilon = 0.00001;
            var array = LinearInterpolator.GetSampleArray(100, 200, 10, 90, 50);
            Assert.IsTrue(Math.Abs(array[9].Item2 - 200) < epsilon);
            Assert.IsTrue(Math.Abs(array[89].Item2 - 200) < epsilon);
            Assert.IsTrue(Math.Abs(array[90].Item2 - 181) < epsilon);

            var resampled = LinearInterpolator.Resample(array, 60);

            array = LinearInterpolator.GetSampleArray(10, 200, 30, 70, 40);
            Assert.IsTrue(Math.Abs(array[0].Item2 - 93.33333) < epsilon);
            Assert.IsTrue(Math.Abs(array[1].Item2 - 133.33333) < epsilon);
            Assert.IsTrue(Math.Abs(array[2].Item2 - 200) < epsilon);
            Assert.IsTrue(Math.Abs(array[6].Item2 - 200) < epsilon);
            Assert.IsTrue(Math.Abs(array[7].Item2 - 133.33333) < epsilon);
            Assert.IsTrue(Math.Abs(array[8].Item2 - 93.33333) < epsilon);
            Assert.IsTrue(Math.Abs(array[9].Item2 - 80) < epsilon);

            resampled = LinearInterpolator.Resample(array, 60);
        }

    }
}