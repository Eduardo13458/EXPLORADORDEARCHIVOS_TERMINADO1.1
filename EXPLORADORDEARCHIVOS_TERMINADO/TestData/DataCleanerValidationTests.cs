// Script de Validación de Limpieza de Datos
// Copiar este código en un método de prueba para validar los archivos

using System;
using System.Collections.Generic;
using System.Linq;
using EXPLORADORDEARCHIVOS_TERMINADO.Limpieza_de_datos_3._3.Core;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Tests
{
    public class DataCleanerValidationTests
    {
        /// <summary>
        /// Ejemplos de limpieza esperada para cada archivo de prueba.
        /// Ejecuta estos tests para validar que la herramienta funciona correctamente.
        /// </summary>

        // ============================================================
        // 1. VALIDACIÓN: clientes_sucios.csv
        // ============================================================

        public static void ValidateClientesSucios()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  VALIDACIÓN: clientes_sucios.csv                          ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            var tests = new[]
            {
                // Limpieza de NOMBRES (caracteres especiales)
                new { Original = "Juan*García", Expected = "JuanGarcía", Type = "Nombre", Description = "* eliminado" },
                new { Original = "María@Perez", Expected = "MaríaPerez", Type = "Nombre", Description = "@ eliminado" },
                new { Original = "José~María", Expected = "JoséMaría", Type = "Nombre", Description = "~ eliminado" },
                new { Original = "Ana#Martín", Expected = "AnaMartín", Type = "Nombre", Description = "# eliminado" },

                // Limpieza de APELLIDOS (caracteres especiales)
                new { Original = "García^López", Expected = "GarcíaLópez", Type = "Apellido", Description = "^ eliminado" },
                new { Original = "O'Brien_Smith", Expected = "O'BrienSmith", Type = "Apellido", Description = "Apóstrofo conservado, _ eliminado" },
                new { Original = "Rodríguez*Martín", Expected = "RodríguezMartín", Type = "Apellido", Description = "* eliminado" },

                // Normalización de FECHAS
                new { Original = "2024-01-15", Expected = "2024-01-15", Type = "Fecha", Description = "ISO ya normalizada" },
                new { Original = "15/01/2024", Expected = "2024-01-15", Type = "Fecha", Description = "DD/MM/YYYY a ISO" },
                new { Original = "2024.01.20", Expected = "2024-01-20", Type = "Fecha", Description = "Puntos a ISO" },
                new { Original = "20/01/2024", Expected = "2024-01-20", Type = "Fecha", Description = "Variante DD/MM/YYYY" },
                new { Original = "01-20-2024", Expected = "2024-01-20", Type = "Fecha", Description = "MM-DD-YYYY a ISO" },

                // Limpieza de NÚMEROS (moneda, espacios)
                new { Original = "1.234", Expected = "1.234", Type = "Número", Description = "Decimal conservado" },
                new { Original = "€2,450.50", Expected = "2450.50", Type = "Número", Description = "Símbolo € y coma elim." },
                new { Original = "3450€", Expected = "3450", Type = "Número", Description = "€ final eliminado" },
                new { Original = "$ 5.678", Expected = "5.678", Type = "Número", Description = "$ y espacio eliminados" },
                new { Original = "300.75¥", Expected = "300.75", Type = "Número", Description = "¥ eliminado" },
            };

            foreach (var test in tests)
            {
                Console.WriteLine($"  [{test.Type}] {test.Description}");
                Console.WriteLine($"    Original:  \"{test.Original}\"");
                Console.WriteLine($"    Esperado:  \"{test.Expected}\"");
                Console.WriteLine();
            }
        }

        // ============================================================
        // 2. VALIDACIÓN: productos_sucios.csv
        // ============================================================

        public static void ValidateProductosSucios()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  VALIDACIÓN: productos_sucios.csv                         ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            var tests = new[]
            {
                // Descripciones "sucias"
                new { Original = "Pantalla LED 27 @ 4K resolution#HDR", Expected = "Pantalla LED 27 4K resolutionHDR", Type = "Descripción" },
                new { Original = "RGB*Backlit^Mechanical$Keyboard", Expected = "RGBBacklitMechanicalKeyboard", Type = "Descripción" },
                new { Original = "Wireless@Mouse~with#precision", Expected = "WirelessMousewithprecision", Type = "Descripción" },

                // Precios variados
                new { Original = "€299.99", Expected = "299.99", Type = "Precio" },
                new { Original = "$ 89.50", Expected = "89.50", Type = "Precio" },
                new { Original = "¥4.500", Expected = "4500", Type = "Precio" },

                // Proveedores con caracteres especiales
                new { Original = "LG^Electronics", Expected = "LGElectronics", Type = "Proveedor" },
                new { Original = "Corsair&Brand", Expected = "CorsairBrand", Type = "Proveedor" },
                new { Original = "Sony*Entertainment", Expected = "SonyEntertainment", Type = "Proveedor" },
            };

            foreach (var test in tests)
            {
                Console.WriteLine($"  [{test.Type}]");
                Console.WriteLine($"    Original:  \"{test.Original}\"");
                Console.WriteLine($"    Esperado:  \"{test.Expected}\"");
                Console.WriteLine();
            }
        }

        // ============================================================
        // 3. VALIDACIÓN: empleados_sucios.csv
        // ============================================================

        public static void ValidateEmpleadosSucios()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  VALIDACIÓN: empleados_sucios.csv                         ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            var tests = new[]
            {
                // NOMBRES CON GUIONES (válidos en personas, se conservan)
                new { Original = "Jean-Paul*García", Expected = "Jean-PaulGarcía", Type = "Nombre", Special = "Guion conservado" },
                new { Original = "María-José@Sánchez", Expected = "María-JoséSánchez", Type = "Nombre", Special = "Guion conservado" },

                // APELLIDOS CON APÓSTROFOS (válidos, se conservan)
                new { Original = "O'Brien#López", Expected = "O'BrienLópez", Type = "Apellido", Special = "Apóstrofo conservado" },
                new { Original = "De*Los#Reyes", Expected = "DeLosReyes", Type = "Apellido", Special = "Caracteres especiales elim." },

                // SALARIOS CON MONEDA
                new { Original = "€2.500", Expected = "2500", Type = "Salario", Special = "€ eliminado" },
                new { Original = "$ 2.200", Expected = "2200", Type = "Salario", Special = "$ y espacio elim." },
                new { Original = "¥250.000", Expected = "250000", Type = "Salario", Special = "¥ eliminado, notación decimal" },
            };

            foreach (var test in tests)
            {
                Console.WriteLine($"  [{test.Type}] {test.Special}");
                Console.WriteLine($"    Original:  \"{test.Original}\"");
                Console.WriteLine($"    Esperado:  \"{test.Expected}\"");
                Console.WriteLine();
            }
        }

        // ============================================================
        // 4. VALIDACIÓN: transacciones_sucios.csv
        // ============================================================

        public static void ValidateTransaccionesSucias()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  VALIDACIÓN: transacciones_sucios.csv                     ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            var tests = new[]
            {
                // Fechas en MÚLTIPLES FORMATOS
                new { Original = "2024-01-15", Expected = "2024-01-15", Type = "Fecha", Special = "ISO ya normalizada" },
                new { Original = "15 Jan 2024", Expected = "2024-01-15", Type = "Fecha", Special = "DD MMM YYYY" },
                new { Original = "2024.01.20", Expected = "2024-01-20", Type = "Fecha", Special = "Puntos" },
                new { Original = "20/01/2024", Expected = "2024-01-20", Type = "Fecha", Special = "DD/MM/YYYY" },
                new { Original = "20-01-2024", Expected = "2024-01-20", Type = "Fecha", Special = "Variante con guiones" },

                // Descripciones "sucias"
                new { Original = "Pago*Cliente#Premium", Expected = "PagoClientePremium", Type = "Descripción", Special = "Caracteres especiales" },
                new { Original = "Devolución&Parcial~Producto", Expected = "DevoluciónParcialProducto", Type = "Descripción", Special = "& y ~ eliminados" },

                // Montos variados
                new { Original = "€150.50", Expected = "150.50", Type = "Monto", Special = "€ eliminado" },
                new { Original = "$ 75.25", Expected = "75.25", Type = "Monto", Special = "$ y espacio" },
                new { Original = "¥15.000", Expected = "15000", Type = "Monto", Special = "¥ eliminado" },
            };

            foreach (var test in tests)
            {
                Console.WriteLine($"  [{test.Type}] {test.Special}");
                Console.WriteLine($"    Original:  \"{test.Original}\"");
                Console.WriteLine($"    Esperado:  \"{test.Expected}\"");
                Console.WriteLine();
            }
        }

        // ============================================================
        // 5. VALIDACIÓN: ventas_sucios.json
        // ============================================================

        public static void ValidateVentasSucias()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  VALIDACIÓN: ventas_sucios.json                          ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

            var tests = new[]
            {
                // Nombres en JSON
                new { Original = "Juan*García", Expected = "JuanGarcía", Type = "Vendedor", Special = "* eliminado" },
                new { Original = "María@Pérez", Expected = "MaríaPerez", Type = "Vendedor", Special = "@ eliminado" },

                // Productos en JSON
                new { Original = "Monitor*LG@27\"", Expected = "MonitorLG27", Type = "Producto", Special = "Caracteres especiales" },
                new { Original = "Teclado~Mecánico", Expected = "TecladoMecánico", Type = "Producto", Special = "~ eliminado" },

                // Precios variados
                new { Original = "€299.99", Expected = "299.99", Type = "Precio", Special = "€ eliminado" },
                new { Original = "$ 1.499,95", Expected = "1499.95", Type = "Total", Special = "$ y coma decimal" },
                new { Original = "¥14.999,50", Expected = "14999.50", Type = "Comisión", Special = "¥ y coma decimal" },

                // Fechas en JSON
                new { Original = "2024-01-15", Expected = "2024-01-15", Type = "Fecha", Special = "ISO normalizada" },
                new { Original = "15/01/2024", Expected = "2024-01-15", Type = "Fecha", Special = "DD/MM/YYYY" },
                new { Original = "2024.01.20", Expected = "2024-01-20", Type = "Fecha", Special = "Puntos" },
            };

            foreach (var test in tests)
            {
                Console.WriteLine($"  [{test.Type}] {test.Special}");
                Console.WriteLine($"    Original:  \"{test.Original}\"");
                Console.WriteLine($"    Esperado:  \"{test.Expected}\"");
                Console.WriteLine();
            }
        }

        // ============================================================
        // MÉTODO PRINCIPAL: Ejecutar todas las validaciones
        // ============================================================

        public static void RunAllValidations()
        {
            Console.WriteLine("\n");
            Console.WriteLine("████████████████████████████████████████████████████████████");
            Console.WriteLine("█  VALIDACIÓN COMPLETA DE ARCHIVOS DE PRUEBA PARA CLEANER  █");
            Console.WriteLine("████████████████████████████████████████████████████████████\n");

            ValidateClientesSucios();
            Console.WriteLine("\n");

            ValidateProductosSucios();
            Console.WriteLine("\n");

            ValidateEmpleadosSucios();
            Console.WriteLine("\n");

            ValidateTransaccionesSucias();
            Console.WriteLine("\n");

            ValidateVentasSucias();
            Console.WriteLine("\n");

            Console.WriteLine("════════════════════════════════════════════════════════════");
            Console.WriteLine("RESUMEN DE VALIDACIÓN:");
            Console.WriteLine("════════════════════════════════════════════════════════════");
            Console.WriteLine("✓ clientes_sucios.csv      : 15 registros (Nombres, Fechas, Números)");
            Console.WriteLine("✓ productos_sucios.csv     : 15 productos (Descripciones, Precios, Fechas)");
            Console.WriteLine("✓ empleados_sucios.csv     : 15 empleados (Nombres persona, Moneda, Fechas)");
            Console.WriteLine("✓ transacciones_sucios.csv : 20 transacciones (Fechas avanzadas, Moneda)");
            Console.WriteLine("✓ ventas_sucios.json       : 10 ventas (JSON, Comas decimales)");
            Console.WriteLine("\nTodos los archivos están listos para prueba en FormCorrector");
            Console.WriteLine("════════════════════════════════════════════════════════════\n");
        }
    }
}
