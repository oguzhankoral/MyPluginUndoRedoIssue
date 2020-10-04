using System;
/// <summary>
/// Extensions for double type.
/// </summary>
namespace Modelur.DoubleExtensions
{
    /// <summary>
    /// Extension class for double type.
    /// </summary>
        public static class DoubleExtensions
        {
        /// <summary>
        /// Check weather two floats are almost equal. If the difference between
        /// two floating point numbers is suficiently small, we say that the numbers are 
        /// almost equal. 
        /// </summary>
        /// <param name="value">The first value to compare</param>
        /// <param name="otherValue">The second value to compare</param>
        /// <param name="tolerance">The maximal difference that between values that are considered almost equal</param>
        /// <returns>The method returns true if the difference between value and other value is less than the tolerance.</returns>
        /// <example>
        /// <code>
        /// 1.0.AlmostEqual(0.99999999)
        /// 1.0.AlmostEqual(0.9, tolerance: 0.01) 
        /// </code>
        /// </example>
            public static bool AlmostEqual(this double value, double otherValue, double tolerance=1e-5)
            {
                return Math.Abs(otherValue - value) < tolerance;
            }
        }
}