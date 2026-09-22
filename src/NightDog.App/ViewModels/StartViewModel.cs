using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NightDog.App.ViewModels;

public partial class StartViewModel : ObservableObject
{
	public StartViewModel()
	{
		DelayOptions =
		[
			new("Aus", TimeSpan.Zero),
			new("15 min", TimeSpan.FromMinutes(15)),
			new("30 min", TimeSpan.FromMinutes(30)),
			new("45 min", TimeSpan.FromMinutes(45)),
			new("60 min", TimeSpan.FromMinutes(60)),
		];
		DelayOptions[2].IsSelected = true;
	}

	public IReadOnlyList<DelayOption> DelayOptions { get; }

	public string Greeting => DateTime.Now.Hour switch
	{
		< 5 => "Gute Nacht",
		< 11 => "Guten Morgen",
		< 17 => "Hallo",
		_ => "Guten Abend",
	};

	public string Today => DateTime.Now.ToString("dddd, d. MMMM");

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(RecordingDescription))]
	public partial bool IsRecordingEnabled { get; set; } = true;

	public string RecordingDescription => IsRecordingEnabled
		? "Kurze Clips, nur auf diesem Gerät"
		: "Nur Statistik, kein Audio gespeichert";

	[ObservableProperty]
	public partial TimeSpan AutoStopTime { get; set; } = new(7, 0, 0);

	[ObservableProperty]
	public partial string? StatusMessage { get; set; }

	[RelayCommand]
	private void SelectDelay(DelayOption option) => DelayOptions.Select(option);

	[RelayCommand]
	private void StartNight()
	{
		// Die Überwachung (Foreground-Service) folgt in Phase 1.
		StatusMessage = "Die Überwachung wird in Phase 1 umgesetzt.";
	}
}
