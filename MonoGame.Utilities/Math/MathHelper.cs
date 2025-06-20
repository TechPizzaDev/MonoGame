// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace MonoGame.Framework
{
    /// <summary>
    /// Contains commonly used precalculated values and mathematical operations.
    /// </summary>
    public static partial class MathHelper
    {
        /// <summary>
        /// Returns the Cartesian coordinate for one axis of a point that 
        /// is defined by a given triangle and two normalized barycentric (areal) coordinates.
        /// </summary>
        /// <param name="a">The coordinate on one axis of vertex 1 of the defining triangle.</param>
        /// <param name="b">The coordinate on the same axis of vertex 2 of the defining triangle.</param>
        /// <param name="c">The coordinate on the same axis of vertex 3 of the defining triangle.</param>
        /// <param name="amount1">
        /// The normalized barycentric (areal) coordinate b2, equal to the weighting factor for vertex 2,
        /// the coordinate of which is specified in value2.
        /// </param>
        /// <param name="amount2">
        /// The normalized barycentric (areal) coordinate b3, equal to the weighting factor for vertex 3, 
        /// the coordinate of which is specified in value3.
        /// </param>
        /// <returns>Cartesian coordinate of the specified point with respect to the axis being used.</returns>
        public static float Barycentric(float a, float b, float c, float amount1, float amount2)
        {
            return MathF.FusedMultiplyAdd(c - a, amount2, MathF.FusedMultiplyAdd(b - a, amount1, a));
        }

        /// <summary>
        /// Performs a Catmull-Rom interpolation using the specified positions.
        /// </summary>
        /// <param name="a">The first position in the interpolation.</param>
        /// <param name="b">The second position in the interpolation.</param>
        /// <param name="c">The third position in the interpolation.</param>
        /// <param name="d">The fourth position in the interpolation.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>A position that is the result of the Catmull-Rom interpolation.</returns>
        public static float CatmullRom(float a, float b, float c, float d, float amount)
        {
            // Using formula from http://www.mvps.org/directx/articles/catmull/
            // Internally using doubles not to lose precission
            double aSquared = amount * amount;
            double aCubed = aSquared * amount;

            return (float)(0.5 * (
                (c - a) * amount +
                (2.0 * a - 5.0 * b + 4.0 * c - d) * aSquared +
                (3.0 * b - a - 3.0 * c + d) * aCubed +
                2.0 * b));
        }

        /// <summary>
        /// Calculates the absolute value of the difference of two values.
        /// </summary>
        /// <param name="a">Source value.</param>
        /// <param name="b">Source value.</param>
        /// <returns>Distance between the two values.</returns>
        public static float Distance(float a, float b)
        {
            return Math.Abs(a - b);
        }

        /// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="position1">Source position.</param>
        /// <param name="tangent1">Source tangent.</param>
        /// <param name="position2">Source position.</param>
        /// <param name="tangent2">Source tangent.</param>
        /// <param name="amount">Weighting factor.</param>
        /// <returns>The result of the Hermite spline interpolation.</returns>
        public static float Hermite(float position1, float tangent1, float position2, float tangent2, float amount)
        {
            if (amount == 0f)
            {
                return position1;
            }
            else if (amount == 1f)
            {
                return position2;
            }
            else
            {
                // Cast to double to not lose precision
                // Otherwise, for high numbers of "amount" the result is NaN instead of Infinity
                double v1 = position1;
                double v2 = position2;
                double t1 = tangent1;
                double t2 = tangent2;
                double a = amount;
                double aSquared = a * a;
                double aCubed = aSquared * a;

                return (float)(
                    (2 * v1 - 2 * v2 + t2 + t1) * aCubed +
                    (3 * v2 - 3 * v1 - 2 * t1 - t2) * aSquared +
                    t1 * a +
                    v1);
            }
        }

        /// <summary>
        /// Linearly interpolates between two values.
        /// </summary>
        /// <param name="a">Source value.</param>
        /// <param name="b">Destination value.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of value2.</param>
        /// <returns>Interpolated value.</returns> 
        /// <remarks>This method performs the linear interpolation based on the following formula:
        /// <code>value1 + (value2 - value1) * amount</code>.
        /// Passing amount a value of 0 will cause value1 to be returned, a value of 1 will cause value2 to be returned.
        /// See <see cref="LerpPrecise"/> for a less efficient version with more precision around edge cases.
        /// </remarks>
        public static float Lerp(float a, float b, float amount)
        {
            return MathF.FusedMultiplyAdd(b - a, amount, a);
        }

        /// <summary>
        /// Linearly interpolates between two values.
        /// This method is a slightly less efficient, more precise version of <see cref="Lerp"/>.
        /// See remarks for more info.
        /// </summary>
        /// <param name="a">Source value.</param>
        /// <param name="b">Destination value.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of value2.</param>
        /// <returns>The interpolated value.</returns>
        /// <remarks>
        /// This method performs the linear interpolation based on the following formula:
        /// <code>((1 - amount) * value1) + (value2 * amount)</code>.
        /// Passing amount a value of 0 will cause value1 to be returned, a value of 1 will cause value2 to be returned.
        /// This method does not have the floating-point precision issue that <see cref="Lerp"/> has.
        /// i.e. If there is a big gap between value1 and value2 in magnitude (e.g. value1=10000000000000000, value2=1),
        /// right at the edge of the interpolation range (amount=1),
        /// <see cref="Lerp"/> will return 0 (whereas it should return 1).
        /// This also holds for value1=10^17, value2=10; value1=10^18,value2=10^2... so on.
        /// <para>
        /// For an in depth explanation of the issue see relevant Wikipedia article and StackOverflow answer: 
        /// https://en.wikipedia.org/wiki/Linear_interpolation#Programming_language_support
        /// http://stackoverflow.com/questions/4353525/floating-point-linear-interpolation#answer-23716956
        /// </para>
        /// </remarks>
        public static float LerpPrecise(float a, float b, float amount)
        {
            return MathF.FusedMultiplyAdd(1 - amount, a, b * amount);
        }

        /// <summary>
        /// Interpolates between two values using a cubic equation.
        /// </summary>
        /// <param name="a">Source value.</param>
        /// <param name="b">Source value.</param>
        /// <param name="amount">Weighting value.</param>
        /// <returns>Interpolated value.</returns>
        public static float SmoothStep(float a, float b, float amount)
        {
            // It is expected that 0 < amount < 1
            // If amount < 0, return value1
            // If amount > 1, return value2
            float result = Math.Clamp(amount, 0f, 1f);
            return Hermite(a, 0f, b, 0f, result);
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="radians">The angle in radians.</param>
        /// <returns>The angle in degrees.</returns>
        /// <remarks>Factor = 180 / PI</remarks>
        public static double ToDegrees(double radians)
        {
            return radians * 57.295779513082320876798154814105;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">The angle in degrees.</param>
        /// <returns>The angle in radians.</returns>
        /// <remarks>Factor = PI / 180</remarks>
        public static double ToRadians(double degrees)
        {
            return degrees * 0.017453292519943295769236907684886;
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="radians">The angle in radians.</param>
        /// <returns>The angle in degrees.</returns>
        /// <remarks>
        /// This method uses double precission internally, though it returns single float.
        /// Factor = 180 / PI
        /// </remarks>
        public static float ToDegrees(float radians)
        {
            return (float)(radians * 57.295779513082320876798154814105);
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">The angle in degrees.</param>
        /// <returns>The angle in radians.</returns>
        /// <remarks>
        /// This method uses double precission internally, though it returns single float.
        /// Factor = PI / 180
        /// </remarks>
        public static float ToRadians(float degrees)
        {
            return (float)(degrees * 0.017453292519943295769236907684886);
        }

        /// <summary>
        /// Reduces a given angle to a value between π and -π.
        /// </summary>
        /// <param name="angle">The angle to reduce, in radians.</param>
        /// <returns>The new angle, in radians.</returns>
        public static float WrapAngle(float angle)
        {
            const float TwoPI = MathF.PI * 2;

            if ((angle > -MathF.PI) && (angle <= MathF.PI))
                return angle;
            angle %= TwoPI;
            if (angle <= -MathF.PI)
                return angle + TwoPI;
            if (angle > MathF.PI)
                return angle - TwoPI;
            return angle;
        }

        /// <summary>
        /// Reduces a given angle to a value between π and -π.
        /// </summary>
        /// <param name="angle">The angle to reduce, in radians.</param>
        /// <returns>The new angle, in radians.</returns>
        public static double WrapAngle(double angle)
        {
            const double TwoPI = Math.PI * 2;

            if ((angle > -Math.PI) && (angle <= Math.PI))
                return angle;
            angle %= TwoPI;
            if (angle <= -Math.PI)
                return angle + TwoPI;
            if (angle > Math.PI)
                return angle - TwoPI;
            return angle;
        }

        /// <summary>
        /// Determines if value is a power of two.
        /// </summary>
        /// <param name="value">A value.</param>
        /// <returns>
        /// <see langword="true"/> if <c>value</c> is a power of two; otherwise <see langword="false"/>.
        /// </returns>
        public static bool IsPowerOfTwo(int value)
        {
            return (value > 0) && ((value & (value - 1)) == 0);
        }
    }
}