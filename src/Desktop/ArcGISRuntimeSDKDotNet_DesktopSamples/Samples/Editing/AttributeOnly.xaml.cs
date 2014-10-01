﻿using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates how to selectively import and update feature attributes while using dynamic layer to render the features.
    /// </summary>
    /// <title>Attribute Only</title>
    /// <category>Editing</category>
    public partial class AttributeOnly : UserControl
    {
        private Grid formGrid;
        private ServiceFeatureTable table;

        public AttributeOnly()
        {
            InitializeComponent();
        }

        private ArcGISDynamicMapServiceLayer GetArcGISDynamicMapServiceLayer()
        {
            if (MyMapView.Map == null || MyMapView.Map.Layers == null)
                return null;
            return MyMapView.Map.Layers["PoolPermitDynamicLayer"] as ArcGISDynamicMapServiceLayer;
        }

        private GraphicsLayer GetGraphicsLayer()
        {
            if (MyMapView.Map == null || MyMapView.Map.Layers == null)
                return null;
            return MyMapView.Map.Layers["PoolPermit"] as GraphicsLayer;
        }

        private void ClearLocalGraphics()
        {
            var layer = GetGraphicsLayer();
            if (layer != null && layer.Graphics != null)
                layer.Graphics.Clear();
        }

        private void BuildAttributeEditor()
        {
            if (table == null || table.ServiceInfo == null || table.ServiceInfo.Fields == null)
                return;
            // Builds the Attribute Editor based on FieldInfo (i.e. Editable, Domain, Length, DataType)
            // For better validation and customization support use FeatureDataForm from the Toolkit. 
            var itemtTemplate = this.Resources["MyItemTemplate"] as DataTemplate;
            formGrid = new Grid() { Margin = new Thickness(2d) };
            formGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            formGrid.ColumnDefinitions.Add(new ColumnDefinition());
            var fieldCount = table.ServiceInfo.Fields.Count + 1; // Fields + Apply/Close button
            for (int i = 0; i < fieldCount; i++)
                formGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            int row = 0;
            foreach (var field in table.ServiceInfo.Fields)
            {
                var label = new TextBlock() { Text = field.Alias ?? field.Name, Margin = new Thickness(2d) };
                label.SetValue(Grid.RowProperty, row);
                formGrid.Children.Add(label);
                FrameworkElement value = null;
                // This binding will be resolved once the DataContext for formGrid is set to feature.
                var binding = new Binding(string.Format("Attributes[{0}]", field.Name));
                if (field.IsEditable)
                {
                    binding.Mode = BindingMode.TwoWay;
                    // This service only contains CodedValueDomain.
                    // Depending on your service, you might consider handling item selection for:
                    // RangeDomain and FeatureTypes.
                    if (field.Domain is CodedValueDomain)
                    {
                        value = new ComboBox() { ItemTemplate = itemtTemplate, Margin = new Thickness(2d) };
                        ((ComboBox)value).ItemsSource = ((CodedValueDomain)field.Domain).CodedValues;
                        binding.Converter = this.Resources["KeyValueConverter"] as KeyValueConverter;
                        binding.ConverterParameter = ((ComboBox)value).ItemsSource;
                        ((ComboBox)value).SetBinding(ComboBox.SelectedItemProperty, binding);
                    }
                    else
                    {
                        value = new TextBox() { Margin = new Thickness(2d) };
                        // Fields of DataType other than string will also need a converter.
                        ((TextBox)value).SetBinding(TextBox.TextProperty, binding);
                        if (field.Length.HasValue)
                            ((TextBox)value).MaxLength = field.Length.Value;
                    }
                }
                else
                {
                    value = new TextBlock() { Margin = new Thickness(2d) };
                    ((TextBlock)value).SetBinding(TextBlock.TextProperty, binding);
                }
                value.SetValue(Grid.ColumnProperty, 1);
                value.SetValue(Grid.RowProperty, row);
                formGrid.Children.Add(value);
                row++;
            }
            var buttonGrid = new Grid() { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(2d) };
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            buttonGrid.SetValue(Grid.ColumnSpanProperty, 2);
            buttonGrid.SetValue(Grid.RowProperty, row);
            var applyButton = new Button() { Content = "Apply", Margin = new Thickness(2d) };
            applyButton.Click += ApplyButton_Click;
            buttonGrid.Children.Add(applyButton);
            var closeButton = new Button() { Content = "Close", Margin = new Thickness(2d) };
            closeButton.SetValue(Grid.ColumnProperty, 1);
            closeButton.Click += CloseButton_Click;
            buttonGrid.Children.Add(closeButton);
            formGrid.Children.Add(buttonGrid);
        }

        private async void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.LoadError != null || !(e.Layer is ArcGISDynamicMapServiceLayer))
                return;
            // Creates the table based on visible layer from dynamic layer.
            var dynamicLayer = (ArcGISDynamicMapServiceLayer)e.Layer;
            var layerID = dynamicLayer.VisibleLayers.FirstOrDefault();
            var featureServiceUri = dynamicLayer.ServiceUri.Replace("MapServer", "FeatureServer");
            featureServiceUri = string.Format("{0}/{1}", featureServiceUri, layerID);
            string message = null;
            try
            {
                table = await ServiceFeatureTable.OpenAsync(new Uri(featureServiceUri), null, MyMapView.SpatialReference);
                table.OutFields = OutFields.All;
                if (!table.IsInitialized)
                    await table.InitializeAsync();
                BuildAttributeEditor();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }

        private object ConvertToDataType(object value, Type dataType, Domain domain)
        {
            if (value == null || dataType == value.GetType())
                return value;
            if (domain is CodedValueDomain)
            {
                var cvd = (CodedValueDomain)domain;
                var stringValue = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (cvd.CodedValues != null)
                    return cvd.CodedValues.FirstOrDefault(kvp => kvp.Value == stringValue).Key;
            }
            if (dataType == typeof(int))
                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            if (dataType == typeof(string))
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            return value;
        }

        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            var dynamicLayer = GetArcGISDynamicMapServiceLayer();
            var layer = GetGraphicsLayer();
            if (dynamicLayer == null || layer == null || table == null ||
                table.ServiceInfo == null || table.ServiceInfo.Fields == null)
                return;
            ClearLocalGraphics();
            // Perform an identify to pull graphic into local layer.
            var mapPoint = MyMapView.ScreenToLocation(e.Position);
            var parameters = new IdentifyParameters(mapPoint, MyMapView.Extent, 2, (int)MyMapView.ActualHeight, (int)MyMapView.ActualWidth);
            var task = new IdentifyTask(new Uri(dynamicLayer.ServiceUri));
            string message = null;
            Graphic graphic = null;
            try
            {
                var result = await task.ExecuteAsync(parameters);
                if (result == null || result.Results == null || result.Results.Count < 1)
                    return;
                var item = result.Results[0];
                graphic = item.Feature as Graphic;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
            if (graphic == null)
                return;
            // IdentifyResult use Field.Alias.
            // Ensure that correct data type is stored as Field.Name.
            foreach (var field in table.ServiceInfo.Fields)
            {
                if (graphic.Attributes.ContainsKey(field.Alias))
                {
                    var value = graphic.Attributes[field.Alias];
                    graphic.Attributes[field.Name] = ConvertToDataType(value, field.DataType, field.Domain);
                }
            }
            graphic.IsSelected = true;
            // Add selected feature to local layer.
            layer.Graphics.Add(graphic);
            var dataForm = new Window() { Content = formGrid, Height = 300, Width = 500, Title = "Attribute Editor" };
            dataForm.DataContext = graphic;
            dataForm.Show();
        }

        private async Task UpdateFeatureAttributesAsync(Graphic graphic)
        {
            if (graphic == null || table == null || table.ServiceInfo == null ||
                table.ServiceInfo.Fields == null || string.IsNullOrWhiteSpace(table.ObjectIDField) ||
                !graphic.Attributes.ContainsKey(table.ObjectIDField))
                return;            
            // Query for the feature and update its attribute.           
            var featureID = Convert.ToInt64(graphic.Attributes[table.ObjectIDField], CultureInfo.InvariantCulture);
            var feature = await table.QueryAsync(featureID);
            if (feature == null)
                return;
            foreach (var field in table.ServiceInfo.Fields)
            {
                if (field.IsEditable && graphic.Attributes.ContainsKey(field.Name))
                    feature.Attributes[field.Name] = graphic.Attributes[field.Name];
            }
            await table.UpdateAsync(feature);
            if (!table.HasEdits)
                return;
            // Submit the edits to server.
            await table.ApplyEditsAsync();
            // Should change in attribute caused a change in symbology (i.e. feature type)
            // Invalidate dynamiclayer to retrieve a new server image.
            var dynamicLayer = GetArcGISDynamicMapServiceLayer();
            if (dynamicLayer != null)
                dynamicLayer.Invalidate();
        }

        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            var graphic = (Graphic)((Button)sender).DataContext;
            string message = null;
            try
            {
                var editPrompt = MessageBox.Show("Do you want to apply the changes to your database?", "Apply edits", MessageBoxButton.OKCancel);
                if (editPrompt == MessageBoxResult.OK)
                    await UpdateFeatureAttributesAsync(graphic);
                ClearLocalGraphics();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (formGrid != null && formGrid.Parent is Window)
            {
                var dataForm = (Window)formGrid.Parent;
                dataForm.Close();
            }
        }
    }
}