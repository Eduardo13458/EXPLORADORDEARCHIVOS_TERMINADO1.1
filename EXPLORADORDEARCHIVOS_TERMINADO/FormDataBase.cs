using DocumentFormat.OpenXml.Packaging;
using OxlSheet = DocumentFormat.OpenXml.Spreadsheet;
using EXPLORADORDEARCHIVOS_TERMINADO.Data;
using EXPLORADORDEARCHIVOS_TERMINADO.Models;
using EXPLORADORDEARCHIVOS_TERMINADO.Processing;
using EXPLORADORDEARCHIVOS_TERMINADO.Visualization;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    /// <summary>
    /// Ventana principal de Data Fusion Arena.
    /// Orquesta la carga, procesamiento y visualizacion de 5 fuentes de datos.
    /// </summary>
    public partial class FormDataBase : Form
    {
        // Repositorio y servicio para separar responsabilidades (inyectados)
        private readonly Data.DataRepository _repository;
        private readonly Services.DataService _dataService;

        public FormDataBase(Data.DataRepository repository, Services.DataService dataService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));

            InitializeComponent();
        }

        // ════════════════════════════════════════════════════════════════════
        //  BOTONES: Importar desde Bases de Datos
        // ════════════════════════════════════════════════════════════════════

        private void btnImportSqlServer_Click(object? sender, EventArgs e)
            => ImportFromDb("SQL Server", "1433",
                   DatabaseExporter.GetTablesSqlServer,
                   (cs, t) => DataReader.ReadFromSqlServer(cs, t));

        private void btnImportMariaDb_Click(object? sender, EventArgs e)
            => ImportFromDb("MariaDB", "3306",
                   DatabaseExporter.GetTablesMariaDb,      // ✅ YA IMPLEMENTADO
                   (cs, t) => DataReader.ReadFromMariaDb(cs, t));

        private void btnImportPostgre_Click(object? sender, EventArgs e)
            => ImportFromDb("PostgreSQL", "5432",
                   DatabaseExporter.GetTablesPostgreSql,   // ✅ YA IMPLEMENTADO
                   (cs, t) => DataReader.ReadFromPostgreSql(cs, t));

        private void ImportFromDb(string dbName, string defaultPort,
            Func<string, List<string>> getTablesFunc,
            Func<string, string, List<DataItem>> readFunc)
        {
            var result = DatabaseExporter.ShowImportDialog(dbName, defaultPort, getTablesFunc);
            if (result == null) return;

            var (connStr, tableName) = result.Value;

            try
            {
                Cursor = Cursors.WaitCursor;
                var dbItems = readFunc(connStr, tableName);

                if (dbItems.Count == 0)
                {
                    lblStatus.Text = $"No se obtuvieron registros de {dbName} ({tableName}).";
                    Cursor = Cursors.Default;
                    return;
                }

                _repository.AddItems(dbItems);
                _repository.UpdateLastImported(dbItems);
                var all = _repository.GetAll();
                var idIndex = _dataService.BuildIdIndex(all);

                FillDataGridView();

                lblStatus.Text = $"Importados {dbItems.Count} registros desde {dbName} [{tableName}]  " +
                                 $"(Total: {all.Count})";
                tabControl.SelectedTab = tabData;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al importar desde {dbName}:\n\n{ex.Message}",
                    "Error de conexion", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  BOTON: Procesar (sort, filtros, duplicados)
        // ════════════════════════════════════════════════════════════════════

        private void btnProcess_Click(object? sender, EventArgs e)
        {
            var allItems = _repository.GetAll();
            var allItems2 = allItems is System.Collections.Generic.List<DataItem> l ? l : allItems.ToList();
            if (allItems2.Count == 0)
            {
                MessageBox.Show(
                    "No hay datos cargados.\n\n" +
                    "Importe archivos (CSV, JSON, XML, TXT) o use el boton BD.",
                    "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var sb = new StringBuilder();
            var (stringFields, numericFields) = _dataService.DiscoverFields(allItems);

            sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║              PROCESAMIENTO DINAMICO DE DATOS                ║");
            sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");
            sb.AppendLine();
            sb.AppendLine($"  Campos de texto:    {string.Join(", ", stringFields)}");
            sb.AppendLine($"  Campos numericos:   {string.Join(", ", numericFields)}");
            sb.AppendLine($"  Total registros:    {allItems.Count}");
            sb.AppendLine();

            // ── 1. Ordenar por primer campo numerico (Insertion Sort) ─────
            if (numericFields.Count > 0)
            {
                string sortField = numericFields[0];
                var sorted = new List<DataItem>(allItems);
                _dataService.DynamicSort(sorted, sortField);

                sb.AppendLine($"=== ORDENADOS POR {sortField.ToUpper()} ASC (Insertion Sort) " +
                              new string('=', 20));
                int show = Math.Min(sorted.Count, Utilities.UiConstants.MAX_ROWS_DISPLAYED_IN_PREVIEW);
                for (int i = 0; i < show; i++)
                    sb.AppendLine($"  {_dataService.GetNumericValue(sorted[i], sortField),10:F2}  " +
                                  sorted[i].Label);
                if (sorted.Count > show)
                    sb.AppendLine($"  ... y {sorted.Count - show} registros mas");
                sb.AppendLine();
            }

            // ── 2. Filtro dinamico: campo numerico > percentil 75 ────────
            if (numericFields.Count > 0)
            {
                string filterField = numericFields[0];
                double threshold = _dataService.ComputeThreshold(
                    allItems, filterField, 0.75);
                var filtered = _dataService.DynamicFilter(
                    allItems, filterField, threshold);

                sb.AppendLine($"=== FILTRO: {filterField.ToUpper()} >= {threshold:F2} " +
                              $"({filtered.Count} resultados) " + new string('=', 15));
                int show = Math.Min(filtered.Count, Utilities.UiConstants.MAX_ROWS_DISPLAYED_IN_PREVIEW);
                for (int i = 0; i < show; i++)
                    sb.AppendLine($"  {DataProcessor.GetNumericValue(filtered[i], filterField),10:F2}  " +
                                  filtered[i].Label);
                if (filtered.Count > show)
                    sb.AppendLine($"  ... y {filtered.Count - show} registros mas");
                sb.AppendLine();
            }

            // ── 3. Segundo campo numerico desc (Bubble Sort) ─────────────
            {
                string sortField2 = numericFields.Count > 1
                    ? numericFields[1]
                    : numericFields[0];
                var sorted2 = new List<DataItem>(allItems);
                _dataService.DynamicBubbleSort(sorted2, sortField2, ascending: false);

                sb.AppendLine($"=== ORDENADOS POR {sortField2.ToUpper()} DESC (Bubble Sort) " +
                              new string('=', 20));
                int show = Math.Min(sorted2.Count, Utilities.UiConstants.MAX_ROWS_DISPLAYED_IN_PREVIEW);
                for (int i = 0; i < show; i++)
                    sb.AppendLine($"  {DataProcessor.GetNumericValue(sorted2[i], sortField2),10:F2}  " +
                                  sorted2[i].Label);
                if (sorted2.Count > show)
                    sb.AppendLine($"  ... y {sorted2.Count - show} registros mas");
                sb.AppendLine();
            }

            // ── 4. Agrupacion dinamica con Dictionary ────────────────────
            if (stringFields.Count > 0 && numericFields.Count > 0)
            {
                string catField = stringFields[0];
                string valField = numericFields[0];
                bool useAvg = DataProcessor.IsAveragingField(valField);

                if (useAvg)
                {
                var grouped = DataProcessor.DynamicGroupAvg(
                        allItems2, catField, valField);
                    sb.AppendLine($"=== PROMEDIO DE {valField.ToUpper()} POR " +
                                  $"{catField.ToUpper()} " + new string('=', 10));
                    foreach (var kv in grouped)
                        sb.AppendLine($"  {kv.Key,-20} {kv.Value,10:F2}");
                }
                else
                {
                    var summed = DataProcessor.DynamicGroupSum(
                        allItems2, catField, valField);
                    sb.AppendLine($"=== TOTAL DE {valField.ToUpper()} POR " +
                                  $"{catField.ToUpper()} " + new string('=', 10));
                    foreach (var kv in summed)
                        sb.AppendLine($"  {kv.Key,-20} {kv.Value,10:F2}");
                }
                sb.AppendLine();

                var counts = DataProcessor.DynamicGroupCount(allItems2, catField);
                sb.AppendLine($"=== CONTEO POR {catField.ToUpper()} " +
                              new string('=', 20));
                foreach (var kv in counts)
                    sb.AppendLine($"  {kv.Key,-20} {kv.Value,5} registros");
                sb.AppendLine();
            }

            // ── 5. Duplicados (HashSet) ──────────────────────────────────
            var dupes = DataProcessor.DetectDuplicates(allItems2);
            sb.AppendLine($"=== DUPLICADOS DETECTADOS: {dupes.Count} " +
                          new string('=', 30));
            if (dupes.Count == 0) sb.AppendLine("  (ninguno)");
            else
            {
                int show = Math.Min(dupes.Count, 20);
                for (int i = 0; i < show; i++)
                    sb.AppendLine($"  {dupes[i]}");
                if (dupes.Count > show)
                    sb.AppendLine($"  ... y {dupes.Count - show} mas");
            }
            sb.AppendLine();

            // ── 6. Busqueda rapida por ID (Dictionary) ───────────────────
            sb.AppendLine("=== BUSQUEDA RAPIDA POR ID (Dictionary) " +
                          new string('=', 25));
            var sampleIds = new List<int>();
            int step = allItems2.Count > 5 ? allItems2.Count / 5 : 1;
            for (int i = 0; i < allItems2.Count && sampleIds.Count < 5; i += step)
                sampleIds.Add(allItems2[i].Id);
            foreach (int id in sampleIds)
            {
                // Try to build id index on demand
                var idIndex2 = _dataService.BuildIdIndex(allItems);
                if (idIndex2.TryGetValue(id, out var found))
                    sb.AppendLine($"  ID {id,5} -> {found}");
                else
                    sb.AppendLine($"  ID {id,5} -> (no encontrado)");
            }
            sb.AppendLine();

            rtbConsole.Text = sb.ToString();
            tabControl.SelectedTab = tabConsole;
            lblStatus.Text = "Procesamiento dinamico completado.";
        }

        // ════════════════════════════════════════════════════════════════════
        //  BOTON: Ver Consola (tablas + graficas ASCII)
        // ════════════════════════════════════════════════════════════════════

        private void btnConsole_Click(object? sender, EventArgs e)
        {
            var all = _repository.GetAll();
            if (all.Count == 0)
            {
                MessageBox.Show(
                    "No hay datos cargados.\n\n" +
                    "Importe archivos (CSV, JSON, XML, TXT) o use el boton BD.",
                    "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            rtbConsole.Text = ConsoleVisualizer.RenderDynamicTable(all);
            tabControl.SelectedTab = tabConsole;
            lblStatus.Text = $"Consola: {all.Count} registros mostrados.";
        }

        // ════════════════════════════════════════════════════════════════════
        //  IMPORTACION DE ARCHIVOS EXTERNOS
        // ════════════════════════════════════════════════════════════════════

        private void btnImportCsv_Click(object? sender, EventArgs e) =>
            ImportFile("Archivos CSV|*.csv");

        private void btnImportJson_Click(object? sender, EventArgs e) =>
            ImportFile("Archivos JSON|*.json");

        private void btnImportXml_Click(object? sender, EventArgs e) =>
            ImportFile("Archivos XML|*.xml");

        private void btnImportTxt_Click(object? sender, EventArgs e) =>
            ImportFile("Archivos de texto|*.txt");

        private void btnImportXlsx_Click(object? sender, EventArgs e) =>
            ImportFile("Archivos Excel|*.xlsx");

        private void btnClearData_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Limpiar todos los datos cargados?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _repository.Clear();
            FillDataGridView();
            chartAutoBar.Series.Clear();
            chartAutoPie.Series.Clear();
            chartAutoDoughnut.Series.Clear();
            chartAutoLine.Series.Clear();
            rtbConsole.Clear();
            lblStatus.Text = "Datos limpiados.";
        }

        // ════════════════════════════════════════════════════════════════════
        //  EXPORTACION A BASES DE DATOS
        // ════════════════════════════════════════════════════════════════════

        private void btnExportSql_Click(object? sender, EventArgs e)
        {
            if (!HasData()) return;

            var result = DatabaseExporter.ShowExportDialog("SQL Server", "1433");
            if (result == null) return;

            var (connStr, tableName) = result.Value;
            ExportToDb("SQL Server", () => DatabaseExporter.ExportToSqlServer(_repository.GetAll(), connStr, tableName));
        }

        private void btnExportMariaDb_Click(object? sender, EventArgs e)
        {
            if (!HasData()) return;

            var result = DatabaseExporter.ShowExportDialog("MariaDB", "3306");
            if (result == null) return;

            var (connStr, tableName) = result.Value;
            ExportToDb("MariaDB", () => DatabaseExporter.ExportToMariaDb(_repository.GetAll(), connStr, tableName));
        }

        private void btnExportPostgre_Click(object? sender, EventArgs e)
        {
            if (!HasData()) return;

            var result = DatabaseExporter.ShowExportDialog("PostgreSQL", "5432");
            if (result == null) return;

            var (connStr, tableName) = result.Value;
            ExportToDb("PostgreSQL", () => DatabaseExporter.ExportToPostgreSql(_repository.GetAll(), connStr, tableName));
        }

        private bool HasData()
        {
            if (_repository.GetAll().Count == 0)
            {
                MessageBox.Show(
                    "No hay datos cargados.\n\n" +
                    "Primero importe archivos o use el boton BD.",
                    "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        private void ExportToDb(string dbName, Func<int> exportAction)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                int count = exportAction();
                Cursor = Cursors.Default;

                lblStatus.Text = $"Exportados {count} registros a {dbName}.";
                MessageBox.Show(
                    $"Se exportaron {count} registros\n" +
                    $"a la tabla 'DatosIntegrados' en {dbName}.",
                    "Exportacion exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show(
                    $"Error al exportar a {dbName}:\n\n{ex.Message}",
                    "Error de conexion", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  BOTON: Generar Grafica (universal, cualquier archivo)
        // ════════════════════════════════════════════════════════════════════

        private void btnGenerateChart_Click(object? sender, EventArgs e)
        {
            if (_repository.GetAll().Count == 0)
            {
                MessageBox.Show(
                    "No hay datos cargados.\n\n" +
                    "Primero importe un archivo (CSV, JSON, XML o TXT)\n" +
                    "usando los botones de la barra superior.",
                    "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cursor = Cursors.WaitCursor;

            var itemsForChart = _repository.GetLastImported().Count > 0
                ? _repository.GetLastImported() : _repository.GetAll();

            // ── Detectar hasta 4 pares categorico+numerico de calidad ──────
            var smartPairs = DataProcessor.AutoDetectSmartPairs(itemsForChart.ToList(), 4);
            var lineSeries = DataProcessor.AutoDetectLineSeries(itemsForChart.ToList());

            if (smartPairs.Count == 0 && lineSeries.Count == 0)
            {
                Cursor = Cursors.Default;
                MessageBox.Show(
                    "No se encontraron campos categóricos + numéricos\n" +
                    "suficientes para generar gráficas automáticas.",
                    "Datos insuficientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            tlpAutoCharts.SuspendLayout();

            // Pares asignados a cada grafica (fallback al primero disponible)
            var pairBar = smartPairs.Count > 0 ? smartPairs[0] : null;
            var pairPie = smartPairs.Count > 1 ? smartPairs[1] : pairBar;
            var pairDnut = smartPairs.Count > 2 ? smartPairs[2] : pairPie;
            var pairLine = smartPairs.Count > 3 ? smartPairs[3] : pairBar;

            // ── 1. Barras (mas categorias, mejor para comparar) ───────────
            if (pairBar != null)
                FillAutoBarChart(pairBar.GroupedData,
                    pairBar.CategoryField, pairBar.ValueField, pairBar.AggregationLabel);
            else
                ResetChart(chartAutoBar);

            // ── 2. Pastel (pocas categorias, proporciones) ────────────────
            if (pairPie != null)
                FillAutoPieChart(pairPie.GroupedData,
                    pairPie.CategoryField, pairPie.ValueField, pairPie.AggregationLabel);
            else
                ResetChart(chartAutoPie);

            // ── 3. Anillo (variante del pastel, diferente campo) ──────────
            if (pairDnut != null)
                FillAutoDoughnutChart(pairDnut.GroupedData,
                    pairDnut.CategoryField, pairDnut.ValueField, pairDnut.AggregationLabel);
            else
                ResetChart(chartAutoDoughnut);

            // ── 4. Lineas (series numericas o ranking) ────────────────────
            if (lineSeries.Count > 0)
            {
                FillAutoLineChart(lineSeries);
            }
            else if (pairLine != null)
            {
                // Si no hay series temporales, grafica el ranking del par como linea
                var rankSeries = new Dictionary<string, List<double>>();
                var sortedVals = DataProcessor.LimitTopN(pairLine.GroupedData, MaxLinePoints);
                var vals = new List<double>();
                foreach (var kv in sortedVals)
                    vals.Add(kv.Value);
                rankSeries[$"{pairLine.AggregationLabel} {pairLine.ValueField} por {pairLine.CategoryField}"] = vals;
                FillAutoLineChart(rankSeries);
            }
            else
                ResetChart(chartAutoLine);

            tlpAutoCharts.ResumeLayout(true);

            tabControl.SelectedTab = tabAutoChart;

            string pairDesc = pairBar != null
                ? $"{pairBar.CategoryField} → {pairBar.ValueField} ({pairBar.UniqueCats} cat)"
                : "sin par";
            lblStatus.Text = $"Graficas generadas — Barras: {pairDesc} | " +
                             $"Pastel: {pairPie?.CategoryField ?? "-"} | " +
                             $"Anillo: {pairDnut?.CategoryField ?? "-"} | " +
                             $"Series: {lineSeries.Count}";
            Cursor = Cursors.Default;
        }

        // ════════════════════════════════════════════════════════════════════
        //  BOTON: Grafica en Consola (ASCII)
        // ════════════════════════════════════════════════════════════════════

        private void btnChartConsole_Click(object? sender, EventArgs e)
        {
            if (_repository.GetAll().Count == 0)
            {
                MessageBox.Show(
                    "No hay datos cargados.\n\n" +
                    "Primero importe un archivo (CSV, JSON, XML o TXT)\n" +
                    "usando los botones de la barra superior.",
                    "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cursor = Cursors.WaitCursor;

            var itemsForChart = _repository.GetLastImported().Count > 0
                ? _repository.GetLastImported() : _repository.GetAll();

            var smartPairs = DataProcessor.AutoDetectSmartPairs(itemsForChart.ToList(), 4);
            var lineSeries = DataProcessor.AutoDetectLineSeries(itemsForChart.ToList());

            if (smartPairs.Count == 0 && lineSeries.Count == 0)
            {
                Cursor = Cursors.Default;
                MessageBox.Show(
                    "No se encontraron campos categóricos + numéricos\n" +
                    "suficientes para generar gráficas en consola.",
                    "Datos insuficientes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Usar el mejor par para consola
            var bestPair = smartPairs.Count > 0 ? smartPairs[0] : null;
            string catLabel = bestPair?.CategoryField ?? "";
            string valLabel = bestPair?.ValueField ?? "";
            var groupedData = bestPair?.GroupedData
                ?? new Dictionary<string, double>();

            var sb = new StringBuilder();
            sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║           GRAFICAS EN CONSOLA  (ASCII / Unicode)            ║");
            sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");
            sb.AppendLine();

            if (groupedData.Count > 0)
            {
                // Barras
                sb.Append(ConsoleVisualizer.RenderBarChart(
                    groupedData, $"{valLabel} por {catLabel} (Barras)"));

                // Pastel / porcentajes
                sb.Append(ConsoleVisualizer.RenderPieAscii(
                    groupedData, $"{valLabel} por {catLabel} (Pastel)"));
            }

            if (lineSeries.Count > 0)
            {
                // Sparklines (compacto)
                sb.Append(ConsoleVisualizer.RenderSparkLines(
                    lineSeries, "Series Numericas (Sparklines)"));

                // Grafica vertical detallada
                sb.Append(ConsoleVisualizer.RenderVerticalLineChart(
                    lineSeries, "Series Numericas (Lineas)"));
            }

            rtbConsole.Text = sb.ToString();
            tabControl.SelectedTab = tabConsole;
            lblStatus.Text = $"Graficas consola: {catLabel} → {valLabel} " +
                             $"({groupedData.Count} categorias, {lineSeries.Count} series)";
            Cursor = Cursors.Default;
        }

        // ════════════════════════════════════════════════════════════════════
        //  GRAFICAS AUTO-GENERADAS (tab "Grafica Generada")
        // ════════════════════════════════════════════════════════════════════

        private const int MaxBarCategories = 15;
        private const int MaxPieSlices = 10;
        private const int MaxLinePoints = 50;

        private static void ResetChart(Chart chart)
        {
            chart.Series.Clear();
            chart.Titles.Clear();
            chart.Legends.Clear();
            chart.ChartAreas.Clear();
            chart.ChartAreas.Add(new ChartArea("Default"));
        }

        private void FillAutoBarChart(Dictionary<string, double> data,
            string catLabel, string valLabel, string aggLabel = "Total")
        {
            ResetChart(chartAutoBar);

            if (data.Count == 0) return;

            var limited = DataProcessor.LimitTopN(data, MaxBarCategories);

            var area = chartAutoBar.ChartAreas[0];
            area.AxisX.Title = catLabel;
            area.AxisY.Title = $"{aggLabel} {valLabel}";
            area.AxisX.LabelStyle.Angle = -45;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 7F);
            area.AxisX.LabelStyle.TruncatedLabels = true;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(220, 220, 220);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 7F);

            string titleSuffix = data.Count > MaxBarCategories
                ? $" (Top {MaxBarCategories} de {data.Count})" : "";
            chartAutoBar.Titles.Add($"{aggLabel} de {valLabel} por {catLabel}{titleSuffix}");
            chartAutoBar.Titles[0].Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            var series = chartAutoBar.Series.Add($"{aggLabel} {valLabel}");
            series.ChartType = SeriesChartType.Column;
            series.IsValueShownAsLabel = limited.Count <= 10;
            series.LabelFormat = "{0:F1}";
            series.Font = new Font("Segoe UI", 6.5F);

            int idx = 0;
            foreach (var kv in limited)
            {
                string label = kv.Key.Length > 18
                    ? kv.Key.Substring(0, 15) + "..." : kv.Key;
                int pt = series.Points.AddXY(label, kv.Value);
                series.Points[pt].Color = GetColor(idx++);
            }

            chartAutoBar.Invalidate();
        }

        private void FillAutoPieChart(Dictionary<string, double> data,
            string catLabel, string valLabel, string aggLabel = "Total")
        {
            ResetChart(chartAutoPie);

            if (data.Count == 0) return;

            var limited = DataProcessor.LimitTopN(data, MaxPieSlices);

            chartAutoPie.Titles.Add($"{aggLabel} de {valLabel} por {catLabel} (Pastel)");
            chartAutoPie.Titles[0].Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            var series = chartAutoPie.Series.Add($"{aggLabel} {valLabel}");
            series.ChartType = SeriesChartType.Pie;
            series["PieLabelStyle"] = limited.Count <= 6 ? "Outside" : "Disabled";
            series.IsValueShownAsLabel = limited.Count <= 6;
            series.LabelFormat = "{0:F1}";
            series.Font = new Font("Segoe UI", 7F);

            int idx = 0;
            foreach (var kv in limited)
            {
                string label = kv.Key.Length > 20
                    ? kv.Key.Substring(0, 17) + "..." : kv.Key;
                int pt = series.Points.AddXY(label, kv.Value);
                series.Points[pt].Color = GetColor(idx++);
                series.Points[pt].LegendText = $"{label}: {kv.Value:F1}";
            }

            var legend = new Legend { Docking = Docking.Right };
            legend.Font = new Font("Segoe UI", 7F);
            chartAutoPie.Legends.Add(legend);

            chartAutoPie.Invalidate();
        }

        private void FillAutoDoughnutChart(Dictionary<string, double> data,
            string catLabel, string valLabel, string aggLabel = "Total")
        {
            ResetChart(chartAutoDoughnut);

            if (data.Count == 0) return;

            var limited = DataProcessor.LimitTopN(data, MaxPieSlices);

            chartAutoDoughnut.Titles.Add($"{aggLabel} de {valLabel} por {catLabel} (Anillo)");
            chartAutoDoughnut.Titles[0].Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            var series = chartAutoDoughnut.Series.Add($"{aggLabel} {valLabel}");
            series.ChartType = SeriesChartType.Doughnut;
            series["DoughnutRadius"] = "40";
            series.IsValueShownAsLabel = limited.Count <= 6;
            series.LabelFormat = "{0:F1}";
            series.Font = new Font("Segoe UI", 7F);

            int idx = 0;
            foreach (var kv in limited)
            {
                string label = kv.Key.Length > 20
                    ? kv.Key.Substring(0, 17) + "..." : kv.Key;
                int pt = series.Points.AddXY(label, kv.Value);
                series.Points[pt].Color = GetColor(idx++);
                series.Points[pt].LegendText = $"{label}: {kv.Value:F1}";
            }

            var legend = new Legend { Docking = Docking.Right };
            legend.Font = new Font("Segoe UI", 7F);
            chartAutoDoughnut.Legends.Add(legend);

            chartAutoDoughnut.Invalidate();
        }

        private void FillAutoLineChart(Dictionary<string, List<double>> seriesData)
        {
            ResetChart(chartAutoLine);

            if (seriesData.Count == 0) return;

            var area = chartAutoLine.ChartAreas[0];
            area.AxisX.Title = "Indice";
            area.AxisY.Title = "Valor";
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 7F);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 7F);
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(230, 230, 230);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(220, 220, 220);

            chartAutoLine.Titles.Add("Series Numericas (Lineas)");
            chartAutoLine.Titles[0].Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            MarkerStyle[] markers =
                [MarkerStyle.Circle, MarkerStyle.Diamond, MarkerStyle.Triangle,
                 MarkerStyle.Square, MarkerStyle.Cross, MarkerStyle.Star4];

            int sIdx = 0;
            foreach (var kv in seriesData)
            {
                if (sIdx >= 4) break; // max 4 series en espacio reducido

                var points = DataProcessor.SampleSeries(kv.Value, MaxLinePoints);

                var s = chartAutoLine.Series.Add(kv.Key);
                s.ChartType = SeriesChartType.Line;
                s.BorderWidth = 2;
                s.Color = GetColor(sIdx);
                s.MarkerStyle = points.Count <= 25
                    ? markers[sIdx % markers.Length] : MarkerStyle.None;
                s.MarkerSize = 5;

                for (int i = 0; i < points.Count; i++)
                    s.Points.AddXY(i + 1, points[i]);

                sIdx++;
            }

            // Ajustar intervalo del eje X segun cantidad de puntos
            int maxPts = 0;
            foreach (var kv in seriesData)
            {
                int cnt = kv.Value.Count > MaxLinePoints ? MaxLinePoints : kv.Value.Count;
                if (cnt > maxPts) maxPts = cnt;
            }
            area.AxisX.Interval = maxPts <= 20 ? 1 : maxPts <= 50 ? 5 : 10;

            var legend = new Legend { Docking = Docking.Top };
            legend.Font = new Font("Segoe UI", 7.5F);
            chartAutoLine.Legends.Add(legend);

            chartAutoLine.Invalidate();
        }


        /// Abre el dialogo de archivo, llama a DataReader.LoadFromFile() con
        /// la ruta absoluta elegida y refresca toda la UI.

        private void ImportFile(string filter)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Seleccionar archivo de datos",
                Filter = filter + "|Todos los archivos|*.*",
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                var newItems = DataReader.LoadFromFile(dlg.FileName);

                if (newItems.Count == 0)
                {
                    lblStatus.Text = "El archivo no contiene registros validos.";
                    return;
                }

                _repository.AddItems(newItems);
                _repository.UpdateLastImported(newItems);
                var all = _repository.GetAll();
                var idIndex = _dataService.BuildIdIndex(all);

                // Auto-select filter to show imported source
                var importedSource = newItems[0].Source.ToString();
                int filterIdx = cmbSourceFilter.Items.IndexOf(importedSource);
                if (filterIdx >= 0 && cmbSourceFilter.SelectedIndex != filterIdx)
                    cmbSourceFilter.SelectedIndex = filterIdx; // triggers FillDataGridView
                else
                    FillDataGridView();

                var total = _repository.GetAll().Count;
                lblStatus.Text =
                    $"Importados {newItems.Count} registros desde " +
                    $"'{Path.GetFileName(dlg.FileName)}'  " +
                    $"(Total: {total})";

                tabControl.SelectedTab = tabData;
            }
            catch (NotSupportedException ex)
            {
                MessageBox.Show(ex.Message, "Formato no soportado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al importar:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  DataGridView
        // ════════════════════════════════════════════════════════════════════

        private void FillDataGridView()
        {
            dgvData.DataSource = null;

            var selectedSource = cmbSourceFilter.SelectedItem?.ToString() ?? "Todas";

            // Collect items to display
            var allItems = _repository.GetAll();
            var displayItems = new List<DataItem>();
            foreach (var item in allItems)
            {
                if (selectedSource != "Todas" && item.Source.ToString() != selectedSource)
                    continue;
                displayItems.Add(item);
            }

            // Discover extra column names (preserving first-seen order)
            var extraColumns = new List<string>();
            var extraSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in displayItems)
                foreach (var key in item.ExtraFields.Keys)
                    if (extraSet.Add(key))
                        extraColumns.Add(key);

            // Base columns (known DataItem fields)
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Fuente", typeof(string));
            table.Columns.Add("Etiqueta", typeof(string));
            table.Columns.Add("Company", typeof(string));
            table.Columns.Add("TypeName", typeof(string));
            table.Columns.Add("Price", typeof(double));
            table.Columns.Add("Title", typeof(string));
            table.Columns.Add("Genre", typeof(string));
            table.Columns.Add("Sales", typeof(double));
            table.Columns.Add("Tipo", typeof(string));
            table.Columns.Add("Modelo", typeof(string));
            table.Columns.Add("Stock", typeof(int));
            table.Columns.Add("Minuto", typeof(int));
            table.Columns.Add("Temperatura", typeof(double));
            table.Columns.Add("FPS", typeof(double));
            table.Columns.Add("UserName", typeof(string));
            table.Columns.Add("Region", typeof(string));

            // Dynamic columns from ExtraFields
            foreach (var colName in extraColumns)
                if (!table.Columns.Contains(colName))
                    table.Columns.Add(colName, typeof(string));

            // Fill rows
            foreach (var item in displayItems)
            {
                var row = table.NewRow();
                row["ID"] = item.Id;
                row["Fuente"] = item.Source.ToString();
                row["Etiqueta"] = item.Label;
                row["Company"] = item.Company;
                row["TypeName"] = item.TypeName;
                row["Price"] = item.Price;
                row["Title"] = item.Title;
                row["Genre"] = item.Genre;
                row["Sales"] = item.Sales;
                row["Tipo"] = item.Tipo;
                row["Modelo"] = item.Modelo;
                row["Stock"] = item.Stock;
                row["Minuto"] = item.Minuto;
                row["Temperatura"] = item.Temperatura;
                row["FPS"] = item.FPS;
                row["UserName"] = item.UserName;
                row["Region"] = item.Region;

                foreach (var kv in item.ExtraFields)
                    if (table.Columns.Contains(kv.Key))
                        row[kv.Key] = kv.Value;

                table.Rows.Add(row);
            }

            dgvData.DataSource = table;
            dgvData.AutoResizeColumns();

            // Hide columns that are entirely empty/default
            foreach (DataGridViewColumn col in dgvData.Columns)
            {
                if (col.Name == "ID" || col.Name == "Fuente" || col.Name == "Etiqueta")
                    continue;

                bool hasData = false;
                foreach (DataGridViewRow row in dgvData.Rows)
                {
                    var val = row.Cells[col.Index].Value;
                    if (val != null && val != DBNull.Value
                        && val.ToString() != "" && val.ToString() != "0")
                    {
                        hasData = true;
                        break;
                    }
                }
                col.Visible = hasData;
            }

            // ── Formato de moneda y alineacion derecha ────────────────────
            // Columnas conocidas como monetarias en DataItem
            var currencyColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Price", "Sales"
            };

            // Palabras clave para detectar columnas monetarias en ExtraFields
            string[] currencyKeywords =
            [
                "price", "cost", "precio", "costo", "monto", "importe",
                "amount", "total", "valor", "revenue", "ingreso", "tarifa",
                "gasto", "sales", "fee", "charge", "rate", "tasa"
            ];

            foreach (DataGridViewColumn col in dgvData.Columns)
            {
                if (!col.Visible) continue;

                bool isCurrency = currencyColumns.Contains(col.Name);
                if (!isCurrency)
                {
                    string lower = col.Name.ToLowerInvariant();
                    foreach (var kw in currencyKeywords)
                        if (lower.Contains(kw)) { isCurrency = true; break; }
                }

                if (!isCurrency) continue;

                // Alineacion a la derecha (columna y encabezado)
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;

                // Simbolo $ solo para columnas numericas (double/decimal)
                if (table.Columns.Contains(col.Name))
                {
                    var dataType = table.Columns[col.Name].DataType;
                    if (dataType == typeof(double) || dataType == typeof(decimal)
                        || dataType == typeof(float))
                    {
                        col.DefaultCellStyle.Format = "$#,##0.00";
                    }
                }
            }

            foreach (DataGridViewRow row in dgvData.Rows)
            {
                var src = row.Cells["Fuente"].Value?.ToString() ?? "";
                row.DefaultCellStyle.BackColor = GetSourceColor(src);
            }

            lblRowCount.Text = $"{table.Rows.Count} registros";
        }

        private void cmbSourceFilter_SelectedIndexChanged(object? sender, EventArgs e) =>
            FillDataGridView();

        private static Color GetSourceColor(string source) => source switch
        {
            "CSV" => Color.FromArgb(220, 235, 255),
            "JSON" => Color.FromArgb(255, 235, 210),
            "XML" => Color.FromArgb(215, 245, 215),
            "TXT" => Color.FromArgb(240, 220, 255),
            "DB" => Color.FromArgb(255, 250, 210),
            "XLSX" => Color.FromArgb(198, 239, 206),
            _ => Color.White,
        };

        // ── Paleta de colores para las graficas

        private static Color GetColor(int index)
        {
            Color[] palette =
            [
                Color.FromArgb(70, 130, 180),   // SteelBlue
                Color.FromArgb(255, 127, 80),   // Coral
                Color.FromArgb(60, 179, 113),   // MediumSeaGreen
                Color.FromArgb(186, 85, 211),   // MediumOrchid
                Color.FromArgb(255, 165, 0),    // Orange
                Color.FromArgb(220, 20, 60),    // Crimson
                Color.FromArgb(32, 178, 170),   // LightSeaGreen
                Color.FromArgb(218, 165, 32),   // GoldenRod
                Color.FromArgb(100, 149, 237),  // CornflowerBlue
                Color.FromArgb(255, 99, 71),    // Tomato
            ];
            return palette[index % palette.Length];
        }

        // ════════════════════════════════════════════════════════════════════
        //  EXPORTACION A ARCHIVOS (JSON, TXT, XML)
        // ════════════════════════════════════════════════════════════════════

        private void btnExportFileJson_Click(object? sender, EventArgs e)
            => ExportToFile("JSON", "Archivos JSON|*.json", ExportItemsToJson);

        private void btnExportFileTxt_Click(object? sender, EventArgs e)
            => ExportToFile("TXT", "Archivos de texto|*.txt", ExportItemsToTxt);

        private void btnExportFileXml_Click(object? sender, EventArgs e)
            => ExportToFile("XML", "Archivos XML|*.xml", ExportItemsToXml);

        private void btnExportFileXlsx_Click(object? sender, EventArgs e)
        {
            if (!HasData()) return;

            using var dlg = new SaveFileDialog
            {
                Title = "Exportar datos a Excel",
                Filter = "Archivos Excel|*.xlsx",
                FileName = "datos_exportados"
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                Cursor = Cursors.WaitCursor;

                var rows = _repository.GetAll().Select(ItemToRow).ToList();
                var columns = DiscoverAllColumns(rows);

                using var spreadsheet = SpreadsheetDocument.Create(
                    dlg.FileName, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);

                var workbookPart = spreadsheet.AddWorkbookPart();
                workbookPart.Workbook = new OxlSheet.Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new OxlSheet.SheetData();
                worksheetPart.Worksheet = new OxlSheet.Worksheet(sheetData);

                var sheets = spreadsheet.WorkbookPart!.Workbook.AppendChild(
                    new OxlSheet.Sheets());
                sheets.AppendChild(new OxlSheet.Sheet
                {
                    Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Datos"
                });

                // Fila de encabezados
                var headerRow = new OxlSheet.Row();
                foreach (var col in columns)
                    headerRow.AppendChild(CreateTextCell(col));
                sheetData.AppendChild(headerRow);

                // Filas de datos
                foreach (var row in rows)
                {
                    var dataRow = new OxlSheet.Row();
                    foreach (var col in columns)
                        dataRow.AppendChild(CreateTextCell(
                            row.TryGetValue(col, out var v) ? v : string.Empty));
                    sheetData.AppendChild(dataRow);
                }

                workbookPart.Workbook.Save();
                Cursor = Cursors.Default;

                lblStatus.Text = $"Exportados {_repository.GetAll().Count} registros a '{Path.GetFileName(dlg.FileName)}'";
                MessageBox.Show(
                    $"Se exportaron {_repository.GetAll().Count} registros a:\n{dlg.FileName}",
                    "Exportacion exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OfrecerEnviarPorCorreo(dlg.FileName);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Error al exportar a Excel:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static OxlSheet.Cell CreateTextCell(string text)
        {
            return new OxlSheet.Cell
            {
                DataType = OxlSheet.CellValues.InlineString,
                InlineString = new OxlSheet.InlineString(
                    new OxlSheet.Text(text))
            };
        }

        private void btnExportFileCsv_Click(object? sender, EventArgs e)
        {
            if (!HasData()) return;

            using var dlg = new SaveFileDialog
            {
                Title = "Exportar datos a CSV",
                Filter = "Archivos CSV|*.csv",
                FileName = "datos_exportados"
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                Cursor = Cursors.WaitCursor;

                // Usar DataExporter para exportar a CSV de forma simple
                var rows = new List<Dictionary<string, string>>(_repository.GetAll().Count);
                foreach (var item in _repository.GetAll())
                    rows.Add(ItemToRow(item));

                var columns = DiscoverAllColumns(rows);
                var sb = new StringBuilder();
                sb.AppendLine(string.Join(",", columns.Select(c => $"\"{c}\"")));

                foreach (var row in rows)
                {
                    var values = new List<string>(columns.Count);
                    foreach (var col in columns)
                    {
                        var val = row.TryGetValue(col, out var v) ? v : "";
                        // Escapar comillas y valores con comas
                        if (val.Contains(",") || val.Contains("\"") || val.Contains("\n"))
                            values.Add($"\"{val.Replace("\"", "\"\"")}\"");
                        else
                            values.Add(val);
                    }
                    sb.AppendLine(string.Join(",", values));
                }

                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                Cursor = Cursors.Default;
                lblStatus.Text = $"Exportados {_repository.GetAll().Count} registros a '{Path.GetFileName(dlg.FileName)}'";
                MessageBox.Show(
                    $"Se exportaron {_repository.GetAll().Count} registros a:\n{dlg.FileName}",
                    "Exportacion exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OfrecerEnviarPorCorreo(dlg.FileName);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Error al exportar a CSV:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToFile(string format, string filter, Action<List<DataItem>, string> writeAction)
        {
            if (!HasData()) return;

            using var dlg = new SaveFileDialog
            {
                Title = $"Exportar datos a {format}",
                Filter = filter,
                FileName = $"datos_exportados"
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                writeAction(_repository.GetAll(), dlg.FileName);
                Cursor = Cursors.Default;
                var total = _repository.GetAll().Count;
                lblStatus.Text = $"Exportados {total} registros a '{Path.GetFileName(dlg.FileName)}'";
                MessageBox.Show(
                    $"Se exportaron {total} registros a:\n{dlg.FileName}",
                    "Exportacion exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OfrecerEnviarPorCorreo(dlg.FileName);
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Error al exportar a {format}:\n\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Convierte un DataItem a Dictionary con solo campos no vacios + ExtraFields.
        /// </summary>
        private static Dictionary<string, string> ItemToRow(DataItem item)
        {
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Id"] = item.Id.ToString(CultureInfo.InvariantCulture),
                ["Fuente"] = item.Source.ToString(),
                ["Etiqueta"] = item.Label
            };

            if (!string.IsNullOrEmpty(item.Company)) row["Company"] = item.Company;
            if (!string.IsNullOrEmpty(item.TypeName)) row["TypeName"] = item.TypeName;
            if (!string.IsNullOrEmpty(item.Cpu)) row["Cpu"] = item.Cpu;
            if (item.Ram != 0) row["Ram"] = item.Ram.ToString(CultureInfo.InvariantCulture);
            if (item.Price != 0) row["Price"] = item.Price.ToString(CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(item.Title)) row["Title"] = item.Title;
            if (!string.IsNullOrEmpty(item.Genre)) row["Genre"] = item.Genre;
            if (item.Sales != 0) row["Sales"] = item.Sales.ToString(CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(item.Platform)) row["Platform"] = item.Platform;
            if (!string.IsNullOrEmpty(item.Tipo)) row["Tipo"] = item.Tipo;
            if (!string.IsNullOrEmpty(item.Modelo)) row["Modelo"] = item.Modelo;
            if (item.Stock != 0) row["Stock"] = item.Stock.ToString(CultureInfo.InvariantCulture);
            if (item.Minuto != 0) row["Minuto"] = item.Minuto.ToString(CultureInfo.InvariantCulture);
            if (item.UsoCPU != 0) row["UsoCPU"] = item.UsoCPU.ToString(CultureInfo.InvariantCulture);
            if (item.Temperatura != 0) row["Temperatura"] = item.Temperatura.ToString(CultureInfo.InvariantCulture);
            if (item.FPS != 0) row["FPS"] = item.FPS.ToString(CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(item.UserName)) row["UserName"] = item.UserName;
            if (!string.IsNullOrEmpty(item.Email)) row["Email"] = item.Email;
            if (!string.IsNullOrEmpty(item.Region)) row["Region"] = item.Region;

            foreach (var kv in item.ExtraFields)
                if (!string.IsNullOrEmpty(kv.Key) && !row.ContainsKey(kv.Key))
                    row[kv.Key] = kv.Value;

            return row;
        }

        /// <summary>Descubre la union de todas las columnas preservando orden.</summary>
        private static List<string> DiscoverAllColumns(List<Dictionary<string, string>> rows)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var columns = new List<string>();

            // Columnas fijas primero
            foreach (var fix in new[] { "Id", "Fuente", "Etiqueta" })
            { seen.Add(fix); columns.Add(fix); }

            foreach (var row in rows)
                foreach (var key in row.Keys)
                    if (seen.Add(key))
                        columns.Add(key);

            return columns;
        }

        private static void ExportItemsToJson(List<DataItem> items, string path)
        {
            var rows = new List<Dictionary<string, string>>(items.Count);
            foreach (var item in items)
                rows.Add(ItemToRow(item));

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string json = JsonSerializer.Serialize(rows, options);
            File.WriteAllText(path, json, Encoding.UTF8);
        }

        private static void ExportItemsToTxt(List<DataItem> items, string path)
        {
            var rows = new List<Dictionary<string, string>>(items.Count);
            foreach (var item in items)
                rows.Add(ItemToRow(item));

            var columns = DiscoverAllColumns(rows);

            var sb = new StringBuilder();
            sb.AppendLine(string.Join("|", columns));

            foreach (var row in rows)
            {
                var values = new List<string>(columns.Count);
                foreach (var col in columns)
                    values.Add(row.TryGetValue(col, out var v) ? v : "");
                sb.AppendLine(string.Join("|", values));
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private static void ExportItemsToXml(List<DataItem> items, string path)
        {
            var rows = new List<Dictionary<string, string>>(items.Count);
            foreach (var item in items)
                rows.Add(ItemToRow(item));

            var columns = DiscoverAllColumns(rows);

            var doc = new XmlDocument();
            var root = doc.CreateElement("Datos");
            doc.AppendChild(root);

            foreach (var row in rows)
            {
                var elem = doc.CreateElement("Registro");
                foreach (var col in columns)
                {
                    var child = doc.CreateElement(SanitizeXmlName(col));
                    child.InnerText = row.TryGetValue(col, out var v) ? v : "";
                    elem.AppendChild(child);
                }
                root.AppendChild(elem);
            }

            using var writer = XmlWriter.Create(path, new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8
            });
            doc.Save(writer);
        }

        /// <summary>Limpia un nombre para que sea un nombre XML valido.</summary>
        private static string SanitizeXmlName(string name)
        {
            var sb = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
                else if (c == ' ')
                    sb.Append('_');
            }
            string result = sb.ToString();
            if (result.Length == 0 || !char.IsLetter(result[0]) && result[0] != '_')
                result = "_" + result;
            return result;
        }

        // ════════════════════════════════════════════════════════════════════
        //  CORREO ELECTRONICO
        // ════════════════════════════════════════════════════════════════════

        private void OfrecerEnviarPorCorreo(string rutaArchivo)
        {
            var respuesta = MessageBox.Show(
                $"¿Deseas enviar el archivo exportado por correo electrónico?\n\nÐ Adjunto: {Path.GetFileName(rutaArchivo)}",
                "Enviar por correo",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (respuesta == DialogResult.Yes)
            {
                using var formCorreo = new FormCorreoEnvio(rutaArchivo);
                formCorreo.ShowDialog(this);
            }
        }
    }
}