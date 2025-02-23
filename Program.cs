﻿using System.Numerics;

// Program to sense check approach for calculating PI on my 8-bit relay computer using
// the Bailey-Borwein-Plouffe formula (which calculates the first n digits of Pi in base 16).
// This program will also provide values which can confirm the relay computer's output.

// The challenge is to calculate PI to 20 decimal places but actually I'll be doing it to 22 places. 
// To accuarately represent PI to 20 decimal places requires at least 17 hex digits but as my computer
// works with 8-bit values I need to use 18 hex digits (9 bytes).

// Lets start with the known value for PI to 20 decimal places + 1 extra to be accurate within 17 hex digits.
const decimal PI = 3.141592653589793238462M;

#region Console Colour Helpers
const string NORMAL = "\x1b[39m";
const string GREEN = "\x1b[92m";
const string YELLOW = "\x1b[93m";
#endregion



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




// Part C - Use Bailey-Borwein-Plouffe formula to calculate PI in base 16 (to 17 hex digit precision)
Console.WriteLine("\n\nC) Calculate PI using Bailey-Borwein-Plouffe formula:\n");
var calcPiBin = CalculatePi((17 + 2) * 4);      // Calculate PI with extra 2 hex digits of precision
var roundedCalcPiBin = calcPiBin with           // to allow truncating result to 17 hex digits
{
    FractionalPart = calcPiBin.FractionalPart >> 8,
    Precision = calcPiBin.Precision - 8
};

Console.WriteLine("\nOutcome:");
Console.WriteLine($"Calculated PI Value: {calcPiBin}");
Console.WriteLine($"Truncated to 17bp:   {roundedCalcPiBin}");
Console.WriteLine($"Known PI Value:      {piBin}\n");

var roundedCalcPiDec = ConvertFixedPointBinaryToDecimal(roundedCalcPiBin, 20);
Console.WriteLine("\nOutcome:");
Console.WriteLine($"PI Original Value:   {roundedCalcPiDec} ({roundedCalcPiDec.Scale} dp)");
Console.WriteLine($"Calculated PI Value: {piDec} ({piDec.Scale} dp)");

// Function to calculate PI in base 16 using the Bailey-Borwein-Plouffe formula
// Calculation is performed to given fractional part precision (in bits).
static FixedPointBinary CalculatePi(int precision)
{
    var size = precision + 4;       // Full size of values allow extra word for integral part
    var sum = new BigInteger(0);    // Variable to store sum

    for (int k = 0; k < 17; k++)
    {
        var dv = 8 * k;
        var a = (new BigInteger(4) << (precision)) / (dv + 1);
        var b = (new BigInteger(2) << (precision)) / (dv + 4);
        var c = (new BigInteger(1) << (precision)) / (dv + 5);
        var d = (new BigInteger(1) << (precision)) / (dv + 6);
        var v = a - b - c - d;
        var vs = v >> (4 * k);

        Console.WriteLine($"{YELLOW}k = {k:d2}{NORMAL}");
        Console.WriteLine($"    {a.ToString($"b{size}")[..size]} (4/{dv + 1:d3})");
        Console.WriteLine($"  - {b.ToString($"b{size}")[..size]} (2/{dv + 4:d3})");
        Console.WriteLine($"  - {c.ToString($"b{size}")[..size]} (1/{dv + 5:d3})");
        Console.WriteLine($"  - {d.ToString($"b{size}")[..size]} (1/{dv + 6:d3})");
        Console.WriteLine($"{GREEN}  = {v.ToString($"b{size}")[..size]} {v.ToString($"x{size/4}")[..(size/4)]}{NORMAL}");
        Console.WriteLine($" >> {vs.ToString($"b{size}")[..size]} {vs.ToString($"x{size/4}")[..(size/4)]}");
        Console.WriteLine($"  + {sum.ToString($"b{size}")[..size]} {sum.ToString($"x{size/4}")[..(size/4)]}");
        sum += vs;
        Console.WriteLine($"{GREEN}  = {sum.ToString($"b{size}")[..size]} {sum.ToString($"x{size/4}")[..(size/4)]}\n{NORMAL}");
    }

    // Create bit masks required to isolate integral and fractional parts when multiplying
    var integralMask = new BigInteger(0xF) << precision;
    var fractionalMask = (new BigInteger(0x1) << precision) - 1;
    return new FixedPointBinary((byte)((sum & integralMask) >> precision), sum & fractionalMask, precision);
}








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
