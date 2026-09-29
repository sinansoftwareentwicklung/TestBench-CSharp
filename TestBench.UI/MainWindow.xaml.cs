using System.Windows;
using System.Windows.Controls;
using Grpc.Net.Client;
using TestBench.Proto;
using Grpc.Core;
using System.Net.Http;
using System.Collections.ObjectModel;

namespace TestBench.UI;

public partial class MainWindow : Window
{
    public ObservableCollection<LedResult> LedResults { get; set; } = new();
    private DeviceService.DeviceServiceClient? _client;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        string[] names = { "Ready", "Power", "Error", "Test" };
        for (int i = 1; i <= 4; i++)
        {
            LedResults.Add(new LedResult { LedNumber = i, Name = names[i - 1] });
        }

        Loaded += MainWindow_Loaded;
    }
    private async Task StreamLiveMeasurementsAsync()
    {
        try
        {
            var measurementRequest = new MeasurementRequest
            {
                TestName = "Live Monitor"
            };

            using var call = _client!.StreamMeasurements(measurementRequest);

            await foreach (var reading in call.ResponseStream.ReadAllAsync())
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    var voltages = new double[]
                    {
                    reading.Voltage1,
                    reading.Voltage2,
                    reading.Voltage3,
                    reading.Voltage4
                    };

                    for (int i = 0; i < 4; i++)
                    {
                        LedResults[i].Voltage = voltages[i];

                        LedResults[i].IsOn =
                            voltages[i] >= 1.75 &&
                            voltages[i] <= 3.25;
                    }
                });
            }
        }
        catch (RpcException ex)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    $"Live Stream gRPC Error\n\n" +
                    $"Status: {ex.StatusCode}\n\n" +
                    $"Detail: {ex.Status.Detail}");
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(ex.ToString());
            });
        }
    }
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        var channel = GrpcChannel.ForAddress("http://localhost:5000", new GrpcChannelOptions
        {
            HttpHandler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            }
        });
        _client = new DeviceService.DeviceServiceClient(channel);

        var request = new StartTestRequest { TestName = "Led Check Test" };
        var response = await _client.StartTestAsync(request);
        StatusText.Text = "Device Status Unit — Status Indicators Verification";

	_ = StreamLiveMeasurementsAsync();
    
  }
	
	private async void RunSequence_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			string serialNumber = SerialNumberTextBox.Text.Trim();

			string[] sequence =
			{
				"PowerOff",
				"PowerOn",
				"Ready",
				"Test",
				"Error",
				"PowerOff"
			};

			var request = new TestSequenceRequest();
			request.Steps.AddRange(sequence);
			request.SerialNumber = SerialNumberTextBox.Text.Trim();

			using var call = _client!.ExecuteTestSequence(request);

			SequenceResultsList.Items.Clear();

			bool finalPassed = true;

			await foreach (var result in call.ResponseStream.ReadAllAsync())
			{
				SequenceResultsList.Items.Add(result);

				foreach (var led in result.Results)
				{
					if (!led.Passed)
					{
						finalPassed = false;
					}
				}
			}

			FinalResultText.Text = finalPassed ? "PASS" : "FAIL";
			FinalResultText.Foreground =
				finalPassed
					? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LimeGreen)
					: new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
		}
		catch (RpcException ex)
		{
			MessageBox.Show(
				$"gRPC Error\n\nStatus: {ex.StatusCode}\nDetail: {ex.Status.Detail}");
		}
	}
}