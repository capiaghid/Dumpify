using Spectre.Console;

namespace Dumpify.Renderers.Spectre.Console;

internal static class SpectreConsoleExtensions
{
    public static TableColumn ZeroPadding(this TableColumn column)
    {
        column.Padding = new Padding(0);
        return column;
    }
}
