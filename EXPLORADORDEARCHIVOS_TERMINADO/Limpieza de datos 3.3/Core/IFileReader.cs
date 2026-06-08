namespace EXPLORADORDEARCHIVOS_TERMINADO.Limpieza_de_datos_3._3.Core;
public interface IFileReader
{
    IEnumerable<IDictionary<string, object>> ReadFile(string path);
}
