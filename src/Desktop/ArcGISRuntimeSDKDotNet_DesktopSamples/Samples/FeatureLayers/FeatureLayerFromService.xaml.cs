﻿using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstrates the Feature Layer. Feature layers allow you to easily plot layers from a feature service on a map. As the sample XAML shows, this simply requires declaring a FeatureLayer element in the Map's layers collection and specifying the URL of the its ServiceFeatureTable attribute to the feature service layer.
    /// </summary>
    /// <title>Feature Layer From Service</title>
	/// <category>Layers</category>
	/// <subcategory>Feature Layers</subcategory>
	public partial class FeatureLayerFromService : UserControl
    {
        public FeatureLayerFromService()
        {
            InitializeComponent();

            // Note: code to create feature layer from a feature service
            //CreateFeatureLayer();
        }

        private async void CreateFeatureLayer()
        {
            try
            {
                var gdbFeatureServiceTable = await ServiceFeatureTable.OpenAsync(
                    new Uri("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Earthquakes/EarthquakesFromLastSevenDays/FeatureServer/0"));

				MyMapView.Map.Layers.Add(new FeatureLayer(gdbFeatureServiceTable) { ID = "featureLayer" });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Cannot initialize FeatureLayer: {0}", ex.ToString());
            }
        }
    }
}
