String a;

void setup() {
  Serial.begin(9600); // opens serial port, sets data rate to 9600 bps
  pinMode(11, OUTPUT);
  pinMode(12, OUTPUT);
}

void loop() {

  while (Serial.available()) {
    a = Serial.readString(); // read the incoming data as string
  }
  if (a == "1")
  {
    digitalWrite(11, 1);
    digitalWrite(12, 1);
  }
  else if (a == "0")
  {
    digitalWrite(11, 0);
    digitalWrite(12, 0);
  }
}
