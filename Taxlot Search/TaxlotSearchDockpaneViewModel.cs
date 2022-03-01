using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using ArcGIS.Desktop.Framework.Events;
using ArcGIS.Desktop.Layouts;
using System.Collections.ObjectModel;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using System.Windows.Input;



namespace Taxlot_Search
{
    internal class TaxlotSearchDockpaneViewModel : DockPane
    {
        private const string _dockPaneID = "Taxlot_Search_TaxlotSearchDockpane";
        private object _lock = new object();
        Dictionary<Map, SelectedLayerInfo> _selectedLayerInfos = new Dictionary<Map, SelectedLayerInfo>();
        public static List<string> allowedTaxlotNames = new List<string> { "taxlots", "tax lots" };
        public static List<string> allowedAddressNames = new List<string> { "address" };
        Map _activeMap;

        protected TaxlotSearchDockpaneViewModel() 
        {
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_layers, _lock);
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_propertyInfoList, _lock);
            
            LayersAddedEvent.Subscribe(OnLayersAdded);
            LayersRemovedEvent.Subscribe(OnLayersRemoved);
            //ActiveToolChangedEvent.Subscribe(OnActiveToolChanged);
            MapSelectionChangedEvent.Subscribe(OnSelectionChanged);
            ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
            MapRemovedEvent.Subscribe(OnMapRemoved);
        }

        ~TaxlotSearchDockpaneViewModel()
        {
            LayersAddedEvent.Unsubscribe(OnLayersAdded);
            LayersRemovedEvent.Unsubscribe(OnLayersRemoved);
            //ActiveToolChangedEvent.Unsubscribe(OnActiveToolChanged);
            MapSelectionChangedEvent.Unsubscribe(OnSelectionChanged);
            ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);
        }

        /// <summary>
        /// Called when the dock pane is first initialized.
        /// </summary>
        protected override Task InitializeAsync()
        {
            if (MapView.Active == null)
                return Task.FromResult(0);

            _activeMap = MapView.Active.Map;
            return UpdateForActiveMap();
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
                    FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                    return;
                }
                _selectedLayerInfos[_activeMap].SelectedLayer = _selectedLayer;
                //SelectedLayerChanged();
            }
        }

        private ObservableCollection<PropertyIntoItems> _propertyInfoList = new ObservableCollection<PropertyIntoItems>();
        public ObservableCollection<PropertyIntoItems> PropertyInfoList
        {
            get { return _propertyInfoList; }
            set { SetProperty(ref _propertyInfoList, value); }
        }

        //private AddressItems _selectedAddress;
        //public AddressItems SelectedAddress
        //{
        //    get { return _selectedAddress; }
        //    set { SetProperty(ref _selectedAddress, value); }
        //}

        private bool _hasError = false;
        public bool HasError
        {
            get { return _hasError; }
            set
            {
                SetProperty(ref _hasError, value, () => HasError);
            }
        }

        private bool _isValid = false;
        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                SetProperty(ref _isValid, value, () => IsValid);
            }
        }

        #endregion

        #region Private Methods

        private Task UpdateForActiveMap(bool activeMapChanged = true, Dictionary<MapMember, List<long>> mapSelection = null)
        {
            return QueuedTask.Run(() =>
            {
                SelectedLayerInfo selectedLayerInfo = null;
                if (!_selectedLayerInfos.ContainsKey(_activeMap))
                {
                    selectedLayerInfo = new SelectedLayerInfo();
                    _selectedLayerInfos.Add(_activeMap, selectedLayerInfo);
                }
                else
                    selectedLayerInfo = _selectedLayerInfos[_activeMap];

                if (activeMapChanged)
                {
                    RefreshLayerCollection();

                    SetProperty(ref _selectedLayer, (selectedLayerInfo.SelectedLayer != null) ? selectedLayerInfo.SelectedLayer : Layers.FirstOrDefault(), () => SelectedLayer);
                    if (_selectedLayer == null)
                    {
                        FrameworkApplication.SetCurrentToolAsync("esri_mapping_exploreTool");
                        return;
                    }
                    selectedLayerInfo.SelectedLayer = SelectedLayer;
                }

                //if (SelectedLayer == null)
                //    RefreshSelectionOIDs(new List<long>());
                //else
                //{
                //    List<long> oids = new List<long>();
                //    if (mapSelection != null)
                //    {
                //        if (mapSelection.ContainsKey(SelectedLayer))
                //            oids.AddRange(mapSelection[SelectedLayer]);
                //    }
                //    else
                //    {
                //        oids.AddRange(SelectedLayer.GetSelection().GetObjectIDs());
                //    }
                //    RefreshSelectionOIDs(oids);
                //}

                //SetProperty(ref _selectedOID, (selectedLayerInfo.SelectedOID != null && LayerSelection.Contains(selectedLayerInfo.SelectedOID)) ? selectedLayerInfo.SelectedOID : LayerSelection.FirstOrDefault(), () => SelectedOID);
                //selectedLayerInfo.SelectedOID = SelectedOID;
                //ShowAttributes();
            });
        }

        private void RefreshLayerCollection()
        {
            Layers.Clear();
            if (_activeMap == null)
                return;

            var layers = _activeMap.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>();
            lock (_lock)
            {
                foreach (var layer in layers)
                {
                    if (allowedTaxlotNames.Contains(layer.Name.ToLower()) || allowedAddressNames.Contains(layer.Name.ToLower()))
                        Layers.Add(layer);
                }
            }
        }

        #endregion

        #region Event Handlers

        private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs args)
        {
            if (args.IncomingView == null)
            {
                SetProperty(ref _selectedLayer, null, () => SelectedLayer);
                _layers.Clear();
                _propertyInfoList.Clear();
                //SetProperty(ref _selectedOID, null, () => SelectedOID);
                //_layerSelection.Clear();
                //_fieldAttributes.Clear();
                return;
            }

            _activeMap = args.IncomingView.Map;
            UpdateForActiveMap();
        }

        private void OnSelectionChanged(MapSelectionChangedEventArgs args)
        {
            if (args.Map != _activeMap)
                return;

            UpdateForActiveMap(false, args.Selection);

            // load selected property into into the address list combo box
            PropertyInfoList.Clear();
            if (args.Selection.Count() != 0)
            {            
                if (allowedAddressNames.Contains(args.Selection.Keys.FirstOrDefault().Name.ToLower()) || allowedTaxlotNames.Contains(args.Selection.Keys.FirstOrDefault().Name.ToLower()))
                {
                    string selectedLayer;
                    if (allowedTaxlotNames.Contains(args.Selection.Keys.FirstOrDefault().Name.ToLower()))
                        selectedLayer = "taxlots";
                    else if (allowedAddressNames.Contains(args.Selection.Keys.FirstOrDefault().Name.ToLower()))
                        selectedLayer = "address";
                    else
                        selectedLayer = "(Not Found)";

                    lock (_lock)
                    {
                        _ = QueuedTask.Run(() =>
                        {
                            PropertyIntoItems newPropItem;
                            var firstSelectionSet = args.Selection.First();

                            // create an instance of the inspector class
                            var inspector = new ArcGIS.Desktop.Editing.Attributes.Inspector(false);

                            // load the selected features into the inspector using a list of object IDs
                            foreach (long selectedOID in firstSelectionSet.Value)
                            {
                                inspector.Load(firstSelectionSet.Key, selectedOID);

                                newPropItem = new PropertyIntoItems();

                                if (selectedLayer == "address")
                                    newPropItem.Name = inspector["SITUS_1"].ToString() + ", " + inspector["CITY"].ToString();
                                else if (selectedLayer == "taxlots")
                                    newPropItem.Name = inspector["OWNER1"].ToString();
                                else
                                    newPropItem.Name = "Layer not recognized";

                                PropertyInfoList.Add(newPropItem);
                                if (PropertyInfoList.Count() > 500)
                                {
                                    //MessageBox.Show("Warning: Only the first 500 search results returned", "Taxlot/Address Search");
                                    return;
                                }
                            }
                        });
                    }
                }
            }
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
            {
                SelectedLayer = Layers.FirstOrDefault();
                //SelectedLayerChanged();
            }
        }

        private void OnLayersAdded(LayerEventsArgs args)
        {
            foreach (var layer in args.Layers)
            {
                if (layer.Map == _activeMap && layer is BasicFeatureLayer)
                {
                    if (allowedTaxlotNames.Contains(layer.Name.ToLower()) || allowedAddressNames.Contains(layer.Name.ToLower()))
                    {
                        Layers.Add((BasicFeatureLayer)layer);
                        if (SelectedLayer == null)
                            SelectedLayer = (BasicFeatureLayer)layer;
                    }
                }
            }
        }

        private void OnMapRemoved(MapRemovedEventArgs args)
        {
            var map = _selectedLayerInfos.Where(kvp => kvp.Key.URI == args.MapPath).FirstOrDefault().Key;
            if (map != null)
                _selectedLayerInfos.Remove(map);
        }

        #endregion

        /// <summary>
        /// Used to persist the state of the selected layer and object ID for a given map.
        /// </summary>
        internal class SelectedLayerInfo
        {
            public SelectedLayerInfo() { }
            public SelectedLayerInfo(BasicFeatureLayer selectedLayer, long? selectedOID)
            {
                SelectedLayer = selectedLayer;
                SelectedOID = selectedOID;
            }

            public BasicFeatureLayer SelectedLayer { get; set; }

            public long? SelectedOID { get; set; }
        }


        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "My DockPane";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }
    }

    public class PropertyIntoItems : DockPane
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class TaxlotSearchDockpane_ShowButton : Button
    {
        protected override async void OnClick()
        {
            TaxlotSearchDockpaneViewModel.Show();

            await LoadDataClass.CheckForMap();
            LoadDataClass.LoadDatasetToLayer(LoadDataClass.localGISdirectory, "taxlots");
            LoadDataClass.LoadDatasetToLayer(LoadDataClass.localGISdirectory, "Address");
        }

        //private async void AddNewMap(string mapName)
        //{
        //    IMapPane createMapResult = await QueuedTask.Run<IMapPane>(() =>
        //    {
        //        var map = MapFactory.Instance.CreateMap(mapName, basemap: Basemap.ProjectDefault);
        //        return ProApp.Panes.CreateMapPaneAsync(map);
        //        //return map;
        //    });
        //    // Execution automatically resumes here when the Task above completes!
        //    LoadShapefileToLayer("C:\\GIS\\shapefiles", "taxlots");
        //    LoadShapefileToLayer("C:\\GIS\\shapefiles", "Address");
        //}

        //public static async void FindOpenExistingMapAsync(string mapName)
        //{
        //    Map openNewMap = await QueuedTask.Run<Map>(async () =>
        //    {
        //        Map map = null;
        //        Project proj = Project.Current;

        //        //Finding the first project item with name matches with mapName
        //        MapProjectItem mpi =
        //            proj.GetItems<MapProjectItem>()
        //                .FirstOrDefault(m => m.Name.Equals(mapName, StringComparison.CurrentCultureIgnoreCase));
        //        if (mpi != null)
        //        {
        //            map = mpi.GetMap();
        //            //Opening the map in a mapview
        //            await ProApp.Panes.CreateMapPaneAsync(map);
        //        }
        //        return map;
        //    });
        //}

        //private async void LoadShapefileToLayer(string pathToShp, string shpName)
        //{
        //    FeatureLayer loadedLayer = await QueuedTask.Run<FeatureLayer>(() =>
        //    {            
        //        try
        //        {
        //            // open shapefile to map layer
        //            System.Uri uriPath = new System.Uri(pathToShp + "\\" + shpName + ".shp"); 
        //            FeatureLayer shp_layer = LayerFactory.Instance.CreateFeatureLayer(uriPath, MapView.Active.Map, layerName: shpName);

        //            symbolizeBaseLayers(shpName, shp_layer);

        //            return MapView.Active.GetSelectedLayers()[0] as FeatureLayer;
        //        }
        //        catch
        //        {
        //            MessageBox.Show("Load data layer failed for: " + shpName + " attempting to load layer from the LinnPublication SDE");
        //            return null;
        //        }

        //        //var uriPath = new Uri(pathToShp);
        //        //var FSpath = new FileSystemConnectionPath(uriPath, FileSystemDatastoreType.Shapefile);
        //        //using (var shapefileFolder = new FileSystemDatastore(FSpath))
        //        //{
        //        //    FeatureClass taxLotsFeatureClass = shapefileFolder.OpenDataset<FeatureClass>(shpName);
        //        //    int nCount = taxLotsFeatureClass.GetCount();
        //        //    MessageBox.Show(String.Format("Feature count: {0}", nCount.ToString()));

        //        //}
        //    });

        //    if (loadedLayer == null)
        //    {
        //        await QueuedTask.Run(() =>
        //        {
        //            try
        //            {
        //                using (Geodatabase linnPublicationSDE = new Geodatabase(new DatabaseConnectionFile(new Uri(@"\\lc-gis\data\linn_geodatabase\Publication\Connection to lc-sql2016.sde"))))
        //                {
        //                    FeatureClass fcSDE = linnPublicationSDE.OpenDataset<FeatureClass>(shpName);
        //                    FeatureLayer SDElayer = LayerFactory.Instance.CreateFeatureLayer(fcSDE, MapView.Active.Map, layerName: shpName) as FeatureLayer;

        //                    // load taxlots off the Portal - Note, this is very slow to load
        //                    // var by_ref_id = @"https://gis.co.linn.or.us/public/rest/services/baselayers/Pub_taxlots/MapServer/0";
        //                    //FeatureLayer portalLayer = LayerFactory.Instance.CreateLayer(new Uri(by_ref_id, UriKind.Absolute), MapView.Active.Map, layerName: shpName) as FeatureLayer;

        //                    symbolizeBaseLayers(shpName, SDElayer);
        //                }
        //            }
        //            catch
        //            {
        //                MessageBox.Show("Load data from SDE failed for: " + shpName);
        //            }

        //        });
        //    }
        //}

        //private void symbolizeBaseLayers(string layerName, FeatureLayer layerToSymbolize)
        //{
        //    if (layerName == "taxlots")
        //    {
        //        QueuedTask.Run(() =>
        //        {
        //            CIMPolygonSymbol polySymbol = SymbolFactory.Instance.ConstructPolygonSymbol();
        //            polySymbol.SetOutlineColor(CIMColor.CreateRGBColor(156, 156, 156));
        //            polySymbol.SetColor(CIMColor.NoColor());
        //            polySymbol.SetSize(0.8);

        //            //Define the renderer object and use the symbol object to set the symbol reference of the current renderer
        //            SimpleRendererDefinition rendDef = new SimpleRendererDefinition();
        //            rendDef.SymbolTemplate = polySymbol.MakeSymbolReference();
        //            //Set the current renderer to the target layer
        //            layerToSymbolize.SetRenderer(layerToSymbolize.CreateRenderer(rendDef));
        //        });
        //    }
        //}

    }
}