using System.ComponentModel;

namespace TestBench.UI;

public class LedResult : INotifyPropertyChanged
{
	public string Name { get; set; } = "";
    public int LedNumber { get; set; }

    private bool _isOn;
    public bool IsOn
    {
        get => _isOn;
        set
        {
            _isOn = value;
            OnPropertyChanged(nameof(IsOn));
            OnPropertyChanged(nameof(StatusColor));
        }
    }

    private double _voltage;
    public double Voltage
    {
        get => _voltage;
        set
        {
            _voltage = value;
            OnPropertyChanged(nameof(Voltage));
        }
    }

    public string StatusColor => IsOn ? "LimeGreen" : "Gray";

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}