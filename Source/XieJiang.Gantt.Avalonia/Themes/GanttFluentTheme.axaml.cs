using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using XieJiang.Gantt.Avalonia.Locale;

namespace XieJiang.Gantt.Avalonia.Themes;

public class GanttFluentTheme : Styles
{
    public static ThemeVariant Aquatic => new ThemeVariant(nameof(Aquatic), ThemeVariant.Dark);
    public static ThemeVariant Desert => new ThemeVariant(nameof(Desert), ThemeVariant.Light);
    public static ThemeVariant Dust => new ThemeVariant(nameof(Dust), ThemeVariant.Dark);
    public static ThemeVariant NightSky => new ThemeVariant(nameof(NightSky), ThemeVariant.Dark);

    private static readonly Dictionary<CultureInfo, ResourceDictionary> LocaleToResource = new()
                                                                                            {
                                                                                                { new CultureInfo("zh-cn"), new zh_cn() },
                                                                                                { new CultureInfo("en-us"), new en_us() },
                                                                                                { new CultureInfo("ja-jp"), new ja_jp() },
                                                                                                { new CultureInfo("uk-uk"), new uk_uk() },
                                                                                                { new CultureInfo("ru-ru"), new ru_ru() },
                                                                                            };

    private readonly IServiceProvider? _sp;

    public GanttFluentTheme(IServiceProvider? provider = null)
    {
        _sp = provider;
        AvaloniaXamlLoader.Load(provider, this);
    }

    private CultureInfo? _locale;

    public CultureInfo? Locale
    {
        get => _locale;
        set
        {
            _locale = value;
            var resource = TryGetLocaleResource(value);
            if (resource is null) return;
            foreach (var kv in resource)
            {
                this.Resources.Add(kv);
            }
        }
    }

    private static ResourceDictionary? TryGetLocaleResource(CultureInfo? locale)
    {
        if (locale is null)
        {
            return LocaleToResource[new CultureInfo("zh-cn")];
        }

        if (LocaleToResource.TryGetValue(locale, out var resource))
        {
            return resource;
        }

        return LocaleToResource[new CultureInfo("zh-cn")];
    }
}