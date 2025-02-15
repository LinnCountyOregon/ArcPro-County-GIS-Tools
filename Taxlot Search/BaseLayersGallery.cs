﻿using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Threading;
using CommonArcProAddin;

namespace Taxlot_Search
{
    internal class BaseLayersGallery : Gallery
    {
        private bool _isInitialized;

        protected override void OnDropDownOpened()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_isInitialized)
                return;

            // Add base layers to the gallery
            Add(new GalleryItem("Contours", "pack://application:,,,/Taxlot Search;component/Images/GalleryImages/AddContours64.png", "Add 5ft Contours to the map"));
            Add(new GalleryItem("Taxlots", "pack://application:,,,/Taxlot Search;component/Images/GalleryImages/AddTaxlots64.png", "Add Taxlots to the map"));
            Add(new GalleryItem("Addresses", "pack://application:,,,/Taxlot Search;component/Images/GalleryImages/AddAddresses64.png", "Add Addresses to the map"));
            Add(new GalleryItem("City Limits", "pack://application:,,,/Taxlot Search;component/Images/GalleryImages/AddCities64.png", "Add City Limits to the map"));
            Add(new GalleryItem("UGB", "pack://application:,,,/Taxlot Search;component/Images/GalleryImages/AddCities64.png", "Add UGB to the map"));
            Add(new GalleryItem("County Boundary", "pack://application:,,,/Taxlot Search;component/Images/GalleryImages/AddCountyBoundary64.png", "Add the county boundary to the map"));
            Add(new GalleryItem("Roads", "pack://application:,,,/Taxlot Search;component/Images/GalleryImages/AddRoads64.png", "Add county roads to the map"));
            Add(new GalleryItem("Zoning", "pack://application:,,,/Taxlot Search;component/Images/GalleryImages/AddZoning64.png", "Add county zoning to the map"));
            Add(new GalleryItem("Soils (code)", "pack://application:,,,/Taxlot Search;component/Images/GalleryImages/AddSoils64.png", "Add county soils to the map"));

            //Add 6 items to the gallery
            //for (int i = 0; i < 6; i++)
            //{
            //    string name = string.Format("Item {0}", i);
            //    Add(new GalleryItem(name, this.LargeImage != null ? ((ImageSource)this.LargeImage).Clone() : null, name));
            //}

            _isInitialized = true;

        }

        protected override async void OnClick(GalleryItem item)
        {
            //TODO - insert your code to manipulate the clicked gallery item here
            //ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("selected: " + item.Text);
            //System.Diagnostics.Debug.WriteLine("Remove this line after adding your custom behavior.");
            if (item.Text == "Contours")
                LoadContours();
            else if (item.Text == "Taxlots")
                await LoadDataClass.LoadDatasetToLayer(LoadDataClass.localGISdirectory, "taxlots");
            else if (item.Text == "Addresses")
                await LoadDataClass.LoadDatasetToLayer(LoadDataClass.localGISdirectory, "Address");
            else if (item.Text == "City Limits")
                await LoadDataClass.LoadDatasetToLayer(LoadDataClass.localGISdirectory, "EntireCities");
            else if (item.Text == "UGB")
                await LoadDataClass.LoadDatasetToLayer(LoadDataClass.localGISdirectory, "UGB");
            else if (item.Text == "County Boundary")
                await LoadDataClass.LoadDatasetToLayer(LoadDataClass.localGISdirectory, "countyline");
            else if (item.Text == "Roads")
                await LoadDataClass.LoadDatasetToLayer(LoadDataClass.localGISdirectory, "roads");
            else if (item.Text == "Zoning")
                await LoadDataClass.LoadLYRXToLayer(LoadDataClass.linnOrigLayerFiles + @"\Zoning_web_match.lyrx", "Zoning");
            else if (item.Text == "Soils (code)")
                await LoadDataClass.LoadDatasetToLayer(LoadDataClass.localGISdirectory, "Soils");

            base.OnClick(item);
        }

        public static async void LoadContours()
        {
            await LoadDataClass.CheckForMap();
            await LoadDataClass.LoadDatasetToLayer(LoadDataClass.localGISdirectory, "5ftnorth");
            await LoadDataClass.LoadDatasetToLayer(LoadDataClass.localGISdirectory, "5ftsouth");
            await LoadDataClass.LoadDatasetToLayer(LoadDataClass.localGISdirectory, "2fturban");
        }

    }
}
