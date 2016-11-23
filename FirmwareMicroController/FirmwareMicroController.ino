#define REDPIN 6
#define GREENPIN 5
#define BLUEPIN 9

//variables for overal modes
int colourMode = 1;
int colourR = 0;
int colourG = 0;
int colourB = 0;
float mapR = 0.01;
float mapG = 0.01;
float mapB = 0.01;

//serial input handling variables
String inputString = "";
bool stringComplete = false;

//breathing led variables
int i = 0;
int breathe_delay = 15;   // delay between loops
unsigned long breathe_time = millis();

//rainbow led variables
int rRainbow = 255;
int gRainbow = 0;
int bRainbow = 0;
int rainbow_delay = 5;   // delay between loops
unsigned long rainbow_time = millis();

//generic always on variables
int uptime_delay = 50;
unsigned long uptime_time = millis();

void setup() {
  Serial.begin(9600);
  inputString.reserve(200);

  pinMode(REDPIN, OUTPUT);
  pinMode(GREENPIN, OUTPUT);
  pinMode(BLUEPIN, OUTPUT);
  analogWrite(REDPIN, 0);
  analogWrite(GREENPIN, 0);
  analogWrite(BLUEPIN, 0);

}


void loop() {
  //Serial.println(colourMode);
  //read incoming data
  while (Serial.available()) {
    // get the new byte:
    char inChar = (char)Serial.read();
    // add it to the inputString:
    inputString += inChar;
    // if the incoming character is a newline, set a flag
    // so the main loop can do something about it:
    if (inChar == '\n') {
      stringComplete = true;
    }
  }

  // handle the string coming in
  if (stringComplete) {
    //Serial1.println(inputString);
    // clear the string:
    if (inputString.startsWith("mod,")) {
      String value = inputString.substring(4);
      colourMode = value.toInt();
      Serial.println("changed mode to:" + colourMode);
    }
    if (inputString.startsWith("cR,")) {
      String value = inputString.substring(3);
      colourR = value.toInt();
      mapR = mapfloat(colourR, 0, 255, 0, 1);
      Serial.println("changed red value" + String(mapR));
      inputString = "";
      stringComplete = false;
    }
    if (inputString.startsWith("cG,")) {
      String value = inputString.substring(3);
      colourG = value.toInt();
      mapG = mapfloat(colourG, 0, 255, 0, 1);
      Serial.println("changed green value" + String(mapG));
      inputString = "";
      stringComplete = false;
    }
    if (inputString.startsWith("cB,")) {
      String value = inputString.substring(3);
      colourB = value.toInt();
      mapB = mapfloat(colourB, 0, 255, 0, 1);
      Serial.println("changed blue value:" + String(mapB));
      inputString = "";
      stringComplete = false;
    }
    if (inputString.startsWith("cRGB,")) {
      Serial.println(inputString);
      int commaIndex = inputString.indexOf(',');
      int secondCommaIndex = inputString.indexOf(',', commaIndex + 1);
      int thirdCommaIndex = inputString.indexOf(',', secondCommaIndex + 1);
      String firstValue = inputString.substring(commaIndex + 1, secondCommaIndex);
      String secondValue = inputString.substring(secondCommaIndex + 1, thirdCommaIndex);
      String thirdValue = inputString.substring(thirdCommaIndex + 1);

      colourR = firstValue.toInt();
      colourG = secondValue.toInt();
      colourB = thirdValue.toInt();
      mapR = mapfloat(colourR, 0, 255, 0, 1);
      mapG = mapfloat(colourG, 0, 255, 0, 1);
      mapB = mapfloat(colourB, 0, 255, 0, 1);

      Serial.println("thing: " + firstValue + secondValue +  thirdValue);
      inputString = "";
      stringComplete = false;
    }

    if (inputString.startsWith("tD,")) {
      String value = inputString.substring(3);
      rainbow_delay = value.toInt();
      breathe_delay = value.toInt();
      Serial.println("changed timing value:" + String(value));
      inputString = "";
      stringComplete = false;
    }

    if (inputString.startsWith("gief")){
      Serial.println("data," + String(colourR) + "," + String(colourG) + "," + String(colourB) + "," + String(rainbow_delay) + "," + String(colourMode));
    }

    inputString = "";
    stringComplete = false;
  }

  switch (colourMode) {
    case 0:
      setAllLeds(0);
      break;
    case 1:
      setAllLeds(255);
      break;
    case 2:
      setColourLeds(colourR, colourG, colourB);
      break;
    case 3:
      breathLeds(mapR, mapG, mapB);
      break;
    case 4:
      rainbowLeds();
      break;
  }
}

void setColourLeds(int r, int g, int b) {
  if ((uptime_time + uptime_delay) < millis()) {
    uptime_time = millis();
    setAllLeds(0);
    
    analogWrite(REDPIN, r);

    analogWrite(GREENPIN, g);

    analogWrite(BLUEPIN, b);
  }
}

void breathLeds(float r, float g, float b) {
  if ( (breathe_time + breathe_delay) < millis() ) {
    breathe_time = millis();
    float valr = r * (exp(sin(i / 2000.0 * PI * 10)) - 0.36787944) * 108.0;
    float valg = g * (exp(sin(i / 2000.0 * PI * 10)) - 0.36787944) * 108.0;
    float valb = b * (exp(sin(i / 2000.0 * PI * 10)) - 0.36787944) * 108.0;
    // this is the math function recreating the effect
    analogWrite(REDPIN, valr);

    analogWrite(GREENPIN, valg);

    analogWrite(BLUEPIN, valb);

    i++;
  }
}

void setAllLeds(int n) {
  if ((uptime_time + uptime_delay) < millis()) {
    uptime_time = millis();
    analogWrite(REDPIN, n);

    analogWrite(GREENPIN, n);

    analogWrite(BLUEPIN, n);
  }
}

void rainbowLeds() {
  if ( (rainbow_time + rainbow_delay) < millis() ) {
    rainbow_time = millis();
    if (rRainbow == 255 && gRainbow != 255 && bRainbow == 0) {
      gRainbow++;
    }
    if (gRainbow == 255 && rRainbow != 0 && bRainbow == 0) {
      rRainbow--;
    }
    if (rRainbow == 0 && gRainbow == 255 && bRainbow != 255) {
      bRainbow++;
    }
    if (bRainbow == 255 && rRainbow == 0 && gRainbow != 0) {
      gRainbow--;
    }
    if (gRainbow == 0 && bRainbow == 255 && rRainbow != 255) {
      rRainbow++;
    }
    if (rRainbow == 255 && gRainbow == 0 && bRainbow != 0) {
      bRainbow--;
    }
    analogWrite(REDPIN, rRainbow);

    analogWrite(GREENPIN, gRainbow);

    analogWrite(BLUEPIN, bRainbow);
  }
}

float mapfloat(int x, int a, int b, int c, int d)
{
  c = c * 100;
  d = d * 100;
  int q = map(x, a, b, c, d);
  //Serial.println(q);
  float f = (float)q / 100;
  //Serial.println(f);
  return f;
}

String splitValue(String data, char separator, int index)
{
  int found = 0;
  int strIndex[] = {0, -1};
  int maxIndex = data.length() - 1;

  for (int i = 0; i <= maxIndex && found <= index; i++) {
    if (data.charAt(i) == separator || i == maxIndex) {
      found++;
      strIndex[0] = strIndex[1] + 1;
      strIndex[1] = (i == maxIndex) ? i + 1 : i;
    }
  }

  return found > index ? data.substring(strIndex[0], strIndex[1]) : "";
}
