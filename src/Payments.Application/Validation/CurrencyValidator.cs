namespace Payments.Application.Validation;

/// <summary>
/// Valida códigos de moneda según ISO 4217
/// </summary>
public static class CurrencyValidator
{
    /// <summary>
    /// Códigos ISO 4217 soportados (LATAM + principales globales)
    /// </summary>
    private static readonly HashSet<string> ValidCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        // Latinoamérica
        "ARS", // Peso Argentino
        "BRL", // Real Brasileño
        "CLP", // Peso Chileno
        "UYU", // Peso Uruguayo
        "PYG", // Guaraní Paraguayo
        "BOB", // Boliviano
        "COP", // Peso Colombiano
        "PEN", // Sol Peruano
        "MXN", // Peso Mexicano
        
        // Principales globales
        "USD", // Dólar Estadounidense
        "EUR", // Euro
        "GBP", // Libra Esterlina
        "JPY", // Yen Japonés
        "CNY", // Yuan Chino
        "CHF", // Franco Suizo
        "CAD", // Dólar Canadiense
        "AUD", // Dólar Australiano
    };

    /// <summary>
    /// Valida si un código de moneda es válido
    /// </summary>
    public static bool IsValid(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return false;

        if (currency.Length != 3)
            return false;

        return ValidCurrencies.Contains(currency);
    }

    /// <summary>
    /// Normaliza el código de moneda (uppercase)
    /// </summary>
    public static string Normalize(string currency)
    {
        return currency?.ToUpperInvariant() ?? string.Empty;
    }

    /// <summary>
    /// Obtiene mensaje con las monedas válidas
    /// </summary>
    public static string GetValidCurrenciesMessage()
    {
        var sorted = ValidCurrencies.OrderBy(c => c);
        return $"Valid currencies: {string.Join(", ", sorted)}";
    }

    /// <summary>
    /// Obtiene todas las monedas válidas
    /// </summary>
    public static IReadOnlySet<string> GetValidCurrencies()
    {
        return ValidCurrencies;
    }
}
