namespace EXPLORADORDEARCHIVOS_TERMINADO.Limpieza_de_datos_3._3.Core;

/// <summary>
/// Diálogo que muestra el tipo inferido para cada columna y permite al usuario:
///   1. Corregir el tipo antes de aplicar la limpieza.
///   2. Seleccionar exactamente qué columnas desea limpiar.
///   3. Ver cuántos errores detectados tiene cada columna.
/// Si una columna no tiene errores, aparece desmarcada por defecto.
/// </summary>
public sealed class ColumnTypesForm : Form
{
    private static readonly string[] TypeLabels = ["Texto", "Numérico", "Fecha", "Teléfono", "Nombre Persona", "Salario", "Correo Electrónico"];

    private readonly DataGridView _grid;
    private readonly Button _btnConfirmar;
    private readonly Button _btnCancelar;
    private readonly Button _btnMarcarTodo;
    private readonly Button _btnDesmarcarTodo;
    private readonly Label _lblInstruccion;
    private readonly Label _lblResumen;

    /// <summary>
    /// Tipos confirmados por el usuario (columna → ColumnDataType).
    /// Solo se rellena si el usuario presionó Confirmar.
    /// </summary>
    public IReadOnlyDictionary<string, ColumnDataType>? ConfirmedTypes { get; private set; }

    /// <summary>
    /// Columnas que el usuario marcó para limpiar.
    /// Solo las columnas presentes aquí serán procesadas por DataCleaner.
    /// </summary>
    public IReadOnlySet<string>? ColumnsToClean { get; private set; }

    public ColumnTypesForm(
        IReadOnlyDictionary<string, ColumnDataType> inferredTypes,
        IReadOnlyList<CellError>? cellErrors = null)
    {
        Text = "Revisar columnas antes de limpiar";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(640, 520);
        Font = new Font("Segoe UI", 9f);

        // Conteo de errores por columna
        var errorCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (cellErrors is not null)
        {
            foreach (var err in cellErrors)
            {
                errorCount.TryGetValue(err.Column, out var n);
                errorCount[err.Column] = n + 1;
            }
        }

        _lblInstruccion = new Label
        {
            Text = "Revisa el tipo de cada columna y marca las que quieres limpiar.\n" +
                   "Solo se aplicarán cambios en las columnas marcadas con ✓.",
            Location = new Point(12, 12),
            Size = new Size(604, 36),
            ForeColor = Color.DarkSlateGray
        };

        // Conteo total de errores para el resumen
        int totalErrors = errorCount.Values.Sum();
        int columnsWithErrors = errorCount.Count;
        _lblResumen = new Label
        {
            Text = totalErrors > 0
                ? $"Se detectaron {totalErrors} error(es) en {columnsWithErrors} columna(s). " +
                  "Las columnas con errores están marcadas automáticamente."
                : "✔ No se detectaron errores de tipo. Puedes marcar columnas para aplicar limpieza de formato.",
            Location = new Point(12, 52),
            Size = new Size(604, 32),
            ForeColor = totalErrors > 0 ? Color.DarkOrange : Color.SeaGreen,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
        };

        _grid = new DataGridView
        {
            Location = new Point(12, 90),
            Size = new Size(604, 330),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Col 1: nombre de columna (solo lectura)
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Columna",
            Name = "colNombre",
            ReadOnly = true,
            FillWeight = 38
        });

        // Col 2: tipo detectado (ComboBox editable)
        _grid.Columns.Add(new DataGridViewComboBoxColumn
        {
            HeaderText = "Tipo",
            Name = "colTipo",
            DataSource = TypeLabels,
            FlatStyle = FlatStyle.Flat,
            FillWeight = 28
        });

