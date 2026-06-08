using System.Dynamic;
using System.Linq.Dynamic.Core;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Limpieza_de_datos_3._3.Core;

/// <summary>
/// Resultado del pipeline: datos válidos, filas rechazadas y log de errores.
/// </summary>
public sealed record PipelineResult(
    IReadOnlyList<IDictionary<string, object>> ValidRows,
    IReadOnlyList<IDictionary<string, object>> InvalidRows,
    IReadOnlyList<string> ErrorLog
);

/// <summary>
/// Orquestador principal: normaliza, valida y ordena los datos dinámicos.
/// Separa el flujo en pasos atómicos y produce un PipelineResult detallado.
/// </summary>
public sealed class DataPipeline
{
    private readonly DynamicRowValidator _validator = new();

    /// <summary>
    /// Ejecuta el pipeline completo sobre la colección de filas.
    /// </summary>
    /// <param name="rows">Filas leídas por el IFileReader.</param>
    /// <param name="orderByExpression">
    /// Expresión de ordenamiento compatible con Dynamic LINQ, p.ej. "Nombre ASC" o "Fecha DESC".
    /// Si es null o vacío no se aplica ordenamiento.
    /// </param>
    public PipelineResult Execute(
        IEnumerable<IDictionary<string, object>> rows,
        string? orderByExpression = null)
    {
        var normalized = Normalize(rows).ToList();
        var (valid, invalid, log) = Validate(normalized);
        var ordered = ApplyOrdering(valid, orderByExpression);

        return new PipelineResult(ordered, invalid, log);
    }

    // ── Paso 1: Normalización ───────────────────────────────────────────────
    private static IEnumerable<IDictionary<string, object>> Normalize(
        IEnumerable<IDictionary<string, object>> rows)
    {
        foreach (var row in rows)
        {
            var clean = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in row)
            {
                // Trim en strings; los demás tipos se conservan tal cual.
                clean[kvp.Key] = kvp.Value is string s ? s.Trim() : kvp.Value;
            }
            yield return clean;
        }
    }

    // ── Paso 2: Validación ──────────────────────────────────────────────────
    private (List<IDictionary<string, object>> valid,
             List<IDictionary<string, object>> invalid,
             List<string> log)
        Validate(IReadOnlyList<IDictionary<string, object>> rows)
    {
        var valid = new List<IDictionary<string, object>>();
        var invalid = new List<IDictionary<string, object>>();
        var log = new List<string>();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var result = _validator.Validate(row);

            if (result.IsValid)
            {
                valid.Add(row);
            }
            else
            {
                invalid.Add(row);
                foreach (var error in result.Errors)
                {
                    log.Add($"[Fila {i + 2}] Campo '{error.PropertyName}': {error.ErrorMessage}");
                }
            }
        }

        return (valid, invalid, log);
    }

    // ── Paso 3: Ordenamiento dinámico ───────────────────────────────────────
    private static IReadOnlyList<IDictionary<string, object>> ApplyOrdering(
        IEnumerable<IDictionary<string, object>> rows,
        string? orderByExpression)
    {
        if (string.IsNullOrWhiteSpace(orderByExpression))
            return rows.ToList();

        // Convertir a ExpandoObject para que Dynamic LINQ pueda acceder a las propiedades.
        var expandos = rows
            .Select(r =>
            {
                IDictionary<string, object?> expando = new ExpandoObject();
                foreach (var kvp in r)
                    expando[kvp.Key] = kvp.Value;
                return (ExpandoObject)expando;
            })
            .AsQueryable();

        try
        {
            var ordered = expandos.OrderBy(orderByExpression).ToList();

            // Reconvertir a IDictionary<string, object> para mantener la interfaz pública.
            return ordered
                .Select(e => (IDictionary<string, object>)e
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value ?? (object)string.Empty))
                .ToList();
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                $"La expresión de ordenamiento '{orderByExpression}' no es válida: {ex.Message}", ex);
        }
    }
}
