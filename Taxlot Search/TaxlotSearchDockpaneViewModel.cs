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
        private ICommand _clearSearchListCmd;
        private ICommand _searchMapCmd;
        private ICommand _searchPINCmd;
        private ICommand _searchAssessorNumCmd;
        private ICommand _searchOwnerCmd;
        private ICommand _searchAddressCmd;
        private bool clearUIselections = false;

        protected TaxlotSearchDockpaneViewModel()
        {
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_layers, _lock);
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_propertyInfoList, _lock);
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_cBoxTownship, _lock);
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_cBoxRange, _lock);
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_cBoxSection, _lock);
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_cBoxQtrSec, _lock);
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_cBoxQtrQtrSec, _lock);
            System.Windows.Data.BindingOperations.EnableCollectionSynchronization(_cBoxCity, _lock);

            _clearSearchListCmd = new RelayCommand(() => ClearSearchList(), () => true);
            _searchMapCmd = new RelayCommand(() => SearchMap(), () => true);
            _searchPINCmd = new RelayCommand(() => SearchPin(), () => true);
            _searchAssessorNumCmd = new RelayCommand(() => SearchAssessorNum(), () => true);
            _searchOwnerCmd = new RelayCommand(() => SearchOwner(), () => true);
            _searchAddressCmd = new RelayCommand(() => ValidateSelectAddress(), () => true);

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
            initilizeCBoxValues();

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

        private ObservableCollection<string> _propertyInfoList = new ObservableCollection<string>();
        public ObservableCollection<string> PropertyInfoList
        {
            get { return _propertyInfoList; }
            set { SetProperty(ref _propertyInfoList, value); }
        }

        private string _selPropertyInfoList;
        public string SelPropertyInfoList
        {
            get { return _selPropertyInfoList; }
            set
            {
                SetProperty(ref _selPropertyInfoList, value, () => SelPropertyInfoList);
                SelectPropertyInfo();
            }
        }

        private ObservableCollection<string> _cBoxTownship = new ObservableCollection<string>();
        public ObservableCollection<string> CBoxTownship
        {
            get { return _cBoxTownship; }
        }

        private string _selCBoxTownship;
        public string SelCBoxTownship
        {
            get { return _selCBoxTownship; }
            set
            {
                SetProperty(ref _selCBoxTownship, value, () => SelCBoxTownship);
                setMapAndPINText();
            }
        }

        private ObservableCollection<string> _cBoxRange = new ObservableCollection<string>();
        public ObservableCollection<string> CBoxRange
        {
            get { return _cBoxRange; }
        }

        private string _selCBoxRange;
        public string SelCBoxRange
        {
            get { return _selCBoxRange; }
            set
            {
                SetProperty(ref _selCBoxRange, value, () => SelCBoxRange);
                setMapAndPINText();
            }
        }

        private ObservableCollection<string> _cBoxSection = new ObservableCollection<string>();
        public ObservableCollection<string> CBoxSection
        {
            get { return _cBoxSection; }
        }

        private string _selCBoxSection;
        public string SelCBoxSection
        {
            get { return _selCBoxSection; }
            set
            {
                SetProperty(ref _selCBoxSection, value, () => SelCBoxSection);
                setMapAndPINText();
            }
        }

        private ObservableCollection<string> _cBoxQtrSec = new ObservableCollection<string>();
        public ObservableCollection<string> CBoxQtrSec
        {
            get { return _cBoxQtrSec; }
        }

        private string _selCBoxQtrSec;
        public string SelCBoxQtrSec
        {
            get { return _selCBoxQtrSec; }
            set
            {
                SetProperty(ref _selCBoxQtrSec, value, () => SelCBoxQtrSec);
                setMapAndPINText();
            }
        }

        private ObservableCollection<string> _cBoxQtrQtrSec = new ObservableCollection<string>();
        public ObservableCollection<string> CBoxQtrQtrSec
        {
            get { return _cBoxQtrQtrSec; }
        }

        private string _selCBoxQtrQtrSec;
        public string SelCBoxQtrQtrSec
        {
            get { return _selCBoxQtrQtrSec; }
            set
            {
                SetProperty(ref _selCBoxQtrQtrSec, value, () => SelCBoxQtrQtrSec);
                setMapAndPINText();
            }
        }

        private ObservableCollection<string> _cBoxCity = new ObservableCollection<string>();
        public ObservableCollection<string> CBoxCity
        {
            get { return _cBoxCity; }
        }

        private string _selCBoxCity;
        public string SelCBoxCity
        {
            get { return _selCBoxCity; }
            set
            {
                SetProperty(ref _selCBoxCity, value, () => SelCBoxCity);
                if (clearUIselections == false)
                    ValidateSelectAddress();
            }
        }

        private string _txtTaxlot;
        public string TxtTaxlot
        {
            get { return _txtTaxlot; }
            set
            {
                SetProperty(ref _txtTaxlot, value, () => TxtTaxlot);
                bool success = Int32.TryParse(TxtTaxlot, out int number);
                if (TxtTaxlot != "")
                {
                    if (success)
                        setMapAndPINText();
                    else { MessageBox.Show("Enter a valid number into the Taxlot number box.", "Taxlot/Address Search Alert"); }
                }
            }
        }

        private string _txtTaxlotMap;
        public string TxtTaxlotMap
        {
            get { return _txtTaxlotMap; }
            set { SetProperty(ref _txtTaxlotMap, value, () => TxtTaxlotMap); }
        }

        private string _txtTaxlotPIN;
        public string TxtTaxlotPIN
        {
            get { return _txtTaxlotPIN; }
            set { SetProperty(ref _txtTaxlotPIN, value, () => TxtTaxlotPIN); }
        }

        private string _txtAssessorNum;
        public string TxtAssessorNum
        {
            get { return _txtAssessorNum; }
            set { SetProperty(ref _txtAssessorNum, value, () => TxtAssessorNum); }
        }

        private string _txtOwnerLastName;
        public string TxtOwnerLastName
        {
            get { return _txtOwnerLastName; }
            set { SetProperty(ref _txtOwnerLastName, value, () => TxtOwnerLastName); }
        }

        private string _txtNumber;
        public string TxtNumber
        {
            get { return _txtNumber; }
            set
            {
                SetProperty(ref _txtNumber, value, () => TxtNumber);
                if (clearUIselections == false)
                    ValidateSelectAddress();
            }
        }

        private string _txtStreet;
        public string TxtStreet
        {
            get { return _txtStreet; }
            set
            {
                SetProperty(ref _txtStreet, value, () => TxtStreet);
                if (clearUIselections == false)
                    ValidateSelectAddress();
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

        private void initilizeCBoxValues()
        {
            _cBoxTownship.Clear();
            foreach (string value in new List<string> { "09S", "10S", "11S", "12S", "13S", "14S", "15S", "16S" })
                CBoxTownship.Add(value);

            _cBoxRange.Clear();
            foreach (string value in new List<string> { "05W", "04W", "03W", "02W", "01W", "01E", "02E", "03E", "04E", "05E", "06E", "07E", "75E", "08E" })
                CBoxRange.Add(value);

            _cBoxSection.Clear();
            foreach (int value in Enumerable.Range(0, 37))
                CBoxSection.Add(value.ToString().PadLeft(2, '0'));

            _cBoxQtrSec.Clear();
            _cBoxQtrQtrSec.Clear();
            foreach (string value in new List<string> { " ", "A", "B", "C", "D" })
            {
                CBoxQtrSec.Add(value);
                CBoxQtrQtrSec.Add(value);
            }

            _cBoxCity.Clear();
            CBoxCity.Add("Entire County");
            CBoxCity.Add("Albany");
            CBoxCity.Add("Brownsville");
            CBoxCity.Add("Cascadia");
            CBoxCity.Add("Corvallis");
            CBoxCity.Add("Crabtree");
            CBoxCity.Add("Crawfordsville");
            CBoxCity.Add("Foster");
            CBoxCity.Add("Halsey");
            CBoxCity.Add("Harrisburg");
            CBoxCity.Add("Idanha");
            CBoxCity.Add("Jefferson");
            CBoxCity.Add("Lacomb");
            CBoxCity.Add("Lebanon");
            CBoxCity.Add("Lyons");
            CBoxCity.Add("McKenzie Bridge");
            CBoxCity.Add("Mill City");
            CBoxCity.Add("Millersburg");
            CBoxCity.Add("Scio");
            CBoxCity.Add("Shedd");
            CBoxCity.Add("Sodaville");
            CBoxCity.Add("Stayton");
            CBoxCity.Add("Sweet Home");
            CBoxCity.Add("Tangent");
            CBoxCity.Add("Waterloo");

            ClearSearchList();
        }

        private void ClearSearchList()
        {
            clearUIselections = true;

            ClearAndZoomFull();

            TxtTaxlot = "";
            SelCBoxTownship = null;
            SelCBoxRange = null;
            SelCBoxSection = null;
            SelCBoxQtrSec = null;
            SelCBoxQtrQtrSec = null;
            TxtTaxlotMap = "";
            TxtTaxlotPIN = "";
            TxtAssessorNum = "";
            TxtOwnerLastName = "";
            TxtNumber = "";
            TxtStreet = "";
            SelCBoxCity = "Entire County";

            clearUIselections = false;
        }

        private void ClearAndZoomFull()
        {
            if (MapView.Active != null)
            {
                // clear all map selections
                if (SelectedLayer != null)
                {
                    QueuedTask.Run(() =>
                    {
                        MapView.Active.Map.ClearSelection();
                        MapView.Active.ZoomTo(SelectedLayer);
                    });
                }
                else
                {
                    QueuedTask.Run(() => MapView.Active.Map.ClearSelection());
                }
            }
        }

        private void setMapAndPINText()
        {
            string township = "";
            string range = "";
            string section = "";

            if (SelCBoxTownship != null)
                township = SelCBoxTownship;
            if (SelCBoxRange != null)
                range = SelCBoxRange;
            if (SelCBoxSection != null)
                section = SelCBoxSection;
            else
                section = "00";

            string MAPtxt = township + range + section;
            string PINtxt = MAPtxt;

            if (SelCBoxQtrSec != null)
                if (SelCBoxQtrSec != " ")
                    MAPtxt += SelCBoxQtrSec;

            if (SelCBoxQtrQtrSec != null)
                if (SelCBoxQtrQtrSec != " ")
                    MAPtxt += SelCBoxQtrQtrSec;

            TxtTaxlotMap = MAPtxt;

            // calculate PIN
            if (SelCBoxQtrSec is null)
                PINtxt += " ";
            else
                PINtxt += SelCBoxQtrSec;

            if (SelCBoxQtrQtrSec is null)
                PINtxt += " ";
            else
                PINtxt += SelCBoxQtrQtrSec;

            if (TxtTaxlot != null && TxtTaxlot != "")
                PINtxt += TxtTaxlot.PadLeft(5, '0');

            TxtTaxlotPIN = PINtxt;
        }

        private async void SearchMap()
        {
            if (SelectedLayer != null)
            {
                if (allowedTaxlotNames.Contains(SelectedLayer.Name.ToLower()) || allowedAddressNames.Contains(SelectedLayer.Name.ToLower()))
                {
                    if (TxtTaxlotMap.Length > 10)
                        await selectTaxlotAddress(TxtTaxlotMap.Substring(0, 10));
                    else
                        await selectTaxlotAddress(TxtTaxlotMap);
                }
                else { MessageBox.Show("To search on taxlot MAP, the Linn County taxlot or address layer must be selected in the layer dropdown list.", "Taxlot/Address Search Alert"); }
            }
            else { MessageBox.Show("Please load the taxlots or address layer to the map.", "Taxlot/Address Search Alert"); }
        }

        private async void SearchPin()
        {
            if (SelectedLayer != null)
            {
                if (allowedTaxlotNames.Contains(SelectedLayer.Name.ToLower()) || allowedAddressNames.Contains(SelectedLayer.Name.ToLower()))
                    await selectTaxlotAddress(TxtTaxlotPIN);
                else { MessageBox.Show("To search on taxlot PIN, the Linn County taxlot or address layer must be selected in the layer dropdown list.", "Taxlot/Address Search Alert"); }
            }
            else { MessageBox.Show("Please load the taxlots or address layer to the map.", "Taxlot/Address Search Alert"); }
        }

        private async void SearchAssessorNum()
        {
            if (!allowedTaxlotNames.Contains(SelectedLayer.Name.ToLower()))
            {
                var featurelayerCheck = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().ToList();
                foreach (FeatureLayer layerCheckMember in featurelayerCheck)
                {
                    if (allowedTaxlotNames.Contains(layerCheckMember.Name.ToLower()))
                        SelectedLayer = layerCheckMember;
                }
            }

            if (SelectedLayer != null)
            {
                if (allowedTaxlotNames.Contains(SelectedLayer.Name.ToLower()))
                {
                    if (validateNumber(TxtAssessorNum))
                    {
                        int assessorNum;
                        bool success = Int32.TryParse(TxtAssessorNum, out assessorNum);
                        await selectTaxlotAddress("", assessorNum);
                    }
                    else { MessageBox.Show("Enter a valid number into the Assessor number box.", "Taxlot/Address Search Alert"); }
                }
                else { MessageBox.Show("To search on Assessor number, the Linn County taxlot layer must be selected in the layer dropdown list.", "Taxlot/Address Search Alert"); }
            }
            else { MessageBox.Show("Please load the taxlots or address layer to the map.", "Taxlot/Address Search Alert"); }
        }

        private async void SearchOwner()
        {
            if (!allowedTaxlotNames.Contains(SelectedLayer.Name.ToLower()))
            {
                var featurelayerCheck = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().ToList();
                foreach (FeatureLayer layerCheckMember in featurelayerCheck)
                {
                    if (allowedTaxlotNames.Contains(layerCheckMember.Name.ToLower()))
                        SelectedLayer = layerCheckMember;
                }
            }

            if (SelectedLayer != null)
            {
                if (TxtOwnerLastName != "")
                {
                    if (allowedTaxlotNames.Contains(SelectedLayer.Name.ToLower()))
                    {
                        await selectTaxlotByOwner(TxtOwnerLastName);
                    }
                    else { MessageBox.Show("To search on owner name, the Linn County taxlot layer must be selected in the layer dropdown list.", "Taxlot/Address Search Alert"); }
                }
                else { MessageBox.Show("Enter an Owner Name into the Owner Last Name textbox.", "Taxlot/Address Search Alert"); }
            }
            else { MessageBox.Show("Please load the taxlots or address layer to the map.", "Taxlot/Address Search Alert"); }
        }

        private bool validateNumber(string numberText)
        {
            int number;
            bool success = Int32.TryParse(numberText, out number);
            if (success && number > 0)
                return true;
            else
                return false;
        }

        private async Task selectTaxlotAddress(string mapPIN, int AssessorNum = 0)
        {
            //var featLayer = MapView.Active.Map.FindLayers(cBoxLayer.Text).FirstOrDefault() as FeatureLayer;
            if (SelectedLayer != null)
            {
                string runMode;
                if (allowedTaxlotNames.Contains(SelectedLayer.Name.ToLower()))
                    runMode = "taxlots";
                else if (allowedAddressNames.Contains(SelectedLayer.Name.ToLower()))
                    runMode = "address";
                else
                    runMode = "(Not Found)";

                Selection featSelection = await QueuedTask.Run<Selection>(() =>
                {
                    // clear all map selections
                    MapView.Active.Map.SetSelection(null);

                    // create query filter for selection
                    QueryFilter queryFilter = new QueryFilter();

                    if (AssessorNum > 0)
                    {
                        if (runMode == "taxlots")
                            queryFilter.WhereClause = "ACTNUM = " + AssessorNum.ToString();
                        else if (runMode == "address")
                            queryFilter.WhereClause = "ACT_NUM = " + AssessorNum.ToString();
                    }
                    else if (mapPIN.Length == 15)
                    {
                        if (runMode == "taxlots")
                            queryFilter.WhereClause = "PIN = '" + mapPIN + "'";
                        else if (runMode == "address")
                            queryFilter.WhereClause = "MAP_PIN = '" + mapPIN + "'";
                    }
                    else
                    {
                        if (runMode == "taxlots")
                            queryFilter.WhereClause = "MAP = '" + mapPIN + "'";
                        else if (runMode == "address")
                            queryFilter.WhereClause = "MAP_NUM = '" + mapPIN + "'";
                    }

                    Selection resultSel = SelectedLayer.Select(queryFilter, SelectionCombinationMethod.New);

                    if (resultSel.GetCount() == 0)
                        if (AssessorNum > 0)
                            MessageBox.Show("Assessor Number: " + AssessorNum.ToString() + " not found on layer: " + SelectedLayer.Name.ToUpper(), "Taxlot/Address Search Alert");
                        else
                            MessageBox.Show("Map/PIN: " + mapPIN + " not found on layer: " + SelectedLayer.Name.ToUpper(), "Taxlot/Address Search Alert");
                    else
                    {
                        MapView.Active.ZoomToSelected();
                        MapView.Active.ZoomOutFixed();
                    }

                    return resultSel;
                });
            }
        }

        private async Task selectTaxlotByOwner(string ownerName)
        {
            //var featLayer = MapView.Active.Map.FindLayers(cBoxLayer.Text).FirstOrDefault() as FeatureLayer;
            if (SelectedLayer != null)
            {
                string layerName = SelectedLayer.Name;
                Selection featSelection = await QueuedTask.Run<Selection>(() =>
                {
                    // clear all map selections
                    MapView.Active.Map.SetSelection(null);

                    // create query filter for selection
                    QueryFilter queryFilter = new QueryFilter();
                    queryFilter.WhereClause = "OWNER1 LIKE '%" + ownerName.ToUpper() + "%'";

                    Selection resultSel = SelectedLayer.Select(queryFilter, SelectionCombinationMethod.New);

                    if (resultSel.GetCount() == 0)
                        MessageBox.Show("Owner name: " + ownerName + " not found on layer: " + layerName, "Taxlot/Address Search Alert");
                    else
                    {
                        MapView.Active.ZoomToSelected();
                        MapView.Active.ZoomOutFixed();
                    }

                    return resultSel;
                });
            }
        }

        private async void ValidateSelectAddress()
        {
            if (!allowedAddressNames.Contains(SelectedLayer.Name.ToLower()))
            {
                var featurelayerCheck = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().ToList();
                foreach (FeatureLayer layerCheckMember in featurelayerCheck)
                {
                    if (allowedAddressNames.Contains(layerCheckMember.Name.ToLower()))
                        SelectedLayer = layerCheckMember;
                }
            }

            if (SelectedLayer != null)
            {
                if (validateNumber(TxtNumber) || TxtNumber == "")
                {
                    if (allowedAddressNames.Contains(SelectedLayer.Name.ToLower()))
                    {
                        await selectAddressesAsync(TxtNumber, TxtStreet, SelCBoxCity);
                    }
                    else { MessageBox.Show("To search for an address, the Linn County Address layer must be selected in the layer dropdown list.", "Taxlot/Address Search Alert"); }
                }
                else { MessageBox.Show("Enter a valid number into the address number field.", "Taxlot/Address Search Alert"); }
            }
            else { MessageBox.Show("Please load the taxlots or address layer to the map.", "Taxlot/Address Search Alert"); }
        }

        private async Task selectAddressesAsync(string numberVal, string streetVal, string cityVal = "")
        {
            //var featLayer = MapView.Active.Map.FindLayers(cBoxLayer.Text).FirstOrDefault() as FeatureLayer;
            if (SelectedLayer != null)
            {
                //string layerName = cBoxLayer.Text;
                await QueuedTask.Run(() =>
                {
                    // clear all map selections
                    MapView.Active.Map.SetSelection(null);

                    // create query filter for selection
                    QueryFilter queryFilter = new QueryFilter();
                    //List<string> addressListAsync = new List<string>();

                    if (numberVal != "" && numberVal != null)
                    {
                        if (streetVal != "" && streetVal != null)
                        {
                            if (cityVal != "Entire County")
                                queryFilter.WhereClause = "NUMBER = " + numberVal + " and " + "STREET LIKE '%" + streetVal.ToUpper() + "%'" + " and " + "CITY = '" + cityVal.ToUpper() + "'";
                            else
                                queryFilter.WhereClause = "NUMBER = " + numberVal + " and " + "STREET LIKE '%" + streetVal.ToUpper() + "%'";
                        }
                        else
                        {
                            if (cityVal != "Entire County")
                                queryFilter.WhereClause = "NUMBER = " + numberVal + " and " + "CITY = '" + cityVal.ToUpper() + "'";
                            else
                                queryFilter.WhereClause = "NUMBER = " + numberVal;
                        }
                    }
                    else
                    {
                        if (streetVal != "" && streetVal != null)
                        {
                            if (cityVal != "Entire County")
                                queryFilter.WhereClause = "STREET LIKE '%" + streetVal.ToUpper() + "%'" + " and " + "CITY = '" + cityVal.ToUpper() + "'";
                            else
                                queryFilter.WhereClause = "STREET LIKE '%" + streetVal.ToUpper() + "%'";
                        }
                        else
                        {
                            if (cityVal != "Entire County")
                                queryFilter.WhereClause = "CITY = '" + cityVal.ToUpper() + "'";
                            else
                                return;
                        }
                    }

                    Selection resultSel = SelectedLayer.Select(queryFilter, SelectionCombinationMethod.New);
                    //using (RowCursor rowCursor = resultSel.Search())
                    //{
                    //    while (rowCursor.MoveNext())
                    //    {
                    //        using (Row row = rowCursor.Current)
                    //        {
                    //            addressListAsync.Add(row[rowCursor.FindField("SITUS_1")].ToString());
                    //        }
                    //    }
                    //}

                    //if (resultSel.GetCount() == 0)
                    //    MessageBox.Show("Addresses were not found matching your search terms on layer: " + layerName, "Taxlot/Address Search Alert");
                    //else
                    //{
                    //MapView.Active.ZoomToSelected();
                    //MapView.Active.ZoomOutFixed();
                    //}

                    return;
                });
            }
        }

        private void SelectPropertyInfo()
        {
            // zoom to selected address
            if (SelPropertyInfoList != null)
            {
                string layerName = SelectedLayer.Name;
                string selectedLayer;
                if (allowedTaxlotNames.Contains(layerName.ToLower()))
                    selectedLayer = "taxlots";
                else if (allowedAddressNames.Contains(layerName.ToLower()))
                    selectedLayer = "address";
                else
                    selectedLayer = "(Not Found)";

                string propInfoString = SelPropertyInfoList;
                var featLayer = MapView.Active.Map.FindLayers(layerName).FirstOrDefault() as FeatureLayer;
                if (featLayer != null)
                {
                    QueuedTask.Run(() =>
                    {
                        // clear all map selections
                        MapView.Active.Map.SetSelection(null);

                        // create query filter for selection
                        QueryFilter queryFilter = new QueryFilter();

                        if (selectedLayer == "address")
                            queryFilter.WhereClause = "SITUS_1 = '" + propInfoString.Split(',')[0] + "'";
                        else if (selectedLayer == "taxlots")
                            queryFilter.WhereClause = "OWNER1 = '" + propInfoString + "'";
                        else
                            return;

                        var resultSel = featLayer.Select(queryFilter, SelectionCombinationMethod.New);

                        if (resultSel.GetCount() > 0)
                        {
                            MapView.Active.ZoomToSelected();
                            //MapView.Active.ZoomOutFixed();
                        }

                        if (resultSel.GetCount() == 1 && selectedLayer == "address")
                        {
                            // get a camera to set the zoom level
                            var camera = MapView.Active.Camera;
                            camera.Scale = 5000.0;
                            MapView.Active.ZoomTo(camera);
                        }
                    });
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
                            string newPropItem;
                            var firstSelectionSet = args.Selection.First();

                            // create an instance of the inspector class
                            var inspector = new ArcGIS.Desktop.Editing.Attributes.Inspector(false);

                            // load the selected features into the inspector using a list of object IDs
                            foreach (long selectedOID in firstSelectionSet.Value)
                            {
                                inspector.Load(firstSelectionSet.Key, selectedOID);

                                //newPropItem = new PropertyIntoItems();

                                if (selectedLayer == "address")
                                    newPropItem = inspector["SITUS_1"].ToString() + ", " + inspector["CITY"].ToString();
                                else if (selectedLayer == "taxlots")
                                    newPropItem = inspector["OWNER1"].ToString();
                                else
                                    newPropItem = "Layer not recognized";

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

        #region Commands
        public ICommand ClearSearchListCmd => _clearSearchListCmd;
        public ICommand SearchMapCmd => _searchMapCmd;
        public ICommand SearchPINCmd => _searchPINCmd;
        public ICommand SearchAssessorNumCmd => _searchAssessorNumCmd;
        public ICommand SearchOwnerCmd => _searchOwnerCmd;
        public ICommand SeaerchAddressCmd => _searchAddressCmd;

        #endregion
    }

    //public class PropertyIntoItems : DockPane
    //{
    //    private string _name;
    //    public string Name
    //    {
    //        get { return _name; }
    //        set { SetProperty(ref _name, value); }
    //    }
    //}

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

    }
}