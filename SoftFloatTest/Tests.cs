using SoftFloat;
using System;
using System.Diagnostics;

namespace SoftFloatTest
{
    public static class Tests
    {
        const int RandomTestCount = 100_000;

        public static void RunAllTests()
        {
            TestAddition();
            TestSubtraction();
            TestMultiplication();
            TestDivision();

            RandomTestUnaryOperations();
            RandomTestBinaryOperations();
        }

        private static void RandomTestUnaryOperations()
        {
            RandomTestUnaryOperation(UnaryOperationType.Round);
            RandomTestUnaryOperation(UnaryOperationType.Floor);
            RandomTestUnaryOperation(UnaryOperationType.Ceiling);

            // trigonometry functions are implemented as an approximation, because the
            // libm implementation uses 64-bit double precision floats (which we cannot use here)
            // see https://github.com/rust-lang/libm/blob/master/src/math/sinf.rs for example
            RandomTestTrigonometryOperation(UnaryOperationType.Sine);
            RandomTestTrigonometryOperation(UnaryOperationType.Cosine);

            // tangent can produce inaccurate results near pi/2 + 2k*pi
            //RandomTestTrigonometryOperation(UnaryOperationType.Tangent);

            RandomTestUnaryOperation(UnaryOperationType.SquareRoot);

            // exponential is less accurate at higher values
            RandomTestUnaryOperation(UnaryOperationType.Exponential, 100.0f);

            RandomTestUnaryOperation(UnaryOperationType.LogarithmNatural);
            RandomTestUnaryOperation(UnaryOperationType.LogarithmBase2);

            RandomTestUnaryOperation(UnaryOperationType.ArcSine);
            RandomTestUnaryOperation(UnaryOperationType.ArcCosine);
            RandomTestUnaryOperation(UnaryOperationType.ArcTangent);
        }

        private static void RandomTestBinaryOperations()
        {
            RandomTestBinaryOperation(BinaryOperationType.Addition);
            RandomTestBinaryOperation(BinaryOperationType.Subtraction);
            RandomTestBinaryOperation(BinaryOperationType.Multiplication);
            RandomTestBinaryOperation(BinaryOperationType.Division);
            RandomTestBinaryOperation(BinaryOperationType.Modulus);

            RandomTestBinaryOperation(BinaryOperationType.Power);
            RandomTestBinaryOperation(BinaryOperationType.ArcTangent2);
        }


        private delegate sfloat BinaryOperation(sfloat a, sfloat b);

        private enum BinaryOperationType : int
        {
            Addition, Subtraction, Multiplication, Division, Modulus, Power, ArcTangent2
        }

        private static readonly BinaryOperation[] binaryOperations = new BinaryOperation[]
        {
            (a, b) => a + b,
            (a, b) => a - b,
            (a, b) => a * b,
            (a, b) => a / b,
            (a, b) => a % b,
            libm.powf,
            libm.atan2f
        };

        private static void TestBinaryOperationFloatExact(float a, float b, float expected, BinaryOperationType op)
        {
            BinaryOperation func = binaryOperations[(int)op];
            sfloat result = func((sfloat)a, (sfloat)b);
            bool isOk = result.Equals((sfloat)expected);
            Debug.Assert(isOk);
        }

        private static void TestBinaryOperationFloatApproximate(float a, float b, float expected, BinaryOperationType op)
        {
            BinaryOperation func = binaryOperations[(int)op];
            sfloat result = func((sfloat)a, (sfloat)b);

            if (float.IsNaN(expected) && result.IsNaN())
            {
                // special case, NaN-s cannot be compared
                return;
            }

            if (float.IsInfinity(expected) && result.IsInfinity() && MathF.Sign(expected) == result.Sign())
            {
                // both are the same infinities
                return;
            }

            float allowedError = MathF.Max(1e-6f * MathF.Pow(2.0f, MathF.Log2(MathF.Abs(expected) + 1.0f)), 1e-6f);
            float difference = MathF.Abs((float)result - expected);
            bool isOk = difference < allowedError;
            Debug.Assert(isOk);
        }


        private enum UnaryOperationType : int
        {
            Round, Floor, Ceiling, Sine, Cosine, Tangent, SquareRoot, Exponential, LogarithmNatural, LogarithmBase2,
            ArcSine, ArcCosine, ArcTangent
        }

        private delegate sfloat UnaryOperation(sfloat x);

        private static readonly UnaryOperation[] unaryOperations = new UnaryOperation[]
        {
            libm.roundf,
            libm.floorf,
            libm.ceilf,
            libm.sinf,
            libm.cosf,
            libm.tanf,
            libm.sqrtf,
            libm.expf,
            libm.logf,
            libm.log2f,
            libm.asinf,
            libm.acosf,
            libm.atanf
        };

