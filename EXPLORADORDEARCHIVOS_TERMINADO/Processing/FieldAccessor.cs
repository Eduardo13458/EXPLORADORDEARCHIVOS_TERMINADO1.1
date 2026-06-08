using EXPLORADORDEARCHIVOS_TERMINADO.Models;
using System.Globalization;
using System;
using System.Collections.Generic;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Processing;

/// <summary>
/// Responsabilidad única: acceso dinámico a campos de un DataItem.
/// Abstrae si el campo es una propiedad conocida o vive en ExtraFields,
/// de modo que el resto del sistema no necesita hacer ese switch.
/// </summary>
public static class FieldAccessor
{
    private static readonly Dictionary<string, Func<DataItem, string>> StringFieldAccessors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "company", i => i.Company }, { "marca", i => i.Company },
            { "typename", i => i.TypeName },
            { "cpu", i => i.Cpu },
            { "title", i => i.Title }, { "titulo", i => i.Title },
            { "genre", i => i.Genre }, { "genero", i => i.Genre },
            { "platform", i => i.Platform }, { "plataforma", i => i.Platform },
            { "tipo", i => i.Tipo }, { "type", i => i.Tipo },
            { "modelo", i => i.Modelo }, { "model", i => i.Modelo },
            { "username", i => i.UserName }, { "nombre", i => i.UserName },
            { "email", i => i.Email }, { "correo", i => i.Email },
            { "region", i => i.Region }, { "zona", i => i.Region },
            { "source", i => i.Source.ToString() }
        };

    public static string GetStringValue(DataItem item, string fieldName)
    {
        if (StringFieldAccessors.TryGetValue(fieldName, out var accessor))
            return accessor(item) ?? string.Empty;

        return item.ExtraFields.TryGetValue(fieldName, out var extra) ? extra : string.Empty;
    }

    private static readonly Dictionary<string, Func<DataItem, double>> NumericFieldAccessors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "id", i => i.Id },
            { "price", i => i.Price }, { "precio", i => i.Price },
            { "ram", i => i.Ram },
            { "sales", i => i.Sales }, { "ventas", i => i.Sales },
            { "stock", i => i.Stock }, { "cantidad", i => i.Stock },
            { "minuto", i => i.Minuto }, { "minute", i => i.Minuto },
            { "usocpu", i => i.UsoCPU },
            { "temperatura", i => i.Temperatura }, { "temperature", i => i.Temperatura },
            { "fps", i => i.FPS }
        };

    public static double GetNumericValue(DataItem item, string fieldName)
    {
        if (NumericFieldAccessors.TryGetValue(fieldName, out var getter))
            return getter(item);

        if (item.ExtraFields.TryGetValue(fieldName, out var extra) &&
            double.TryParse(extra, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
            return val;

        return 0;
    }

    /// <summary>
    /// Devuelve true si el campo existe en el item (aunque su valor sea cero).
    /// Útil para distinguir "campo no presente" de "campo con valor 0".
    /// </summary>
    private static readonly Dictionary<string, Func<DataItem, (double value, bool exists)>> NumericTryAccessors =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "id", i => (i.Id, true) },
            { "price", i => (i.Price, i.Price != 0 || i.Source == DataSource.CSV) },
            { "precio", i => (i.Price, i.Price != 0 || i.Source == DataSource.CSV) },
            { "ram", i => (i.Ram, i.Ram != 0) },
            { "sales", i => (i.Sales, i.Sales != 0 || i.Source == DataSource.JSON) },
            { "ventas", i => (i.Sales, i.Sales != 0 || i.Source == DataSource.JSON) },
            { "stock", i => (i.Stock, i.Stock != 0 || i.Source == DataSource.XML) },
            { "cantidad", i => (i.Stock, i.Stock != 0 || i.Source == DataSource.XML) },
            { "minuto", i => (i.Minuto, i.Minuto != 0) },
            { "minute", i => (i.Minuto, i.Minuto != 0) },
            { "usocpu", i => (i.UsoCPU, i.UsoCPU != 0) },
            { "temperatura", i => (i.Temperatura, i.Temperatura != 0) },
            { "temperature", i => (i.Temperatura, i.Temperatura != 0) },
            { "fps", i => (i.FPS, i.FPS != 0) }
        };

    public static bool TryGetNumericValue(DataItem item, string fieldName, out double value)
    {
        if (NumericTryAccessors.TryGetValue(fieldName, out var accessor))
        {
            var (val, exists) = accessor(item);
            value = val;
            return exists;
        }

        if (item.ExtraFields.TryGetValue(fieldName, out var extra) &&
            double.TryParse(extra, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            return true;

        value = 0;
        return false;
    }

    /// <summary>
    /// Analiza los items y descubre qué campos de texto y numéricos contienen datos reales.
    /// Incluye propiedades conocidas y ExtraFields.
    /// </summary>
    public static (List<string> StringFields, List<string> NumericFields)
        DiscoverFields(List<DataItem> items)
    {
        var stringFields = new List<string>();
        var numericFields = new List<string>();
        var strSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var numSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.Company)  && strSet.Add("Company"))  stringFields.Add("Company");
            if (!string.IsNullOrEmpty(item.TypeName) && strSet.Add("TypeName")) stringFields.Add("TypeName");
            if (!string.IsNullOrEmpty(item.Cpu)      && strSet.Add("Cpu"))      stringFields.Add("Cpu");
            if (!string.IsNullOrEmpty(item.Title)    && strSet.Add("Title"))    stringFields.Add("Title");
            if (!string.IsNullOrEmpty(item.Genre)    && strSet.Add("Genre"))    stringFields.Add("Genre");
            if (!string.IsNullOrEmpty(item.Platform) && strSet.Add("Platform")) stringFields.Add("Platform");
            if (!string.IsNullOrEmpty(item.Tipo)     && strSet.Add("Tipo"))     stringFields.Add("Tipo");
            if (!string.IsNullOrEmpty(item.Modelo)   && strSet.Add("Modelo"))   stringFields.Add("Modelo");
            if (!string.IsNullOrEmpty(item.UserName) && strSet.Add("UserName")) stringFields.Add("UserName");
            if (!string.IsNullOrEmpty(item.Email)    && strSet.Add("Email"))    stringFields.Add("Email");
            if (!string.IsNullOrEmpty(item.Region)   && strSet.Add("Region"))   stringFields.Add("Region");

            if (item.Price      > 0 && numSet.Add("Price"))       numericFields.Add("Price");
            if (item.Ram        > 0 && numSet.Add("Ram"))         numericFields.Add("Ram");
            if (item.Sales      > 0 && numSet.Add("Sales"))       numericFields.Add("Sales");
            if (item.Stock      > 0 && numSet.Add("Stock"))       numericFields.Add("Stock");
            if (item.Minuto     > 0 && numSet.Add("Minuto"))      numericFields.Add("Minuto");
            if (item.UsoCPU     > 0 && numSet.Add("UsoCPU"))      numericFields.Add("UsoCPU");
            if (item.Temperatura> 0 && numSet.Add("Temperatura")) numericFields.Add("Temperatura");
            if (item.FPS        > 0 && numSet.Add("FPS"))         numericFields.Add("FPS");

            foreach (var kv in item.ExtraFields)
            {
                if (string.IsNullOrWhiteSpace(kv.Value)) continue;
                if (double.TryParse(kv.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                { if (numSet.Add(kv.Key)) numericFields.Add(kv.Key); }
                else
                { if (strSet.Add(kv.Key)) stringFields.Add(kv.Key); }
            }
        }

        return (stringFields, numericFields);
    }
}
