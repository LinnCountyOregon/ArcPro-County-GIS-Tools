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
using ArcGIS.Desktop.Mapping.Events;
using ArcGIS.Desktop.Layouts.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Taxlot_Search
{
    internal class LabelDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "Taxlot_Search_LabelDockpane";

        protected LabelDockpaneViewModel() 
        {
            ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
            ActiveLayoutViewChangedEvent.Subscribe(OnActiveLayoutViewChanged);
        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();
        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "No Active Map/Layout";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        protected override void OnShow(bool isVisible)
        {
            if (isVisible)
            {
                if (MapView.Active == null)
                {
                    if (LayoutView.Active == null)
                        _heading = "No Active Map or Layout";
                    else
                        _heading = "Label Layout: " + LayoutView.Active.Layout.Name;
                }
                else
                {
                    _heading = "Label Map: " + MapView.Active.Map.Name;
                }

                NotifyPropertyChanged(() => Heading);
            }
        }

        private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs args)
        {
            if (args.IncomingView != null)
                _heading = "Label Map: " + MapView.Active.Map.Name;
            else
                if (ProApp.Panes.Count == 1)
                    _heading = "No Active Map or Layout";

            NotifyPropertyChanged(() => Heading);
        }
        private void OnActiveLayoutViewChanged(LayoutViewEventArgs args)
        {
            if (args.LayoutView != null)
                _heading = "Label Layout: " + LayoutView.Active.Layout.Name;
            else
                if (ProApp.Panes.Count == 0)
                    _heading = "No Active Map or Layout";

            NotifyPropertyChanged(() => Heading);
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class LabelDockpane_ShowButton : Button
    {
        protected override void OnClick()
        {
            LabelDockpaneViewModel.Show();

        }
    }
}
