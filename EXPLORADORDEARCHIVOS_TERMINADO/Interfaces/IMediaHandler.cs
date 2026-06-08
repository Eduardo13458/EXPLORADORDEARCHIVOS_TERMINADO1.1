namespace EXPLORADORDEARCHIVOS_TERMINADO.Interfaces
{
    /// <summary>
    /// Handler ligero que encapsula la apertura/reproducción de un medio.
    /// La fábrica devuelve un handler según la extensión y Form1 lo usa sin conocer
    /// las implementaciones concretas (reduce acoplamiento).
    /// </summary>
    public interface IMediaHandler
    {
        void Handle(string filePath);
        void Close();
    }
}
