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
using CommonArcProAddin;

namespace Taxlot_Search
{
    /// <summary>
    /// Represents the ComboBox
    /// </summary>
    internal class LoadImagesHistoricalCbox : ComboBox
    {

        private bool _isInitialized;

        /// <summary>
        /// Combo Box constructor
        /// </summary>
        public LoadImagesHistoricalCbox()
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
             //   Add(new ComboBoxItem("Federal 2009"));
             //   Add(new ComboBoxItem("Fed 2009 half meter"));
             //   Add(new ComboBoxItem("Linn 2008"));
                Add(new ComboBoxItem("Linn 2005"));
                Add(new ComboBoxItem("Linn BW 2000"));
                Add(new ComboBoxItem("Linn 1996"));
                Add(new ComboBoxItem("Linn 1967"));
                Add(new ComboBoxItem("Linn 1948"));

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
            if (item.Text == "Linn 2005")
                LoadDataClass.LoadImageToLayer(@"\\lc-gis\f\orthos2005\orthos2005_mosaic.gdb", "Orthos2005_tif_webM");
            if (item.Text == "Linn BW 2000")
                LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/Orthos2000_bw_webM/MapServer");
            if (item.Text == "Linn 1996")
                LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/Hosted/LinnCounty1996_bw_tile/MapServer");
            if (item.Text == "Linn 1967")
                LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/Hosted/Historical_1967_Aerials/MapServer");
            if (item.Text == "Linn 1948")
                LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/Pub_1948_Photos_location_not_exact/MapServer");

            SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox
        }

    }
}