        // Col 3: errores detectados (solo lectura, informativa)
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Errores",
            Name = "colErrores",
            ReadOnly = true,
            FillWeight = 18
        });

        // Col 4: checkbox "Limpiar esta columna"
        _grid.Columns.Add(new DataGridViewCheckBoxColumn
        {
            HeaderText = "Limpiar ✓",
            Name = "colLimpiar",
            FillWeight = 16
        });

        // Llenar filas
        foreach (var kvp in inferredTypes)
        {
            errorCount.TryGetValue(kvp.Key, out var errCnt);
            bool hasErrors = errCnt > 0;
            var errorText = hasErrors ? $"⚠ {errCnt}" : "✔ 0";
            _grid.Rows.Add(kvp.Key, TypeToLabel(kvp.Value), errorText, hasErrors);
        }

        _grid.CellFormatting += Grid_CellFormatting;

        // Botones Marcar/Desmarcar todo
        _btnMarcarTodo = new Button
        {
            Text = "Marcar todo",
            Size = new Size(100, 26),
            Location = new Point(12, 430)
        };
        _btnMarcarTodo.Click += (_, _) => SetAllCheckboxes(true);

        _btnDesmarcarTodo = new Button
        {
            Text = "Desmarcar todo",
            Size = new Size(110, 26),
            Location = new Point(118, 430)
        };
        _btnDesmarcarTodo.Click += (_, _) => SetAllCheckboxes(false);

        _btnConfirmar = new Button
        {
            Text = "Confirmar y limpiar",
            Size = new Size(150, 28),
            Location = new Point(378, 428),
            TabIndex = 0
        };
        _btnConfirmar.Click += BtnConfirmar_Click;

        _btnCancelar = new Button
        {
            Text = "Cancelar",
            Size = new Size(90, 28),
            Location = new Point(534, 428),
            TabIndex = 1,
            DialogResult = DialogResult.Cancel
        };

        AcceptButton = _btnConfirmar;
        CancelButton = _btnCancelar;

        Controls.AddRange([_lblInstruccion, _lblResumen, _grid,
            _btnMarcarTodo, _btnDesmarcarTodo, _btnConfirmar, _btnCancelar]);
    }

    private void SetAllCheckboxes(bool value)
    {
        _grid.EndEdit();
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (!row.IsNewRow)
                row.Cells["colLimpiar"].Value = value;
        }
    }

    private void BtnConfirmar_Click(object sender, EventArgs e)
    {
        _grid.EndEdit();

        var types = new Dictionary<string, ColumnDataType>(StringComparer.OrdinalIgnoreCase);
        var toClean = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.IsNewRow) continue;
            var colName = row.Cells["colNombre"].Value?.ToString() ?? string.Empty;
            var label = row.Cells["colTipo"].Value?.ToString() ?? "Texto";
            types[colName] = LabelToType(label);

            if (row.Cells["colLimpiar"].Value is true)
                toClean.Add(colName);
        }

        ConfirmedTypes = types;
        ColumnsToClean = toClean;
        DialogResult = DialogResult.OK;
        Close();
    }

    private static void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || sender is not DataGridView grid) return;

        var label = grid.Rows[e.RowIndex].Cells["colTipo"].Value?.ToString();
        var errorText = grid.Rows[e.RowIndex].Cells["colErrores"].Value?.ToString() ?? "";
        bool hasErrors = errorText.StartsWith("⚠");

        Color backColor = label switch
        {
            "Numérico" => Color.FromArgb(232, 245, 233),
            "Fecha" => Color.FromArgb(227, 242, 253),
            "Teléfono" => Color.FromArgb(245, 235, 240),
            "Nombre Persona" => Color.FromArgb(245, 245, 220),
            "Salario" => Color.FromArgb(240, 255, 240),
            "Correo Electrónico" => Color.FromArgb(240, 248, 255),
            _ => Color.FromArgb(255, 253, 231)
        };

        // Si tiene errores, intensificar el color de fondo
        if (hasErrors)
            backColor = label switch
            {
                "Numérico" => Color.FromArgb(200, 240, 200),
                "Fecha" => Color.FromArgb(197, 225, 252),
                "Teléfono" => Color.FromArgb(230, 200, 220),
                "Nombre Persona" => Color.FromArgb(255, 255, 180),
                "Salario" => Color.FromArgb(200, 255, 200),
                "Correo Electrónico" => Color.FromArgb(200, 230, 255),
                _ => Color.FromArgb(255, 236, 153)
            };

        grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = backColor;

        // Colorear la celda de errores
        if (e.ColumnIndex == grid.Columns["colErrores"]?.Index)
        {
            e.CellStyle.ForeColor = hasErrors ? Color.DarkOrange : Color.SeaGreen;
            e.CellStyle.Font = new Font(grid.Font, FontStyle.Bold);
            e.FormattingApplied = true;
        }
    }

    private static string TypeToLabel(ColumnDataType t) => t switch
    {
        ColumnDataType.Numeric => "Numérico",
        ColumnDataType.Date => "Fecha",
        ColumnDataType.Phone => "Teléfono",
        ColumnDataType.PersonName => "Nombre Persona",
        ColumnDataType.Salary => "Salario",
        ColumnDataType.Email => "Correo Electrónico",
        _ => "Texto"
    };

    private static ColumnDataType LabelToType(string label) => label switch
    {
        "Numérico" => ColumnDataType.Numeric,
        "Fecha" => ColumnDataType.Date,
        "Teléfono" => ColumnDataType.Phone,
        "Nombre Persona" => ColumnDataType.PersonName,
        "Salario" => ColumnDataType.Salary,
        "Correo Electrónico" => ColumnDataType.Email,
        _ => ColumnDataType.Text
    };
}
