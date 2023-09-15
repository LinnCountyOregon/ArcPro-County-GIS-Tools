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
    internal class LoadImagesCityCbox : ComboBox
    {

        private bool _isInitialized;

        /// <summary>
        /// Combo Box constructor
        /// </summary>
        public LoadImagesCityCbox()
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
                Add(new ComboBoxItem("Albany 2019"));
                Add(new ComboBoxItem("Albany 2015"));
                Add(new ComboBoxItem("Albany 2010"));
                Add(new ComboBoxItem("Albany 2006"));
                Add(new ComboBoxItem("Lebanon 2017"));
                Add(new ComboBoxItem("Lebanon 2012"));
           //     Add(new ComboBoxItem("Lebanon 2005"));
                Add(new ComboBoxItem("Sweet Home 2018"));
                Add(new ComboBoxItem("Sweet Home 2015"));
          //      Add(new ComboBoxItem("Sweet Home 2009"));
          //      Add(new ComboBoxItem("Sweet Home 2006"));

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
            if (item.Text == "Albany 2019")
                LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/baselayers/Pub_Albany_2019_SID/MapServer");
            else if (item.Text == "Albany 2015")
                LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/AlbanyOrtho2015/MapServer");
            else if (item.Text == "Albany 2010")
                LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/Pub_Albany_2010_Orthos/MapServer");
            else if (item.Text == "Albany 2006")
                LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/Pub_Albany_2006_Orthos/MapServer");
            else if (item.Text == "Lebanon 2017")
                LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/Pub_Lebanon_Orthos_2017/MapServer");
            else if (item.Text == "Lebanon 2012")
                LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/Pub_Ortho_Lebanon_2012/MapServer");
            else if (item.Text == "Sweet Home 2018")
                LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/SweetHomeOrtho2018/MapServer");
            else if (item.Text == "Sweet Home 2015")
                LoadDataClass.LoadMapServiceToLayer(@"https://gis.co.linn.or.us/public/rest/services/public/Pub_SweetHome_2015_Orthos/MapServer");

            SelectedItem = ItemCollection.FirstOrDefault(); //set the default item in the comboBox
        }

    }
}
