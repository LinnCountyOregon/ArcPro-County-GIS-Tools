using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Events;
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
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommonArcProAddin;


namespace Taxlot_Search
{
    internal class LabelDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "Taxlot_Search_LabelDockpane";
        private const string _selectToolID = "Taxlot_Search_FeatureSelectionTool";
        private const string _selectTaxlotToolID = "Taxlot_Search_FeatureSelectionTool_Taxlot";
        private const string _selectZoningToolID = "Taxlot_Search_FeatureSelectionTool_Zoning";
        private const string _selectRoadToolID = "Taxlot_Search_FeatureSelectionTool_Roads";
        private const string _selectLayoutToolID = "Taxlot_Search_AddTextToLayout";
        private object _lock = new object();
        Map _activeMap;

        protected LabelDockpaneViewModel()
        {
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_layers, _lock);
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_fields, _lock);

            LayersAddedEvent.Subscribe(OnLayersAdded);
            LayersRemovedEvent.Subscribe(OnLayersRemoved);
            MapSelectionChangedEvent.Subscribe(OnSelectionChanged);
            ActiveToolChangedEvent.Subscribe(OnActiveToolChanged);
            ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
            LayoutViewEvent.Subscribe(OnActiveLayoutViewChanged);
        }

        ~LabelDockpaneViewModel()
        {
            ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);
            LayoutViewEvent.Unsubscribe(OnActiveLayoutViewChanged);
            MapSelectionChangedEvent.Unsubscribe(OnSelectionChanged);
            ActiveToolChangedEvent.Unsubscribe(OnActiveToolChanged);
        }

        #region Bindable Properties

        private ObservableCollection<BasicFeatureLayer> _layers = new ObservableCollection<BasicFeatureLayer>();
        public ObservableCollection<BasicFeatureLayer> Layers
        {
            get { return _layers; }
        }

        private BasicFeatureLayer _selectedLayer;
        public BasicFeatureLayer SelectedLayer
        {
            get { return _selectedLayer; }
            set
            {
                SetProperty(ref _selectedLayer, value, () => SelectedLayer);
                if (_selectedLayer == null)
                {
                    _fields.Clear();
                    FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                    return;
                }
                else
                    GetLayerFieldNames();
            }
        }

        private ObservableCollection<FieldDescription> _fields = new ObservableCollection<FieldDescription>();
        public ObservableCollection<FieldDescription> Fields
        {
            get { return _fields; }
        }

        private FieldDescription _selectedField;
        public FieldDescription SelectedField
        {
            get { return _selectedField; }
            set
            {
                SetProperty(ref _selectedField, value, () => SelectedField);
                if (_selectedField == null)
                {
                    FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                    return;
                }
            }
        }

        private long? _selectedOID;
        public long? SelectedOID
        {
            get { return _selectedOID; }
            set
            {
                SetProperty(ref _selectedOID, value, () => SelectedOID);
                // _selectedLayerInfos[_activeMap].SelectedOID = _selectedOID;
                //  if (_selectedOID.HasValue && MapView.Active != null)
                //       MapView.Active.FlashFeature(SelectedLayer, _selectedOID.Value);
                //   ShowAttributes();
            }
        }

        private System.Windows.Point _clickPoint;
        public System.Windows.Point ClickPoint
        {
            get { return _clickPoint; }
            set
            {
                SetProperty(ref _clickPoint, value, () => ClickPoint);
            }
        }

        private string _selectedFontSize;
        public string SelectedFontSize
        {
            get { return _selectedFontSize; }
            set
            {
                SetProperty(ref _selectedFontSize, value, () => SelectedFontSize);
                if (_selectedFontSize == null)
                {
                    FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                    return;
                }
            }
        }

        private bool _checkedTaxlotPIN;
        public bool CheckedTaxlotPIN
        {
            get { return _checkedTaxlotPIN; }
            set { SetProperty(ref _checkedTaxlotPIN, value, () => CheckedTaxlotPIN); }
        }

        private bool _checkedTaxlotActNum;
        public bool CheckedTaxlotActNum
        {
            get { return _checkedTaxlotActNum; }
            set { SetProperty(ref _checkedTaxlotActNum, value, () => CheckedTaxlotActNum); }
        }

        private bool _checkedTaxlotOwnerName;
        public bool CheckedTaxlotOwnerName
        {
            get { return _checkedTaxlotOwnerName; }
            set { SetProperty(ref _checkedTaxlotOwnerName, value, () => CheckedTaxlotOwnerName); }
        }

        private bool _checkedTaxlotOwnerAddress;
        public bool CheckedTaxlotOwnerAddress
        {
            get { return _checkedTaxlotOwnerAddress; }
            set { SetProperty(ref _checkedTaxlotOwnerAddress, value, () => CheckedTaxlotOwnerAddress); }
        }

        private bool _checkedTaxlotAcres;
        public bool CheckedTaxlotAcres
        {
            get { return _checkedTaxlotAcres; }
            set { SetProperty(ref _checkedTaxlotAcres, value, () => CheckedTaxlotAcres); }
        }


        #endregion

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
                    if (Layers.Count == 0)
                    {
                        foreach (var layer in MapView.Active.Map.Layers)
                        {
                            if (layer is BasicFeatureLayer)
                            {
                                Layers.Add((BasicFeatureLayer)layer);
                                if (SelectedLayer == null)
                                    SelectedLayer = (BasicFeatureLayer)layer;
                            }
                        }
                    }
                }

                if (MapView.Active != null)
                {
                    _activeMap = MapView.Active.Map;
                    CreateGraphicsLayer();
                }

                NotifyPropertyChanged(() => Heading);
            }
        }

        private void CreateGraphicsLayer()
        {
            if (_activeMap.MapType != MapType.Map)
                return;// not 2D

            var gl_param = new GraphicsLayerCreationParams { Name = "Graphics Layer" };
            bool addGraphicsLayer = true;
            QueuedTask.Run(() =>
            {
                //get the first graphics layer in the map's collection of graphics layers
                var graphicsLayerList = _activeMap.GetLayersAsFlattenedList().OfType<GraphicsLayer>().ToList();
                foreach (var graphicsLayer in graphicsLayerList)
                {
                    if (graphicsLayer.Name == "Graphics Layer")
                        addGraphicsLayer = false;
                }

                if (addGraphicsLayer)
                {
                    //By default will be added to the top of the TOC
                    var newGrahicsLayer = LayerFactory.Instance.CreateLayer<GraphicsLayer>(gl_param, _activeMap);

                    //or add to the bottom of the TOC
                    //LayerFactory.Instance.CreateLayer<GraphicsLayer>(gl_param, map, 
                    //                                                   LayerPosition.AddToBottom);

                    //or add a graphics layer to a group layer...
                    //var group_layer = map.GetLayersAsFlattenedList().OfType<GroupLayer>().First();
                    //LayerFactory.Instance.CreateLayer<GraphicsLayer>(gl_param, group_layer);

                    //TODO...use the graphics layer
                }
            });
        }

        private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs args)
        {
            _fields.Clear();
            if (args.IncomingView != null)
            {
                _activeMap = args.IncomingView.Map;
                _heading = "Label Map: " + MapView.Active.Map.Name;

                foreach (var layer in args.IncomingView.Map.Layers)
                {
                    if (layer is BasicFeatureLayer)
                    {
                        Layers.Add((BasicFeatureLayer)layer);
                        if (SelectedLayer == null)
                            SelectedLayer = (BasicFeatureLayer)layer;
                    }
                }
            }
            else
            {
                SetProperty(ref _selectedLayer, null, () => SelectedLayer);
                _layers.Clear();

                if (ProApp.Panes.Count == 1)
                    _heading = "No Active Map or Layout";
            }

            NotifyPropertyChanged(() => Heading);
        }
        private void OnActiveLayoutViewChanged(LayoutViewEventArgs args)
        {
            if (args.Hint == LayoutViewEventHint.Activated || args.Hint == LayoutViewEventHint.Opened)
                _heading = "Label Layout: " + LayoutView.Active.Layout.Name;
            else
                if (ProApp.Panes.Count == 0)
                _heading = "No Active Map or Layout";

            NotifyPropertyChanged(() => Heading);
        }

        private void OnLayersRemoved(LayerEventsArgs args)
        {
            foreach (var layer in args.Layers)
            {
                if (layer.Map == _activeMap)
                {
                    if (Layers.Contains(layer))
                        Layers.Remove((BasicFeatureLayer)layer);
                }
            }

            if (SelectedLayer == null)
                SelectedLayer = Layers.FirstOrDefault();

        }

        private void OnLayersAdded(LayerEventsArgs args)
        {
            foreach (var layer in args.Layers)
            {
                if (layer.Map == _activeMap && layer is BasicFeatureLayer)
                {
                    Layers.Add((BasicFeatureLayer)layer);
                    if (SelectedLayer == null)
                        SelectedLayer = (BasicFeatureLayer)layer;
                }
            }
        }

        private async void OnSelectionChanged(MapSelectionChangedEventArgs args)
        {
            if (args.Map != _activeMap)
                return;

            if (SelectToolActive)
            {
                if (SelectedField != null)
                {
                    await UpdateForActiveMap(SelectedLayer, false, args.Selection);

                    if (SelectedOID != null)
                        await AddTextFromSelection(SelectedLayer, SelectedField.Name);
                }
                else
                    MessageBox.Show("Please select a field from the dropdown list to get the label text");
            }
            else if (SelectTaxlotToolActive)
            {
                var featurelayerCheck = MapView.Active.Map.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>().ToList();
                foreach (BasicFeatureLayer layerCheckMember in featurelayerCheck)
                {
                    if ("taxlots" == layerCheckMember.Name.ToLower())
                    {
                        await UpdateForActiveMap(layerCheckMember, false, args.Selection);

                        if (SelectedOID != null)
                            await AddTextFromSelection(layerCheckMember, "");
                    }
                }
            }
            else if (SelectZoningToolActive)
            {
                var featurelayerCheck = MapView.Active.Map.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>().ToList();
                foreach (BasicFeatureLayer layerCheckMember in featurelayerCheck)
                {
                    if ("zoning" == layerCheckMember.Name.ToLower())
                    {
                        await UpdateForActiveMap(layerCheckMember, false, args.Selection);

                        if (SelectedOID != null)
                            await AddTextFromSelection(layerCheckMember, "Zoning");
                    }
                }
            }
            else if (SelectRoadToolActive)
            {
                var featurelayerCheck = MapView.Active.Map.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>().ToList();
                foreach (BasicFeatureLayer layerCheckMember in featurelayerCheck)
                {
                    if ("roads" == layerCheckMember.Name.ToLower())
                    {
                        await UpdateForActiveMap(layerCheckMember, false, args.Selection);

                        if (SelectedOID != null)
                            await AddTextFromSelection(layerCheckMember, "Street");
                    }
                }
            }
        }

        private async void OnActiveToolChanged(ToolEventArgs args)
        {
            if (args.CurrentID == _selectToolID)
            {
                SetProperty(ref _selectToolActive, true, () => SelectToolActive);
                SetProperty(ref _selectTaxlotToolActive, false, () => SelectTaxlotToolActive);
                SetProperty(ref _selectZoningToolActive, false, () => SelectZoningToolActive);
                SetProperty(ref _selectRoadToolActive, false, () => SelectRoadToolActive);
                SetProperty(ref _selectLayoutToolActive, false, () => SelectLayoutToolActive);
            }
            else if (args.CurrentID == _selectTaxlotToolID)
            {
                SetProperty(ref _selectToolActive, false, () => SelectToolActive);
                SetProperty(ref _selectTaxlotToolActive, true, () => SelectTaxlotToolActive);
                SetProperty(ref _selectZoningToolActive, false, () => SelectZoningToolActive);
                SetProperty(ref _selectRoadToolActive, false, () => SelectRoadToolActive);
                SetProperty(ref _selectLayoutToolActive, false, () => SelectLayoutToolActive);
                await LoadDataClass.LoadDatasetToLayer(LoadDataClass.localGISdirectory, "taxlots");
            }
            else if (args.CurrentID == _selectZoningToolID)
            {
                SetProperty(ref _selectToolActive, false, () => SelectToolActive);
                SetProperty(ref _selectTaxlotToolActive, false, () => SelectTaxlotToolActive);
                SetProperty(ref _selectZoningToolActive, true, () => SelectZoningToolActive);
                SetProperty(ref _selectRoadToolActive, false, () => SelectRoadToolActive);
                SetProperty(ref _selectLayoutToolActive, false, () => SelectLayoutToolActive);
                await LoadDataClass.LoadDatasetToLayer(LoadDataClass.localGISdirectory, "Zoning");
            }
            else if (args.CurrentID == _selectRoadToolID)
            {
                SetProperty(ref _selectToolActive, false, () => SelectToolActive);
                SetProperty(ref _selectTaxlotToolActive, false, () => SelectTaxlotToolActive);
                SetProperty(ref _selectZoningToolActive, false, () => SelectZoningToolActive);
                SetProperty(ref _selectRoadToolActive, true, () => SelectRoadToolActive);
                SetProperty(ref _selectLayoutToolActive, false, () => SelectLayoutToolActive);
                await LoadDataClass.LoadDatasetToLayer(LoadDataClass.localGISdirectory, "roads");
            }
            else if (args.CurrentID == _selectLayoutToolID)
            {
                SetProperty(ref _selectToolActive, false, () => SelectToolActive);
                SetProperty(ref _selectTaxlotToolActive, false, () => SelectTaxlotToolActive);
                SetProperty(ref _selectZoningToolActive, false, () => SelectZoningToolActive);
                SetProperty(ref _selectRoadToolActive, false, () => SelectRoadToolActive);
                SetProperty(ref _selectLayoutToolActive, true, () => SelectLayoutToolActive);
            }
            else
            {
                SetProperty(ref _selectToolActive, false, () => SelectToolActive);
                SetProperty(ref _selectTaxlotToolActive, false, () => SelectTaxlotToolActive);
                SetProperty(ref _selectZoningToolActive, false, () => SelectZoningToolActive);
                SetProperty(ref _selectRoadToolActive, false, () => SelectRoadToolActive);
                SetProperty(ref _selectLayoutToolActive, false, () => SelectLayoutToolActive);
            }
        }

        private async void GetLayerFieldNames()
        {
            await QueuedTask.Run(() =>
            {
                Fields.Clear();
                foreach (var field in SelectedLayer.GetFieldDescriptions())
                {
                    Fields.Add(field);
                }
            });
        }

        private Task UpdateForActiveMap(BasicFeatureLayer LabelLayer, bool activeMapChanged = true, SelectionSet mapSelection = null)
        {
            return QueuedTask.Run(() =>
            {
                if (SelectedLayer == null)
                    SetProperty(ref _selectedOID, null, () => SelectedOID);
                else
                {
                    long oid = -1;
                    if (mapSelection != null)
                    {
                        if (mapSelection.Contains(LabelLayer))
                            oid = mapSelection[LabelLayer].FirstOrDefault();
                        // oids.AddRange(mapSelection[SelectedLayer]);
                    }

                    if (oid == -1)
                        SetProperty(ref _selectedOID, null, () => SelectedOID);
                    else
                        SetProperty(ref _selectedOID, oid, () => SelectedOID);
                }
            });
        }

        private Task AddTextFromSelection(BasicFeatureLayer LabelLayer, string LabelFieldName)
        {
            // define the text symbol
            var textSymbol = new CIMTextSymbol();
            // define the text graphic
            var textGraphic = new CIMTextGraphic();
            // define the point to draw the text
            MapPoint textPoint = null;

            return QueuedTask.Run(() =>
            {
                //get the first graphics layer in the map's collection of graphics layers
                var graphicsLayer = _activeMap.GetLayersAsFlattenedList().OfType<GraphicsLayer>().FirstOrDefault();
                if (graphicsLayer != null)
                {
                    _activeMap.TargetGraphicsLayer = graphicsLayer;
                    // use CIMGraphicsLayer to change "normal" layer properties, such as visibility, selectablity, minimum and maximum viewing scales, names and so forth


                    //Create a simple text symbol
                    textSymbol = SymbolFactory.Instance.ConstructTextSymbol(ColorFactory.Instance.BlackRGB, Convert.ToDouble(SelectedFontSize), "Aerial", "Regular");
                    textSymbol.HorizontalAlignment = HorizontalAlignment.Center;
                    textSymbol.VerticalAlignment = VerticalAlignment.Center;

                    //Sets the geometry of the text graphic
                    var insp = new ArcGIS.Desktop.Editing.Attributes.Inspector();
                    insp.Load(LabelLayer, (long)SelectedOID);

                    // get text point from the point clicked on the map
                    textPoint = MapView.Active.ClientToMap(ClickPoint);

                    if (SelectRoadToolActive)
                    {
                        textSymbol.Angle = GetAngleOfNearLine(insp, ref textPoint);
                    }

                    textGraphic.Shape = textPoint;
                    //textGraphic.Shape = insp.Shape.Extent.Center;
                    textGraphic.Placement = Anchor.CenterPoint;

                    //Sets the text string to use in the text graphic
                    textGraphic.Text = "";
                    if (SelectTaxlotToolActive)
                    {
                        //Sets the text string to use in the text graphic
                        if (CheckedTaxlotPIN)
                            textGraphic.Text += insp["PIN"].ToString();
                        if (CheckedTaxlotActNum)
                        {
                            if (textGraphic.Text != "")
                                textGraphic.Text += "\n";

                            textGraphic.Text += insp["ACTNUM"].ToString();
                        }
                        if (CheckedTaxlotOwnerName)
                        {
                            if (textGraphic.Text != "")
                                textGraphic.Text += "\n";

                            textGraphic.Text += insp["OWNER1"].ToString();
                        }
                        if (CheckedTaxlotOwnerAddress)
                        {
                            if (textGraphic.Text != "")
                                textGraphic.Text += "\n";

                            textGraphic.Text += insp["MAIL1"].ToString() + ", " + insp["MAILCITY"].ToString() + ", " + insp["MAILST"].ToString() + ", " + insp["ZIP"].ToString();
                        }
                        if (CheckedTaxlotAcres)
                        {
                            if (textGraphic.Text != "")
                                textGraphic.Text += "\n";

                            textGraphic.Text += insp["TaxlotAcre"].ToString() + " acres";
                        }
                    }
                    else
                        textGraphic.Text = insp[LabelFieldName].ToString();

                    //Sets symbol to use to draw the text graphic
                    textGraphic.Symbol = textSymbol.MakeSymbolReference();
                    //Draw the overlay text graphic
                    //_graphic = MapView.Active.AddOverlay(textGraphic);
                    graphicsLayer.AddElement(textGraphic);

                }
            });
        }

        private double GetAngleOfNearLine(ArcGIS.Desktop.Editing.Attributes.Inspector SelectedLine, ref MapPoint pointClicked)
        {
            double degOfTangent = 0;
            if (SelectedLine.Shape.GeometryType == GeometryType.Polyline)
            {
                // Don't extent the segment
                SegmentExtensionType extension = SegmentExtensionType.NoExtension;
                Polyline polyline = PolylineBuilderEx.CreatePolyline(SelectedLine.Shape as Polyline);

                double distanceAlongCurve, distanceFromCurve;
                LeftOrRightSide whichSide;
                AsRatioOrLength asRatioOrLength = AsRatioOrLength.AsLength;
                double TangentLength = 1.0;

                // find closest point along the road line and the distance along the curve to the point. Move clicked point to the nearest point on the curve
                MapPoint outPoint = GeometryEngine.Instance.QueryPointAndDistance(polyline, extension, pointClicked, asRatioOrLength, out distanceAlongCurve, out distanceFromCurve, out whichSide);
                pointClicked = outPoint;

                // find the tangent line at the above closest point. Old app used esriExtendedTangentAtFrom
                Polyline tangent = GeometryEngine.Instance.QueryTangent(polyline, extension, distanceAlongCurve, asRatioOrLength, TangentLength);
                if (tangent.PointCount == 2)
                {
                    List<double> xTanList = tangent.Points.Select(a => a.X).ToList();
                    List<double> yTanList = tangent.Points.Select(a => a.Y).ToList();
                    degOfTangent = Math.Atan2(yTanList[1] - yTanList[0], xTanList[1] - xTanList[0]) * (180 / Math.PI);

                    // correct for text that becomes upside down
                    if (degOfTangent > 90.0)
                        degOfTangent -= 180.0;
                    else if (degOfTangent < -90.0)
                        degOfTangent += 180.0;
                }
                else
                    MessageBox.Show("Error in GetAngleOfNearLine: There are more or less than 2 points in the tangent line");
            }

            return degOfTangent;
        }

        #region Commands

        private RelayCommand _selectToolCmd;
        public ICommand SelectToolCmd
        {
            get
            {
                if (_selectToolCmd == null)
                {
                    _selectToolCmd = new RelayCommand(() => FrameworkApplication.SetCurrentToolAsync(_selectToolID), () => { return MapView.Active != null && SelectedLayer != null; });
                }
                return _selectToolCmd;
            }
        }

        private bool _selectToolActive = false;
        public bool SelectToolActive
        {
            get { return _selectToolActive; }
            set
            {
                SetProperty(ref _selectToolActive, value, () => SelectToolActive);
            }
        }

        private RelayCommand _selectTaxlotToolCmd;
        public ICommand SelectTaxlotToolCmd
        {
            get
            {
                if (_selectTaxlotToolCmd == null)
                {
                    _selectTaxlotToolCmd = new RelayCommand(() => FrameworkApplication.SetCurrentToolAsync(_selectTaxlotToolID), () => { return MapView.Active != null && SelectedLayer != null; });
                }
                return _selectTaxlotToolCmd;
            }
        }

        private bool _selectTaxlotToolActive = false;
        public bool SelectTaxlotToolActive
        {
            get { return _selectTaxlotToolActive; }
            set
            {
                SetProperty(ref _selectTaxlotToolActive, value, () => SelectTaxlotToolActive);
            }
        }

        private RelayCommand _selectZoningToolCmd;
        public ICommand SelectZoningToolCmd
        {
            get
            {
                if (_selectZoningToolCmd == null)
                {
                    _selectZoningToolCmd = new RelayCommand(() => FrameworkApplication.SetCurrentToolAsync(_selectZoningToolID), () => { return MapView.Active != null && SelectedLayer != null; });
                }
                return _selectZoningToolCmd;
            }
        }

        private bool _selectZoningToolActive = false;
        public bool SelectZoningToolActive
        {
            get { return _selectZoningToolActive; }
            set
            {
                SetProperty(ref _selectZoningToolActive, value, () => SelectZoningToolActive);
            }
        }

        private RelayCommand _selectRoadToolCmd;
        public ICommand SelectRoadToolCmd
        {
            get
            {
                if (_selectRoadToolCmd == null)
                {
                    _selectRoadToolCmd = new RelayCommand(() => FrameworkApplication.SetCurrentToolAsync(_selectRoadToolID), () => { return MapView.Active != null && SelectedLayer != null; });
                }
                return _selectRoadToolCmd;
            }
        }

        private bool _selectRoadToolActive = false;
        public bool SelectRoadToolActive
        {
            get { return _selectRoadToolActive; }
            set
            {
                SetProperty(ref _selectRoadToolActive, value, () => SelectRoadToolActive);
            }
        }

        private RelayCommand _selectLayoutToolCmd;
        public ICommand SelectLayoutToolCmd
        {
            get
            {
                if (_selectLayoutToolCmd == null)
                {
                    _selectLayoutToolCmd = new RelayCommand(() => FrameworkApplication.SetCurrentToolAsync(_selectLayoutToolID), () => { return LayoutView.Active != null; });
                }
                return _selectLayoutToolCmd;
            }
        }

        private bool _selectLayoutToolActive = false;
        public bool SelectLayoutToolActive
        {
            get { return _selectLayoutToolActive; }
            set
            {
                SetProperty(ref _selectLayoutToolActive, value, () => SelectLayoutToolActive);
            }
        }


        #endregion Commands


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
