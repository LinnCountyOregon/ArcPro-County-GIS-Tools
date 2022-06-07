using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
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

namespace Taxlot_Search
{
    internal class Module1 : Module
    {
        private static Module1 _this = null;
        private const string _dockPaneID = "Taxlot_Search_TaxlotSearchDockpane";
        private const string _labelDockPaneID = "Taxlot_Search_LabelDockpane";

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("Taxlot_Search_Module"));
            }
        }

        /// <summary>
        /// Stores the instance of the Feature Selection dock pane viewmodel
        /// </summary>
        private static LabelDockpaneViewModel _labelDockPane;
        internal static LabelDockpaneViewModel LabelVM
        {
            get
            {
                if (_labelDockPane == null)
                {
                    _labelDockPane = FrameworkApplication.DockPaneManager.Find(_labelDockPaneID) as LabelDockpaneViewModel;
                }
                return _labelDockPane;
            }
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane != null)
                pane.IsVisible = false;

            DockPane pane2 = FrameworkApplication.DockPaneManager.Find(_labelDockPaneID);
            if (pane2 != null)
                pane2.IsVisible = false;

            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
