using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // ?? Aseguramos el soporte para FirstOrDefault()
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TagLib;
using HtmlAgilityPack; // <-- Agrega esto arriba del todo junto a tus otros using

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    public class MusicMetadataFetcher
    {
        private readonly string _geniusAccessToken;
        private readonly string _spotifyClientId;
        private readonly string _spotifyClientSecret;
        private string _spotifyAccessToken;
        private DateTime _spotifyTokenExpiry;

        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly Dictionary<string, string> _coverCache = new Dictionary<string, string>();
        public static string UltimaLetraDescargada { get; private set; }
        public static byte[] UltimaPortadaDescargada { get; private set; }

        public MusicMetadataFetcher(string geniusAccessToken, string spotifyClientId, string spotifyClientSecret)
        {
            _geniusAccessToken = geniusAccessToken ?? throw new ArgumentNullException(nameof(geniusAccessToken));
            _spotifyClientId = spotifyClientId ?? throw new ArgumentNullException(nameof(spotifyClientId));
            _spotifyClientSecret = spotifyClientSecret ?? throw new ArgumentNullException(nameof(spotifyClientSecret));
        }

        /// <summary>
        /// Obtiene un token de acceso de Spotify (con caché de 1 hora)
        /// </summary>
        private async Task<string> ObtenerTokenSpotifyAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(_spotifyAccessToken) && DateTime.UtcNow < _spotifyTokenExpiry)
                {
                    return _spotifyAccessToken;
                }

                System.Diagnostics.Debug.WriteLine($"?? Obteniendo nuevo token de Spotify...");

                var authString = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes($"{_spotifyClientId}:{_spotifyClientSecret}"));

                var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
                request.Headers.Add("Authorization", $"Basic {authString}");
                request.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"? Error obteniendo token Spotify: {response.StatusCode}");
                    return null;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();

                var tokenMatch = Regex.Match(jsonResponse, @"""access_token""\s*:\s*""([^""]+)""");
                var expiryMatch = Regex.Match(jsonResponse, @"""expires_in""\s*:\s*(\d+)");

                if (tokenMatch.Success && expiryMatch.Success)
                {
                    _spotifyAccessToken = tokenMatch.Groups[1].Value;
                    int expiresIn = int.Parse(expiryMatch.Groups[1].Value);
                    _spotifyTokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60);

                    System.Diagnostics.Debug.WriteLine($"? Token Spotify obtenido (válido por {expiresIn}s)");
                    return _spotifyAccessToken;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error obteniendo token Spotify: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extrae metadatos del archivo MP3 con validaciones y corrección inteligente de títulos
        /// </summary>
        public static (string artista, string titulo, string album) ExtraerMetadatos(string rutaArchivo)
        {
            try
            {
                using (var file = TagLib.File.Create(rutaArchivo))
                {
                    string artista = file.Tag.FirstPerformer
                        ?? file.Tag.AlbumArtists.FirstOrDefault()
                        ?? "Desconocido";

                    string titulo = !string.IsNullOrWhiteSpace(file.Tag.Title)
                        ? file.Tag.Title
                        : "Desconocido";

                    string album = file.Tag.Album ?? "Unknown Album";

                    // ??? VALIDACIÓN INTELIGENTE: Si el título no existe, repite el artista, o contiene la extensión
                    if (titulo == "Desconocido" || titulo.Contains("-") || titulo.ToLower().EndsWith(".mp3"))
                    {
                        // Obtenemos el nombre del archivo sin el ".mp3" (ej: "Joji - SLOW DANCING IN THE DARK")
                        string nombreArchivoLimpio = Path.GetFileNameWithoutExtension(rutaArchivo);

                        if (nombreArchivoLimpio.Contains("-"))
                        {
                            var partes = nombreArchivoLimpio.Split('-');

                            // Tomamos la segunda parte (el título real) eliminando los espacios de los lados
                            titulo = partes[1].Trim(); // "SLOW DANCING IN THE DARK"

                            // Si el tag del artista original falló, usamos la primera parte del archivo como respaldo
                            if (artista == "Desconocido")
                            {
                                artista = partes[0].Trim(); // "Joji"
                            }
                        }
                        else
                        {
                            // Si no tiene guion pero venía roto el tag, usamos el nombre limpio del archivo como título
                            titulo = nombreArchivoLimpio;
                        }
                    }

                    // Conservamos tus filtros de limpieza originales
                    artista = LimpiarTexto(artista);
                    titulo = LimpiarTexto(titulo);
                    album = LimpiarTexto(album);

                    return (artista, titulo, album);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al extraer metadatos: {ex.Message}");
                return ("Desconocido", "Desconocido", "Unknown Album");
            }
        }

        private static string LimpiarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "Desconocido";

            texto = Regex.Replace(texto, @"[^\w\s\-\.]", "");
            texto = Regex.Replace(texto, @"\s+", " ");
            return texto.Trim().Substring(0, Math.Min(200, texto.Trim().Length));
        }

        /// <summary>
        /// Obtiene la letra desde Genius usando su API REST
        /// </summary>
        /// <summary>
        /// Obtiene la letra desde Genius usando su API REST con autenticación segura en Header
        /// </summary>
        public async Task<string> ObtenerLetraAsync(string artista, string titulo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(artista) || string.IsNullOrWhiteSpace(titulo))
                    return null;

                if (artista == "Desconocido" || titulo == "Desconocido")
                    return null;

                string query = $"{artista} {titulo}";
                // ?? Limpiamos la URL dejando solo la consulta
                string url = $"https://api.genius.com/search?q={Uri.EscapeDataString(query)}";

                System.Diagnostics.Debug.WriteLine($"?? Buscando letra en Genius: {query}");

                var request = new HttpRequestMessage(HttpMethod.Get, url);

                // ??? SOLUCIÓN AL UNAUTHORIZED: Enviamos el token estructurado en el Header Authorization
                request.Headers.Add("Authorization", $"Bearer {_geniusAccessToken.Trim()}");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    // Te avisará con lujo de detalle en la consola si sigue rebotando el token
                    System.Diagnostics.Debug.WriteLine($"? Genius API error: {response.StatusCode} ({response.ReasonPhrase})");
                    return null;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();

                var urlMatch = Regex.Match(jsonResponse, @"""url""\s*:\s*""([^""]+)""");
                if (!urlMatch.Success)
                {
                    System.Diagnostics.Debug.WriteLine($"? No se encontró canción en Genius");
                    return null;
                }

                string cancionUrl = urlMatch.Groups[1].Value;
                System.Diagnostics.Debug.WriteLine($"? Canción encontrada: {cancionUrl}");

                return await ExtraerLetraDesdeUrl(cancionUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error al obtener letra: {ex.Message}");
                return null;
            }
        }
        private async Task<string> ExtraerLetraDesdeUrl(string url)
        {
            try
            {
                // Usamos HtmlAgilityPack en lugar del HttpClient manual + Regex viejo
                var web = new HtmlAgilityPack.HtmlWeb();
                var doc = await web.LoadFromWebAsync(url);

                // Buscar todos los divs cuyo atributo 'class' contenga "Lyrics__Container" (Formato actual de Genius)
                var nodosLetra = doc.DocumentNode.SelectNodes("//div[contains(@class, 'Lyrics__Container')]");

                if (nodosLetra != null)
                {
                    StringBuilder letraCompleta = new StringBuilder();
                    foreach (var nodo in nodosLetra)
                    {
                        // Reemplazar los <br> por saltos de línea reales antes de limpiar el HTML
                        string textoLimpio = nodo.InnerHtml.Replace("<br>", "\n").Replace("<br/>", "\n");

                        // Cargar en un sub-nodo para remover etiquetas internas como <a> (links de anotaciones)
                        var subDoc = new HtmlAgilityPack.HtmlDocument();
                        subDoc.LoadHtml(textoLimpio);

                        letraCompleta.AppendLine(subDoc.DocumentNode.InnerText);
                    }

                    string letraFinal = letraCompleta.ToString().Trim();
                    System.Diagnostics.Debug.WriteLine($"? Letra encontrada ({letraFinal.Length} caracteres)");
                    return System.Net.WebUtility.HtmlDecode(letraFinal);
                }
                else
                {
                    // Plan B: Intentar por si es una canción vieja con el contenedor antiguo de Genius
                    var nodoAntiguo = doc.DocumentNode.SelectSingleNode("//div[@class='lyrics']");
                    if (nodoAntiguo != null)
                    {
                        string letraAntigua = nodoAntiguo.InnerText.Trim();
                        System.Diagnostics.Debug.WriteLine($"? Letra encontrada en contenedor antiguo ({letraAntigua.Length} caracteres)");
                        return System.Net.WebUtility.HtmlDecode(letraAntigua);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"? No se pudo extraer letra del HTML (Estructura no reconocida)");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error al extraer letra con HtmlAgilityPack: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene portada desde Spotify API o fallbacks alternativos
        /// </summary>
        public async Task<byte[]> ObtenerPortadaAsync(string artista, string album)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(artista) || string.IsNullOrWhiteSpace(album))
                    return null;

                if (artista == "Desconocido" || album == "Unknown Album")
                    return null;

                string cacheKey = $"{artista}_{album}".ToLower();

                if (_coverCache.ContainsKey(cacheKey))
                {
                    string cachedUrl = _coverCache[cacheKey];
                    if (!string.IsNullOrEmpty(cachedUrl) && Uri.IsWellFormedUriString(cachedUrl, UriKind.Absolute))
                    {
                        System.Diagnostics.Debug.WriteLine($"? Portada en caché encontrada");
                        return await DescargarImagenAsync(cachedUrl);
                    }
                    else if (!string.IsNullOrEmpty(cachedUrl))
                    {
                        System.Diagnostics.Debug.WriteLine($"?? URL en caché inválida, intentando nuevamente");
                    }
                    else
                    {
                        return null;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"?? Intentando obtener portada para: {artista} - {album}");

                byte[] resultado = await BuscarPortadaEnSpotifyAsync(artista, album);
                if (resultado != null)
                {
                    _coverCache[cacheKey] = "";
                    return resultado;
                }

                System.Diagnostics.Debug.WriteLine($"?? Spotify falló, intentando iTunes...");

                resultado = await BuscarPortadaEnITunesAsync(artista, album);
                if (resultado != null)
                {
                    return resultado;
                }

                System.Diagnostics.Debug.WriteLine($"?? iTunes falló, intentando Last.fm...");

                resultado = await BuscarPortadaEnLastFmAsync(artista, album);
                if (resultado != null)
                {
                    return resultado;
                }

                System.Diagnostics.Debug.WriteLine($"? No se encontró portada en ningún servicio");
                _coverCache[cacheKey] = "";
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error general en ObtenerPortadaAsync: {ex.Message}");
                return null;
            }
        }

        private async Task<byte[]> BuscarPortadaEnSpotifyAsync(string artista, string album)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"?? Buscando en Spotify: {artista} - {album}");

                string token = await ObtenerTokenSpotifyAsync();
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine($"? No se pudo obtener token de Spotify");
                    return null;
                }

                string query = $"{artista} {album}";
                string spotifyUrl = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=album&limit=1";

                var request = new HttpRequestMessage(HttpMethod.Get, spotifyUrl);
                request.Headers.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"? Spotify API error: {response.StatusCode}");
                    return null;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var imageMatch = Regex.Match(jsonResponse, @"""url""\s*:\s*""([^""]+\.jpg)""");

                if (imageMatch.Success)
                {
                    string imageUrl = imageMatch.Groups[1].Value;
                    System.Diagnostics.Debug.WriteLine($"? Portada encontrada en Spotify");
                    return await DescargarImagenAsync(imageUrl);
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error en Spotify: {ex.Message}");
                return null;
            }
        }

        private async Task<byte[]> BuscarPortadaEnITunesAsync(string artista, string album)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"?? Buscando en iTunes: {artista} - {album}");

                string query = $"{artista} {album}";
                string itunesUrl = $"https://itunes.apple.com/search?term={Uri.EscapeDataString(query)}&media=music&entity=album&limit=1";

                var request = new HttpRequestMessage(HttpMethod.Get, itunesUrl);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var imageMatch = Regex.Match(jsonResponse, @"""artworkUrl\d+""\s*:\s*""([^""]+)""");

                    if (imageMatch.Success)
                    {
                        string imageUrl = imageMatch.Groups[1].Value;
                        imageUrl = imageUrl.Replace("100x100", "600x600");

                        System.Diagnostics.Debug.WriteLine($"? Portada encontrada en iTunes: {imageUrl}");
                        return await DescargarImagenAsync(imageUrl);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error en iTunes: {ex.Message}");
                return null;
            }
        }

        private async Task<byte[]> BuscarPortadaEnLastFmAsync(string artista, string album)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"?? Buscando en Last.fm: {artista} - {album}");

                string lastfmUrl = $"https://www.last.fm/music/{Uri.EscapeDataString(artista)}/{Uri.EscapeDataString(album)}";

                var request = new HttpRequestMessage(HttpMethod.Get, lastfmUrl);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string html = await response.Content.ReadAsStringAsync();
                    var imageMatch = Regex.Match(html, @"<img[^>]*src=""([^""]*\.(?:jpg|png))""[^>]*class=""[^""]*cover[^""]*""");

                    if (imageMatch.Success)
                    {
                        string imageUrl = imageMatch.Groups[1].Value;
                        if (!imageUrl.StartsWith("http"))
                        {
                            imageUrl = "https:" + imageUrl;
                        }

                        System.Diagnostics.Debug.WriteLine($"? Portada encontrada en Last.fm");
                        return await DescargarImagenAsync(imageUrl);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error en Last.fm: {ex.Message}");
                return null;
            }
        }

        private async Task<byte[]> DescargarImagenAsync(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    return null;

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error descargando imagen: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Busca y actualiza metadatos con soporte de contingencia en memoria si el archivo está reproduciéndose
        /// </summary>
        public async Task<bool> ActualizarMetadatosDelArchivoAsync(string rutaArchivo)
        {
            const int maxIntentos = 3;
            const int delayMilisegundos = 300;

            try
            {
                UltimaLetraDescargada = null;
                UltimaPortadaDescargada = null;

                var (artista, titulo, album) = ExtraerMetadatos(rutaArchivo);

                string letraDescargada = await ObtenerLetraAsync(artista, titulo);
                byte[] portadaBytes = await ObtenerPortadaAsync(artista, album);

                UltimaLetraDescargada = letraDescargada;
                UltimaPortadaDescargada = portadaBytes;

                if (string.IsNullOrWhiteSpace(letraDescargada) && (portadaBytes == null || portadaBytes.Length == 0))
                {
                    return false;
                }

                for (int intento = 0; intento < maxIntentos; intento++)
                {
                    try
                    {
                        bool cambios = false;

                        using (var stream = new System.IO.FileStream(rutaArchivo, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite))
                        // ??? CORRECCIÓN AQUÍ: Instanciamos explícitamente usando la clase del mismo namespace
                        using (var file = TagLib.File.Create(new EXPLORADORDEARCHIVOS_TERMINADO.FileStreamAbstraction(rutaArchivo, stream, stream)))
                        {
                            if (string.IsNullOrWhiteSpace(file.Tag.Lyrics) && !string.IsNullOrWhiteSpace(letraDescargada))
                            {
                                file.Tag.Lyrics = letraDescargada;
                                cambios = true;
                            }

                            if (file.Tag.Pictures.Length == 0 && portadaBytes != null && portadaBytes.Length > 0)
                            {
                                TagLib.Picture picture = new TagLib.Picture
                                {
                                    Type = TagLib.PictureType.FrontCover,
                                    Description = "Portada",
                                    MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg,
                                    Data = portadaBytes
                                };
                                file.Tag.Pictures = new TagLib.IPicture[] { picture };
                                cambios = true;
                            }

                            if (cambios)
                            {
                                file.Save();
                                System.Diagnostics.Debug.WriteLine($"?? [ÉXITO] Guardado en disco.");
                                return true;
                            }
                        }
                        return false;
                    }
                    catch (IOException)
                    {
                        if (intento < maxIntentos - 1)
                        {
                            await Task.Delay(delayMilisegundos);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"?? Canción en reproducción. Los datos se mantendrán en RAM para la UI.");
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error: {ex.Message}");
            }

            return false;
        }
    }

    public class FileStreamAbstraction : TagLib.File.IFileAbstraction
    {
        public FileStreamAbstraction(string name, System.IO.Stream readStream, System.IO.Stream writeStream)
        {
            Name = name;
            ReadStream = readStream;
            WriteStream = writeStream;
        }

        public string Name { get; private set; }
        public System.IO.Stream ReadStream { get; private set; }
        public System.IO.Stream WriteStream { get; private set; }

        public void CloseStream(System.IO.Stream stream) { }
    }
}