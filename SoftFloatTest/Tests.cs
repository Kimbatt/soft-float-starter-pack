using SoftFloat;
using System.Diagnostics;

namespace SoftFloatTest
{
    public static class Tests
    {
        public static void RunAllTests()
        {
            TestAddition();
            TestSubtraction();
            TestMultiplication();
            TestDivision();
        }


        private delegate sfloat BinaryOperation(sfloat a, sfloat b);

        private enum BinaryOperationType : int
        {
            Addition, Subtraction, Multiplication, Division, Power
        }

        private static readonly BinaryOperation[] binaryOperations = new BinaryOperation[]
        {
            (a, b) => a + b,
            (a, b) => a - b,
            (a, b) => a * b,
            (a, b) => a / b,
            (a, b) => a % b,
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
            bool isOk = sfloat.Abs(result - (sfloat)expected) < (sfloat)0.0001f;
            Debug.Assert(isOk);
        }


        private enum UnaryOperationType : int
        {
            Round, Floor, Ceil, Sine, Cosine, Tangent, SquareRoot, Exponential, LogarithmNatural, LogarithmBase2
        }

        private delegate sfloat UnaryOperation(sfloat x);

        private static readonly UnaryOperation[] unaryOperations = new UnaryOperation[]
        {
            x => libm.roundf(x),
            x => libm.floorf(x),
            x => libm.ceilf(x),
            x => libm.sinf(x),
            x => libm.cosf(x),
            x => libm.tanf(x),
            x => libm.sqrtf(x),
            x => libm.expf(x),
            x => libm.logf(x),
            x => libm.log2f(x)
        };

        private static void TestUnaryOperationFloatExact(float x, float expected, UnaryOperationType op)
        {
            UnaryOperation func = unaryOperations[(int)op];
            sfloat result = func((sfloat)x);
            bool isOk = result.Equals((sfloat)expected);
            Debug.Assert(isOk);
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
        }
    }
}
