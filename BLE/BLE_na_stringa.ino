	#include <BLEDevice.h>
	#include <BLEServer.h>
	#include <BLEUtils.h>
	#include <BLE2902.h>

	#define serviceID BLEUUID((uint16_t)0x1801)

	char charArr[]="Tablica znakow 1";
	String str = "Tablica znakow 2";
	int integer = 3;
	float decimalNumber = 4.00000;

	BLECharacteristic customCharacteristic(
	  BLEUUID((uint16_t)0x1A00),
	  BLECharacteristic::PROPERTY_READ |
	  BLECharacteristic::PROPERTY_NOTIFY
	);


	bool deviceConnected = false;
	class ServerCallbacks: public BLEServerCallbacks {
		void onConnect(BLEServer* MyServer) {
		  deviceConnected = true;
		};
		void onDisconnect(BLEServer* MyServer) {
		  deviceConnected = false;
		}
	};

	void setup() {
		Serial.begin(115200);

		// Ustawienie nazwy
		BLEDevice::init("Helisz_GLazer");
			
		//Stworzenie serwera BLE
		BLEServer *MyServer = BLEDevice::createServer();  
		MyServer->setCallbacks(new ServerCallbacks());  

		// Utworzenie uslugi BLE
		BLEService *customService = MyServer->createService(serviceID); 
		// Scharakteryzowanie uslugi
		customService->addCharacteristic(&customCharacteristic);  
		// Utworzenie descryptora
		customCharacteristic.addDescriptor(new BLE2902());  
		// Skonfigurowanie rozglaszania
		MyServer->getAdvertising()->addServiceUUID(serviceID);  
	  
		// Uruchomienie uslugi 
		customService->start(); 
		// Uruchomienie rozglaszania
		MyServer->getAdvertising()->start();  

		Serial.println("Waiting for a client to connect....");
	}
	void loop() {
	  for (int i = 0; i < 4; ++i)
	  {
		switch(i){
		  case 0:{  
			//Wysyłanie tablicy znakow
			customCharacteristic.setValue((char*)&charArr);
			customCharacteristic.notify();
			break;
		  }
		  case 1:{  //send string
			char buffer[str.length()+1];
			str.toCharArray(buffer,str.length()+1);
			customCharacteristic.setValue((char*)&buffer);
			customCharacteristic.notify();
			break;
		  }
		  case 2:{  //send integer
			char buffer[20];
			dtostrf(integer,1,0,buffer);
			customCharacteristic.setValue((char*)&buffer);
			customCharacteristic.notify();
			break;
		  }
		  case 3:{  
		  //Wysyłanie liczby zmiennoprzecinkowe
			char buffer[20];
			dtostrf(decimalNumber,1,5,buffer);  
			customCharacteristic.setValue((char*)&buffer);
			customCharacteristic.notify();
			break;
		  }
		}
		delay(1000);
	  }
	}