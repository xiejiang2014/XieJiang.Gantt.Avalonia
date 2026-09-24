using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using XieJiang.Gantt.Avalonia.Locale;

namespace XieJiang.Gantt.Avalonia.Themes;

public class GanttTheme : Styles
{
    // Keyed by language so regional variants (en-GB, zh-TW, ...) match too.
    // Factories rather than shared instances: a ResourceDictionary can only be merged into one owner.
    private static readonly Dictionary<string, Func<ResourceDictionary>> LanguageToResource = new()
                                                                                             {
                                                                                                 { "zh", () => new zh_cn() },
                                                                                                 { "en", () => new en_us() },
                                                                                                 { "ja", () => new ja_jp() },
                                                                                                 { "uk", () => new uk_uk() },
                                                                                                 { "ru", () => new ru_ru() },
                                                                                             };

    private readonly IServiceProvider? _sp;

    public GanttTheme(IServiceProvider? provider = null)
    {
        _sp = provider;
        AvaloniaXamlLoader.Load(provider, this);

        // Load the default strings; setting Locale later replaces them.
        Locale = null;
    }

    private CultureInfo?        _locale;
    private ResourceDictionary? _localeResource;

    public CultureInfo? Locale
    {
        get => _locale;
        set
        {
            _locale = value;

            if (_localeResource is not null)
            {
                Resources.MergedDictionaries.Remove(_localeResource);
            }

            _localeResource = TryGetLocaleResource(value);
            Resources.MergedDictionaries.Add(_localeResource);
        }
    }

    /// <summary>
    /// Picks the strings for <paramref name="locale"/> (the current UI culture when null), falling back to English.
    /// </summary>
    private static ResourceDictionary TryGetLocaleResource(CultureInfo? locale)
    {
        locale ??= CultureInfo.CurrentUICulture;

        if (LanguageToResource.TryGetValue(locale.TwoLetterISOLanguageName, out var factory))
        {
            return factory();
        }

        return LanguageToResource["en"]();
    }
}