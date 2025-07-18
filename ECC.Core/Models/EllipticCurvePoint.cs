﻿using ECC.Core.Algorithms;
using System.Numerics;

namespace ECC.Core.Models
{
    internal class EllipticCurvePoint
    {
        internal FiniteFieldNumber X { get; set; }
        internal FiniteFieldNumber Y { get; set; }
        internal BigInteger Order { get; set; }

        internal bool IsInfinite { get; }

        internal bool IsOnCurve
        {
            get
            {
                if (IsInfinite)
                    return true;

                return FiniteFieldNumber.Pow(Y, 2) == FiniteFieldNumber.Pow(X, 3) + EllipticCurve.A * X + EllipticCurve.B;
            }
        }

        internal static EllipticCurvePoint Infinite => new();

        private EllipticCurvePoint()
        {
            X = new(0, 1);
            Y = new(0, 1);
            IsInfinite = true;
        }

        internal EllipticCurvePoint(FiniteFieldNumber x, FiniteFieldNumber y, bool isInfinite = false)
        {
            if (x.Prime != y.Prime)
                throw new ArgumentException("Primes must match");

            X = x;
            Y = y;
            IsInfinite = isInfinite;
        }

        internal EllipticCurvePoint(BigInteger x, BigInteger y, bool isInfinite = false)
        {
            X = new(x, EllipticCurve.Prime);
            Y = new(y, EllipticCurve.Prime);
            IsInfinite = isInfinite;
        }

        internal EllipticCurvePoint Copy() => new(X, Y, IsInfinite);

        internal void CalculateOrder()
        {
            var divisors = EllipticCurve.GetDivisorsOfOrder();

            foreach (var divisor in divisors)
            {
                if ((divisor * this).IsInfinite)
                {
                    Order = divisor;
                    break;
                }
            }
        }

        public static EllipticCurvePoint operator +(EllipticCurvePoint P, EllipticCurvePoint Q)
        {
            if (P.IsInfinite)
                return Q;

            if (Q.IsInfinite)
                return P;

            if (P == -Q)
                return Infinite;

            FiniteFieldNumber slope;

            if (P == Q)
                slope = (3 * FiniteFieldNumber.Pow(P.X, 2) + EllipticCurve.A) / (2 * P.Y);
            else
                slope = (Q.Y - P.Y) / (Q.X - P.X);

            FiniteFieldNumber x = FiniteFieldNumber.Pow(slope, 2) - P.X - Q.X;
            FiniteFieldNumber y = slope * (P.X - x) - P.Y;

            return new(x, y);
        }

        public static EllipticCurvePoint operator *(BigInteger n, EllipticCurvePoint P)
        {
            EllipticCurvePoint result = Infinite;
            EllipticCurvePoint addend = P.Copy();

            int sign = n.Sign;
            n *= sign;

            while (n > 0)
            {
                if ((n & 1) == 1)
                    result += addend;

                addend += addend;
                n >>= 1;
            }

            return sign < 0 ? -result : result;
        }

        public static EllipticCurvePoint operator *(EllipticCurvePoint P, BigInteger n) => n * P;

        public static EllipticCurvePoint operator -(EllipticCurvePoint P) => new(P.X, -P.Y, P.IsInfinite);

        public static EllipticCurvePoint operator -(EllipticCurvePoint P, EllipticCurvePoint Q) => P + -Q;

        public static bool operator ==(EllipticCurvePoint P, EllipticCurvePoint Q) => P.Equals(Q);

        public static bool operator !=(EllipticCurvePoint P, EllipticCurvePoint Q) => !P.Equals(Q);

        public override bool Equals(object? obj)
        {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));

            if (obj is EllipticCurvePoint P)
                return X == P.X && Y == P.Y;
            else
                throw new ArgumentException("Argument not a point");
        }

        public override int GetHashCode() => HashCode.Combine(X.Value, Y.Value);
    }
}
