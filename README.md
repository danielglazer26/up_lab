# UP_lab
The repository contains applications and reports from the Peripheral Devices labs conducted in the 2021/2022 academic year.
## Other languages
[![pl](https://img.shields.io/badge/lang-pl-red.svg)](https://github.com/danielglazer26/UP_lab/blob/main/README.pl.md)
## Table of Contents
* [Joystick](#joystick)
* [Sound card](#soundcard)
* [BLE](#ble)
* [Signal Acquisition](#signal-acquisition)
* [Stepper motor](#steppermotor)
* [GPS](#gps) 
* [Webcam](#webcam)

## Joystick
The program was written in C# language, it implements basic joystick control functions such as:
* reading the joystick name.
* Illustrating joystick actions by reading its coordinates.
* emulating the action of the mouse with the pad.
* a simple graphical editor using the gamepad

## Sound card
The program was written in C# language, its purpose was to play and record sound using various methods, the application realized functions such as:
* sound playback using the PlaySound method.
* playback by using ActiveX component "Windows Media Player".
* loading and displaying WAV header
* Playback using Waveform and Auxiliary Audio.
* Recording audio and saving it to a file.
* Playback using DirectSound.
* echo effect

## BLE
The program was written in Arduino. Its purpose was to program the ESP32 chip to send a dataset using BLE (Bluetooth Low Energy), and then the dataset would be displayed on the phone using the nRF Connect app from the Google Play store.

## Signal Acquisition
As with BLE, the ESP32 chip was tasked with sending a set of data.  In this case, it was temperature and humidity, which were taken from the DHT11 sensor connected to the ESP32 module. This data was then sent to the smartphone using BLE. An application was written for the smartphone in Kotlin that received the data and presented it in a graph.

## Stepper Motor
The purpose of this program was to control an M42SP-7 stepper motor. The program was written in C# and performed functions such as:
* wave motion 
* full step motion
* half-step motion.
* motor rotation by a certain angle
* Rotation of the motor by a certain number of steps.

## GPS
The program was written in Java as a mobile application, it implemented functions such as:
* determining the phone's position on a map
* sending and receiving location via BLE channel. 

## Webcam
The purpose of the program was to operate a webcam. The application was written in C# and implemented functions such as:
* displaying a preview of the camera image
* changing camera settings
* saving camera image as *.jpg picture
* saving camera image as a movie in *.avi format
* motion detection. 
