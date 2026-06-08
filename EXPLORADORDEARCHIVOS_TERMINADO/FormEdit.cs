using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Web.WebView2.WinForms;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    public partial class FormEdit : Form
    {
        private string _currentFile = "";
        private string _currentFileType = "";
        private DataTable _currentDataTable = new DataTable();

        public FormEdit()
        {
            InitializeComponent();
            ConfigurarControles();
        }

        private void ConfigurarControles()
        {
            // Configurar DataGridView
            dgvDatos.AllowUserToAddRows = true;
            dgvDatos.AllowUserToDeleteRows = true;
            dgvDatos.ReadOnly = false;
            dgvDatos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            // Configurar TextBox
            rtbContenido.ReadOnly = false;
            rtbContenido.WordWrap = true;
        }

        // ════════════════════════════════════════════════════════════════════
        //  BOTON: Cargar Archivo
        // ════════════════════════════════════════════════════════════════════

        private void btnCargar_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Abrir archivo para editar",
                Filter = "Archivos soportados (*.csv;*.xlsx;*.xls;*.docx;*.doc;*.pptx;*.ppt;*.txt;*.json;*.xml;*.pdf)|*.csv;*.xlsx;*.xls;*.docx;*.doc;*.pptx;*.ppt;*.txt;*.json;*.xml;*.pdf|" +
                         "Archivos CSV (*.csv)|*.csv|" +
                         "Archivos Excel (*.xlsx;*.xls)|*.xlsx;*.xls|" +
                         "Archivos Word (*.docx;*.doc)|*.docx;*.doc|" +
                         "Archivos PowerPoint (*.pptx;*.ppt)|*.pptx;*.ppt|" +
                         "Archivos de Texto (*.txt)|*.txt|" +
                         "Archivos JSON (*.json)|*.json|" +
                         "Archivos XML (*.xml)|*.xml|" +
                         "Archivos PDF (*.pdf)|*.pdf|" +
                         "Todos los archivos (*.*)|*.*"
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            _currentFile = dlg.FileName;
            string ext = Path.GetExtension(_currentFile).ToLower();

            try
            {
                Cursor = Cursors.WaitCursor;

                if (ext == ".csv")
                    CargarCSV();
                else if (ext == ".xlsx" || ext == ".xls")
                    CargarExcel();
                else if (ext == ".docx" || ext == ".doc")
                    CargarWord();
                else if (ext == ".pptx" || ext == ".ppt")
                    CargarPowerPoint();
                else if (ext == ".txt")
                    CargarTexto();
                else if (ext == ".json")
                    CargarJSON();
                else if (ext == ".xml")
                    CargarXML();
                else if (ext == ".pdf")
                    CargarPDF();

                this.Text = $"Editor - {Path.GetFileName(_currentFile)}";
                Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Error al cargar archivo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  CARGA DE ARCHIVOS
        // ════════════════════════════════════════════════════════════════════

        private void CargarCSV()
        {
            _currentFileType = "CSV";
            dgvDatos.DataSource = null;
            dgvDatos.Rows.Clear();
            dgvDatos.Columns.Clear();
            rtbContenido.Clear();
            MostrarTextBox();

            var lines = File.ReadAllLines(_currentFile);
            if (lines.Length == 0) return;

            // Obtener encabezados
            var headers = lines[0].Split(',');
            foreach (var header in headers)
            {
                dgvDatos.Columns.Add(header.Trim(), header.Trim());
            }

            // Cargar datos
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');
                dgvDatos.Rows.Add(values.Select(v => v.Trim()).ToArray());
            }

            lblEstado.Text = $"✓ Archivo CSV cargado ({dgvDatos.Rows.Count} filas)";
        }

        private void CargarExcel()
        {
            _currentFileType = "EXCEL";
            dgvDatos.DataSource = null;
            dgvDatos.Rows.Clear();
            dgvDatos.Columns.Clear();
            rtbContenido.Clear();
            MostrarTextBox();

            try
            {
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(_currentFile, false))
                {
                    WorkbookPart workbookPart = doc.WorkbookPart;
                    WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                    SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

                    // Obtener valores de celdas
                    var rows = sheetData.Elements<Row>().ToList();

                    if (rows.Count == 0)
                    {
                        lblEstado.Text = "⚠ El archivo Excel está vacío";
                        return;
                    }

                    // Cargar encabezados (primera fila)
                    var firstRow = rows[0];
                    foreach (Cell cell in firstRow.Elements<Cell>())
                    {
                        string headerValue = GetCellValue(workbookPart, cell);
                        dgvDatos.Columns.Add(headerValue, headerValue);
                    }

                    // Cargar datos (resto de filas)
                    for (int i = 1; i < rows.Count; i++)
                    {
                        var row = rows[i];
                        var cellValues = new List<string>();

                        foreach (Cell cell in row.Elements<Cell>())
                        {
                            string cellValue = GetCellValue(workbookPart, cell);
                            cellValues.Add(cellValue);
                        }

                        if (cellValues.Any(v => !string.IsNullOrEmpty(v)))
                        {
                            dgvDatos.Rows.Add(cellValues.ToArray());
                        }
                    }

                    lblEstado.Text = $"✓ Archivo Excel cargado ({dgvDatos.Rows.Count} filas)";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar Excel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblEstado.Text = "⚠ Error al cargar Excel";
            }
        }

        private void CargarWord()
        {
            _currentFileType = "WORD";
            dgvDatos.DataSource = null;
            dgvDatos.Rows.Clear();
            dgvDatos.Columns.Clear();
            rtbContenido.Clear();
            MostrarTextBox();

            try
            {
                var content = LeerArchivoWord(_currentFile);
                rtbContenido.Text = content;
                lblEstado.Text = $"✓ Documento Word cargado (SOLO LECTURA - edita el texto y guarda)";
            }
            catch
            {
                rtbContenido.Text = "(Archivo Word protegido o formato no soportado)";
                lblEstado.Text = "⚠ Error al cargar Word";
            }
        }

        private void CargarPowerPoint()
        {
            _currentFileType = "POWERPOINT";
            dgvDatos.DataSource = null;
            dgvDatos.Rows.Clear();
            dgvDatos.Columns.Clear();
            rtbContenido.Clear();
            MostrarTextBox();

            try
            {
                var content = LeerArchivoPowerPoint(_currentFile);
                rtbContenido.Text = content;
                lblEstado.Text = $"✓ Presentación PowerPoint cargada (SOLO LECTURA)";
            }
            catch
            {
                rtbContenido.Text = "(Archivo PowerPoint protegido o formato no soportado)";
                lblEstado.Text = "⚠ Error al cargar PowerPoint";
            }
        }

        private void CargarTexto()
        {
            _currentFileType = "TXT";
            dgvDatos.DataSource = null;
            dgvDatos.Rows.Clear();
            dgvDatos.Columns.Clear();
            rtbContenido.Clear();
            MostrarTextBox();

            var content = File.ReadAllText(_currentFile, Encoding.UTF8);
            rtbContenido.Font = new System.Drawing.Font("Consolas", 11F);
            rtbContenido.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            rtbContenido.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            rtbContenido.Text = content;
            lblEstado.Text = $"✓ Archivo de texto cargado";
        }

        private void CargarJSON()
        {
            _currentFileType = "JSON";
            dgvDatos.DataSource = null;
            dgvDatos.Rows.Clear();
            dgvDatos.Columns.Clear();
            rtbContenido.Clear();
            MostrarTextBox();

            var content = File.ReadAllText(_currentFile, Encoding.UTF8);
            rtbContenido.Font = new System.Drawing.Font("Consolas", 11F);
            rtbContenido.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            rtbContenido.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            rtbContenido.Text = content;
            AplicarColoreado_JSON();
            lblEstado.Text = $"✓ Archivo JSON cargado";
        }

        private void CargarXML()
        {
            _currentFileType = "XML";
            dgvDatos.DataSource = null;
            dgvDatos.Rows.Clear();
            dgvDatos.Columns.Clear();
            rtbContenido.Clear();
            MostrarTextBox();

            var content = File.ReadAllText(_currentFile, Encoding.UTF8);
            rtbContenido.Font = new System.Drawing.Font("Consolas", 11F);
            rtbContenido.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            rtbContenido.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            rtbContenido.Text = content;
            AplicarColoreado_XML();
            lblEstado.Text = $"✓ Archivo XML cargado";
        }

        private async void CargarPDF()
        {
            _currentFileType = "PDF";
            dgvDatos.DataSource = null;
            dgvDatos.Rows.Clear();
            dgvDatos.Columns.Clear();
            rtbContenido.Clear();

            rtbContenido.Visible = false;
            webViewPdf.Visible = true;

            await webViewPdf.EnsureCoreWebView2Async();
            webViewPdf.CoreWebView2.Navigate(new Uri(_currentFile).AbsoluteUri);
            lblEstado.Text = $"✓ Archivo PDF cargado (solo visualización)";
        }

        // ════════════════════════════════════════════════════════════════════
        //  BOTON: Guardar Cambios
        // ════════════════════════════════════════════════════════════════════

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFile))
            {
                MessageBox.Show("No hay archivo cargado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;

                if (_currentFileType == "CSV")
                    GuardarCSV();
                else if (_currentFileType == "EXCEL")
                    GuardarExcel();
                else if (_currentFileType == "WORD")
                    GuardarWord();
                else if (_currentFileType == "POWERPOINT")
                    MessageBox.Show("PowerPoint no se puede editar en modo texto. Por favor, usa Microsoft PowerPoint.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else if (_currentFileType == "PDF")
                    MessageBox.Show("Los archivos PDF son de solo visualización y no se pueden editar aquí.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else if (_currentFileType == "TXT" || _currentFileType == "JSON" || _currentFileType == "XML")
                    GuardarTexto();

                Cursor = Cursors.Default;
                MessageBox.Show("Archivo guardado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblEstado.Text = "✓ Cambios guardados";
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GuardarCSV()
        {
            var sb = new StringBuilder();

            // Guardar encabezados
            var headers = dgvDatos.Columns.Cast<DataGridViewColumn>().Select(c => c.Name).ToList();
            sb.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));

            // Guardar filas
            foreach (DataGridViewRow row in dgvDatos.Rows)
            {
                if (row.IsNewRow) continue;

                var values = new List<string>();
                foreach (DataGridViewCell cell in row.Cells)
                {
                    var val = cell.Value?.ToString() ?? "";
                    if (val.Contains(",") || val.Contains("\""))
                        values.Add($"\"{val.Replace("\"", "\"\"")}\"");
                    else
                        values.Add($"\"{val}\"");
                }
                sb.AppendLine(string.Join(",", values));
            }

            File.WriteAllText(_currentFile, sb.ToString(), Encoding.UTF8);
        }

        private void GuardarExcel()
        {
            try
            {
                // PASO 1: Crear backup del archivo original
                string backup = _currentFile + ".backup";
                if (File.Exists(backup))
                    File.Delete(backup);
                File.Copy(_currentFile, backup, true);

                // PASO 2: Crear archivo temporal
                string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".xlsx");

                // PASO 3: Copiar el archivo original al temporal
                File.Copy(_currentFile, tempFile, true);

                // PASO 4: Cerrar cualquier acceso previo (importante para Office)
                GC.Collect();
                GC.WaitForPendingFinalizers();
                System.Threading.Thread.Sleep(100);

                // PASO 5: Abrir el archivo temporal y modificarlo
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(tempFile, true))
                {
                    if (doc.WorkbookPart == null)
                        throw new Exception("El archivo Excel no tiene estructura válida");

                    WorkbookPart workbookPart = doc.WorkbookPart;

                    if (!workbookPart.WorksheetParts.Any())
                        throw new Exception("El archivo Excel no tiene hojas de cálculo");

                    WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                    SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();

                    if (sheetData == null)
                    {
                        sheetData = new SheetData();
                        worksheetPart.Worksheet.AppendChild(sheetData);
                    }

                    // PASO 6: Limpiar datos existentes
                    sheetData.RemoveAllChildren();

                    uint rowIndex = 1;

                    // PASO 7: Guardar encabezados
                    var headers = dgvDatos.Columns.Cast<DataGridViewColumn>().Select(c => c.Name).ToList();
                    var headerRow = new Row { RowIndex = rowIndex };

                    uint colIndex = 1;
                    foreach (var header in headers)
                    {
                        var cell = new Cell
                        {
                            CellReference = GetCellReference(rowIndex, colIndex),
                            CellValue = new CellValue(header),
                            DataType = CellValues.String
                        };
                        headerRow.Append(cell);
                        colIndex++;
                    }
                    sheetData.Append(headerRow);
                    rowIndex++;

                    // PASO 8: Guardar datos
                    foreach (DataGridViewRow dgvRow in dgvDatos.Rows)
                    {
                        if (dgvRow.IsNewRow) continue;

                        var dataRow = new Row { RowIndex = rowIndex };
                        colIndex = 1;

                        foreach (DataGridViewCell dgvCell in dgvRow.Cells)
                        {
                            var cellValue = dgvCell.Value?.ToString() ?? "";
                            var newCell = new Cell
                            {
                                CellReference = GetCellReference(rowIndex, colIndex),
                                CellValue = new CellValue(cellValue),
                                DataType = CellValues.String
                            };
                            dataRow.Append(newCell);
                            colIndex++;
                        }

                        sheetData.Append(dataRow);
                        rowIndex++;
                    }

                    // PASO 9: Guardar cambios en el documento
                    doc.Save();
                }

                // PASO 10: Cerrar y esperar
                GC.Collect();
                GC.WaitForPendingFinalizers();
                System.Threading.Thread.Sleep(200);

                // PASO 11: Reemplazar archivo original si todo fue bien
                File.Delete(_currentFile);
                File.Move(tempFile, _currentFile, true);

                // PASO 12: Eliminar backup
                if (File.Exists(backup))
                    File.Delete(backup);
            }
            catch (Exception ex)
            {
                // Si hay error, intentar restaurar desde backup
                string backup = _currentFile + ".backup";
                if (File.Exists(backup))
                {
                    try
                    {
                        if (File.Exists(_currentFile))
                            File.Delete(_currentFile);
                        File.Copy(backup, _currentFile, true);
                    }
                    catch { }
                }
                throw new Exception($"Error al guardar Excel: {ex.Message}. El archivo ha sido restaurado desde el backup.");
            }
        }

        private string GetCellReference(uint row, uint col)
        {
            string colRef = "";
            uint colNum = col;

            while (colNum > 0)
            {
                uint remainder = (colNum - 1) % 26;
                colRef = (char)('A' + remainder) + colRef;
                colNum = (colNum - remainder - 1) / 26;
            }

            return colRef + row;
        }

        // ✅ CORREGIDO: Guardar Word manteniendo la estructura XML
        private void GuardarWord()
        {
            try
            {
                // PASO 1: Crear backup
                string backup = _currentFile + ".backup";
                if (File.Exists(backup))
                    File.Delete(backup);
                File.Copy(_currentFile, backup, true);

                // PASO 2: Crear archivo temporal
                string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".docx");
                File.Copy(_currentFile, tempFile, true);

                // PASO 3: Cerrar acceso previo
                GC.Collect();
                GC.WaitForPendingFinalizers();
                System.Threading.Thread.Sleep(100);

                // PASO 4: Abrir archivo temporal
                using (ZipArchive archive = ZipFile.Open(tempFile, ZipArchiveMode.Update))
                {
                    var docEntry = archive.Entries.FirstOrDefault(e => e.FullName == "word/document.xml");
                    if (docEntry != null)
                    {
                        // Leer XML
                        string xmlContent;
                        using (var stream = docEntry.Open())
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                xmlContent = reader.ReadToEnd();
                            }
                        }

                        var xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xmlContent);

                        // Limpiar texto existente
                        var textNodes = xmlDoc.GetElementsByTagName("w:t");
                        while (textNodes.Count > 0)
                        {
                            textNodes[0].ParentNode?.RemoveChild(textNodes[0]);
                        }

                        // Obtener body
                        var body = xmlDoc.GetElementsByTagName("w:body")[0];

                        // Crear párrafos nuevos
                        var lines = rtbContenido.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                        foreach (var line in lines)
                        {
                            var pElement = xmlDoc.CreateElement("w:p", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
                            var rElement = xmlDoc.CreateElement("w:r", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
                            var tElement = xmlDoc.CreateElement("w:t", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");


                            tElement.InnerText = line;
                            rElement.AppendChild(tElement);
                            pElement.AppendChild(rElement);
                            body.AppendChild(pElement);
                        }

                        // Eliminar entrada antigua
                        docEntry.Delete();

                        // Crear entrada nueva
                        var newEntry = archive.CreateEntry("word/document.xml");
                        using (var entryStream = newEntry.Open())
                        {
                            using (var writer = new StreamWriter(entryStream, Encoding.UTF8))
                            {
                                xmlDoc.Save(writer);
                            }
                        }
                    }
                }

                // PASO 5: Cerrar y esperar
                GC.Collect();
                GC.WaitForPendingFinalizers();
                System.Threading.Thread.Sleep(200);

                // PASO 6: Reemplazar original
                File.Delete(_currentFile);
                File.Move(tempFile, _currentFile, true);

                // PASO 7: Limpiar backup
                if (File.Exists(backup))
                    File.Delete(backup);
            }
            catch (Exception ex)
            {
                // Restaurar desde backup
                string backup = _currentFile + ".backup";
                if (File.Exists(backup))
                {
                    try
                    {
                        if (File.Exists(_currentFile))
                            File.Delete(_currentFile);
                        File.Copy(backup, _currentFile, true);
                    }
                    catch { }
                }
                throw new Exception($"Error al guardar Word: {ex.Message}. El archivo ha sido restaurado desde el backup.");
            }
        }

        private void GuardarTexto()
        {
            File.WriteAllText(_currentFile, rtbContenido.Text, Encoding.UTF8);
        }

        // ════════════════════════════════════════════════════════════════════
        //  HELPERS DE VISUALIZACIÓN
        // ════════════════════════════════════════════════════════════════════

        private void MostrarTextBox()
        {
            rtbContenido.Visible = true;
            webViewPdf.Visible = false;
        }

        private void AplicarColoreado_JSON()
        {
            rtbContenido.SuspendLayout();
            // Llaves y corchetes
            ColorearPatron(@"[{}\[\]]", System.Drawing.Color.FromArgb(255, 200, 50));
            // Strings (claves y valores)
            ColorearPatron(@"\""[^\""]*\""", System.Drawing.Color.FromArgb(100, 220, 100));
            // Números
            ColorearPatron(@"(?<=:\s*)-?\d+(\.\d+)?", System.Drawing.Color.FromArgb(100, 180, 255));
            // true / false / null
            ColorearPatron(@"\b(true|false|null)\b", System.Drawing.Color.FromArgb(220, 120, 60));
            rtbContenido.ResumeLayout();
        }

        private void AplicarColoreado_XML()
        {
            rtbContenido.SuspendLayout();
            // Etiquetas
            ColorearPatron(@"<[^>]+>", System.Drawing.Color.FromArgb(86, 156, 214));
            // Atributos
            ColorearPatron(@"\b[\w:-]+=", System.Drawing.Color.FromArgb(156, 220, 254));
            // Valores de atributos
            ColorearPatron(@"=\""[^\""]*\""", System.Drawing.Color.FromArgb(206, 145, 120));
            // Comentarios
            ColorearPatron(@"<!--.*?-->", System.Drawing.Color.FromArgb(106, 153, 85));
            rtbContenido.ResumeLayout();
        }

        private void ColorearPatron(string patron, System.Drawing.Color color)
        {
            var text = rtbContenido.Text;
            foreach (Match m in Regex.Matches(text, patron, RegexOptions.Singleline))
            {
                rtbContenido.Select(m.Index, m.Length);
                rtbContenido.SelectionColor = color;
            }
            rtbContenido.SelectionStart = 0;
            rtbContenido.SelectionLength = 0;
        }

        // ════════════════════════════════════════════════════════════════════
        //  HELPERS
        // ════════════════════════════════════════════════════════════════════

        private string GetCellValue(WorkbookPart workbookPart, Cell cell)
        {
            if (cell?.CellValue == null)
                return string.Empty;

            string value = cell.CellValue.Text;

            // Si el tipo de dato es una referencia compartida de strings (SST)
            if (cell.DataType?.Value == CellValues.SharedString)
            {
                int index = int.Parse(value);
                value = workbookPart.SharedStringTablePart?.SharedStringTable.Elements<SharedStringItem>()
                    .ElementAt(index)?.Text?.Text ?? string.Empty;
            }

            return value;
        }

        private string LeerArchivoWord(string path)
        {
            try
            {
                using var zip = ZipFile.OpenRead(path);
                var entry = zip.GetEntry("word/document.xml");
                if (entry != null)
                {
                    using var stream = entry.Open();
                    using var reader = new StreamReader(stream);
                    var xml = reader.ReadToEnd();

                    var doc = new XmlDocument();
                    doc.LoadXml(xml);

                    var textNodes = doc.GetElementsByTagName("w:t");
                    var sb = new StringBuilder();
                    foreach (XmlNode node in textNodes)
                    {
                        sb.Append(node.InnerText);
                    }
                    return sb.ToString();
                }
            }
            catch { }

            return "(No se pudo leer el contenido del documento)";
        }

        private string LeerArchivoPowerPoint(string path)
        {
            try
            {
                using var zip = ZipFile.OpenRead(path);
                var sb = new StringBuilder();
                var slideEntries = zip.Entries.Where(e => e.FullName.StartsWith("ppt/slides/slide") && e.FullName.EndsWith(".xml"));

                int slideNum = 1;
                foreach (var entry in slideEntries.OrderBy(e => e.Name))
                {
                    using var stream = entry.Open();
                    using var reader = new StreamReader(stream);
                    var xml = reader.ReadToEnd();

                    var doc = new XmlDocument();
                    doc.LoadXml(xml);

                    sb.AppendLine($"--- Diapositiva {slideNum} ---");
                    var textNodes = doc.GetElementsByTagName("a:t");
                    foreach (XmlNode node in textNodes)
                    {
                        sb.AppendLine(node.InnerText);
                    }
                    sb.AppendLine();
                    slideNum++;
                }

                return sb.ToString();
            }
            catch { }

            return "(No se pudo leer la presentación)";
        }
    }
}
