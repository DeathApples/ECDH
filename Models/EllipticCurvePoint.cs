﻿using System.Numerics;

namespace ECDH.Models
{
    public class EllipticCurvePoint
    {
        public FiniteFieldElement X { get; set; }
        public FiniteFieldElement Y { get; set; }
        public bool IsInfinite { get; }

        public bool IsOnCurve
        {
            get
            {
                if (IsInfinite)
                    return true;

                return FiniteFieldElement.Pow(Y, 2) == FiniteFieldElement.Pow(X, 3) + EllipticCurve.A * X + EllipticCurve.B;
            }
        }

        public static EllipticCurvePoint Infinite => new();

        private EllipticCurvePoint()
        {
            X = new(0, 1);
            Y = new(0, 1);
            IsInfinite = true;
        }

        public EllipticCurvePoint(FiniteFieldElement x, FiniteFieldElement y, bool isInfinite = false)
        {
            if (x.Prime != y.Prime)
                throw new ArgumentException("Primes must match");

            X = x;
            Y = y;
            IsInfinite = isInfinite;

            //if (!IsOnCurve)
            //    throw new ArgumentException("Point not on curve");
        }

        public EllipticCurvePoint(BigInteger x, BigInteger y, bool isInfinite = false)
        {
            X = new(x, EllipticCurve.Prime);
            Y = new(y, EllipticCurve.Prime);
            IsInfinite = isInfinite;

            //if (!IsOnCurve)
            //    throw new ArgumentException("Point not on curve");
        }

        public EllipticCurvePoint Copy() => new(X, Y, IsInfinite);

        public static EllipticCurvePoint operator +(EllipticCurvePoint P, EllipticCurvePoint Q)
        {
            if (P.IsInfinite)
                return Q;

            if (Q.IsInfinite)
                return P;

            if (P == -Q)
                return Infinite;

            FiniteFieldElement slope;

            if (P == Q)
                slope = (3 * FiniteFieldElement.Pow(P.X, 2) + EllipticCurve.A) / (2 * P.Y);
            else
                slope = (Q.Y - P.Y) / (Q.X - P.X);

            FiniteFieldElement x = FiniteFieldElement.Pow(slope, 2) - P.X - Q.X;
            FiniteFieldElement y = slope * (P.X - x) - P.Y;

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

        public static EllipticCurvePoint operator -(EllipticCurvePoint P, EllipticCurvePoint Q) => P + (-Q);

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

        public override string? ToString()
        {
            if (IsInfinite)
                return "(∞, ∞)";

            return $"({X.Value}, {Y.Value})";
        }
    }
}
