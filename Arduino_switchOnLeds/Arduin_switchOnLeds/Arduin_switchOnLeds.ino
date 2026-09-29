const int pin2 = D2;
const int pin3 = D3;
const int pin4 = D4;
const int pin5 = D5;

const int analogPins[] = {A0, A1, A2, A3};

void setup() {
  pinMode(pin2, OUTPUT);
  pinMode(pin3, OUTPUT);
  pinMode(pin4, OUTPUT);
  pinMode(pin5, OUTPUT);

  digitalWrite(pin2, HIGH);
  digitalWrite(pin3, LOW);
  digitalWrite(pin4, LOW);
  digitalWrite(pin5, LOW);

  Serial.begin(9600);
}

void loop() {

  if (Serial.available() > 0) {
    String command = Serial.readStringUntil('\n');
    command.trim();

    if (command == "Ready") {
      digitalWrite(pin2, HIGH);
      digitalWrite(pin3, HIGH);
      digitalWrite(pin4, LOW);
      digitalWrite(pin5, LOW);
    }
    else if (command == "PowerOn") {
      digitalWrite(pin2, HIGH);
      digitalWrite(pin3, LOW);
      digitalWrite(pin4, LOW);
      digitalWrite(pin5, LOW);
    }
    else if (command == "Test") {
      digitalWrite(pin2, HIGH);
      digitalWrite(pin3, LOW);
      digitalWrite(pin4, LOW);
      digitalWrite(pin5, HIGH);
    }
    else if (command == "Error") {
      digitalWrite(pin2, HIGH);
      digitalWrite(pin3, LOW);
      digitalWrite(pin4, HIGH);
      digitalWrite(pin5, LOW);
    }
    else if (command == "PowerOff") {
    digitalWrite(pin2, LOW);
    digitalWrite(pin3, LOW);
    digitalWrite(pin4, LOW);
    digitalWrite(pin5, LOW);
    }
  }

  for (int i = 0; i < 4; i++) {
    int rawValue = analogRead(analogPins[i]);
    float voltage = (rawValue / 4095.0) * 3.3;

    Serial.print(voltage, 2);

    if (i < 3)
      Serial.print(",");
  }
  Serial.println();
  delay(500);
}