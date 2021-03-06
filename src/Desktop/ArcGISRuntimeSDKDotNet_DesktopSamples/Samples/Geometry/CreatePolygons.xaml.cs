﻿using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates how to create polygon geometries, attach them to graphics and display them on the map. Polygon geometry objects are used to store geographic polygons.
    /// </summary>
    /// <title>Create Polygons</title>
	/// <category>Geometry</category>
	public partial class CreatePolygons : UserControl
    {
		private GraphicsOverlay _graphicsOverlay;

        /// <summary>Construct Create Polygons sample control</summary>
        public CreatePolygons()
        {
            InitializeComponent();
			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
			MyMapView.NavigationCompleted += MyMapView_NavigationCompleted;
		}

		// Create Polygon graphics on the map in the center and the center of four equal quadrants
		private void MyMapView_NavigationCompleted(object sender, EventArgs e)
		{
			MyMapView.NavigationCompleted -= MyMapView_NavigationCompleted;
			try
			{
				var height = MyMapView.Extent.Height / 4;
				var width = MyMapView.Extent.Width / 4;
				var length = width / 4;
				var center = MyMapView.Extent.GetCenter();

				var topLeft = new MapPoint(center.X - width, center.Y + height, MyMapView.SpatialReference);
				var topRight = new MapPoint(center.X + width, center.Y + height, MyMapView.SpatialReference);
				var bottomLeft = new MapPoint(center.X - width, center.Y - height, MyMapView.SpatialReference);
				var bottomRight = new MapPoint(center.X + width, center.Y - height, MyMapView.SpatialReference);

				var redSymbol = new SimpleFillSymbol() { Color = System.Windows.Media.Colors.Red };
				var blueSymbol = new SimpleFillSymbol() { Color = System.Windows.Media.Colors.Blue };

				_graphicsOverlay.Graphics.Add(new Graphic() { Geometry = CreatePolygonBox(center, length), Symbol = blueSymbol });
				_graphicsOverlay.Graphics.Add(new Graphic() { Geometry = CreatePolygonBox(topLeft, length), Symbol = redSymbol });
				_graphicsOverlay.Graphics.Add(new Graphic() { Geometry = CreatePolygonBox(topRight, length), Symbol = redSymbol });
				_graphicsOverlay.Graphics.Add(new Graphic() { Geometry = CreatePolygonBox(bottomLeft, length), Symbol = redSymbol });
				_graphicsOverlay.Graphics.Add(new Graphic() { Geometry = CreatePolygonBox(bottomRight, length), Symbol = redSymbol });
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error occured : " + ex.Message, "Create Polygons Sample");
			}  
        }

        // Creates a square polygon with a hole centered at the given point
        private Polygon CreatePolygonBox(MapPoint center, double length)
        {
            var halfLen = length / 2.0;

            PointCollection points = new PointCollection();
			points.Add(new MapPoint(center.X - halfLen, center.Y + halfLen));
			points.Add(new MapPoint(center.X + halfLen, center.Y + halfLen));
			points.Add(new MapPoint(center.X + halfLen, center.Y - halfLen));
			points.Add(new MapPoint(center.X - halfLen, center.Y - halfLen));
			points.Add(new MapPoint(center.X - halfLen, center.Y + halfLen));

            halfLen /= 3;
			PointCollection pointsHole = new PointCollection();
			pointsHole.Add(new MapPoint(center.X - halfLen, center.Y + halfLen));
			pointsHole.Add(new MapPoint(center.X - halfLen, center.Y - halfLen));
			pointsHole.Add(new MapPoint(center.X + halfLen, center.Y - halfLen));
			pointsHole.Add(new MapPoint(center.X + halfLen, center.Y + halfLen));
			pointsHole.Add(new MapPoint(center.X - halfLen, center.Y + halfLen));

			return new Polygon(
				new List<PointCollection> { points, pointsHole }, 
				MyMapView.SpatialReference);
        }
    }
}