        private static void TestUnaryOperationFloatExact(float x, float expected, UnaryOperationType op)
        {
            UnaryOperation func = unaryOperations[(int)op];
            sfloat result = func((sfloat)x);
            bool isOk = result.Equals((sfloat)expected);
            Debug.Assert(isOk);
        }

        private static void TestUnaryOperationFloatApproximate(float x, float expected, UnaryOperationType op, float allowedErrorMultiplier = 1.0f)
        {
            UnaryOperation func = unaryOperations[(int)op];
            sfloat result = func((sfloat)x);

            if (float.IsNaN(expected) && result.IsNaN())
            {
                // special case, NaN-s cannot be compared
                return;
            }

            if (float.IsInfinity(expected) && result.IsInfinity() && MathF.Sign(expected) == result.Sign())
            {
                // both are the same infinities
                return;
            }

            float allowedError = MathF.Max(1e-6f * allowedErrorMultiplier * MathF.Pow(2.0f, MathF.Log2(MathF.Abs(expected) + 1.0f)), 1e-6f);

            float difference = MathF.Abs((float)result - expected);
            bool isOk = difference <= allowedError;
            if (!isOk && (op == UnaryOperationType.Round || op == UnaryOperationType.Floor || op == UnaryOperationType.Ceiling))
            {
                // Because of the loss of precision that can result from representing decimal values
                // as floating-point numbers or performing arithmetic operations on floating-point values,
                // in some cases the Round method may not appear to round midpoint values to the nearest even integer.
                // https://docs.microsoft.com/en-us/dotnet/api/system.math.round

                if (MathF.Abs(x % 1.0f) - 0.5f < 0.01f)
                {
                    // x is near a midpoint, it's possible that rounding happened in a different direction
                    isOk = MathF.Abs((float)result - expected) <= 1.0f;
                }
            }

            Debug.Assert(isOk);
        }

        private static void TestTrigonometryOperationApproximate(float x, float expected, UnaryOperationType op)
        {
            UnaryOperation func = unaryOperations[(int)op];
            sfloat result = func((sfloat)x);

            if (float.IsNaN(expected) && result.IsNaN())
            {
                // special case, NaN-s cannot be compared
                return;
            }

            if (float.IsInfinity(expected) && result.IsInfinity() && MathF.Sign(expected) == result.Sign())
            {
                // both are the same infinities
                return;
            }

            float allowedError = MathF.Max(0.005f * MathF.Pow(2.0f, MathF.Log2(MathF.Abs(expected) + 1.0f)), 1e-6f);

            float difference = MathF.Abs((float)result - expected);
            bool isOk = difference <= allowedError;

            Debug.Assert(isOk);
        }


        private static void RandomTestBinaryOperation(BinaryOperationType op)
        {
            Func<float, float, float> func = op switch
            {
                BinaryOperationType.Addition => (float a, float b) => a + b,
                BinaryOperationType.Subtraction => (float a, float b) => a - b,
                BinaryOperationType.Multiplication => (float a, float b) => a * b,
                BinaryOperationType.Division => (float a, float b) => a / b,
                BinaryOperationType.Modulus => (float a, float b) => a % b,
                BinaryOperationType.Power => MathF.Pow,
                BinaryOperationType.ArcTangent2 => MathF.Atan2,
                _ => throw new ArgumentException(),
            };

            PCG rand = new PCG(0, 0);

            // very small values
            for (int i = 0; i < RandomTestCount; ++i)
            {
                float a = rand.FloatInclusive(-1e-10f, 1e-10f);
                float b = rand.FloatInclusive(-1e-10f, 1e-10f);
                TestBinaryOperationFloatApproximate(a, b, func(a, b), op);
            }

            // small values
            for (int i = 0; i < RandomTestCount; ++i)
            {
                float a = rand.FloatInclusive(-1.0f, 1.0f);
                float b = rand.FloatInclusive(-1.0f, 1.0f);
                TestBinaryOperationFloatApproximate(a, b, func(a, b), op);
            }

            // large values
            for (int i = 0; i < RandomTestCount; ++i)
            {
                float a = rand.FloatInclusive(-100000.0f, 100000.0f);
                float b = rand.FloatInclusive(-100000.0f, 100000.0f);
                TestBinaryOperationFloatApproximate(a, b, func(a, b), op);
            }

            // huge values
            for (int i = 0; i < RandomTestCount; ++i)
            {
                float a = rand.FloatInclusive(-1000000000.0f, 1000000000.0f);
                float b = rand.FloatInclusive(-1000000000.0f, 1000000000.0f);
                TestBinaryOperationFloatApproximate(a, b, func(a, b), op);
            }

            // gigantic values
            for (int i = 0; i < RandomTestCount; ++i)
            {
                float a = rand.FloatInclusive(-1e38f, 1e38f);
                float b = rand.FloatInclusive(-1e38f, 1e38f);
                TestBinaryOperationFloatApproximate(a, b, func(a, b), op);
            }
        }

