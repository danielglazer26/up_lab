package com.example.ble_gps;

import android.Manifest;
import android.content.Context;
import android.content.pm.PackageManager;
import android.location.Location;
import android.os.Looper;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.core.app.ActivityCompat;

import com.google.android.gms.location.FusedLocationProviderClient;
import com.google.android.gms.location.LocationCallback;
import com.google.android.gms.location.LocationRequest;
import com.google.android.gms.location.LocationResult;
import com.google.android.gms.location.LocationServices;
import com.google.android.gms.tasks.OnSuccessListener;

import org.osmdroid.views.MapView;

public class GPS {

    private FusedLocationProviderClient fusedLocationClient;
    private Location location;
    private TextView textView;
    private Map map;
    private PeripheralActivity peripheralActivity;
    private LocationCallback locationCallback;
    private LocationRequest locationRequest;

    GPS(PeripheralActivity peripheralActivity, TextView textView, MapView mapView, Context ctx) {

        this.peripheralActivity = peripheralActivity;
        this.textView = textView;

        map = new Map(mapView, ctx);
        fusedLocationClient = LocationServices.getFusedLocationProviderClient(peripheralActivity);

        startingLocation();

        locationRequest = LocationRequest.create();
        locationRequest.setInterval(1000);
        locationRequest.setFastestInterval(500);
        locationRequest.setPriority(LocationRequest.PRIORITY_HIGH_ACCURACY);

        locationCallback = new LocationCallback() {
            @Override
            public void onLocationResult(@NonNull LocationResult locationResult) {
                setLocation(locationResult.getLastLocation());
            }

        };

    }

    GPS(MapView mapView, Context ctx) {
        map = new Map(mapView, ctx);
    }

    public void setCoordinates(String coordinates) {
        map.setMapController(coordinates);
    }

    public void startingLocation() {
        if (ActivityCompat.checkSelfPermission(peripheralActivity,
                Manifest.permission.ACCESS_FINE_LOCATION) != PackageManager.PERMISSION_GRANTED
                && ActivityCompat.checkSelfPermission(peripheralActivity,
                Manifest.permission.ACCESS_COARSE_LOCATION) != PackageManager.PERMISSION_GRANTED)
            return;

        fusedLocationClient.getLastLocation().addOnSuccessListener(peripheralActivity, this::setLocation);
    }

    public void setLocation(Location location) {
        this.location = location;
        String l = locationToString();
        textView.setText(l);
        map.setMapController(l);
    }


    void onResume(Context context) {

        if (ActivityCompat.checkSelfPermission(context, Manifest.permission.ACCESS_FINE_LOCATION)
                != PackageManager.PERMISSION_GRANTED
                && ActivityCompat.checkSelfPermission(context, Manifest.permission.ACCESS_COARSE_LOCATION)
                != PackageManager.PERMISSION_GRANTED) {
            return;
        }

        fusedLocationClient.requestLocationUpdates(locationRequest,
                locationCallback, Looper.getMainLooper());
    }

    void onPause() {
        fusedLocationClient.removeLocationUpdates(locationCallback);
    }

    public String locationToString() {

        if (location != null)
            return location.getLatitude() + ", " + location.getLongitude();
        else
            return "1, 1";
    }
}
