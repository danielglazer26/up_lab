package com.example.ble_gps;

import androidx.appcompat.app.AppCompatActivity;

import android.bluetooth.le.*;
import android.bluetooth.*;
import android.os.ParcelUuid;
import android.bluetooth.BluetoothManager;
import android.bluetooth.BluetoothGatt;
import android.bluetooth.BluetoothGattCharacteristic;
import android.bluetooth.BluetoothGattDescriptor;
import android.bluetooth.BluetoothGattServer;
import android.bluetooth.BluetoothGattService;
import android.bluetooth.le.BluetoothLeAdvertiser;
import android.content.Context;
import android.os.Bundle;
import android.os.Handler;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ScrollView;
import android.widget.TextView;

import com.google.android.material.switchmaterial.SwitchMaterial;

import org.osmdroid.views.MapView;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Date;
import java.util.Locale;
import java.util.UUID;

public class PeripheralActivity extends AppCompatActivity {

    private final int ENABLE_BLUETOOTH_REQUEST_CODE = 1;
    private final int LOCATION_PERMISSION_REQUEST_CODE = 2;
    private final String SERVICE_UUID = "25AE1441-05D3-4C5B-8281-93D4E07420CF";
    private final String CHAR_FOR_INDICATE_UUID = "25AE1444-05D3-4C5B-8281-93D4E07420CF";
    private final String CCC_DESCRIPTOR_UUID = "00002902-0000-1000-8000-00805f9b34fb";

    private Button buttonSend;
    private TextView textViewLog;
    private TextView textViewConnectionState;
    private TextView textViewSubscribers;
    private TextView editTextCharForIndicate;
    private ScrollView scrollViewLog;
    private SwitchMaterial switchAdvertising;

    private Boolean isAdvertising = false;

    private BluetoothLeAdvertiser bleAdvertiser;
    private BluetoothManager bluetoothManager;
    private BluetoothGattServer gattServer = null;
    private BluetoothGattCharacteristic charForIndicate;

    private GPS mapPeripheral;
    private MapView mapView;


    private ArrayList<BluetoothDevice> subscribedDevices = new ArrayList<>();

    private AdvertiseSettings advertiseSettings = new AdvertiseSettings.Builder()
            .setAdvertiseMode(AdvertiseSettings.ADVERTISE_MODE_BALANCED)
            .setTxPowerLevel(AdvertiseSettings.ADVERTISE_TX_POWER_MEDIUM)
            .setConnectable(true)
            .build();

    private AdvertiseData advertiseData = new AdvertiseData.Builder()
            .setIncludeDeviceName(false)
            .addServiceUuid(new ParcelUuid(UUID.fromString(SERVICE_UUID)))
            .build();

    private AdvertiseCallback advertiseCallback = new AdvertiseCallback() {
        @Override
        public void onStartSuccess(AdvertiseSettings settingsInEffect) {
            appendLog("Advertise start success\n" + SERVICE_UUID);
        }
    };

