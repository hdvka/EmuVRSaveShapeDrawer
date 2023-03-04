using System.Drawing;

namespace EmuVRShapeDrawer;

public class Palette
{
    public readonly Dictionary<int, Color> PaletteIndexes;

    public Palette()
    {
        PaletteIndexes = new Dictionary<int, Color>();

        var diskImageBasePath = @"C:\EmuVR\Custom\Labels\PlayStation\palette";
        var discExtension = ".png";

        for (int i = 0; i <= 13; i++)
        {
            var paletteColor = new Bitmap(diskImageBasePath + i.ToString() + discExtension).GetPixel(0, 0);
            PaletteIndexes.Add(i, paletteColor);
        }
    }

    public int GetPaletteIndexHSV(Color color)
    {
        Color closestColor = Color.Empty;
        double minDistance = double.MaxValue;

        // Convert the color to HSV color space
        double h1, s1, v1;
        ColorToHSV(color, out h1, out s1, out v1);

        // Loop through each color in the list
        foreach (Color paletteColor in PaletteIndexes.Select(paletteEntry => paletteEntry.Value))
        {
            // Convert the palette color to HSV color space
            double h2, s2, v2;
            ColorToHSV(paletteColor, out h2, out s2, out v2);

            // Calculate the distance between the two colors
            double distance = Math.Sqrt(Math.Pow(h1 - h2, 2) + Math.Pow(s1 - s2, 2) + Math.Pow((v1 - v2) * 0.5, 2));

            // If the distance is smaller than the current minimum distance, update the closest color
            if (distance < minDistance)
            {
                minDistance = distance;
                closestColor = paletteColor;
            }
        }

        return PaletteIndexes
            .Where(paletteEntry => paletteEntry.Value == closestColor)
            .Select(p => p.Key)
            .First();
    }

    // Helper method to convert a color to HSV color space
    private void ColorToHSV(Color color, out double h, out double s, out double v)
    {
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;
        double cmax = Math.Max(r, Math.Max(g, b));
        double cmin = Math.Min(r, Math.Min(g, b));
        double delta = cmax - cmin;
        if (delta == 0)
        {
            h = 0;
        }
        else if (cmax == r)
        {
            h = ((g - b) / delta) % 6;
        }
        else if (cmax == g)
        {
            h = ((b - r) / delta) + 2;
        }
        else
        {
            h = ((r - g) / delta) + 4;
        }
        h = Math.Round(h * 60);
        if (h < 0)
        {
            h += 360;
        }
        if (cmax == 0)
        {
            s = 0;
        }
        else
        {
            s = delta / cmax;
        }
        v = cmax;
    }

    public int GetPaletteIndexEuclidean(Color color)
    {
        Color closestColor = Color.Empty;
        double minDistance = double.MaxValue;

        double r1 = color.R;
        double g1 = color.G;
        double b1 = color.B;

        // Loop through each color in the list
        foreach (Color paletteColor in PaletteIndexes.Select(paletteEntry => paletteEntry.Value))
        {
            double r2 = paletteColor.R;
            double g2 = paletteColor.G;
            double b2 = paletteColor.B;

            // Calculate the Euclidean distance between the normalized colors
            double distance = Math.Sqrt(Math.Pow(r1 - r2, 2) + Math.Pow(g1 - g2, 2) + Math.Pow(b1 - b2, 2));
            
            
            // If the distance is smaller than the current minimum distance, update the closest color
            if (distance < minDistance)
            {
                minDistance = distance;
                closestColor = paletteColor;
            }
        }

        return PaletteIndexes
            .Where(paletteEntry => paletteEntry.Value == closestColor)
            .Select(p => p.Key)
            .First();
    }
}
