using CommunityToolkit.Mvvm.ComponentModel;

namespace NightDog.App.ViewModels;

/// <summary>
/// Ein Eintrag eines Segment-Controls (z. B. Start-Verzögerung oder Theme-Auswahl).
/// </summary>
public partial class SegmentOption<T>(string label, T value) : ObservableObject
{
	public string Label { get; } = label;

	public T Value { get; } = value;

	[ObservableProperty]
	public partial bool IsSelected { get; set; }
}

public sealed class DelayOption(string label, TimeSpan value) : SegmentOption<TimeSpan>(label, value);

public sealed class ThemeOption(string label, AppTheme value) : SegmentOption<AppTheme>(label, value);

public static class SegmentOptionExtensions
{
	public static void Select<T>(this IEnumerable<SegmentOption<T>> options, SegmentOption<T> selected)
	{
		foreach (var option in options)
			option.IsSelected = ReferenceEquals(option, selected);
	}
}
