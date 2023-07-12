﻿using Dumpify.Renderers;
using Dumpify.Renderers.Spectre.Console.TableRenderer;

namespace Dumpify.Config;

public static class Renderers
{
    public static IRenderer Table { get; } = new SpectreConsoleTableRenderer();

    //Still work in progress
    //public static IRenderer Text { get; } = new SpectreConsoleTextRenderer();
}
