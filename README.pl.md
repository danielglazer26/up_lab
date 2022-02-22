# UP_lab
Repozytorium zawiera aplikacje i sprawozdania z laboratoriów z Urządzeń Peryferyjnych realizowanych w roku akademickim 2021/2022.

## Pozostałe języki
[![en](https://img.shields.io/badge/lang-en-red.svg)](https://github.com/danielglazer26/UP_lab)

## Spis treści
* [Joystick](#joystick)
* [Karta dźwiękowa](#karta-dźwiękowa)
* [BLE](#ble)
* [Akwizycja Sygnałów](#akwizycja-sygnałów)
* [Silnik Krokowy](#silnik-krokowy)
* [GPS](#gps) 
* [Kamera internetowa](#kamera-internetowa)

## Joystick
Program został napisany w języku C#, realizuje podstawowe funkcje sterowania joystickiem takie jak:
* odczytywanie  nazwy joysticka
* ilustrowanie działania joysticka za pomocą odczytywanie jego współrzędnych
* emulowanie działania myszy za pomocą pada
* prosty edytor graficzny obsługiwany za pomocą gamepada

## Karta dźwiękowa
Program został napisany w języku C#, jego celem było odtwarzanie i nagrywanie dźwięku za pomocą różnych metod, aplikacja realizowała takie funkcje jak:
*  odtwarzanie dźwięku z wykorzystaniem metody PlaySound
*  odtwarzanie za pomocą wykorzystania Komponentu ActiveX "Windows Media Player"
*  wczytywanie i wyświetlanie nagłówka WAV
*  odtwarzanie za pomocą Waveform and Auxiliary Audio
*  nagrywanie dźwięku i zapisywanie go do pliku
*  odtwarzanie z wykorzystaniem DirectSound
*  efekt echo

## BLE
Program został napisany w Arduino. Jego celem było zaprogramowanie układu ESP32 w taki sposób, aby wysyłał zbiór danych za pomocą BLE (ang. Bluetooth Low Energy), a następnie zbiór ten miał zostać wyświetlony na telefonie za pomocą aplikacji nRF Connect ze skepu Google Play.

## Akwizycja Sygnałów
Podobnie jak w przypadku BLE układ ESP32 miał za zadanie wysyłać zbiór danych.  W tym wypadku była to temperatura i wilgotność powietrza, które pobrano z czujnika DHT11 podłączonego do modułu ESP32. Następnie te dane zostały wysłane do smartfona za pomocą BLE. Dla smartfona została napisana aplikacja w Kotlinie, która odbierała dane i przedstawiała je na wykresie.

## Silnik Krokowy
Celem programu było sterowanie silnikiem krokowym M42SP-7. Program został napisany w C# i realizował takie funkcje jak:
* ruch falowy 
* ruch pełnokrokowy
* ruch półkrokowy
* obrót silnika o określony kąt
* obrót silnika o określoną liczbę kroków

## GPS
Program został napisany w Javie jako aplikacja mobilna, realizwał takie funkcje jak:
* ustalenie pozycji telefonu na mapie
* wysyłanie i odbieranie lokalizacji kanałem BLE 

## Kamera internetowa
Celem programu była obsługa kamery internetowej. Aplikacja została napisana w C# i realizowała takie funkcje jak:
* wyświeltanie podglądu obrazu z kamery
* zmienianie ustawień kamery
* zapisywanie obrazu z kamery w postaci zdjęcia w formacie *.jpg
* zapisywanie obrazu z kamery w postaci filmu w formacie *.avi
* detekcje ruchu 
