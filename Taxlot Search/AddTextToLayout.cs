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
    internal class AddTextToLayout : LayoutTool
    {
        public AddTextToLayout()
        {
            SketchType = SketchGeometryType.Point;
        }
        protected override Task OnToolActivateAsync(bool active)
        {
            return base.OnToolActivateAsync(active);
        }
        protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            //TODO: Use geometry. Add graphic, select elements, etc.
            //QueuedTask.Run( () => {
            //  ActiveElementContainer.SelectElements(geometry, SelectionCombinationMethod.New, false);
            //});

            //Get the instance of the ViewModel from the dock pane
            var LabelVM = Module1.LabelVM;
            if (LabelVM == null || LabelVM.SelectedFontSize == null)
                return base.OnSketchCompleteAsync(geometry);

            //Construct on the worker thread
            QueuedTask.Run(() =>
            {
                //Build 2D point geometry
                //Coordinate2D coord2D = new Coordinate2D(3.5, 10);

                //Set symbolology, create and add element to layout
                CIMTextSymbol sym = SymbolFactory.Instance.ConstructTextSymbol(ColorFactory.Instance.BlackRGB, Convert.ToDouble(LabelVM.SelectedFontSize), "Arial", "Regular");
                sym.HorizontalAlignment = HorizontalAlignment.Center;
                sym.VerticalAlignment = VerticalAlignment.Center;

                const string quote = "\"";
                string textString = "<dyn type=" + quote + "date" + quote + " format=" + quote + quote + "/>";
                //GraphicElement ptTxtElmOld = LayoutElementFactory.Instance.CreatePointTextGraphicElement(ActiveElementContainer, (Coordinate2D)geometry, textString, sym);
                GraphicElement ptTxtElm = ElementFactory.Instance.CreateTextGraphicElement(ActiveElementContainer, TextType.PointText, geometry, sym, textString);
                ptTxtElm.SetName("Layout Date Text");

                //Change additional text properties
                ptTxtElm.SetAnchor(Anchor.CenterPoint);
            });

            return base.OnSketchCompleteAsync(geometry);
        }
    }
}
