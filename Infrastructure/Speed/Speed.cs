using System;

namespace Infrastructure.Speed
{
    /// <summary>
    /// Represents a speed value that can be constructed from and converted between
    /// meters per second (m/s), kilometers per hour (km/h), and pace (minutes per kilometer).
    /// </summary>
    public class Speed : IEquatable<Speed>
    {
        private readonly double _metersPerSecond;

        private Speed(double metersPerSecond)
        {
            if (metersPerSecond < 0)
                throw new ArgumentOutOfRangeException(nameof(metersPerSecond), "Speed cannot be negative.");
            _metersPerSecond = metersPerSecond;
        }

        /// <summary>
        /// Gets the speed in meters per second, rounded to two decimal places.
        /// </summary>
        public double MetersPerSecond => Math.Round(_metersPerSecond, 2);

        /// <summary>
        /// Gets the speed in kilometers per hour, rounded to two decimal places.
        /// </summary>
        public double KilometersPerHour => Math.Round(_metersPerSecond * 3.6, 2);

        /// <summary>
        /// Gets the pace in minutes per kilometer, rounded to two decimal places. Returns positive infinity for zero speed.
        /// </summary>
        public double MinutesPerKilometer => _metersPerSecond > 0 ? Math.Round(1000.0 / (60.0 * _metersPerSecond), 2) : double.PositiveInfinity;

        /// <summary>
        /// Creates a Speed instance from meters per second.
        /// </summary>
        public static Speed FromMetersPerSecond(double metersPerSecond)
        {
            return new Speed(metersPerSecond);
        }

        /// <summary>
        /// Creates a Speed instance from kilometers per hour.
        /// </summary>
        public static Speed FromKilometersPerHour(double kilometersPerHour)
        {
            if (kilometersPerHour < 0)
                throw new ArgumentOutOfRangeException(nameof(kilometersPerHour), "Speed cannot be negative.");
            return new Speed(kilometersPerHour / 3.6);
        }

        /// <summary>
        /// Creates a Speed instance from pace in minutes per kilometer.
        /// </summary>
        public static Speed FromMinutesPerKilometer(double minutesPerKilometer)
        {
            if (minutesPerKilometer <= 0)
                throw new ArgumentOutOfRangeException(nameof(minutesPerKilometer), "Pace must be positive.");
            var metersPerSecond = 1000.0 / (60.0 * minutesPerKilometer);
            return new Speed(metersPerSecond);
        }

        public bool Equals(Speed? other)
        {
            if (other is null) return false;
            return Math.Abs(_metersPerSecond - other._metersPerSecond) < 1e-9;
        }

        public override bool Equals(object? obj) => Equals(obj as Speed);

        public override int GetHashCode() => _metersPerSecond.GetHashCode();

        public override string ToString() => $"{KilometersPerHour:F2} km/h ({MinutesPerKilometer:F2} min/km)";

        // Equality operators
        public static bool operator ==(Speed? left, Speed? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(Speed? left, Speed? right) => !(left == right);

        // Comparison operators
        public static bool operator <(Speed? left, Speed? right)
        {
            if (left is null || right is null) throw new ArgumentNullException();
            return left._metersPerSecond < right._metersPerSecond;
        }

        public static bool operator >(Speed? left, Speed? right)
        {
            if (left is null || right is null) throw new ArgumentNullException();
            return left._metersPerSecond > right._metersPerSecond;
        }

        public static bool operator <=(Speed? left, Speed? right) => left == right || left < right;

        public static bool operator >=(Speed? left, Speed? right) => left == right || right > left;

        // Arithmetic operators
        /// <summary>
        /// Multiplies a speed by a scalar value.
        /// </summary>
        public static Speed operator *(Speed speed, double scalar)
        {
            if (speed is null) throw new ArgumentNullException(nameof(speed));
            if (scalar < 0)
                throw new ArgumentOutOfRangeException(nameof(scalar), "Scalar must be non-negative.");
            return new Speed(speed._metersPerSecond * scalar);
        }

        /// <summary>
        /// Multiplies a speed by a scalar value.
        /// </summary>
        public static Speed operator *(double scalar, Speed speed)
        {
            return speed * scalar;
        }

        /// <summary>
        /// Divides a speed by a scalar value.
        /// </summary>
        public static Speed operator /(Speed speed, double divisor)
        {
            if (speed is null) throw new ArgumentNullException(nameof(speed));
            if (divisor <= 0)
                throw new ArgumentOutOfRangeException(nameof(divisor), "Divisor must be positive.");
            return new Speed(speed._metersPerSecond / divisor);
        }

        /// <summary>
        /// Divides one speed by another to get a ratio.
        /// </summary>
        public static double operator /(Speed left, Speed right)
        {
            if (left is null) throw new ArgumentNullException(nameof(left));
            if (right is null) throw new ArgumentNullException(nameof(right));
            if (right._metersPerSecond == 0)
                throw new DivideByZeroException("Cannot divide by zero speed.");
            return left._metersPerSecond / right._metersPerSecond;
        }

        /// <summary>
        /// Adds a scalar value (m/s) to the speed.
        /// </summary>
        public static Speed operator +(Speed speed, double metersPerSecond)
        {
            if (speed is null) throw new ArgumentNullException(nameof(speed));
            var result = speed._metersPerSecond + metersPerSecond;
            if (result < 0)
                throw new InvalidOperationException("Resulting speed cannot be negative.");
            return new Speed(result);
        }

        /// <summary>
        /// Adds a scalar value (m/s) to the speed.
        /// </summary>
        public static Speed operator +(double metersPerSecond, Speed speed)
        {
            return speed + metersPerSecond;
        }

        /// <summary>
        /// Subtracts a scalar value (m/s) from the speed.
        /// </summary>
        public static Speed operator -(Speed speed, double metersPerSecond)
        {
            if (speed is null) throw new ArgumentNullException(nameof(speed));
            var result = speed._metersPerSecond - metersPerSecond;
            if (result < 0)
                throw new InvalidOperationException("Resulting speed cannot be negative.");
            return new Speed(result);
        }
    }
}
