using Dumpify.Renderers;

namespace Dumpify.Outputs;
public interface IDumpOutput
{
    public TextWriter TextWriter { get; }

    RendererConfig AdjustConfig(in RendererConfig config);
}
