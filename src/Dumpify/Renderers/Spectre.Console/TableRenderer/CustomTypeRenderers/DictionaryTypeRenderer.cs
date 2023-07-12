﻿using System.Collections;
using System.Reflection;

using Dumpify.Config;
using Dumpify.Descriptors;
using Dumpify.Extensions;
using Dumpify.Renderers.Spectre.Console.Builder;

using Spectre.Console.Rendering;

namespace Dumpify.Renderers.Spectre.Console.TableRenderer.CustomTypeRenderers;

internal class DictionaryTypeRenderer : ICustomTypeRenderer<IRenderable>
{
    private readonly IRendererHandler<IRenderable, SpectreRendererState> _handler;

    public Type DescriptorType { get; } = typeof(MultiValueDescriptor);

    public DictionaryTypeRenderer(IRendererHandler<IRenderable, SpectreRendererState> handler)
    {
        _handler = handler;
    }

    public IRenderable Render(IDescriptor descriptor, object obj, RenderContext baseContext, object? handleContext)
    {
        var context = (RenderContext<SpectreRendererState>)baseContext;

        handleContext.MustNotBeNull();

        var tableBuilder = new ObjectTableBuilder(context, descriptor, obj);
        tableBuilder.SetColumnNames("Key", "Value");

        Type? valueType = null;

        var memberProvider = context.Config.MemberProvider;

        foreach (var pair in ((IEnumerable<(object? key, object? value)>)handleContext!))
        {
            var keyType = pair.key?.GetType();
            var keyDescriptor = keyType is null ? null : DumpConfig.Default.Generator.Generate(keyType, null, context.Config.MemberProvider);
            var keyRenderable = _handler.RenderDescriptor(pair.key, keyDescriptor, context);

            var value = pair.value;

            if (value is not null && valueType is null)
            {
                valueType = value.GetType();
            }

            (IDescriptor? valueDescritor, IRenderable renderedValue) = value switch
            {
                null => (null, _handler.RenderNullValue(null, context)),
                not null => GetDescriptorAndRender(value, context),
            };

            tableBuilder.AddRow(valueDescritor, value, keyRenderable, renderedValue);
        }

        return tableBuilder.Build();
    }

    private (IDescriptor? descriptor, IRenderable renderedValue) GetDescriptorAndRender(object value, RenderContext<SpectreRendererState> context)
    {
        var descriptor = DumpConfig.Default.Generator.Generate(value.GetType(), null, context.Config.MemberProvider);
        var rendered = _handler.RenderDescriptor(value, descriptor, context);

        return (descriptor, rendered);
    }

    private IEnumerable<(object? key, object? value)> GetPairs(IDescriptor descriptor, object obj)
    {
        if (obj is IDictionary nonGenericDictionary)
        {
            foreach (var key in nonGenericDictionary.Keys)
            {
                yield return (key, nonGenericDictionary[key]);
            }

            yield break;
        }

        foreach (var i in descriptor.Type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
        {
            var genericArgument = i.GetGenericArguments()[0];

            if (genericArgument.IsGenericType)
            {
                if (genericArgument.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    var method = i.GetMethod("GetEnumerator", BindingFlags.Instance | BindingFlags.Public)!;
                    var items = (IEnumerable)method.Invoke(obj, null)!;

                    foreach (var item in items)
                    {
                        var itemType = item.GetType();

                        var keyProperty = itemType.GetProperty("Key", BindingFlags.Instance | BindingFlags.Public)!;
                        var key = keyProperty.GetValue(item);

                        var valueProperty = itemType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)!;
                        var value = valueProperty.GetValue(item);

                        yield return (key, value);
                    }
                }
            }
        }
    }

    public (bool, object?) ShouldHandle(IDescriptor descriptor, object obj)
    {
        if (obj is IDictionary map)
        {
            var list = new List<(object? key, object? value)>(map.Count);
            foreach (var key in map.Keys)
            {
                list.Add((key, map[key]));
            }

            return (true, list);
        }

        foreach (var i in descriptor.Type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
        {
            var genericArgument = i.GetGenericArguments()[0];

            if (genericArgument.IsGenericType)
            {
                if (genericArgument.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    var method = i.GetMethod("GetEnumerator", BindingFlags.Instance | BindingFlags.Public)!;
                    var items = (IEnumerable)method.Invoke(obj, null)!;

                    var list = new List<(object? key, object? value)>();

                    foreach (var item in items)
                    {
                        var itemType = item.GetType();

                        var keyProperty = itemType.GetProperty("Key", BindingFlags.Instance | BindingFlags.Public)!;
                        var key = keyProperty.GetValue(item);

                        var valueProperty = itemType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)!;
                        var value = valueProperty.GetValue(item);

                        list.Add((key, value));
                    }

                    return (true, list);
                }
            }
        }

        return (false, null);
    }
}