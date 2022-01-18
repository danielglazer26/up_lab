package com.example.ble_gps;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothGatt;
import android.bluetooth.BluetoothGattCallback;
import android.bluetooth.BluetoothGattCharacteristic;
import android.bluetooth.BluetoothGattDescriptor;
import android.bluetooth.BluetoothGattService;
import android.bluetooth.BluetoothManager;
import android.bluetooth.BluetoothProfile;
import android.bluetooth.le.BluetoothLeScanner;
import android.bluetooth.le.ScanCallback;
import android.bluetooth.le.ScanFilter;
import android.bluetooth.le.ScanResult;
import android.bluetooth.le.ScanSettings;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Build;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.os.ParcelUuid;
import android.util.Log;
import android.view.View;
import android.widget.CompoundButton;
import android.widget.ScrollView;
import android.widget.TextView;

import androidx.annotation.RequiresApi;
import androidx.appcompat.app.AppCompatActivity;

import com.google.android.material.switchmaterial.SwitchMaterial;

import org.osmdroid.views.MapView;

import java.beans.PropertyChangeEvent;
import java.beans.PropertyChangeListener;
import java.beans.PropertyChangeSupport;
import java.nio.charset.Charset;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.Observable;
import java.util.Observer;
import java.util.UUID;


public class CentralActivity extends AppCompatActivity {

    private final String SERVICE_UUID = "25AE1441-05D3-4C5B-8281-93D4E07420CF";
    private final String CHAR_FOR_INDICATE_UUID = "25AE1444-05D3-4C5B-8281-93D4E07420CF";
    private final String CCC_DESCRIPTOR_UUID = "00002902-0000-1000-8000-00805f9b34fb";

    enum BLELifecycleState {
        Disconnected,
        Scanning,
        Connecting,
        ConnectedDiscovering,
        ConnectedSubscribing,
        Connected
    }

    private ScanSettings scanSettings;

    private ScanSettings scanSettingsBeforeM = new ScanSettings.Builder()
            .setScanMode(ScanSettings.SCAN_MODE_BALANCED)
            .setReportDelay(0)
            .build();

    @RequiresApi(Build.VERSION_CODES.M)
    private ScanSettings scanSettingsSinceM = new ScanSettings.Builder()
            .setScanMode(ScanSettings.SCAN_MODE_BALANCED)
            .setCallbackType(ScanSettings.CALLBACK_TYPE_FIRST_MATCH)
            .setMatchMode(ScanSettings.MATCH_MODE_AGGRESSIVE)
            .setNumOfMatches(ScanSettings.MATCH_NUM_ONE_ADVERTISEMENT)
            .setReportDelay(0)
            .build();

    private GPS mapCentral;
    private MapView mapView;

