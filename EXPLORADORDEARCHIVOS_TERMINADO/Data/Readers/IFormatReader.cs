using EXPLORADORDEARCHIVOS_TERMINADO.Models;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Data.Readers;

/// <summary>
/// Contrato del patrón Estrategia para lectores de formato.
/// Cada implementación sabe cómo leer un único formato de archivo
/// y devuelve una lista de DataItem normalizada.
/// DataReader actúa como contexto: elige la estrategia correcta
/// sin conocer los detalles internos de cada formato.
/// </summary>
public interface IFormatReader
{
    /// <summary>Extensiones soportadas en minúsculas, e.g. ".csv".</summary>
    IReadOnlyList<string> SupportedExtensions { get; }

    /// <summary>Lee el archivo y devuelve los ítems parseados.</summary>
    List<DataItem> Read(string fullPath);
}
