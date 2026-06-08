using EXPLORADORDEARCHIVOS_TERMINADO.Constants;
using EXPLORADORDEARCHIVOS_TERMINADO.Data;
using EXPLORADORDEARCHIVOS_TERMINADO.Data.Readers;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using Npgsql;
using EXPLORADORDEARCHIVOS_TERMINADO.Models;
using System.Data.Common;
using System.Globalization;
using System.Text.Json;
using System.Xml;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Data
{
    public static class DataReader
    {
        private static readonly string DataFolder =
            Path.Combine(AppContext.BaseDirectory, "Data");

        // ── Registro de estrategias (patrón Estrategia / OCP) ─────────────
        // Para añadir un nuevo formato basta con crear una clase que implemente
        // IFormatReader y registrarla aquí. DataReader no necesita modificarse.

        private static readonly IReadOnlyList<IFormatReader> FormatReaders =
        [
            new CsvFormatReader(),
            new JsonFormatReader(),
            new XmlFormatReader(),
            new TxtFormatReader(),
            new ExcelFormatReader(),
        ];

        // ── ENRUTADOR: carga cualquier archivo por extension ───────────────

        /// <summary>
        /// Detecta el formato por extensión, selecciona la estrategia correcta
        /// y delega la lectura. No conoce los detalles de ningún formato.
        /// </summary>
        public static List<DataItem> LoadFromFile(string fullPath)
        {
            var ext = Path.GetExtension(fullPath).ToLowerInvariant();

            foreach (var reader in FormatReaders)
                if (reader.SupportedExtensions.Contains(ext))
                    return reader.Read(fullPath);

            throw new NotSupportedException(
                $"Extension '{ext}' no soportada. Use {FileExtensions.Csv}, {FileExtensions.Json}, {FileExtensions.Xml}, {FileExtensions.Txt} o .xlsx");
        }

        // ── CSV ────────────────────────────────────────────────────────────

        /// <summary>
        /// Acepta nombre simple (busca en DataFolder) o ruta absoluta externa.
        /// Mapea columnas por nombre de cabecera para tolerar columnas
        /// desordenadas.
        /// </summary>
        public static List<DataItem> ReadCsv(string fileNameOrPath = "laptops.csv")
        {
            var items = new List<DataItem>();
            var filePath = ResolvePath(fileNameOrPath);

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[CSV] Archivo no encontrado: {filePath}");
                return items;
            }

            try
            {
                var lines = File.ReadAllLines(filePath);
                if (lines.Length < 2) return items;

                var rawH = lines[0].Split(',');
                var headers = new string[rawH.Length];
                for (int h = 0; h < rawH.Length; h++)
                    headers[h] = rawH[h].Trim().ToLowerInvariant();

                int iCompany = Array.IndexOf(headers, "company");
                int iTypeName = Array.IndexOf(headers, "typename");
                int iCpu = Array.IndexOf(headers, "cpu");
                int iRam = Array.IndexOf(headers, "ram");
                int iPrice = Array.IndexOf(headers, "price");

                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    var cols = lines[i].Split(',');

                    try
                    {
                        items.Add(new DataItem
                        {
                            Id = i,
                            Source = DataSource.CSV,
                            Company = SafeGet(cols, iCompany),
                            TypeName = SafeGet(cols, iTypeName),
                            Cpu = SafeGet(cols, iCpu),
                            Ram = int.TryParse(SafeGet(cols, iRam), out int ram) ? ram : 0,
                            Price = double.TryParse(SafeGet(cols, iPrice),
                                           NumberStyles.Any, CultureInfo.InvariantCulture,
                                           out double price) ? price : 0
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CSV] Error en linea {i + 1}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CSV] Error al leer archivo: {ex.Message}");
            }

            return items;
        }

        // ── JSON ───────────────────────────────────────────────────────────

        /// <summary>
        /// Acepta nombre simple o ruta absoluta.
        /// Maneja JSON mal formados con try/catch sobre JsonException.
        /// </summary>
        public static List<DataItem> ReadJson(string fileNameOrPath = "videogames.json")
        {
            var items = new List<DataItem>();
            var filePath = ResolvePath(fileNameOrPath);

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[JSON] Archivo no encontrado: {filePath}");
                return items;
            }

            string jsonText;
            try { jsonText = File.ReadAllText(filePath); }
            catch (Exception ex)
            {
                Console.WriteLine($"[JSON] Error al leer archivo: {ex.Message}");
                return items;
            }

            try
            {
                using var doc = JsonDocument.Parse(jsonText);
                int id = 1000;

                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    try
                    {
                        items.Add(new DataItem
                        {
                            Id = id++,
                            Source = DataSource.JSON,
                            Title = GetJsonString(el, "Title"),
                            Genre = GetJsonString(el, "Genre"),
                            Sales = GetJsonDouble(el, "Sales"),
                            Platform = GetJsonString(el, "Platform")
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[JSON] Error en elemento {id}: {ex.Message}");
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[JSON] JSON mal formado: {ex.Message}");
            }

            return items;
        }

        // ── XML ────────────────────────────────────────────────────────────

        /// <summary>Acepta nombre simple o ruta absoluta.</summary>
        public static List<DataItem> ReadXml(string fileNameOrPath = "inventory.xml")
        {
            var items = new List<DataItem>();
            var filePath = ResolvePath(fileNameOrPath);

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[XML] Archivo no encontrado: {filePath}");
                return items;
            }

            try
            {
                var doc = new XmlDocument();
                doc.Load(filePath);

                var nodes = doc.SelectNodes("//Componente");
                if (nodes == null) return items;

                int id = 2000;
                foreach (XmlNode node in nodes)
                {
                    try
                    {
                        items.Add(new DataItem
                        {
                            Id = id++,
                            Source = DataSource.XML,
                            Tipo = node["Tipo"]?.InnerText ?? string.Empty,
                            Modelo = node["Modelo"]?.InnerText ?? string.Empty,
                            Stock = int.TryParse(node["Stock"]?.InnerText, out int s) ? s : 0
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[XML] Error en nodo {id}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[XML] Error al leer archivo: {ex.Message}");
            }

            return items;
        }

        // ── TXT ────────────────────────────────────────────────────────────

        /// <summary>Acepta nombre simple o ruta absoluta.</summary>
        public static List<DataItem> ReadTxt(string fileNameOrPath = "performance.txt")
        {
            var items = new List<DataItem>();
            var filePath = ResolvePath(fileNameOrPath);

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[TXT] Archivo no encontrado: {filePath}");
                return items;
            }

            try
            {
                var lines = File.ReadAllLines(filePath);

                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    var parts = lines[i].Split('|');
                    if (parts.Length < 4) continue;

                    try
                    {
                        items.Add(new DataItem
                        {
                            Id = 3000 + i,
                            Source = DataSource.TXT,
                            Minuto = ParseInt(parts[0]),
                            UsoCPU = ParseDouble(parts[1]),
                            Temperatura = ParseDouble(parts[2]),
                            FPS = ParseDouble(parts[3])
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[TXT] Error en linea {i + 1}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TXT] Error al leer archivo: {ex.Message}");
            }

            return items;
        }

        // ── SQL Server ─────────────────────────────────────────────────────

        /// <summary>
        /// Lee usuarios de SQL Server. Si la conexion falla, genera datos
        /// simulados. Columnas BD: user_id, full_name, user_email, zone.
        /// </summary>
        public static List<DataItem> ReadFromDatabase(string connectionString)
        {
            var items = new List<DataItem>();

            try
            {
                using var conn = new SqlConnection(connectionString);
                conn.Open();

                const string sql =
                    "SELECT user_id, full_name, user_email, zone FROM dbo.Users";

                using var cmd = new SqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    items.Add(new DataItem
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("user_id")),
                        Source = DataSource.DB,
                        UserName = reader.GetString(reader.GetOrdinal("full_name")),
                        Email = reader.GetString(reader.GetOrdinal("user_email")),
                        Region = reader.GetString(reader.GetOrdinal("zone"))
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB] Conexion fallida, usando datos simulados: {ex.Message}");
                items = GetSimulatedDbData();
            }

            return items;
        }

        // ── Importar dinamico desde DatosIntegrados (SQL Server) ────────

        public static List<DataItem> ReadFromSqlServer(string connectionString,
            string tableName = "DatosIntegrados")
        {
            DatabaseExporter.EnsureSqlServerDatabase(connectionString);

            using var conn = new SqlConnection(connectionString);
            conn.Open();

            using var check = new SqlCommand(
                $"SELECT OBJECT_ID('{tableName}', 'U')", conn);
            if (check.ExecuteScalar() is null or DBNull)
                return [];

            using var cmd = new SqlCommand($"SELECT * FROM [{tableName}]", conn);
            using var reader = cmd.ExecuteReader();
            return ReadItemsFromReader(reader);
        }

        // ── Importar dinamico desde DatosIntegrados (MariaDB) ───────────

        public static List<DataItem> ReadFromMariaDb(string connectionString,
            string tableName = "DatosIntegrados")
        {
            DatabaseExporter.EnsureMySqlDatabase(connectionString);

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            using var check = new MySqlCommand(
                "SELECT COUNT(*) FROM information_schema.tables " +
                $"WHERE table_schema = DATABASE() AND table_name = '{tableName}'", conn);
            if (Convert.ToInt32(check.ExecuteScalar()) == 0)
                return [];

            using var cmd = new MySqlCommand($"SELECT * FROM `{tableName}`", conn);
            using var reader = cmd.ExecuteReader();
            return ReadItemsFromReader(reader);
        }

        // ── Importar dinamico desde DatosIntegrados (PostgreSQL) ────────

        public static List<DataItem> ReadFromPostgreSql(string connectionString,
            string tableName = "DatosIntegrados")
        {
            DatabaseExporter.EnsurePostgreSqlDatabase(connectionString);

            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var check = new NpgsqlCommand(
                "SELECT EXISTS (SELECT FROM information_schema.tables " +
                $"WHERE table_name = '{tableName}')", conn);
            if (check.ExecuteScalar() is false)
                return [];

            using var cmd = new NpgsqlCommand($"SELECT * FROM \"{tableName}\"", conn);
            using var reader = cmd.ExecuteReader();
            return ReadItemsFromReader(reader);
        }

        private static List<DataItem> ReadItemsFromReader(DbDataReader reader)
        {
            var items = new List<DataItem>();
            var columnNames = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
                columnNames.Add(reader.GetName(i));

            int id = 4000;
            while (reader.Read())
            {
                var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (!reader.IsDBNull(i))
                    {
                        string val = reader.GetValue(i)?.ToString() ?? "";
                        if (!string.IsNullOrWhiteSpace(val))
                            fields[columnNames[i]] = val;
                    }
                }
                if (fields.Count > 0)
                    items.Add(DataItemMapper.MapFieldsToItem(fields, DataSource.DB, id++));
            }
            return items;
        }

        // ── Helpers privados ───────────────────────────────────────────────

        // Delegados a DataItemMapper para que los métodos legacy ReadCsv/ReadJson/ReadTxt
        // no necesiten duplicar la lógica de parse.
        private static int ParseInt(string s) => DataItemMapper.ParseInt(s);
        private static double ParseDouble(string s) => DataItemMapper.ParseDouble(s);

        /// <summary>
        /// Si el argumento es ruta absoluta la usa directamente;
        /// si es solo nombre de archivo la combina con DataFolder.
        /// </summary>
        private static string ResolvePath(string fileNameOrPath) =>
            Path.IsPathRooted(fileNameOrPath)
                ? fileNameOrPath
                : Path.Combine(DataFolder, fileNameOrPath);

        private static string SafeGet(string[] arr, int idx) =>
            idx >= 0 && idx < arr.Length ? arr[idx].Trim() : string.Empty;

        private static string GetJsonString(JsonElement el, string prop) =>
            el.TryGetProperty(prop, out var v) ? v.GetString() ?? string.Empty : string.Empty;

        private static double GetJsonDouble(JsonElement el, string prop) =>
            el.TryGetProperty(prop, out var v) && v.TryGetDouble(out double d) ? d : 0;

        private static List<DataItem> GetSimulatedDbData() =>
        [
            new() { Id = 4001, Source = DataSource.DB, UserName = "Ana Garcia",      Email = "ana@mail.com",    Region = "CDMX"        },
            new() { Id = 4002, Source = DataSource.DB, UserName = "Carlos Lopez",    Email = "carlos@mail.com", Region = "Monterrey"   },
            new() { Id = 4003, Source = DataSource.DB, UserName = "Maria Perez",     Email = "maria@mail.com",  Region = "Guadalajara" },
            new() { Id = 4004, Source = DataSource.DB, UserName = "Juan Rodriguez",  Email = "juan@mail.com",   Region = "CDMX"        },
            new() { Id = 4005, Source = DataSource.DB, UserName = "Laura Torres",    Email = "laura@mail.com",  Region = "Monterrey"   },
            new() { Id = 4006, Source = DataSource.DB, UserName = "Pedro Sanchez",   Email = "pedro@mail.com",  Region = "Puebla"      },
            new() { Id = 4007, Source = DataSource.DB, UserName = "Sofia Martinez",  Email = "sofia@mail.com",  Region = "Guadalajara" },
            new() { Id = 4008, Source = DataSource.DB, UserName = "Diego Hernandez", Email = "diego@mail.com",  Region = "CDMX"        },
        ];
    }
}
