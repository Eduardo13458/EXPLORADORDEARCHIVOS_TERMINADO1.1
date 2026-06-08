using System.Drawing;
using System.Drawing.Imaging;

namespace EXPLORADORDEARCHIVOS_TERMINADO
{
    public static class ImageProcessor
    {
        // Adjust brightness, contrast and saturation.
        // brightness: -100..100, contrast: -100..100, saturation: -100..100
        public static Bitmap Adjust(Bitmap source, int brightness, int contrast, int saturation)
        {
            var result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            var b = brightness / 100f; // -1..1
            var c = contrast / 100f; // -1..1
            var s = saturation / 100f; // -1..1

            // Build color matrix
            var cm = BuildColorMatrix(b, c, s);

            using var g = Graphics.FromImage(result);
            using var ia = new ImageAttributes();
            ia.SetColorMatrix(cm);
            g.DrawImage(source, new Rectangle(0, 0, result.Width, result.Height), 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, ia);
            return result;
        }

        private static ColorMatrix BuildColorMatrix(float brightness, float contrast, float saturation)
        {
            // Start with identity
            var cm = new ColorMatrix();

            // Contrast
            float t = (1.0f - contrast) / 2.0f;
            var contrastM = new ColorMatrix(new float[][]
            {
                new float[] {1+contrast, 0, 0, 0, 0},
                new float[] {0, 1+contrast, 0, 0, 0},
                new float[] {0, 0, 1+contrast, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {t, t, t, 0, 1}
            });

            // Brightness
            var brightnessM = new ColorMatrix(new float[][]
            {
                new float[] {1, 0, 0, 0, 0},
                new float[] {0, 1, 0, 0, 0},
                new float[] {0, 0, 1, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {brightness, brightness, brightness, 0, 1}
            });

            // Saturation
            float sat = 1 + saturation;
            float lumR = 0.3086f;
            float lumG = 0.6094f;
            float lumB = 0.0820f;
            var satM = new ColorMatrix(new float[][]
            {
                new float[] {lumR*(1-sat)+sat, lumG*(1-sat), lumB*(1-sat), 0, 0},
                new float[] {lumR*(1-sat), lumG*(1-sat)+sat, lumB*(1-sat), 0, 0},
                new float[] {lumR*(1-sat), lumG*(1-sat), lumB*(1-sat)+sat, 0, 0},
                new float[] {0,0,0,1,0},
                new float[] {0,0,0,0,1}
            });

            // Combine: contrast * saturation * brightness
            cm = Multiply(contrastM, satM);
            cm = Multiply(cm, brightnessM);
            return cm;
        }

        private static ColorMatrix Multiply(ColorMatrix a, ColorMatrix b)
        {
            var r = new ColorMatrix();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    float v = 0;
                    for (int k = 0; k < 5; k++)
                    {
                        v += a[i, k] * b[k, j];
                    }
                    r[i, j] = v;
                }
            }
            return r;
        }

        public static Bitmap ApplyGrayscale(Bitmap src)
        {
            var bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            using var ia = new ImageAttributes();
            var cm = new ColorMatrix(new float[][]
            {
                new float[] {0.299f, 0.299f, 0.299f, 0, 0},
                new float[] {0.587f, 0.587f, 0.587f, 0, 0},
                new float[] {0.114f, 0.114f, 0.114f, 0, 0},
                new float[] {0,0,0,1,0},
                new float[] {0,0,0,0,1}
            });
            ia.SetColorMatrix(cm);
            g.DrawImage(src, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, ia);
            return bmp;
        }

        public static Bitmap ApplySepia(Bitmap src)
        {
            var bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            using var ia = new ImageAttributes();
            var cm = new ColorMatrix(new float[][]
            {
                new float[] {0.393f, 0.349f, 0.272f, 0, 0},
                new float[] {0.769f, 0.686f, 0.534f, 0, 0},
                new float[] {0.189f, 0.168f, 0.131f, 0, 0},
                new float[] {0,0,0,1,0},
                new float[] {0,0,0,0,1}
            });
            ia.SetColorMatrix(cm);
            g.DrawImage(src, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, ia);
            return bmp;
        }

        public static Bitmap InvertColors(Bitmap src)
        {
            var bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            using var ia = new ImageAttributes();
            var cm = new ColorMatrix(new float[][]
            {
                new float[] {-1,0,0,0,0},
                new float[] {0,-1,0,0,0},
                new float[] {0,0,-1,0,0},
                new float[] {0,0,0,1,0},
                new float[] {1,1,1,0,1}
            });
            ia.SetColorMatrix(cm);
            g.DrawImage(src, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, ia);
            return bmp;
        }
    }
}
