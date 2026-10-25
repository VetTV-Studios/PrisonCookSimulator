using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace PrisonCookSimulator;

/// <summary>Builds icon + label content from Assets/Art/*.png (file URIs work packaged and unpackaged).</summary>
public static class Art
{
    public static Uri FileUri(string file) =>
        new(System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "Art", file + ".png"));

    public static Image Icon(string file, double size) => new()
    {
        Source = new BitmapImage(FileUri(file)),
        Width = size,
        Height = size,
    };

    public static StackPanel Labeled(string file, string text, double size, Orientation orientation = Orientation.Horizontal)
    {
        var sp = new StackPanel { Orientation = orientation, Spacing = 8, HorizontalAlignment = HorizontalAlignment.Center };
        sp.Children.Add(Icon(file, size));
        sp.Children.Add(new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center });
        return sp;
    }

    public static string ForStation(Station st) => "station_" + st.ToString().ToLowerInvariant();
    public static string ForRecipe(Recipe r) => "recipe_" + r.Id;
    public static string ForIngredient(Ingredient i) => "ing_" + i.Id;
}
