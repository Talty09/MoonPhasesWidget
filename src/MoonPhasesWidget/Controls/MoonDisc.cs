using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MoonPhasesWidget.Controls;

/// <summary>
/// Draws the moon from a full-moon photograph, lit for a given illuminated fraction.
/// The night side shows a faint copy of the photo, like earthshine. The lit side is on
/// the right while waxing, as seen from the northern hemisphere.
/// </summary>
public class MoonDisc : FrameworkElement
{
    public static readonly DependencyProperty IlluminatedFractionProperty = DependencyProperty.Register(
        nameof(IlluminatedFraction),
        typeof(double),
        typeof(MoonDisc),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IsWaxingProperty = DependencyProperty.Register(
        nameof(IsWaxing),
        typeof(bool),
        typeof(MoonDisc),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    private const int MaskSize = 256;

    // Width of the terminator's soft edge, as a fraction of the moon's radius: the dark
    // side fades in over a short distance and the lit side brightens over a longer one.
    private const double TwilightDarkSide = 0.03;
    private const double TwilightLitSide = 0.08;

    // NASA SVS "Moon Phase and Libration, 2025", frame 311 (full moon), cropped to the disk.
    private static readonly Lazy<BitmapImage> MoonImage = new(() =>
    {
        var image = new BitmapImage(new Uri("pack://application:,,,/Assets/moon.jpg"));
        image.Freeze();
        return image;
    });

    private static readonly Brush NightBrush = Freeze(new SolidColorBrush(Color.FromArgb(0xD9, 0x0B, 0x0E, 0x16)));

    public double IlluminatedFraction
    {
        get => (double)GetValue(IlluminatedFractionProperty);
        set => SetValue(IlluminatedFractionProperty, value);
    }

    public bool IsWaxing
    {
        get => (bool)GetValue(IsWaxingProperty);
        set => SetValue(IsWaxingProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        double radius = Math.Min(ActualWidth, ActualHeight) / 2;
        if (radius <= 0)
        {
            return;
        }

        var center = new Point(ActualWidth / 2, ActualHeight / 2);
        var bounds = new Rect(center.X - radius, center.Y - radius, radius * 2, radius * 2);

        drawingContext.PushClip(new EllipseGeometry(center, radius, radius));

        // Night side: the photo, mostly darkened.
        drawingContext.DrawImage(MoonImage.Value, bounds);
        drawingContext.DrawRectangle(NightBrush, null, bounds);

        // Day side: the photo at full brightness wherever the lighting mask allows.
        drawingContext.PushOpacityMask(new ImageBrush(CreateLightingMask(IlluminatedFraction, IsWaxing)));
        drawingContext.DrawImage(MoonImage.Value, bounds);
        drawingContext.Pop();

        drawingContext.Pop();
    }

    /// <summary>
    /// Builds an alpha mask that is opaque where the sun lights the visible hemisphere.
    /// Each pixel is treated as a point on a unit sphere facing the viewer (+z). The sun
    /// sits at phase angle θ from the far side, where cos θ = 1 − 2·fraction, so
    /// θ = 0 is new moon and θ = 180° is full. Softness is measured in screen space
    /// (distance to the terminator), so the edge looks equally soft across the disk.
    /// </summary>
    private static BitmapSource CreateLightingMask(double fraction, bool isWaxing)
    {
        double cosTheta = 1 - 2 * Math.Clamp(fraction, 0, 1);
        double sinTheta = Math.Sqrt(1 - cosTheta * cosTheta) * (isWaxing ? 1 : -1);
        double sunX = sinTheta;
        double sunZ = -cosTheta;

        var pixels = new byte[MaskSize * MaskSize * 4];
        for (int row = 0; row < MaskSize; row++)
        {
            double y = (row + 0.5) / MaskSize * 2 - 1;
            for (int col = 0; col < MaskSize; col++)
            {
                double x = (col + 0.5) / MaskSize * 2 - 1;
                double r2 = x * x + y * y;
                if (r2 >= 1)
                {
                    continue;
                }

                double z = Math.Max(Math.Sqrt(1 - r2), 1e-4);
                double lighting = x * sunX + z * sunZ;

                // Divide by the screen-space gradient to approximate distance to the terminator.
                double gradX = sunX - sunZ * x / z;
                double gradY = -sunZ * y / z;
                double gradient = Math.Max(Math.Sqrt(gradX * gradX + gradY * gradY), 1e-6);
                double distance = lighting / gradient;

                byte alpha = (byte)Math.Round(255 * SmoothStep(-TwilightDarkSide, TwilightLitSide, distance));

                // Premultiplied white: every channel equals alpha.
                int i = (row * MaskSize + col) * 4;
                pixels[i] = pixels[i + 1] = pixels[i + 2] = pixels[i + 3] = alpha;
            }
        }

        var mask = BitmapSource.Create(MaskSize, MaskSize, 96, 96, PixelFormats.Pbgra32, null, pixels, MaskSize * 4);
        mask.Freeze();
        return mask;
    }

    private static double SmoothStep(double edge0, double edge1, double value)
    {
        double t = Math.Clamp((value - edge0) / (edge1 - edge0), 0, 1);
        return t * t * (3 - 2 * t);
    }

    private static Brush Freeze(Brush brush)
    {
        brush.Freeze();
        return brush;
    }
}
