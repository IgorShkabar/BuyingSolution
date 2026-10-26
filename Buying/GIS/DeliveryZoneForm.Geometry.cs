/** 
 * Copyright (c) 2026 Igor Shkabar. All rights reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Buying.GIS
{
    internal partial class DeliveryZoneForm
    {
        // Code for mathematical coordinate transformations.
        private PointF ScreenToWorld(Point screenPt)
        {
            return new PointF(
                (screenPt.X - _mapOffset.X) / _zoomlevel,
                (screenPt.Y - _mapOffset.Y) / _zoomlevel
            );
        }

        private PointF WorldToScreen(PointF worldPt)
        {
            return new PointF(
                (worldPt.X * _zoomlevel) + _mapOffset.X,
                (worldPt.Y * _zoomlevel) + _mapOffset.Y
            );
        }

        // Math & GIS Transformations 
        
        private PointF LatLonToWebMercator(float lat, float lon)
        {
            float clientWidth = ClientSize.Width > 0 ? ClientSize.Width : 818f;
            float clientHeight = ClientSize.Height > 0 ? ClientSize.Height : 497f;

            // If the longitude is stored without a minus sign, we adjust it to the range used 
            // for the United States in the Western Hemisphere (from -125 to -67).
            
            float adjustedLon = lon > 0 ? -lon : lon;

            // We calculate a uniform scale so that the map does not become distorted.
            float scale = Math.Min(clientWidth / (-67f - (-125f)), clientHeight / (49f - 24f)) * 0.8f;

            // Center the map in the window
            float offsetX = (clientWidth - (-67f - (-125f)) * scale) / 2f;
            float offsetY = (clientHeight - (49f - 24f) * scale) / 2f;

            float screenX = (lon - (-125f)) * scale + offsetX;
            // We invert Y, since in GDI+ the Y-axis points downward, whereas latitude points upward.
            float screenY = (49f - lat) * scale + offsetY;

            return new PointF(screenX, screenY);
        }

        private PointF NormalizeMercatorToScreen(float mx, float my)
        {
            float screenX = (mx + 14000000f) / 18000f;
            float screenY = (6000000f - my) / 18000f;
            return new PointF(screenX, screenY);
        }

        private double CalculateHaversineDistance(PointF pt1, PointF pt2)
        {
            //double r = Constants.EarthConst.RadiusInKilometers; // 6371.0088;
            double dLat = (pt2.X - pt1.X) * Math.PI / 180.0;
            double dLon = (pt2.Y - pt1.Y) * Math.PI / 180.0;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(pt1.X * Math.PI / 180.0) * Math.Cos(pt2.X * Math.PI / 180.0) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return
                Constants.EarthConst.RadiusInKilometers * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        // Converting screen coordinates to geographic coordinates (Lat/Lon)
        // To handle a map click, add a method to convert Web Mercator 
        // coordinates back into latitude and longitude:
        private PointF WebMercatorToLatLon(PointF screenPt)
        {
            float mx = screenPt.X * 18000f - 14000000f;
            float my = 6000000f - (screenPt.Y * 18000f);

            double lon = (mx / 20037508.34) * 180.0;
            double lat = (my / 20037508.34) * 180.0;
            lat = 180.0 / Math.PI * (2.0 * Math.Atan(Math.Exp(lat * Math.PI / 180.0)) - Math.PI / 2.0);

            return new PointF((float)lat, (float)lon);
        }

        private void CenterMapOnPoint(PointF targetWorldPt)
        {
            _zoomlevel = 2.0f;
            int panelWidth = _rightPanel != null ? _rightPanel.Width : 370;

            int mapCenterX = (this.ClientSize.Width - panelWidth) / 2;
            int mapCenterY = this.ClientSize.Height / 2;

            _mapOffset.X = mapCenterX - (targetWorldPt.X * _zoomlevel);
            _mapOffset.Y = mapCenterY - (targetWorldPt.Y * _zoomlevel);
            this.Invalidate();
        }
    }
}

