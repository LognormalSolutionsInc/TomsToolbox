﻿namespace TomsToolbox.Wpf.Tests.Converters;

using System.Windows;
using System.Windows.Data;

using Xunit;

using TomsToolbox.Wpf.Converters;

public class BooleanToVisibilityConverterTests
{
    private static readonly IValueConverter _target = BooleanToVisibilityConverter.Default;
    private static readonly IValueConverter _reference = new System.Windows.Controls.BooleanToVisibilityConverter();

    [Fact]
    public void BooleanToVisibilityConverter_EnsureHasSameBehaviorAsSystemConverter_Test()
    {
        VerifyConvert(true);
        VerifyConvert("true");
        VerifyConvert(false);
        VerifyConvert(Visibility.Visible);
        VerifyConvert(Visibility.Hidden);
        VerifyConvert(Visibility.Collapsed);
        VerifyConvert(null);
        VerifyConvert(DependencyProperty.UnsetValue);
    }

    private static void VerifyConvert(object? source)
    {
        var expected = _reference.Convert(source, null, null, null);
        var result = _target.Convert(source, null, null, null);

        Assert.Equal(expected, result);

        expected = _reference.ConvertBack(source, null, null, null);
        result = _target.ConvertBack(source, null, null, null);

        Assert.Equal(expected, result);
    }
}