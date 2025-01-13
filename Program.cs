using System.Numerics;

// Lets start with the known value for PI to 20 decimal places + 1 extra to be accurate within 17 hex digits.
const decimal PI = 3.141592653589793238462M;



// Part A - Demonstrate converting the decimal value of PI to a fixed point binary value
Console.WriteLine("\nA) Convert known PI Decimal to Fixed Point Binary:\n");
var piBin = ConvertDecimalToFixedPointBinary(PI, 17 * 4);

// Function to convert decimal to fixed point binary up to given precision by 
// repeated x2 multiplication taking off the integral value each time.
// Integral part of initial value is assumed to fit within a byte
static FixedPointBinary ConvertDecimalToFixedPointBinary(decimal value, int precision)
{
    // Initalise result in specialised type with integral part of value and zero fractional part
    var ip = (byte)value;
    var result = new FixedPointBinary(ip, 0, precision);
    Console.WriteLine($"{value} -> {ip} -> {result}");

    // Iterate bit-by-bit up to the required precision
    for (int i = precision; i > 0; i--)
    {
        value -= ip;        // remove current integral part from value
        value *= 2;         // multiply value by 2
        ip = (byte)value;   // get integral part
        var fp =            // Set integral bit in fractional part of result
            result.FractionalPart | new BigInteger(ip) << (i - 1);
        result = result with { FractionalPart = fp };
        Console.WriteLine($"{value} -> {ip} -> {result}");
    }

    return result;
};



// Part B - Demonstrate converting the fixed point binary value back to decimal
Console.WriteLine("\n\nB) Convert known PI Fixed Point Binary back to Decimal:\n");
var piDec = ConvertFixedPointBinaryToDecimal(piBin, 20);
Console.WriteLine("\nOutcome:");
Console.WriteLine($"PI Original Value: {PI} ({PI.Scale} dp)");
Console.WriteLine($"PI From Roundtrip: {piDec}  ({piDec.Scale} dp)");

// Function to convert fixed point binary to decimal up to given precision by
// repeated x10 multiplication taking off the integral value each time.
static decimal ConvertFixedPointBinaryToDecimal(FixedPointBinary value, int precision)
{
    // Build up a string result starting integral part of value and decimal point
    var result = $"{value.IntegralPart}.";
    Console.WriteLine($"{value} -> {result}");

    // Create bit masks required to isolate integral and fractional parts when multiplying
    var integralMask = new BigInteger(0xF) << value.Precision;
    var fractionalMask = (new BigInteger(0x1) << value.Precision) - 1;

    // Iterate digit-by-digit up to the required precision
    for (decimal i = 0; i < precision; i++)
    {
        var fp = value.FractionalPart * 10;  // multiply fractional part by 10
        var ip = fp & integralMask;          // mask out integral part
        ip >>= value.Precision;              // and right shift by binary precision to get integral byte
        result += ip.ToString();             // and integral digit to result string
        fp &= fractionalMask;                // keep fractional part
        value = value with { IntegralPart = (byte)ip, FractionalPart = fp };
        Console.WriteLine($"{value} -> {result}");
    }

    // Can now parse string as decimal result
    return decimal.Parse(result);
};








/// <summary>
/// Type representing a fixed point binary value
/// </summary>
/// <param name="IntegralPart">The integral part of the value (before the binary point)</param>
/// <param name="FractionalPart">The fractional part of the value (after the binary point)</param>
/// <param name="Precision">The number of bits in the fractional part</param>
record FixedPointBinary(byte IntegralPart, BigInteger FractionalPart, int Precision)
{
    public int HexPrecision => Precision / 4;

    public override string ToString() =>
        $"{IntegralPart:b8}.{FractionalPart.ToString($"b{Precision}")[..Precision]} (0x{IntegralPart:x2}.{FractionalPart.ToString($"x{HexPrecision}")[..HexPrecision]})";
}
