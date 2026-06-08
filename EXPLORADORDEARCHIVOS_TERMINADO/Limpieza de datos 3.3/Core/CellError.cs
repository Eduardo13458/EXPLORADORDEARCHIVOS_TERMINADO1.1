namespace EXPLORADORDEARCHIVOS_TERMINADO.Limpieza_de_datos_3._3.Core;

/// <summary>
/// Tipos de dato que el inferidor puede asignar a una columna.
/// </summary>
public enum ColumnDataType { Text, Numeric, Date, Phone, PersonName, Salary, Email }

/// <summary>
/// Clasifica el motivo del error de celda.
/// </summary>
public enum CellErrorKind
{
    /// <summary>Texto encontrado en una columna numérica o de fecha.</summary>
    UnexpectedText,
    /// <summary>Número encontrado en una columna de texto (ej. estado, nombre).</summary>
    UnexpectedNumeric,
    /// <summary>Fecha inválida encontrada en una columna de fecha.</summary>
    UnexpectedDate,
}

/// <summary>
/// Describe una celda cuyo valor no es compatible con el tipo dominante de su columna.
/// </summary>
public sealed record CellError(
    int RowIndex,
    string Column,
    string Value,
    ColumnDataType ExpectedType,
    CellErrorKind ErrorKind
)
{
    public string Description => ErrorKind switch
    {
        CellErrorKind.UnexpectedNumeric =>
            $"Fila {RowIndex + 1}, columna '{Column}': " +
            $"se encontró un número (\"{Value}\") en una columna de texto.",
        CellErrorKind.UnexpectedText =>
            $"Fila {RowIndex + 1}, columna '{Column}': " +
            $"se esperaba {ExpectedType} pero se encontró texto \"{Value}\".",
        CellErrorKind.UnexpectedDate =>
            $"Fila {RowIndex + 1}, columna '{Column}': " +
            $"fecha inválida \"{Value}\".",
        _ =>
            $"Fila {RowIndex + 1}, columna '{Column}': valor inesperado \"{Value}\"."
    };
}