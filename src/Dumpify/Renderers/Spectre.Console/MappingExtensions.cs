using Dumpify.Config;

using Spectre.Console;

namespace Dumpify.Renderers.Spectre.Console;

internal static class MappingExtensions
{
    public static Color? ToSpectreColor(this DumpColor? color)
        => color is null ? null : new Color(color.Color.R, color.Color.G, color.Color.B);
}