    private BluetoothLeScanner bleScanner;
    private BluetoothGattCharacteristic characteristicForIndicate;
    private BLELifecycleState lifecycleState = BLELifecycleState.Disconnected;
    private BluetoothGatt connectedGatt = null;
    private BluetoothGattCallback gattCallback;
    private BroadcastReceiver bleOnOffListener = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            switch (intent.getIntExtra(BluetoothAdapter.EXTRA_STATE, BluetoothAdapter.STATE_OFF)) {
                case BluetoothAdapter.STATE_ON:
                    appendLog("onReceive: Bluetooth ON");
                    if (lifecycleState == BLELifecycleState.Disconnected) {
                        bleRestartLifecycle();
                    }
                    break;
                case BluetoothAdapter.STATE_OFF:
                    appendLog("onReceive: Bluetooth OFF");
                    bleEndLifecycle();
                    break;
            }
        }
    };

    private SwitchMaterial switchConnect;
    private TextView textViewIndicateValue;
    private TextView textViewLog;
    private TextView textViewLifecycleState;
    private ScrollView scrollViewLog;
    private Boolean isScanning = false;

    private ScanCallback scanCallback;
    private ScanFilter scanFilter = new ScanFilter.Builder()
            .setServiceUuid(new ParcelUuid(UUID.fromString(SERVICE_UUID))).build();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_central);
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);

        switchConnect = findViewById(R.id.switchConnect);
        textViewLog = findViewById(R.id.textViewLog);
        textViewLifecycleState = findViewById(R.id.textViewLifecycleState);
        scrollViewLog = findViewById(R.id.scrollViewLog);
        textViewIndicateValue = findViewById(R.id.textViewIndicateValue);

        mapView = findViewById(R.id.map);
        mapCentral = new GPS(mapView, CentralActivity.this);
        mapCentral.setCoordinates("1, 1");


        scanCallback = new ScanCallback() {
            @Override
            public void onScanResult(int callbackType, ScanResult result) {
                String name = result.getScanRecord().getDeviceName();
                appendLog("onScanResult name=" + name + "address=" + result.getDevice().getAddress());
                safeStopBleScan();
                lifecycleState = BLELifecycleState.Connecting;
                textViewLifecycleState.setText("Connecting");
                result.getDevice().connectGatt(CentralActivity.this, false, gattCallback);
            }

            @Override
            public void onBatchScanResults(List<ScanResult> results) {
                appendLog("onBatchScanResults, ignoring");
            }

            @Override
            public void onScanFailed(int errorCode) {
                appendLog("onScanFailed errorCode=" + errorCode);
                safeStopBleScan();
                lifecycleState = BLELifecycleState.Disconnected;
                textViewLifecycleState.setText("Disconnected");
                bleRestartLifecycle();
            }
        };

        gattCallback = new BluetoothGattCallback() {
            @Override
            public void onConnectionStateChange(BluetoothGatt gatt, int status, int newState) {
                String deviceAddress = gatt.getDevice().getAddress();

                if (status == BluetoothGatt.GATT_SUCCESS) {
                    if (newState == BluetoothProfile.STATE_CONNECTED) {
                        appendLog("Connected to" + deviceAddress);


                        new Handler(Looper.getMainLooper()).post(() -> {
                            lifecycleState = BLELifecycleState.ConnectedDiscovering;
                            textViewLifecycleState.setText("ConnectedDiscovering");
                            gatt.discoverServices();
                        });
                    } else if (newState == BluetoothProfile.STATE_DISCONNECTED) {
                        appendLog("Disconnected from" + deviceAddress);
                        setConnectedGattToNull();
                        gatt.close();
                        lifecycleState = BLELifecycleState.Disconnected;
                        textViewLifecycleState.setText("Disconnected");
                        bleRestartLifecycle();
                    }
                } else {

                    appendLog("ERROR: onConnectionStateChange status=" + status + "deviceAddress=" + deviceAddress + "disconnecting");

                    setConnectedGattToNull();
                    gatt.close();
                    lifecycleState = BLELifecycleState.Disconnected;
                    textViewLifecycleState.setText("Disconnected");
                    bleRestartLifecycle();
                }
            }

            @Override
            public void onServicesDiscovered(BluetoothGatt gatt, int status) {
                appendLog("onServicesDiscovered services.count= " +gatt.getServices().size() + " status= " + status);

                if (status == 129) {
                    appendLog("ERROR: status=129 (GATT_INTERNAL_ERROR), disconnecting");
                    gatt.disconnect();
                    return;
                }

                BluetoothGattService service;
                try {
                    service = gatt.getService(UUID.fromString(SERVICE_UUID));
                } catch (Exception e) {
                    appendLog("ERROR: Service not found" + SERVICE_UUID + " disconnecting");
                    gatt.disconnect();
                    return;
                }

                connectedGatt = gatt;
                characteristicForIndicate = service.getCharacteristic(UUID
                        .fromString(CHAR_FOR_INDICATE_UUID));

                lifecycleState = BLELifecycleState.ConnectedSubscribing;
            textViewLifecycleState.setText("ConnectedSubscribing");
                subscribeToIndications(characteristicForIndicate, gatt);

            }

            @Override
            public void onCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic) {
                if (characteristic.getUuid().equals(UUID.fromString(CHAR_FOR_INDICATE_UUID))) {
                    byte [] a = characteristic.getValue();
                    String strValue = new String(a, Charset.defaultCharset());
                    appendLog("onCharacteristicChanged value= " + strValue);
                    runOnUiThread(() ->{
                            textViewIndicateValue.setText(strValue);
                            mapCentral.setCoordinates(strValue);
                    });
                } else {
                    appendLog("onCharacteristicChanged unknown uuid: " + characteristic.getUuid());
                }
            }

            @Override
            public void onDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, int status) {
                if ((descriptor.getCharacteristic().getUuid()).equals(UUID.fromString(CHAR_FOR_INDICATE_UUID))) {
                    if (status != BluetoothGatt.GATT_SUCCESS) {
                        appendLog("ERROR: onDescriptorWrite status="
                                + status
                                + " "
                                + descriptor.getUuid()
                                + " "
                                + descriptor.getCharacteristic().getUuid());
                    }

                    lifecycleState = BLELifecycleState.Connected;
                    textViewLifecycleState.setText("Connected");
                } else {
                    appendLog("onDescriptorWrite unknown uuid " + descriptor.getCharacteristic().getUuid());
                }
            }


        };


        switchConnect.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
            @Override
            public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                if (isChecked) {
                    IntentFilter filter = new IntentFilter(BluetoothAdapter.ACTION_STATE_CHANGED);
                    if(start())
                        registerReceiver(bleOnOffListener, filter);
                } else
                    unregisterReceiver(bleOnOffListener);
                bleRestartLifecycle();
            }
        });

        appendLog("MainActivity.onCreate");

    }

    private boolean start() {
        BluetoothManager bluetoothManager = (BluetoothManager) getSystemService(Context.BLUETOOTH_SERVICE);
        BluetoothAdapter bluetoothAdapter = bluetoothManager.getAdapter();
        if (enableBluetooth(bluetoothAdapter)) {
            bleScanner = bluetoothAdapter.getBluetoothLeScanner();
            scanSettings = scanSettingsSinceM;
            return  true;
        }
        return false;
    }

    public void onTapClearLog(View view) {
        textViewLog.setText("Logs:");
        appendLog("log cleared");
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

    private void bleRestartLifecycle() {
        runOnUiThread(() -> {
            if (switchConnect.isChecked()) {
                if (connectedGatt == null) {
                    prepareAndStartBleScan();
                } else {
                    connectedGatt.disconnect();
                }
            } else {
                bleEndLifecycle();
            }
        });
    }

    private void bleEndLifecycle() {
        safeStopBleScan();
        if (connectedGatt != null)
            connectedGatt.close();
        setConnectedGattToNull();
        lifecycleState = BLELifecycleState.Disconnected;
        textViewLifecycleState.setText("Disconnected");
    }

    private void setConnectedGattToNull() {
        connectedGatt = null;
        characteristicForIndicate = null;
    }

    private void prepareAndStartBleScan() {
        safeStartBleScan();
    }

    private void safeStartBleScan() {
        if (isScanning) {
            appendLog("Already scanning");
            return;
        }

        String serviceFilter = scanFilter.getServiceUuid().getUuid().toString();
        appendLog("Starting BLE scan, filter: " + serviceFilter);

        isScanning = true;
        lifecycleState = BLELifecycleState.Scanning;
        textViewLifecycleState.setText("Scanning");

        List<ScanFilter> scanFilterList = new ArrayList<>();
        scanFilterList.add(scanFilter);

        bleScanner.startScan(scanFilterList, scanSettings, scanCallback);
    }

    private void safeStopBleScan() {
        if (!isScanning) {
            appendLog("Already stopped");
            return;
        }

        appendLog("Stopping BLE scan");
        isScanning = false;
        bleScanner.stopScan(scanCallback);
    }

    private void subscribeToIndications(BluetoothGattCharacteristic characteristic, BluetoothGatt gatt) {
        UUID cccdUuid = UUID.fromString(CCC_DESCRIPTOR_UUID);
        BluetoothGattDescriptor cccDescriptor = characteristic.getDescriptor(cccdUuid);

        if (!gatt.setCharacteristicNotification(characteristic, true)) {
            appendLog("ERROR: setNotification(true) failed for" + characteristic.getUuid());
            return;
        }
        cccDescriptor.setValue(BluetoothGattDescriptor.ENABLE_INDICATION_VALUE);
        gatt.writeDescriptor(cccDescriptor);
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
