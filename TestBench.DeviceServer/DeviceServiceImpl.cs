using Grpc.Core;
using System.IO.Ports;
using TestBench.Proto;

namespace TestBench.DeviceServer;

public class TestSpecification
{
    public LedThresholds LedThresholds { get; set; } = new();
    public Dictionary<string, bool[]> ExpectedStates { get; set; } = new();
}

public class DeviceServiceImpl : DeviceService.DeviceServiceBase, IDisposable
{
	private readonly Database _database;
	private readonly ArduinoSettings _settings;
	private readonly SerialPort _port;
	private readonly TestSpecification _testSpecification;
	private readonly object _portLock = new object();
	private double[] _lastVoltages = new double[4];

    public DeviceServiceImpl(
    ArduinoSettings settings,
    TestSpecification testSpecification,
	Database database)
	{
		_settings = settings;
		_testSpecification = testSpecification;
		_database = database;
		
		_port = new SerialPort(_settings.PortName, _settings.BaudRate);
        _port.DtrEnable = true;
        _port.RtsEnable = true;
        _port.NewLine = "\r\n";
        _port.ReadTimeout = 3000;
        _port.Open();
		
		Thread.Sleep(1500);
        _port.DiscardInBuffer();
    }
	
	public void Dispose()
	{
		if (_port.IsOpen)
		{
			_port.Close();
		}
		_port.Dispose();
	}

    public override Task<StartTestResponse> StartTest(
		StartTestRequest request,
		ServerCallContext context)
	{
		Console.WriteLine(">>> StartTest called");

		return Task.FromResult(new StartTestResponse
		{
			Accepted = true,
			Message = $"Test '{request.TestName}' accepted"
		});
	}
    private double[] ReadVoltagesFromArduino()
{
    lock (_portLock)
    {
        var line = _port.ReadLine();
        var parts = line.Split(',');

        var voltages = new double[4];
        for (int i = 0; i < 4; i++)
        {
            voltages[i] = double.Parse(parts[i]);
        }

        return voltages;
    }
}
	public override Task<CommandResponse> SendCommand(CommandRequest request, ServerCallContext context)
	{
		lock (_portLock)
		{
			_port.WriteLine(request.Command);
		}

		var response = new CommandResponse { Sent = true };
		return Task.FromResult(response);
	}

	public override async Task StreamMeasurements(
		MeasurementRequest request,
		IServerStreamWriter<LiveReading> responseStream,
		ServerCallContext context)
	{
		while (!context.CancellationToken.IsCancellationRequested)
		{
			var voltages = ReadVoltagesFromArduino();
			_lastVoltages = voltages;

			var reading = new LiveReading
			{
				Voltage1 = voltages[0],
				Voltage2 = voltages[1],
				Voltage3 = voltages[2],
				Voltage4 = voltages[3],
				Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			};

			await responseStream.WriteAsync(reading);
		}
	}
    public override Task<LedCheckResponse> RunLedCheck(LedCheckRequest request, ServerCallContext context)
    {
        var voltages = ReadVoltagesFromArduino();
        var response = new LedCheckResponse();
		
		var expectedStates = _testSpecification.ExpectedStates["PowerOn"];

		for (int i = 0; i < 4; i++)
		{
			var isOn =
				voltages[i] >= _testSpecification.LedThresholds.MinVoltage &&
				voltages[i] <= _testSpecification.LedThresholds.MaxVoltage;
					var passed = isOn == expectedStates[i];

            response.Results.Add(new LedResult
            {
                LedNumber = i + 1,
                IsOn = isOn,
                Voltage = voltages[i],
                Passed = passed
            });
        }

        return Task.FromResult(response);
    }
	public override async Task ExecuteTestSequence(
    TestSequenceRequest request,
    IServerStreamWriter<TestStepResult> responseStream,
    ServerCallContext context)
	{
		long testRunId = _database.StartTestRun(request.SerialNumber);
		bool finalPassed = true;
		foreach (var step in request.Steps)
		{
			Console.WriteLine($"Executing: {step}");

			lock (_portLock)
			{
				_port.WriteLine(step);
			}

			await Task.Delay(2000);
			
			var expectedStates = _testSpecification.ExpectedStates[step];

			var result = new TestStepResult
			{
				Step = step,
				Passed = true
			};

			for (int i = 1; i <= 4; i++)
			{
				double voltage = _lastVoltages[i - 1];

				bool isOn =
					voltage >= _testSpecification.LedThresholds.MinVoltage &&
					voltage <= _testSpecification.LedThresholds.MaxVoltage;

				bool passed = isOn == expectedStates[i - 1];
				
				if (!passed)
				{
					finalPassed = false;
				}

				var ledResult = new LedResult
				{
					LedNumber = i,
					IsOn = isOn,
					Voltage = voltage,
					Passed = passed
				};

				result.Results.Add(ledResult);

				_database.SaveLedResult(
					testRunId,
					step,
					i,
					voltage,
					passed);
							}

				await responseStream.WriteAsync(result);
		}
		_database.CompleteTestRun(testRunId, finalPassed);
	}
}