    private BluetoothGattServerCallback gattServerCallback = new BluetoothGattServerCallback() {
        @Override
        public void onConnectionStateChange(BluetoothDevice device, int status, int newState) {
            runOnUiThread(() -> {
                if (newState == BluetoothProfile.STATE_CONNECTED) {
                    textViewConnectionState.setText("Connected");
                    appendLog("Central did connect");
                    buttonSend.setEnabled(true);

                } else {
                    textViewConnectionState.setText("Disconnected");
                    subscribedDevices.remove(device);
                    appendLog("Central did disconnect");
                    buttonSend.setEnabled(false);
                    updateSubscribersUI();
                }
            });
        }

        @Override
        public void onDescriptorReadRequest(BluetoothDevice device, int requestId, int offset, BluetoothGattDescriptor descriptor) {
            String log = "onDescriptorReadRequest";
            if (descriptor.getUuid().equals(UUID.fromString(CCC_DESCRIPTOR_UUID))) {
                byte[] returnValue;
                if (subscribedDevices.contains(device)) {
                    log += " CCCD response=ENABLE_NOTIFICATION";
                    returnValue = BluetoothGattDescriptor.ENABLE_NOTIFICATION_VALUE;
                } else {
                    log += " CCCD response=DISABLE_NOTIFICATION";
                    returnValue = BluetoothGattDescriptor.DISABLE_NOTIFICATION_VALUE;
                }
                gattServer.sendResponse(
                        device,
                        requestId,
                        BluetoothGatt.GATT_SUCCESS,
                        0,
                        returnValue
                );
            } else {
                log += " unknown uuid=" + descriptor.getUuid();
                gattServer.sendResponse(device, requestId, BluetoothGatt.GATT_FAILURE, 0, null);
            }
            appendLog(log);
        }

        @Override
        public void onDescriptorWriteRequest(BluetoothDevice device, int requestId, BluetoothGattDescriptor descriptor, boolean preparedWrite, boolean responseNeeded, int offset, byte[] value) {
            String strLog = "onDescriptorWriteRequest";
            if (descriptor.getUuid().equals(UUID.fromString(CCC_DESCRIPTOR_UUID))) {
                int status = BluetoothGatt.GATT_REQUEST_NOT_SUPPORTED;
                if (descriptor.getCharacteristic().getUuid().equals(UUID.fromString(CHAR_FOR_INDICATE_UUID))) {
                    if (Arrays.equals(value, BluetoothGattDescriptor.ENABLE_INDICATION_VALUE)) {
                        subscribedDevices.add(device);
                        status = BluetoothGatt.GATT_SUCCESS;
                        strLog += ", subscribed";
                    } else if (Arrays.equals(
                            value,
                            BluetoothGattDescriptor.DISABLE_NOTIFICATION_VALUE)) {
                        subscribedDevices.remove(device);
                        status = BluetoothGatt.GATT_SUCCESS;
                        strLog += ", unsubscribed";
                    }
                }
                if (responseNeeded) {
                    gattServer.sendResponse(device, requestId, status, 0, null);
                }
                updateSubscribersUI();
            } else {
                strLog += " unknown uuid=" + descriptor.getUuid();
                if (responseNeeded) {
                    gattServer.sendResponse(device, requestId, BluetoothGatt.GATT_FAILURE, 0, null);
                }
            }
            appendLog(strLog);
        }

        @Override
        public void onNotificationSent(BluetoothDevice device, int status) {
            appendLog("onNotificationSent status= " + status);
        }
    };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_peripheral);
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);

        buttonSend = findViewById(R.id.buttonSend);
        textViewLog = findViewById(R.id.textViewLog);
        textViewConnectionState = findViewById(R.id.textViewConnectionState);
        textViewSubscribers = findViewById(R.id.textViewSubscribers);
        editTextCharForIndicate = findViewById(R.id.editTextCharForIndicate);
        scrollViewLog = findViewById(R.id.scrollViewLog);
        switchAdvertising = findViewById(R.id.switchAdvertising);

        mapView = findViewById(R.id.map);
        mapPeripheral = new GPS(this, editTextCharForIndicate, mapView, this.getBaseContext());


        buttonSend.setEnabled(false);

        switchAdvertising.setOnCheckedChangeListener((buttonView, isChecked) -> {
            if (isChecked) {
                if (start())
                    prepareAndStartAdvertising();
            } else
                bleStopAdvertising();

        });


    }

    private void prepareAndStartAdvertising() {
        runOnUiThread(() -> bleStartAdvertising());
    }

    private void bleStartAdvertising() {
        isAdvertising = true;
        bleStartGattServer();
        bleAdvertiser.startAdvertising(advertiseSettings, advertiseData, advertiseCallback);
    }

    private void bleStopAdvertising() {
        if (isAdvertising) {
            bleStopGattServer();
            bleAdvertiser.stopAdvertising(advertiseCallback);
        }
        isAdvertising = false;
    }

    private void bleStartGattServer() {
        BluetoothGattServer gattServer = bluetoothManager.openGattServer(this, gattServerCallback);
        BluetoothGattService service = new BluetoothGattService(
                UUID.fromString(SERVICE_UUID),
                BluetoothGattService.SERVICE_TYPE_PRIMARY
        );
        charForIndicate = new BluetoothGattCharacteristic(
                UUID.fromString(CHAR_FOR_INDICATE_UUID),
                BluetoothGattCharacteristic.PROPERTY_INDICATE,
                BluetoothGattCharacteristic.PERMISSION_READ
        );
        BluetoothGattDescriptor charConfigDescriptor = new BluetoothGattDescriptor(
                UUID.fromString(CCC_DESCRIPTOR_UUID),
                (BluetoothGattDescriptor.PERMISSION_READ + BluetoothGattDescriptor.PERMISSION_WRITE)
        );
        charForIndicate.addDescriptor(charConfigDescriptor);

        service.addCharacteristic(charForIndicate);


        Boolean result = gattServer.addService(service);
        this.gattServer = gattServer;

        if (result)
            appendLog("addService " + "OK");
        else
            appendLog("addService " + "fail");


    }

    private void bleStopGattServer() {
        if (gattServer != null)
            gattServer.close();
        gattServer = null;
        appendLog("gattServer closed");
        runOnUiThread(() -> {
            textViewConnectionState.setText("Disconnected");
            buttonSend.setEnabled(false);
        });
    }


    private boolean start() {
        bluetoothManager = (BluetoothManager) getSystemService(Context.BLUETOOTH_SERVICE);
        BluetoothAdapter bluetoothAdapter = bluetoothManager.getAdapter();
        if (enableBluetooth(bluetoothAdapter)) {
            bleAdvertiser = bluetoothAdapter.getBluetoothLeAdvertiser();
            return true;
        }
        return false;
    }

    private void updateSubscribersUI() {
        String strSubscribers = subscribedDevices.size() + "subscribers";
        runOnUiThread(() -> {
            textViewSubscribers.setText(strSubscribers);
        });
    }

    public void onTapClearLog(View view) {
        textViewLog.setText("Logs:");
        appendLog("log cleared");
    }

    public void onTapSend(View view) {
        bleIndicate();
    }

    private void bleIndicate() {
        charForIndicate = gattServer.getService(UUID.fromString(SERVICE_UUID)).getCharacteristic(UUID.fromString(CHAR_FOR_INDICATE_UUID));

        String text = editTextCharForIndicate.getText().toString();
        charForIndicate.setValue(text);
        for (BluetoothDevice device : subscribedDevices) {
            appendLog("sending indication " + text);
            gattServer.notifyCharacteristicChanged(device, charForIndicate, true);
        }

    }

    private void appendLog(String message) {
        Log.d("appendLog", message);
        runOnUiThread(() -> {
            String strTime = new SimpleDateFormat("HH:mm:ss", Locale.getDefault()).format(new Date());
            textViewLog.setText(textViewLog.getText().toString() + "\n" + strTime + message);


            new Handler().postDelayed(() -> scrollViewLog.fullScroll(View.FOCUS_DOWN)
                    , 16);
        });
    }

    @Override
    protected void onDestroy() {
        bleStopAdvertising();
        super.onDestroy();
    }

    @Override
    protected void onResume() {
        super.onResume();
        mapPeripheral.onResume(this);
    }

    @Override
    protected void onPause() {
        super.onPause();
        mapPeripheral.onPause();
    }

    private boolean enableBluetooth(BluetoothAdapter bluetoothAdapter) {
        if (!bluetoothAdapter.isEnabled()) {
            bluetoothAdapter.enable();
            try {
                Thread.sleep(1000);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
            if (!bluetoothAdapter.isEnabled())
                enableBluetooth(bluetoothAdapter);
            return true;
        }
        return true;
    }


}