        private static void RandomTestUnaryOperation(UnaryOperationType op, float allowedErrorMultiplier = 1.0f)
        {
            Func<float, float> func = op switch
            {
                UnaryOperationType.Round => MathF.Round,
                UnaryOperationType.Floor => MathF.Floor,
                UnaryOperationType.Ceiling => MathF.Ceiling,
                UnaryOperationType.Sine => MathF.Sin,
                UnaryOperationType.Cosine => MathF.Cos,
                UnaryOperationType.Tangent => MathF.Tan,
                UnaryOperationType.SquareRoot => MathF.Sqrt,
                UnaryOperationType.Exponential => MathF.Exp,
                UnaryOperationType.LogarithmNatural => MathF.Log,
                UnaryOperationType.LogarithmBase2 => MathF.Log2,
                UnaryOperationType.ArcSine => MathF.Asin,
                UnaryOperationType.ArcCosine => MathF.Acos,
                UnaryOperationType.ArcTangent => MathF.Atan,
                _ => throw new ArgumentException(),
            };

            PCG rand = new PCG(0, 0);

            // small values
            for (int i = 0; i < RandomTestCount; ++i)
            {
                float x = rand.FloatInclusive(-1.0f, 1.0f);
                TestUnaryOperationFloatApproximate(x, func(x), op, allowedErrorMultiplier);
            }

            // large values
            for (int i = 0; i < RandomTestCount; ++i)
            {
                float x = rand.FloatInclusive(-100000.0f, 100000.0f);
                TestUnaryOperationFloatApproximate(x, func(x), op, allowedErrorMultiplier);
            }

            // huge values
            for (int i = 0; i < RandomTestCount; ++i)
            {
                float x = rand.FloatInclusive(-1000000000.0f, 1000000000.0f);
                TestUnaryOperationFloatApproximate(x, func(x), op, allowedErrorMultiplier);
            }
        }

        private static void RandomTestTrigonometryOperation(UnaryOperationType op)
        {
            Func<float, float> func = op switch
            {
                UnaryOperationType.Sine => MathF.Sin,
                UnaryOperationType.Cosine => MathF.Cos,
                UnaryOperationType.Tangent => MathF.Tan,
                _ => throw new ArgumentException(),
            };

            PCG rand = new PCG(0, 0);

            // small values
            for (int i = 0; i < RandomTestCount; ++i)
            {
                float x = rand.FloatInclusive(-1.0f, 1.0f);
                TestTrigonometryOperationApproximate(x, func(x), op);
            }

            // medium values
            for (int i = 0; i < RandomTestCount; ++i)
            {
                float x = rand.FloatInclusive(-100.0f, 100.0f);
                TestTrigonometryOperationApproximate(x, func(x), op);
            }
        }

        public static void TestAddition()
        {
            const BinaryOperationType op = BinaryOperationType.Addition;

            TestBinaryOperationFloatExact(0.0f, 0.0f, 0.0f, op);
            TestBinaryOperationFloatExact(1.0f, 0.0f, 1.0f, op);
            TestBinaryOperationFloatExact(0.0f, 1.0f, 1.0f, op);

            TestBinaryOperationFloatExact(-0.0f, 0.0f, 0.0f, op);
            TestBinaryOperationFloatExact(-0.0f, 0.0f, -0.0f, op);
            TestBinaryOperationFloatExact(0.0f, 0.0f, -0.0f, op);

            TestBinaryOperationFloatExact(1.0f, -1.0f, 0.0f, op);
            TestBinaryOperationFloatExact(-1.0f, -1.0f, -2.0f, op);

            TestBinaryOperationFloatApproximate(123.456f, 456.789f, 580.245f, op);

            TestBinaryOperationFloatExact(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, op);
            TestBinaryOperationFloatExact(float.PositiveInfinity, float.NegativeInfinity, float.NaN, op);

            TestBinaryOperationFloatExact(float.NaN, float.NaN, float.NaN, op);
            TestBinaryOperationFloatExact(0.0f, float.NaN, float.NaN, op);
            TestBinaryOperationFloatExact(-999999.0f, float.NaN, float.NaN, op);

            RandomTestBinaryOperation(op);
        }


