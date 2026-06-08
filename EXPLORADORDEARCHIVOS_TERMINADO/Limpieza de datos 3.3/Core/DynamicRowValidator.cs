using FluentValidation;
namespace EXPLORADORDEARCHIVOS_TERMINADO.Limpieza_de_datos_3._3.Core;
public sealed class DynamicRowValidator : AbstractValidator<IDictionary<string, object>>
{
    public DynamicRowValidator()
    {
        // Regla 1: Ningún campo clave debe ser null o cadena vacía.
        RuleFor(row => row)
            .Custom((row, context) =>
            {
                foreach (var kvp in row)
                {
                    var value = kvp.Value?.ToString();
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        context.AddFailure(kvp.Key, $"El campo '{kvp.Key}' está vacío o es nulo.");
                    }
                }
            });

        // Regla 2: Columnas cuyo nombre contenga "fecha" deben tener un formato de fecha válido.
        RuleFor(row => row)
            .Custom((row, context) =>
            {
                var dateKeys = row.Keys.Where(k => k.Contains("fecha", StringComparison.OrdinalIgnoreCase));
                foreach (var key in dateKeys)
                {
                    var raw = row[key]?.ToString();
                    if (!string.IsNullOrWhiteSpace(raw) && !DateTime.TryParse(raw, out _))
                    {
                        context.AddFailure(key, $"El campo '{key}' con valor '{raw}' no es una fecha válida.");
                    }
                }
            });

        // Regla 3: Columnas cuyo nombre contenga "email" deben tener formato de correo básico.
        RuleFor(row => row)
            .Custom((row, context) =>
            {
                var emailKeys = row.Keys.Where(k => k.Contains("email", StringComparison.OrdinalIgnoreCase)
                                                  || k.Contains("correo", StringComparison.OrdinalIgnoreCase));
                foreach (var key in emailKeys)
                {
                    var raw = row[key]?.ToString();
                    if (!string.IsNullOrWhiteSpace(raw) && !raw.Contains('@'))
                    {
                        context.AddFailure(key, $"El campo '{key}' con valor '{raw}' no tiene formato de email válido.");
                    }
                }
            });
    }

}
