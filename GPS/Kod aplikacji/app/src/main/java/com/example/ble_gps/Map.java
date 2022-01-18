package com.example.ble_gps;

import android.content.Context;
import android.preference.PreferenceManager;

import org.osmdroid.api.IMapController;
import org.osmdroid.config.Configuration;
import org.osmdroid.tileprovider.tilesource.TileSourceFactory;
import org.osmdroid.util.GeoPoint;
import org.osmdroid.views.MapView;
import org.osmdroid.views.overlay.Marker;

public class Map {

    private IMapController mapController;
    private MapView map;
    Marker startMarker;

    Map(MapView map, Context ctx) {

        this.map = map;
        map.setTileSource(TileSourceFactory.MAPNIK);
        map.setMultiTouchControls(true);

        Configuration.getInstance().load(ctx, PreferenceManager.getDefaultSharedPreferences(ctx));
        mapController = map.getController();
        mapController.setZoom(12.0);

        startMarker = new Marker(map);
    }

    public void setMapController(String coordinates) {
        String[] results = coordinates.split(", ");

        double aLatitude = Double.parseDouble(results[0]);
        double aLongitude = Double.parseDouble(results[1]);

        GeoPoint geoPoint = new GeoPoint(aLatitude, aLongitude);


        startMarker.setPosition(geoPoint);
        startMarker.setAnchor(Marker.ANCHOR_CENTER, Marker.ANCHOR_BOTTOM);
        map.getOverlays().add(startMarker);

        mapController.animateTo(geoPoint);

    }
}
