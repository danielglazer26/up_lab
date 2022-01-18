#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <BLE2902.h>
#include <stdlib.h>
#include <string>
#include "DHT.h"

#define DHTTYPE DHT11
#define serviceID  "25AE1441-05D3-4C5B-8281-93D4E07420CF"
#define indicateID "25AE1444-05D3-4C5B-8281-93D4E07420CF"

static BLEServer* g_pServer = nullptr;
static BLECharacteristic* g_pCharIndicate = nullptr;

 
uint8_t DHTPin = 4; 
               
DHT dht(DHTPin, DHTTYPE);

float Temperature;
float Humidity;

bool g_centralConnected = false;

class MyServerCallbacks: public BLEServerCallbacks
{
    void onConnect(BLEServer* pServer) override
    {
        Serial.println("onConnect");
        g_centralConnected = true;
    }

    void onDisconnect(BLEServer* pServer) override
    {
        Serial.println("onDisconnect, will start advertising");
        g_centralConnected = false;
        BLEDevice::startAdvertising();
    }
};

void setup() {
  Serial.begin(115200);
  Serial.println("BLE wystartowalo");
  
  dht.begin();  

  BLEDevice::init("Helisz_GLazer");
  g_pServer = BLEDevice::createServer();
  g_pServer->setCallbacks(new MyServerCallbacks());
  BLEService* pService = g_pServer->createService(serviceID);

  pinMode(DHTPin, INPUT);

  // characteristic for indicate
    
        uint32_t propertyFlags = BLECharacteristic::PROPERTY_INDICATE;
        BLECharacteristic* pCharIndicate = pService->createCharacteristic(indicateID, propertyFlags);
        pCharIndicate->addDescriptor(new BLE2902());
        pCharIndicate->setValue("");
        g_pCharIndicate = pCharIndicate;
    

    pService->start();
    BLEAdvertising* pAdvertising = BLEDevice::getAdvertising();
    pAdvertising->addServiceUUID(serviceID);
    pAdvertising->setScanResponse(true);

    BLEDevice::startAdvertising();

}

void loop() {
    Temperature = dht.readTemperature(); 
    Humidity = dht.readHumidity(); 
    
    char buffY[10];
    char buffX[10];
    dtostrf(Temperature, 2, 2, buffY);
    dtostrf(Humidity, 2, 2, buffX);
 
    std::string wartosci = "";
    wartosci += buffX;
    wartosci += "/";
    wartosci += buffY;
    g_pCharIndicate->setValue(wartosci);
    g_pCharIndicate->indicate();
  
  
  delay(3000);

}
