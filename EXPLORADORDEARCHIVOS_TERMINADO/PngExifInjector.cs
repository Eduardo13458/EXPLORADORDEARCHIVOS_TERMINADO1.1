using System;
using System.IO;
using System.Text;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    public static class PngExifInjector
    {
        public static byte[] AddExifChunkToPng(byte[] pngBytes, double latitude, double longitude)
        {
            // 1. Crear el payload EXIF estructurado exactamente bajo el estándar TIFF Little-Endian
            byte[] exifPayload = BuildLittleEndianExifPayload(latitude, longitude);

            // 2. Inyectar el chunk 'eXIf' oficial en la estructura binaria del PNG antes del IEND
            return InsertExifChunkBeforeIend(pngBytes, exifPayload);
        }

        private static byte[] BuildLittleEndianExifPayload(double latitude, double longitude)
        {
            using var ms = new MemoryStream();

            // Nota: El estándar del chunk 'eXIf' en PNG (especificación oficial) dicta 
            // que NO lleva el prefijo "Exif\0\0" dentro de los bytes del payload del chunk, 
            // sino que empieza directamente con el encabezado TIFF.

            long tiffStart = ms.Position;

            // Encabezado TIFF: Little-Endian ("II")
            ms.Write(new byte[] { 0x49, 0x49 }, 0, 2); // "II"
            WriteLittleEndian(ms, (ushort)42);         // Número mágico TIFF
            WriteLittleEndian(ms, (uint)8);            // Offset a IFD0 (justo después, byte 8)

            // --- IFD0 ---
            WriteLittleEndian(ms, (ushort)1);          // 1 entrada en IFD0

            // Entrada: Puntero a GPS IFD
            WriteLittleEndian(ms, (ushort)0x8825);     // Tag: GPS Info
            WriteLittleEndian(ms, (ushort)4);          // Tipo: LONG
            WriteLittleEndian(ms, (uint)1);            // Cantidad: 1
            // Offset a la sección GPS: 
            // 8 bytes (Header) + 2 bytes (Count) + 12 bytes (Esta etiqueta) + 4 bytes (Next IFD pointer) = 26
            WriteLittleEndian(ms, (uint)26);

            WriteLittleEndian(ms, (uint)0);            // Próximo IFD (0 = Ninguno)

            // --- GPS SUB-IFD ---
            string latRef = latitude >= 0 ? "N" : "S";
            string lonRef = longitude >= 0 ? "E" : "W";

            // Obtener la data en formato de racionales (Grados, Minutos, Segundos)
            byte[] latData = ConvertToLittleEndianRational(Math.Abs(latitude));
            byte[] lonData = ConvertToLittleEndianRational(Math.Abs(longitude));

            // Offset de los datos crudos (RATIONALs):
            // Comienza después de las 5 etiquetas de la sección GPS.
            // 26 (Inicio GPS) + 2 (Count GPS) + (5 etiquetas * 12 bytes) + 4 (Next IFD pointer) = 92
            uint rawDataOffset = 92;

            WriteLittleEndian(ms, (ushort)5);          // 5 etiquetas GPS

            // Tag 0x0000: GPS Version ID
            WriteLittleEndian(ms, (ushort)0x0000);
            WriteLittleEndian(ms, (ushort)1);          // BYTE
            WriteLittleEndian(ms, (uint)4);            // 4 bytes
            ms.Write(new byte[] { 2, 2, 0, 0 }, 0, 4); // Data en línea

            // Tag 0x0001: GPS Latitude Ref
            WriteLittleEndian(ms, (ushort)0x0001);
            WriteLittleEndian(ms, (ushort)2);          // ASCII
            WriteLittleEndian(ms, (uint)2);            // 'N' o 'S' + '\0'
            ms.Write(Encoding.ASCII.GetBytes(latRef + "\0"), 0, 2);
            WriteLittleEndian(ms, (ushort)0);          // Padding

            // Tag 0x0002: GPS Latitude
            WriteLittleEndian(ms, (ushort)0x0002);
            WriteLittleEndian(ms, (ushort)5);          // RATIONAL
            WriteLittleEndian(ms, (uint)3);            // 3 componentes
            WriteLittleEndian(ms, rawDataOffset);

            // Tag 0x0003: GPS Longitude Ref
            WriteLittleEndian(ms, (ushort)0x0003);
            WriteLittleEndian(ms, (ushort)2);          // ASCII
            WriteLittleEndian(ms, (uint)2);            // 'E' o 'W' + '\0'
            ms.Write(Encoding.ASCII.GetBytes(lonRef + "\0"), 0, 2);
            WriteLittleEndian(ms, (ushort)0);          // Padding

            // Tag 0x0004: GPS Longitude
            WriteLittleEndian(ms, (ushort)0x0004);
            WriteLittleEndian(ms, (ushort)5);          // RATIONAL
            WriteLittleEndian(ms, (uint)3);            // 3 componentes
            WriteLittleEndian(ms, rawDataOffset + 24); // Desplazado por el tamaño de la latitud (24 bytes)

            WriteLittleEndian(ms, (uint)0);            // Próximo IFD en GPS

            // Escribir los bloques de datos de las coordenadas (6 enteros c/u)
            ms.Write(latData, 0, latData.Length);
            ms.Write(lonData, 0, lonData.Length);

            return ms.ToArray();
        }

        private static byte[] ConvertToLittleEndianRational(double coordinate)
        {
            byte[] result = new byte[24];

            int degrees = (int)coordinate;
            double remainder = (coordinate - degrees) * 60;
            int minutes = (int)remainder;
            double seconds = (remainder - minutes) * 60;
            uint secondsCalculated = (uint)Math.Round(seconds * 1000);

            // Guardar usando BitConverter nativo de Windows (Little Endian)
            BitConverter.GetBytes((uint)degrees).CopyTo(result, 0);
            BitConverter.GetBytes((uint)1).CopyTo(result, 4);

            BitConverter.GetBytes((uint)minutes).CopyTo(result, 8);
            BitConverter.GetBytes((uint)1).CopyTo(result, 12);

            BitConverter.GetBytes(secondsCalculated).CopyTo(result, 16);
            BitConverter.GetBytes((uint)1000).CopyTo(result, 20);

            return result;
        }

        private static byte[] InsertExifChunkBeforeIend(byte[] pngBytes, byte[] exifData)
        {
            using var ms = new MemoryStream();
            ms.Write(pngBytes, 0, 8); // Firma PNG de 8 bytes

            int pos = 8;
            int iendPosition = -1;

            while (pos < pngBytes.Length)
            {
                if (pos + 8 > pngBytes.Length) break;

                uint length = ReadBigEndianUint(pngBytes, pos);
                if (pos + 8 + length + 4 > pngBytes.Length) break;

                string chunkType = Encoding.ASCII.GetString(pngBytes, pos + 4, 4);
                if (chunkType == "IEND")
                {
                    iendPosition = pos;
                    break;
                }
                pos += (int)(4 + 4 + length + 4);
            }

            if (iendPosition == -1)
                iendPosition = pngBytes.Length - 12;

            // Escribir todo el archivo original omitiendo el cierre
            ms.Write(pngBytes, 8, iendPosition - 8);

            // IMPORTANTE: Escribir el chunk con el nombre oficial minúsculas/mayúsculas exactas "eXIf"
            WritePngChunk(ms, "eXIf", exifData);

            // Adjuntar el IEND de cierre original
            ms.Write(pngBytes, iendPosition, pngBytes.Length - iendPosition);

            return ms.ToArray();
        }

        private static void WritePngChunk(MemoryStream ms, string type, byte[] data)
        {
            // La longitud del chunk se escribe en Big-Endian según especificación PNG
            WriteBigEndian(ms, (uint)data.Length);

            byte[] typeBytes = Encoding.ASCII.GetBytes(type);
            ms.Write(typeBytes, 0, 4);
            ms.Write(data, 0, data.Length);

            // Calcular CRC sobre el Tipo + Datos
            byte[] crcBuffer = new byte[4 + data.Length];
            Array.Copy(typeBytes, 0, crcBuffer, 0, 4);
            Array.Copy(data, 0, crcBuffer, 4, data.Length);

            uint crc = CalculateCrc(crcBuffer);
            WriteBigEndian(ms, crc);
        }

        private static uint CalculateCrc(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            uint[] crcTable = GenerateCrcTable();
            foreach (byte b in data)
            {
                crc = crcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
            }
            return crc ^ 0xFFFFFFFF;
        }

        private static uint[] GenerateCrcTable()
        {
            uint[] table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint c = i;
                for (int k = 0; k < 8; k++)
                {
                    c = (c & 1) == 1 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
                }
                table[i] = c;
            }
            return table;
        }

        private static void WriteLittleEndian(MemoryStream ms, ushort value)
        {
            ms.WriteByte((byte)(value & 0xFF));
            ms.WriteByte((byte)((value >> 8) & 0xFF));
        }

        private static void WriteLittleEndian(MemoryStream ms, uint value)
        {
            ms.WriteByte((byte)(value & 0xFF));
            ms.WriteByte((byte)((value >> 8) & 0xFF));
            ms.WriteByte((byte)((value >> 16) & 0xFF));
            ms.WriteByte((byte)((value >> 24) & 0xFF));
        }

        private static void WriteBigEndian(MemoryStream ms, uint value)
        {
            ms.WriteByte((byte)((value >> 24) & 0xFF));
            ms.WriteByte((byte)((value >> 16) & 0xFF));
            ms.WriteByte((byte)((value >> 8) & 0xFF));
            ms.WriteByte((byte)(value & 0xFF));
        }

        private static uint ReadBigEndianUint(byte[] data, int offset)
        {
            return ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) | ((uint)data[offset + 2] << 8) | data[offset + 3];
        }
    }
}