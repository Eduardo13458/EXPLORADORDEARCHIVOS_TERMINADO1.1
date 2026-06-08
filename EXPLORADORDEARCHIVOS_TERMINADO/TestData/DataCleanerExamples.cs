// Ejemplos de Uso: Archivos de Prueba DataCleaner
// Solo usa el método público Clean() de DataCleaner

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EXPLORADORDEARCHIVOS_TERMINADO.Limpieza_de_datos_3._3.Core;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Examples
{
    /// <summary>
    /// Ejemplos prácticos de cómo usar los archivos de prueba
    /// con la herramienta DataCleaner utilizando el método público Clean().
    /// </summary>
    public static class DataCleanerExamples
    {
        // ════════════════════════════════════════════════════════════
        // EJEMPLO 1: Limpiar clientes_sucios.csv
        // ════════════════════════════════════════════════════════════

        public static void Example1_CleanClientes()
        {
            Console.WriteLine("═══ EJEMPLO 1: Limpiar clientes_sucios.csv ═══\n");

            // Simular carga de datos (normalmente vendría de DataPipeline)
            var rows = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "ID", "1" },
                    { "Nombre", "Juan*García" },
                    { "Apellido", "López#García" },
                    { "Fecha_Registro", "2024-01-15" },
                    { "Deuda", "€2,450.50" }
                },
                new Dictionary<string, object>
                {
                    { "ID", "2" },
                    { "Nombre", "María@Perez" },
                    { "Apellido", "García^López" },
                    { "Fecha_Registro", "15/01/2024" },
                    { "Deuda", "$ 1.234" }
                }
            };

            // Inferencia de tipos
            var columnTypes = new Dictionary<string, ColumnDataType>
            {
                { "Nombre", ColumnDataType.Text },
                { "Apellido", ColumnDataType.Text },
                { "Fecha_Registro", ColumnDataType.Date },
                { "Deuda", ColumnDataType.Numeric }
            };

            // Especificar qué columnas limpiar
            var columnsToClean = new HashSet<string> { "Nombre", "Apellido", "Fecha_Registro", "Deuda" };

            // Ejecutar limpieza
            var (cleanedRows, changeLog, changedCells) = DataCleaner.Clean(rows, columnTypes, columnsToClean);

            // Mostrar resultados
            Console.WriteLine("RESULTADOS:\n");
            foreach (var cell in changedCells)
            {
                Console.WriteLine($"  [Fila {cell.RowIndex}, {cell.Column}]");
                Console.WriteLine($"    Original: \"{cell.OriginalValue}\"");
                Console.WriteLine($"    Limpio:   \"{cell.CleanedValue}\"");
                Console.WriteLine();
            }

            Console.WriteLine($"Total cambios: {changedCells.Count}\n");
        }

        // ════════════════════════════════════════════════════════════
        // EJEMPLO 2: Limpiar solo NOMBRES de personas
        // ════════════════════════════════════════════════════════════

        public static void Example2_CleanPersonNames()
        {
            Console.WriteLine("═══ EJEMPLO 2: Limpiar solo nombres de personas ═══\n");

            // Estos nombres tienen caracteres especiales PERO también guiones/apóstrofos válidos
            var testNames = new[]
            {
                "Jean-Paul*García",      // Guion OK, * eliminar
                "O'Brien#López",         // Apóstrofo OK, # eliminar
                "María-José@Sánchez",    // Guion OK, @ eliminar
                "José^Miguel",           // ^ eliminar
            };

            Console.WriteLine("Limpieza de nombres de PERSONAS (strict mode):\n");
            foreach (var name in testNames)
            {
                var cleanedRow = new Dictionary<string, object> { { "Nombre", name } };
                var columnTypes = new Dictionary<string, ColumnDataType> { { "Nombre", ColumnDataType.Text } };
                var columnsToClean = new HashSet<string> { "Nombre" };

                var (cleaned, log, cells) = DataCleaner.Clean(
                    new[] { cleanedRow }.ToList(),
                    columnTypes,
                    columnsToClean
                );

                string cleanedValue = cells.FirstOrDefault()?.CleanedValue ?? name;
                Console.WriteLine($"  {name,-25} → {cleanedValue}");
            }

            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════
        // EJEMPLO 3: Normalizar fechas variadas
        // ════════════════════════════════════════════════════════════

        public static void Example3_NormalizeDates()
        {
            Console.WriteLine("═══ EJEMPLO 3: Normalizar fechas variadas ═══\n");

            var testDates = new[]
            {
                ("2024-01-15", "ISO (sin cambio)"),
                ("15/01/2024", "DD/MM/YYYY"),
                ("2024.01.15", "Con puntos"),
                ("15 Jan 2024", "Letra de mes"),
                ("01-15-2024", "MM-DD-YYYY"),
                ("January 15, 2024", "Mes completo"),
            };

            Console.WriteLine("Normalización de fechas:\n");
            foreach (var (date, desc) in testDates)
            {
                var rows = new List<IDictionary<string, object>>
                {
                    new Dictionary<string, object> { { "Fecha", date } }
                };
                var columnTypes = new Dictionary<string, ColumnDataType> { { "Fecha", ColumnDataType.Date } };
                var (_, _, cells) = DataCleaner.Clean(rows, columnTypes);

                string cleaned = cells.FirstOrDefault()?.CleanedValue ?? date;
                Console.WriteLine($"  {date,-20} → {cleaned,-12} ({desc})");
            }

            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════
        // EJEMPLO 4: Limpiar números con moneda
        // ════════════════════════════════════════════════════════════

        public static void Example4_CleanNumbers()
        {
            Console.WriteLine("═══ EJEMPLO 4: Limpiar números con moneda ═══\n");

            var testNumbers = new[]
            {
                ("€299.99", "Euro"),
                ("$ 89.50", "Dollar con espacio"),
                ("¥4.500", "Yen"),
                ("1.234,56€", "Coma decimal europeo"),
                ("$ 1.499,95", "Dollar con coma decimal"),
                ("2.500", "Sin símbolo"),
            };

            Console.WriteLine("Limpieza de números:\n");
            foreach (var (num, desc) in testNumbers)
            {
                var rows = new List<IDictionary<string, object>>
                {
                    new Dictionary<string, object> { { "Monto", num } }
                };
                var columnTypes = new Dictionary<string, ColumnDataType> { { "Monto", ColumnDataType.Numeric } };
                var (_, _, cells) = DataCleaner.Clean(rows, columnTypes);

                string cleaned = cells.FirstOrDefault()?.CleanedValue ?? num;
                Console.WriteLine($"  {num,-15} → {cleaned,-10} ({desc})");
            }

            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════
        // EJEMPLO 5: Procesar archivo completo (FormCorrector style)
        // ════════════════════════════════════════════════════════════

        public static void Example5_ProcessCompleteFile()
        {
            Console.WriteLine("═══ EJEMPLO 5: Procesar archivo completo ═══\n");

            string filePath = @"TestData/clientes_sucios.csv";

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"❌ Archivo no encontrado: {filePath}\n");
                return;
            }

            Console.WriteLine($"Procesando: {Path.GetFileName(filePath)}\n");
            Console.WriteLine($"Estado: ✓ Archivo encontrado\n");

            // Simular lectura y limpieza (el DataPipeline real hace esto)
            var exampleRows = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "ID", "1" },
                    { "Nombre", "Juan*García" },
                    { "Fecha_Registro", "15/01/2024" },
                    { "Deuda", "€1.234" }
                },
                new Dictionary<string, object>
                {
                    { "ID", "2" },
                    { "Nombre", "María@Pérez" },
                    { "Fecha_Registro", "2024-01-20" },
                    { "Deuda", "$ 2.500" }
                }
            };

            var columnTypes = new Dictionary<string, ColumnDataType>
            {
                { "Nombre", ColumnDataType.Text },
                { "Fecha_Registro", ColumnDataType.Date },
                { "Deuda", ColumnDataType.Numeric }
            };

            var (cleanedRows, changeLog, changedCells) = DataCleaner.Clean(exampleRows, columnTypes);

            Console.WriteLine("CAMBIOS DETECTADOS:\n");
            if (changeLog.Count == 0)
            {
                Console.WriteLine("  (sin cambios)");
            }
            else
            {
                foreach (var log in changeLog)
                {
                    Console.WriteLine($"  • {log}");
                }
            }

            Console.WriteLine($"\nTotal cambios: {changedCells.Count}");
            Console.WriteLine($"Filas procesadas: {cleanedRows.Count}\n");
        }

        // ════════════════════════════════════════════════════════════
        // EJEMPLO 6: Comparar antes y después (tabla)
        // ════════════════════════════════════════════════════════════

        public static void Example6_BeforeAfterComparison()
        {
            Console.WriteLine("═══ EJEMPLO 6: Comparación antes/después ═══\n");

            var rows = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "Nombre", "Juan*García" },
                    { "Email", "juan@test.com" },
                    { "Fecha", "15/01/2024" },
                    { "Monto", "€1.500,50" }
                },
                new Dictionary<string, object>
                {
                    { "Nombre", "María@Pérez" },
                    { "Email", "maria@test.com" },
                    { "Fecha", "2024-01-20" },
                    { "Monto", "$ 2.300" }
                }
            };

            var columnTypes = new Dictionary<string, ColumnDataType>
            {
                { "Nombre", ColumnDataType.Text },
                { "Fecha", ColumnDataType.Date },
                { "Monto", ColumnDataType.Numeric }
            };

            var (cleanedRows, _, changedCells) = DataCleaner.Clean(rows, columnTypes);

            Console.WriteLine("┌─────────┬─────────────────────┬────────────────┐");
            Console.WriteLine("│ Campo   │ ANTES               │ DESPUÉS        │");
            Console.WriteLine("├─────────┼─────────────────────┼────────────────┤");

            foreach (var cell in changedCells)
            {
                Console.WriteLine($"│ {cell.Column,-7} │ {cell.OriginalValue,-19} │ {cell.CleanedValue,-14} │");
            }

            Console.WriteLine("└─────────┴─────────────────────┴────────────────┘\n");
        }

        // ════════════════════════════════════════════════════════════
        // EJEMPLO 7: Seleccionar columnas a limpiar
        // ════════════════════════════════════════════════════════════

        public static void Example7_SelectiveClean()
        {
            Console.WriteLine("═══ EJEMPLO 7: Limpieza selectiva (solo algunas columnas) ═══\n");

            var rows = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "Nombre", "Juan*García" },
                    { "Empresa", "Tech*Company" },
                    { "Monto", "€1.500" }
                }
            };

            var columnTypes = new Dictionary<string, ColumnDataType>
            {
                { "Nombre", ColumnDataType.Text },
                { "Empresa", ColumnDataType.Text },
                { "Monto", ColumnDataType.Numeric }
            };

            // Opción A: Limpiar TODO
            Console.WriteLine("Opción A: Limpiar TODAS las columnas");
            var (cleaned1, _, cells1) = DataCleaner.Clean(rows, columnTypes);
            Console.WriteLine($"  Cambios: {cells1.Count}\n");

            // Opción B: Limpiar SOLO Nombre
            Console.WriteLine("Opción B: Limpiar SOLO 'Nombre'");
            var columnsToClean = new HashSet<string> { "Nombre" };
            var (cleaned2, _, cells2) = DataCleaner.Clean(rows, columnTypes, columnsToClean);
            Console.WriteLine($"  Cambios: {cells2.Count}\n");

            foreach (var cell in cells2)
            {
                Console.WriteLine($"  {cell.Column}: '{cell.OriginalValue}' → '{cell.CleanedValue}'");
            }

            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════
        // MAIN: Ejecutar todos los ejemplos
        // ════════════════════════════════════════════════════════════

        public static void RunAllExamples()
        {
            Console.Clear();
            Console.WriteLine("\n");
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  EJEMPLOS DE USO: DataCleaner con Archivos de Prueba     ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            Example1_CleanClientes();
            Example2_CleanPersonNames();
            Example3_NormalizeDates();
            Example4_CleanNumbers();
            Example5_ProcessCompleteFile();
            Example6_BeforeAfterComparison();
            Example7_SelectiveClean();

            Console.WriteLine("═════════════════════════════════════════════════════════════");
            Console.WriteLine("✓ Todos los ejemplos completados");
            Console.WriteLine("═════════════════════════════════════════════════════════════\n");
        }
    }
}
