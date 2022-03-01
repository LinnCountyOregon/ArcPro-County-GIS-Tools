using ArcGIS.Core.CIM;
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taxlot_Search
{
    /// <summary>
    /// Represents the ComboBox
    /// </summary>
    internal class LoadImagesCbox : ComboBox
    {

        private bool _isInitialized;

        /// <summary>
        /// Combo Box constructor
        /// </summary>
        public LoadImagesCbox()
        {
            UpdateCombo();
        }

        /// <summary>
        /// Updates the combo box with all the items.
        /// </summary>

        private void UpdateCombo()
        {
            // TODO – customize this method to populate the combobox with your desired items  
            if (_isInitialized)
                SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox


            if (!_isInitialized)
            {
                Clear();

                Add(new ComboBoxItem("Select Image"));
                Add(new ComboBoxItem("Linn 2021"));
                Add(new ComboBoxItem("Linn 2021 Subareas"));
                Add(new ComboBoxItem("Linn 2017"));
                Add(new ComboBoxItem("Linn 2017 Subareas"));
               // Add(new ComboBoxItem("Linn 2016"));
                Add(new ComboBoxItem("Linn 2014"));
               // Add(new ComboBoxItem("Linn 2014 Subareas"));

                _isInitialized = true;
            }


            Enabled = true; //enables the ComboBox
            SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox

        }

        /// <summary>
        /// The on comboBox selection change event. 
        /// </summary>
        /// <param name="item">The newly selected combo box item</param>
        protected override void OnSelectionChange(ComboBoxItem item)
        {

            if (item == null)
                return;

            if (string.IsNullOrEmpty(item.Text))
                return;

            // TODO  Code behavior when selection changes.    
            if (item.Text == "Linn 2021")
                LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/Orthos2021_LinnCounty/MapServer");
            else if (item.Text == "Linn 2021 Subareas")
                Load2021Subareas();
            else if (item.Text == "Linn 2017")
                LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/Orthos2017_webM/MapServer");
            else if (item.Text == "Linn 2017 Subareas")
                Load2017Subareas();
            else if (item.Text == "Linn 2014")
                LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/Pub_Linn_2014_Orthos/MapServer");

            SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox
        }

        public static async void Load2021Subareas()
        {
            await LoadDataClass.CheckForMap();
            LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/Ortho_2021_SweetHome/MapServer");
            LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/Ortho2021_Millersburg/MapServer");
            LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/Ortho_2021_Brownsville/MapServer");
            LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/Orthos_2021_Lyons_MillCity/MapServer");
        }
        public static async void Load2017Subareas()
        {
            await LoadDataClass.CheckForMap();
            LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/pub_ortho2017_6inch_Millersburg/MapServer");
            LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/pub_orthos2017_6inch_Harrisburg/MapServer");
            LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/pub_orthos2017_6inch_Scio/MapServer");
            LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/pub_orthos2017_6inch_Brownsville/MapServer");
        }
    }
}
