# Genera ejemplo_graficas.xlsx construyendo manualmente el ZIP OOXML
# No requiere ninguna DLL externa, solo System.IO.Compression (incluido en .NET/PS)

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$outPath = Join-Path $PSScriptRoot "ejemplo_graficas.xlsx"
if (Test-Path $outPath) { Remove-Item $outPath -Force }

# ── Datos: empleados de empresa ficticia ─────────────────────────────────────
# Columnas: Departamento, Cargo, Region, Genero, Salario, Antiguedad, Proyectos, Evaluacion
$filas = @(
	@("Departamento","Cargo",        "Region", "Genero","Salario","Antiguedad","Proyectos","Evaluacion"),
	@("Marketing",   "Analista",     "Norte",  "F",     "32000",  "2",         "4",        "8.5"),
	@("Marketing",   "Senior",       "Norte",  "M",     "48000",  "5",         "7",        "9.1"),
	@("Marketing",   "Director",     "Centro", "F",     "75000",  "10",        "12",       "9.4"),
	@("Marketing",   "Analista",     "Sur",    "M",     "30000",  "1",         "3",        "7.8"),
	@("Marketing",   "Senior",       "Centro", "F",     "47000",  "4",         "6",        "8.9"),
	@("TI",          "Desarrollador","Norte",  "M",     "45000",  "3",         "8",        "8.7"),
	@("TI",          "Desarrollador","Centro", "F",     "44000",  "2",         "5",        "8.2"),
	@("TI",          "Arquitecto",   "Norte",  "M",     "82000",  "8",         "15",       "9.6"),
	@("TI",          "QA",           "Sur",    "F",     "38000",  "2",         "6",        "8.0"),
	@("TI",          "DevOps",       "Centro", "M",     "60000",  "5",         "10",       "9.0"),
	@("TI",          "Desarrollador","Sur",    "F",     "43000",  "3",         "7",        "8.5"),
	@("Ventas",      "Vendedor",     "Norte",  "M",     "28000",  "1",         "2",        "7.5"),
	@("Ventas",      "Vendedor",     "Sur",    "F",     "27000",  "1",         "3",        "7.9"),
	@("Ventas",      "Senior",       "Norte",  "M",     "42000",  "4",         "5",        "8.6"),
	@("Ventas",      "Senior",       "Centro", "F",     "41000",  "3",         "4",        "8.3"),
	@("Ventas",      "Gerente",      "Centro", "M",     "68000",  "7",         "9",        "9.2"),
	@("Ventas",      "Vendedor",     "Sur",    "M",     "26000",  "1",         "2",        "7.2"),
	@("RRHH",        "Analista",     "Centro", "F",     "33000",  "2",         "3",        "8.1"),
	@("RRHH",        "Senior",       "Norte",  "M",     "46000",  "5",         "5",        "8.7"),
	@("RRHH",        "Director",     "Centro", "F",     "72000",  "9",         "8",        "9.3"),
	@("RRHH",        "Analista",     "Sur",    "M",     "31000",  "2",         "3",        "7.6"),
	@("Finanzas",    "Contador",     "Norte",  "F",     "40000",  "3",         "4",        "8.4"),
	@("Finanzas",    "Contador",     "Sur",    "M",     "39000",  "2",         "3",        "8.0"),
	@("Finanzas",    "Senior",       "Centro", "F",     "55000",  "6",         "6",        "9.0"),
	@("Finanzas",    "Director",     "Norte",  "M",     "80000",  "11",        "10",       "9.5")
)

# ── Generar XML de la hoja ────────────────────────────────────────────────────
function Escape-Xml($s) { $s -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;' -replace '"','&quot;' -replace "'","&apos;" }

$colLetras = @('A','B','C','D','E','F','G','H')

$sheetXml = '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
$sheetXml += '<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">'
$sheetXml += '<sheetData>'

for ($r = 0; $r -lt $filas.Count; $r++) {
	$fila = $filas[$r]
	$rowNum = $r + 1
	$sheetXml += "<row r=`"$rowNum`">"
	for ($c = 0; $c -lt $fila.Count; $c++) {
		$cellRef = "$($colLetras[$c])$rowNum"
		$val = $fila[$c]
		$num = 0.0
		if ([double]::TryParse($val, [System.Globalization.NumberStyles]::Any,
				[System.Globalization.CultureInfo]::InvariantCulture, [ref]$num)) {
			# Celda numerica
			$sheetXml += "<c r=`"$cellRef`"><v>$val</v></c>"
		} else {
			# Celda de texto inline
			$escaped = Escape-Xml $val
			$sheetXml += "<c r=`"$cellRef`" t=`"inlineStr`"><is><t>$escaped</t></is></c>"
		}
	}
	$sheetXml += '</row>'
}

$sheetXml += '</sheetData></worksheet>'

# ── Partes del ZIP OOXML ──────────────────────────────────────────────────────
$parts = @{
	'[Content_Types].xml' = '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml"  ContentType="application/xml"/>
  <Override PartName="/xl/workbook.xml"            ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
  <Override PartName="/xl/worksheets/sheet1.xml"   ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
</Types>'

	'_rels/.rels' = '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
</Relationships>'

	'xl/workbook.xml' = '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
		  xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <sheets>
	<sheet name="Empleados" sheetId="1" r:id="rId1"/>
  </sheets>
</workbook>'

	'xl/_rels/workbook.xml.rels' = '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
</Relationships>'

	'xl/worksheets/sheet1.xml' = $sheetXml
}

# ── Escribir ZIP ──────────────────────────────────────────────────────────────
$memStream = New-Object System.IO.MemoryStream
$zip = New-Object System.IO.Compression.ZipArchive($memStream, [System.IO.Compression.ZipArchiveMode]::Create, $true)

foreach ($entry in $parts.GetEnumerator()) {
	$zipEntry = $zip.CreateEntry($entry.Key, [System.IO.Compression.CompressionLevel]::Optimal)
	$writer   = New-Object System.IO.StreamWriter($zipEntry.Open(), [System.Text.Encoding]::UTF8)
	$writer.Write($entry.Value)
	$writer.Flush()
	$writer.Close()
}

$zip.Dispose()

[System.IO.File]::WriteAllBytes($outPath, $memStream.ToArray())
$memStream.Dispose()

Write-Host "Archivo generado: $outPath" -ForegroundColor Green
Write-Host "Filas de datos: $($filas.Count - 1) (mas encabezado)" -ForegroundColor Cyan
