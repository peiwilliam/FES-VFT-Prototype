// Code written initially by Sarah Taghizadeh
// Modified by Emerson Grabke, Alex Scott
// This version has been further modified by William Pei
// Use Arduino IDE v1.8 to run properly!
// Note: Set board type to Diecimila, ATMega168
// PWM Pins 3,5,6,9,10,11 for the board
// For uniformity in PWM speed use pins 3,9,10,11 (5,6 have double the frequency)

// 3-A  -LPF
// 9-B  -LDF
// 10-C -RPF
// 11-D -RDF

int switchPin = 12;
int ledPins[] = {3,9,10,11};
int pinCount = 4;

int w = 250; // ms to wait on button press

float max_volt = 5.0;
float max_pwm = 255.0;

//3: 'a' Compex 1 Port A - Left PF - 1st channel output in LabVIEW
//9: 'b' Compex 1 Port B - Left DF - 2nd channel output in LabVIEW
//10: 'c' Compex 2 Port A - Right PF - 3rd channel output in LabVIEW
//11: 'd' Compex 2 Port B - Right DF - 4th channel output in LabVIEW

void setup()
{
  // Following two commands sets PWM clock divisor to 1
  // for pin pairs {9,10} and {3,11}
  // Results in PWM frequency 31372.55 Hz
  TCCR1B = TCCR1B & B11111000 | B00000001;
  TCCR2B = TCCR2B & B11111000 | B00000001;
  
  Serial.begin(115200);
  
  for(int i=0; i<pinCount; i++)
  {
    pinMode(ledPins[i],OUTPUT);
    analogWrite(ledPins[i],0);
  }  
  pinMode(switchPin,OUTPUT);
  digitalWrite(switchPin,LOW);
}

void loop() 
{  
  while(Serial.available() > 2) // In format {[char][2digit num from 00-64]}
  {
      // Read things
      String inp = "";
      char port = Serial.read();
      char n_p = tolower(port);
      if ( (n_p != 'a') && (n_p != 'b') && (n_p != 'c') && (n_p != 'd') && (n_p != 'z') && (n_p != 's')) // If it's not a recognized letter command, we're out of sync
        { continue; }
      inp = (char) Serial.read();
      inp += (char) Serial.read(); // Hardcoding that digit has to span 2 characters
      //Serial.print(port); // for debugging with unity only
      //Serial.print(inp.c_str()); 
      //Serial.print(',');

      float output_p = (inp.toFloat()/64.0)*max_pwm;
      output_p = (output_p>max_pwm)?max_pwm:output_p; // hard max
      output_p = (output_p<0)?0:output_p; // This shouldn't be possible (unless someone puts something like "d-9") but just in case
      //float output = output_p;
      //Serial.print(output,DEC);
      unsigned char outp = output_p;
      switch (n_p)
      {
        case 'a':
          analogWrite(ledPins[0],outp);
          break;
        case 'b':
          analogWrite(ledPins[1],outp);
          break;
        case 'c':
          analogWrite(ledPins[2],outp);
          break;
        case 'd':
          analogWrite(ledPins[3],outp);
          break;
        case 's':
          Serial.print(inp.c_str());
          Serial.print(',');
          break;
        case 'z': //this isn't actually being used currently since manual switch is only used to turn on compex
          if (output_p != 0)
            {digitalWrite(switchPin,HIGH);
            delay(w); // Every time you hit high, wait. Since you're building up input though I don't know if the buffer would be an issue. Still
            digitalWrite(switchPin,LOW);}
          else
            {digitalWrite(switchPin,LOW);} // Continually keep this low (done in LabVIEW)
          break;
        default:
          break;
      }
  }
}
