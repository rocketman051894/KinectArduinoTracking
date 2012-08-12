
#include <SPI.h>         
#include <Ethernet.h>
#include <EthernetUdp.h>
#include <Servo.h> 


byte mac[] = {  
  0x90, 0xA2, 0xDA, 0x00, 0xC3, 0xA5 };
IPAddress ip(192, 168, 137, 105);

unsigned int localPort = 8888;  

// buffers for receiving and sending data
char packetBuffer[UDP_TX_PACKET_MAX_SIZE]; //buffer to hold incoming packet,
char  ReplyBuffer[] = "acknowledged";       // a string to send back

// An EthernetUDP instance to let us send and receive packets over UDP
EthernetUDP Udp;
//Servo tilt, pan;  // create servo object to control a servo 
                // a maximum of eight servo objects can be created 

Servo servos[2];
 
int tiltPos1 = 90, panPos1 = 90, tiltPos2 = 90, panPos2 = 90;    // variable to store the servo position 
int counter = 0;//
String pos;
 
void setup() 
{ 
  Serial.begin(115200);
  Ethernet.begin(mac,ip);
  Udp.begin(localPort);
  
  servos[0].attach(3); //tilt
  servos[1].attach(6);   //pan
//  servos[2].attach(9);
//  servos[3].attach(3);
  
  servos[0].write(tiltPos1);
  servos[1].write(panPos1);
//  servos[2].write(tiltPos2);
//  servos[3].write(panPos2);
  
  Serial.print("server is at ");
  Serial.println(Ethernet.localIP());
} 
 
 
void loop() 
{ 
  if(Serial.available()){
    char control = Serial.read();
    
    
    if(isDigit(control)){
      pos += (char)control; 
    }
    
    
    else if(control == 'a')
   {
     servos[0].write(pos.toInt());       
     pos = "";
     
   }
   
   else if(control == 'b'){
     servos[1].write(pos.toInt());
     pos = "";
   }
  }
}

//----------------------//   
/*      2nd Servo       */   
//----------------------//   
//   else if(control == 'c')
//   {
//     servos[2].write(pos.toInt());       
//     pos = "";
//     
//   }
//   
//   else if(control == 'd'){
//     servos[3].write(pos.toInt());
//     pos = "";
//   }
//  }

//-------------------------------------------//  
/*          Ethernet connection              */
//-------------------------------------------//  
//  int packetSize = Udp.parsePacket();
//  if(packetSize){  
//     Udp.read(packetBuffer,UDP_TX_PACKET_MAX_SIZE);
//     
//     Serial.println(packetBuffer);
//     for(int i = 1; i < 4; i++){
//       pos += packetBuffer[i]; 
//     }
//     servos[0].write(pos.toInt());
//     Serial.print("x: ");
//     Serial.println(pos.toInt());
//     pos = "";
//     
//     for(int i = 5; i < 8; i++){
//       pos += packetBuffer[i]; 
//     }
//     servos[1].write(pos.toInt());
//     Serial.print("y: ");
//     Serial.println(pos.toInt());
//     pos = "";
//     
//     Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
//     Udp.write("OK");
//     Udp.endPacket();
     
//     if(packetBuffer[0] == 'x'){
//       for(int i = 1; i < sizeof(packetBuffer); i++){
//         pos += packetBuffer[i];
//       }
//       servos[0].write(pos.toInt());
////       Serial.print("x: ");
////       Serial.println(packetBuffer);
////       Serial.println(pos.toInt());
//       pos = "";
//     }
//   
//     if(packetBuffer[0] == 'y'){
//       for(int i = 1; i < sizeof(packetBuffer); i++){
//         pos += packetBuffer[i];
//       }
//       servos[1].write(pos.toInt());
////       Serial.print("y: ");
////       Serial.println(packetBuffer);
////       Serial.println(pos.toInt());
//       pos = "";
//     }
//  }
//  delay(10);
//}
