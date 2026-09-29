# Test Bench Application

A hardware test bench application built as a personal learning project to practice C#/.NET, WPF, gRPC communication, serial communication, and automated test sequence execution.

## Background

This project implements a small hardware test bench in which a C# application communicates with an Arduino-based device under test.

The project was developed as a practical way to strengthen my C#/.NET skills while applying concepts from my professional background in measurement, testing, calibration, and software engineering.

## Architecture

The application consists of three main components:

* **WPF Client** – provides the user interface for controlling and monitoring the test bench
* **gRPC Device Server** – handles communication between the client and the hardware
* **Arduino** – represents the device under test and provides voltage measurements

```text
WPF Client
    ↕
   gRPC
    ↕
Device Server
    ↕
Serial Communication
    ↕
  Arduino
```

## Technologies

* C# / .NET
* WPF
* gRPC
* Protocol Buffers
* JSON
* SQLite
* Arduino / C++
* Serial Communication

## What it does

* Execute predefined test sequences
* Send commands from the WPF application to the Arduino
* Read four voltage measurements from the Arduino
* Evaluate measured voltages against configurable limits
* Compare measured LED states with expected states
* Display PASS/FAIL results for individual test steps
* Store test results and measurements in a SQLite database
* Store test runs using a serial number

## Test Sequence

A typical test sequence consists of several states:

```text
PowerOff
   ↓
PowerOn
   ↓
Ready
   ↓
Test
   ↓
Error
   ↓
PowerOff
```

For each step, the system evaluates the four measured channels against the expected LED states and records the result.

## Configuration

Test limits and expected LED states are defined in a JSON configuration file.

Example:

```json
{
  "LedThresholds": {
    "MinVoltage": 1.75,
    "MaxVoltage": 3.25
  },
  "ExpectedStates": {
    "PowerOff": [false, false, false, false],
    "PowerOn": [false, true, false, false],
    "Ready": [true, true, false, false],
    "Test": [false, true, false, true],
    "Error": [false, true, true, false]
  }
}
```

This allows test conditions and expected states to be changed without modifying the application code.

## Database

SQLite is used to store:

* Test runs
* Serial numbers
* Start and end timestamps
* Overall PASS/FAIL result
* Individual LED measurements
* PASS/FAIL results for each measurement

## Future Work

Possible extensions for this project include:

- Camera-based inspection for visual verification of the device under test
- Automated image-based evaluation of LED states
- Additional test sequences and configurable test parameters
- Extended test reporting and result visualization

## About this project

This project was developed independently as a learning project to strengthen my C#/.NET skills and gain practical experience with modern software development.

It also demonstrates how a measurement and test system can be structured using client/server communication, hardware communication, configurable test sequences, measurement evaluation, and persistent test results.
