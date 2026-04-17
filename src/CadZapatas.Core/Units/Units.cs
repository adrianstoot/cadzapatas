namespace CadZapatas.Core.Units;

/// <summary>
/// Unidades de base del sistema. El modelo trabaja internamente en SI (m, N, Pa, kg).
/// Conversiones a unidades practicas de ingenieria espanola (kN, kNm, MPa, kPa, cm).
/// </summary>
public static class Unit
{
    // Longitud
    public const double Meter = 1.0;
    public const double Centimeter = 0.01;
    public const double Millimeter = 0.001;

    // Fuerza
    public const double Newton = 1.0;
    public const double Kilonewton = 1000.0;
    public const double Meganewton = 1_000_000.0;

    // Momento
    public const double NewtonMeter = 1.0;
    public const double KilonewtonMeter = 1000.0;

    // Presion / tension
    public const double Pascal = 1.0;
    public const double Kilopascal = 1000.0;
    public const double Megapascal = 1_000_000.0;

    // Densidad
    public const double KgPerCubicMeter = 1.0;
    public const double KnPerCubicMeter = 101.9716; // 1 kN/m3 equivalente en kg/m3 (peso especifico)

    // Angulos
    public const double Radian = 1.0;
    public const double Degree = Math.PI / 180.0;
}

/// <summary>
/// Sistema de unidades configurable del proyecto.
/// </summary>
public sealed class UnitSystem
{
    public LengthUnit Length { get; init; } = LengthUnit.Meter;
    public ForceUnit Force { get; init; } = ForceUnit.Kilonewton;
    public StressUnit Stress { get; init; } = StressUnit.Megapascal;
    public AngleUnit Angle { get; init; } = AngleUnit.Degree;

    public static UnitSystem Default => new();
}

public enum LengthUnit { Meter, Centimeter, Millimeter }
public enum ForceUnit { Newton, Kilonewton, Meganewton }
public enum StressUnit { Pascal, Kilopascal, Megapascal }
public enum AngleUnit { Degree, Radian }