        public static void TestSubtraction()
        {
            const BinaryOperationType op = BinaryOperationType.Subtraction;

            TestBinaryOperationFloatExact(0.0f, 0.0f, 0.0f, op);
            TestBinaryOperationFloatExact(1.0f, 0.0f, 1.0f, op);
            TestBinaryOperationFloatExact(0.0f, 1.0f, -1.0f, op);

            TestBinaryOperationFloatExact(-0.0f, 0.0f, 0.0f, op);
            TestBinaryOperationFloatExact(-0.0f, 0.0f, -0.0f, op);
            TestBinaryOperationFloatExact(0.0f, 0.0f, -0.0f, op);

            TestBinaryOperationFloatExact(1.0f, -1.0f, 2.0f, op);
            TestBinaryOperationFloatExact(-1.0f, -1.0f, 0.0f, op);

            TestBinaryOperationFloatApproximate(123.456f, 456.789f, -333.333f, op);

            TestBinaryOperationFloatExact(float.PositiveInfinity, float.PositiveInfinity, float.NaN, op);
            TestBinaryOperationFloatExact(float.PositiveInfinity, float.NegativeInfinity, float.PositiveInfinity, op);

            TestBinaryOperationFloatExact(float.NaN, float.NaN, float.NaN, op);
            TestBinaryOperationFloatExact(0.0f, float.NaN, float.NaN, op);
            TestBinaryOperationFloatExact(-999999.0f, float.NaN, float.NaN, op);

            RandomTestBinaryOperation(op);
        }

        public static void TestMultiplication()
        {
            const BinaryOperationType op = BinaryOperationType.Multiplication;

            TestBinaryOperationFloatExact(0.0f, 0.0f, 0.0f, op);
            TestBinaryOperationFloatExact(1.0f, 0.0f, 0.0f, op);
            TestBinaryOperationFloatExact(0.0f, 1.0f, 0.0f, op);

            TestBinaryOperationFloatExact(-0.0f, 0.0f, 0.0f, op);
            TestBinaryOperationFloatExact(-0.0f, 0.0f, -0.0f, op);
            TestBinaryOperationFloatExact(0.0f, 0.0f, -0.0f, op);

            TestBinaryOperationFloatExact(1.0f, -1.0f, -1.0f, op);
            TestBinaryOperationFloatExact(-1.0f, -1.0f, 1.0f, op);

            TestBinaryOperationFloatApproximate(123.456f, 456.789f, 56393.34f, op);

            TestBinaryOperationFloatExact(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, op);
            TestBinaryOperationFloatExact(float.PositiveInfinity, float.NegativeInfinity, float.NegativeInfinity, op);
            TestBinaryOperationFloatExact(float.NegativeInfinity, float.NegativeInfinity, float.PositiveInfinity, op);
            TestBinaryOperationFloatExact(float.NaN, float.PositiveInfinity, float.NaN, op);
            TestBinaryOperationFloatExact(0.0f, float.PositiveInfinity, float.NaN, op);

            TestBinaryOperationFloatExact(float.NaN, float.NaN, float.NaN, op);
            TestBinaryOperationFloatExact(0.0f, float.NaN, float.NaN, op);
            TestBinaryOperationFloatExact(-999999.0f, float.NaN, float.NaN, op);

            RandomTestBinaryOperation(op);
        }

        public static void TestDivision()
        {
            const BinaryOperationType op = BinaryOperationType.Division;

            TestBinaryOperationFloatExact(0.0f, 0.0f, float.NaN, op);
            TestBinaryOperationFloatExact(1.0f, 0.0f, float.PositiveInfinity, op);
            TestBinaryOperationFloatExact(0.0f, 1.0f, 0.0f, op);

            TestBinaryOperationFloatExact(-0.0f, 0.0f, float.NaN, op);
            TestBinaryOperationFloatExact(-0.0f, 0.0f, float.NaN, op);
            TestBinaryOperationFloatExact(0.0f, 0.0f, float.NaN, op);

            TestBinaryOperationFloatExact(1.0f, -1.0f, -1.0f, op);
            TestBinaryOperationFloatExact(-1.0f, -1.0f, 1.0f, op);

            TestBinaryOperationFloatApproximate(123.456f, 456.789f, 0.2702692f, op);

            TestBinaryOperationFloatExact(float.PositiveInfinity, float.PositiveInfinity, float.NaN, op);
            TestBinaryOperationFloatExact(float.PositiveInfinity, float.NegativeInfinity, float.NaN, op);
            TestBinaryOperationFloatExact(float.NegativeInfinity, float.NegativeInfinity, float.NaN, op);
            TestBinaryOperationFloatExact(float.NaN, float.PositiveInfinity, float.NaN, op);
            TestBinaryOperationFloatExact(0.0f, float.PositiveInfinity, 0.0f, op);
            TestBinaryOperationFloatExact(float.PositiveInfinity, 0.0f, float.PositiveInfinity, op);

            TestBinaryOperationFloatExact(float.NaN, float.NaN, float.NaN, op);
            TestBinaryOperationFloatExact(0.0f, float.NaN, float.NaN, op);
            TestBinaryOperationFloatExact(-999999.0f, float.NaN, float.NaN, op);

            RandomTestBinaryOperation(op);
        }
    }
}
