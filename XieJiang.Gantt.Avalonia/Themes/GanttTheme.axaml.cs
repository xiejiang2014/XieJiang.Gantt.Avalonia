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
    // Factories rather than shared instances: a ResourceDictionary can only be merged into one owner.
    private static readonly Dictionary<CultureInfo, Func<ResourceDictionary>> LocaleToResource = new()
                                                                                                  {
                                                                                                      { new CultureInfo("zh-cn"), () => new zh_cn() },
                                                                                                      { new CultureInfo("en-us"), () => new en_us() },
                                                                                                      { new CultureInfo("ja-jp"), () => new ja_jp() },
                                                                                                      { new CultureInfo("uk-uk"), () => new uk_uk() },
                                                                                                      { new CultureInfo("ru-ru"), () => new ru_ru() },
                                                                                                  };

    private readonly IServiceProvider? _sp;

    public GanttTheme(IServiceProvider? provider = null)
    {
        _sp = provider;
        AvaloniaXamlLoader.Load(provider, this);
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

    private static ResourceDictionary TryGetLocaleResource(CultureInfo? locale)
    {
        if (locale is not null && LocaleToResource.TryGetValue(locale, out var factory))
        {
            return factory();
        }

        return LocaleToResource[new CultureInfo("zh-cn")]();
    }
}