using System;
using System.Collections.Generic;
using System.Text;

namespace FTAnalyzer.Shared.Utilities
{
    public class BigMath
    {
        protected static double LOG2 = Math.log(2.0);
        protected static double LOG10 = Math.log(10.0);

        // numbers greater than 10^MAX_DIGITS_10 or e^MAX_DIGITS_EXP are considered unsafe ('too big') for floating point operations
        protected static int MAX_DIGITS_EXP = 677;
        protected static int MAX_DIGITS_10 = 294; // ~ MAX_DIGITS_EXP/LN(10)
        protected static int MAX_DIGITS_2 = 977; // ~ MAX_DIGITS_EXP/LN(2)

        /**
         * Computes the natural logarithm of a BigInteger. 
         * 
         * Works for really big integers (practically unlimited), even when the argument 
         * falls outside the <tt>double</tt> range
         * 
         * Returns Nan if argument is negative, NEGATIVE_INFINITY if zero.
         * 
         * @param val Argument
         * @return Natural logarithm, as in <tt>Math.log()</tt>
         */
        public static double LogBigInteger(BigInteger val)
        {
            if (val.signum() < 1)
                return val.signum() < 0 ? Double.NaN : Double.NEGATIVE_INFINITY;
            int blex = val.bitLength() - MAX_DIGITS_2; // any value in 60..1023 works ok here
            if (blex > 0)
                val = val.shiftRight(blex);
            double res = Math.log(val.doubleValue());
            return blex > 0 ? res + blex * LOG2 : res;
        }

        /**
         * Computes the natural logarithm of a BigDecimal. 
         * 
         * Works for really big (or really small) arguments, even outside the double range.
         * 
         * Returns Nan if argument is negative, NEGATIVE_INFINITY if zero.
        *
         * @param val Argument
         * @return Natural logarithm, as in <tt>Math.log()</tt>
         */
        public static double LogBigDecimal(BigDecimal val)
        {
            if (val.signum() < 1)
                return val.signum() < 0 ? Double.NaN : Double.NEGATIVE_INFINITY;
            int digits = val.precision() - val.scale();
            if (digits < MAX_DIGITS_10 && digits > -MAX_DIGITS_10)
                return Math.log(val.doubleValue());
            else
                return logBigInteger(val.unscaledValue()) - val.scale() * LOG10;
        }

        /**
         * Computes the exponential function, returning a BigDecimal (precision ~ 16).
         *  
         * Works for very big and very small exponents, even when the result 
         * falls outside the double range
         *
         * @param exponent Any finite value (infinite or Nan throws IllegalArgumentException)
         * @return The value of e (base of the natural logarithms) raised to the given exponent, as in <tt>Math.exp()</tt>
         */
        public static BigDecimal ExpBig(double exponent)
        {
            if (!Double.isFinite(exponent))
                throw new IllegalArgumentException("Infinite not accepted: " + exponent);
            // e^b = e^(b2+c) = e^b2 2^t with e^c = 2^t 
            double bc = MAX_DIGITS_EXP;
            if (exponent < bc && exponent > -bc)
                return new BigDecimal(Math.exp(exponent), MathContext.DECIMAL64);
            boolean neg = false;
            if (exponent < 0)
            {
                neg = true;
                exponent = -exponent;
            }
            double b2 = bc;
            double c = exponent - bc;
            int t = (int)Math.ceil(c / LOG10);
            c = t * LOG10;
            b2 = exponent - c;
            if (neg)
            {
                b2 = -b2;
                t = -t;
            }
            return new BigDecimal(Math.exp(b2), MathContext.DECIMAL64).movePointRight(t);
        }

        /**
         * Same as Math.pow(a,b) but returns a BigDecimal (precision ~ 16). 
         * 
         * Works even for outputs that fall outside the <tt>double</tt> range
         * 
         * The only limit is that b * log(a) does not overflow the double range 
         * 
         * @param a Base. Should be non-negative 
         * @param b Exponent. Should be finite (and non-negative if base is zero)
         * @return Returns the value of the first argument raised to the power of the second argument.
         */
        public static BigDecimal PowBig(double a, double b)
        {
            if (!(Double.isFinite(a) && Double.isFinite(b)))
                throw new IllegalArgumentException(Double.isFinite(b) ? "base not finite: a=" + a : "exponent not finite: b=" + b);
            if (b == 0)
                return BigDecimal.ONE;
            if (b == 1)
                return BigDecimal.valueOf(a);
            if (a == 0)
            {
                if (b >= 0)
                    return BigDecimal.ZERO;
                else
                    throw new IllegalArgumentException("0**negative = infinite");
            }
            if (a < 0)
            {
                throw new IllegalArgumentException("negative base a=" + a);
            }
            double x = b * Math.log(a);
            if (Math.abs(x) < MAX_DIGITS_EXP)
                return BigDecimal.valueOf(Math.pow(a, b));
            else
                return expBig(x);
        }
    }
}
