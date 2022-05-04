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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taxlot_Search
{
    internal class FeatureSelectionTool : MapTool
    {
        public FeatureSelectionTool()
        {
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Map;
        }

        protected override void OnToolMouseDown(MapViewMouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                //Get the instance of the ViewModel from the dock pane
                var LabelVM = Module1.LabelVM;
                if (LabelVM != null)
                {
                    LabelVM.ClickPoint = e.ClientPoint;
                }
            }
        }

        protected override async Task<bool> OnSketchCompleteAsync(ArcGIS.Core.Geometry.Geometry geometry)
        {
            //Get the instance of the ViewModel from the dock pane
            var LabelVM = Module1.LabelVM;
            if (LabelVM == null || LabelVM.SelectedLayer == null)
                return true;

            return await QueuedTask.Run(() =>
            {
                //Return all the features that intersect the sketch geometry
                var result = MapView.Active.GetFeatures(geometry);
                //var layerSelection = result.FirstOrDefault(kvp => kvp.Key == LabelVM.SelectedLayer);

                //Clear the selection if no features where returned
                if (!result.ContainsKey(LabelVM.SelectedLayer))
                {
                    LabelVM.SelectedLayer.Select(null, SelectionCombinationMethod.Subtract);
                    return true;
                }

                //Construct a query filter using the OIDs of the features that intersected the sketch geometry
                var oidList = result[LabelVM.SelectedLayer];
                var oid = LabelVM.SelectedLayer.GetTable().GetDefinition().GetObjectIDField();
                var qf = new ArcGIS.Core.Data.QueryFilter() { WhereClause = string.Format("({0} in ({1}))", oid, string.Join(",", oidList)) };

                var method = SelectionCombinationMethod.New;

                try
                {
                    //Create the new selection
                    LabelVM.SelectedLayer.Select(qf, method);
                }
                catch (Exception) { } //May occur if expression validates but is still invalid expression.
                return true;
            });
        }

        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }

    }
}
