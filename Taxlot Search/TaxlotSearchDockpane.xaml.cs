using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;



namespace Taxlot_Search
{
    /// <summary>
    /// Interaction logic for TaxlotSearchDockpaneView.xaml
    /// </summary>
    public partial class TaxlotSearchDockpaneView : UserControl
    {
        public TaxlotSearchDockpaneView()
        {
            InitializeComponent();

            //cBoxTownship.ItemsSource = new List<string> { "09S","10S","11S","12S","13S","14S","15S","16S" };
            //cBoxRange.ItemsSource = new List<string> { "05W","04W","03W","02W","01W","01E","02E","03E","04E","05E","06E","07E","75E","08E" };

            //for (int secNum = 0; secNum < 37; secNum++)
            //{
            //    cBoxSection.Items.Add(secNum);
            //}                

            //cBoxQtrSec.ItemsSource = new List<string> { " ","A", "B", "C", "D" };
            //cBoxQtrQtrSec.ItemsSource = new List<string> { " ","A", "B", "C", "D" };

            cBoxCity.Items.Add("Entire County");
            cBoxCity.Items.Add("Albany");
            cBoxCity.Items.Add("Brownsville");
            cBoxCity.Items.Add("Cascadia");
            cBoxCity.Items.Add("Corvallis");
            cBoxCity.Items.Add("Crabtree");
            cBoxCity.Items.Add("Crawfordsville");
            cBoxCity.Items.Add("Foster");
            cBoxCity.Items.Add("Halsey");
            cBoxCity.Items.Add("Harrisburg");
            cBoxCity.Items.Add("Idanha");
            cBoxCity.Items.Add("Jefferson");
            cBoxCity.Items.Add("Lacomb");
            cBoxCity.Items.Add("Lebanon");
            cBoxCity.Items.Add("Lyons");
            cBoxCity.Items.Add("McKenzie Bridge");
            cBoxCity.Items.Add("Mill City");
            cBoxCity.Items.Add("Millersburg");
            cBoxCity.Items.Add("Scio");
            cBoxCity.Items.Add("Shedd");
            cBoxCity.Items.Add("Sodaville");
            cBoxCity.Items.Add("Stayton");
            cBoxCity.Items.Add("Sweet Home");
            cBoxCity.Items.Add("Tangent");
            cBoxCity.Items.Add("Waterloo");

            cBoxCity.SelectionChanged -= cBoxCity_SelectionChanged;
            cBoxCity.SelectedIndex = 0;
            cBoxCity.SelectionChanged += cBoxCity_SelectionChanged;
        }

        //private void btnClearPIN_Click(object sender, RoutedEventArgs e)
        //{
        //    // clear all map selections
        //    ClearAndZoomFull();

        //    txtTaxlot.Text = "";
        //    cBoxTownship.SelectedItem = null;
        //    cBoxRange.SelectedItem = null;
        //    cBoxSection.SelectedItem = null;
        //    cBoxQtrSec.SelectedItem = null;
        //    cBoxQtrQtrSec.SelectedItem = null;
        //}

        //private async void btnSearchPIN_Click(object sender, RoutedEventArgs e)
        //{
        //    if (cBoxLayer.SelectedValue != null)
        //    {
        //        if (TaxlotSearchDockpaneViewModel.allowedTaxlotNames.Contains(cBoxLayer.Text.ToLower()) || TaxlotSearchDockpaneViewModel.allowedAddressNames.Contains(cBoxLayer.Text.ToLower()))
        //            await selectTaxlotAddress(txtTaxlotPIN.Text);
        //        else { MessageBox.Show("To search on taxlot PIN, the Linn County taxlot or address layer must be selected in the layer dropdown list.", "Taxlot/Address Search Alert"); }
        //    }
        //    else { MessageBox.Show("Please load the taxlots or address layer to the map.", "Taxlot/Address Search Alert"); }                
        //}

        private bool validateMAPinput()
        {
            if (cBoxTownship.SelectedValue is null)
            {
                MessageBox.Show("Select a Township from the dropdown list", "Taxlot/Address Search Alert");
                return false;
            }
            else if (cBoxRange.SelectedValue is null)
            {
                MessageBox.Show("Select a Range from the dropdown list", "Taxlot/Address Search Alert");
                return false;
            }
            else if (cBoxSection.SelectedValue is null)
            {
                MessageBox.Show("Select a Section number from the dropdown list", "Taxlot/Address Search Alert");
                return false;
            }

            return true;
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

        //private async void btnSearchMap_Click(object sender, RoutedEventArgs e)
        //{
        //    if (cBoxLayer.SelectedValue != null)
        //    {
        //        if (TaxlotSearchDockpaneViewModel.allowedTaxlotNames.Contains(cBoxLayer.Text.ToLower()) || TaxlotSearchDockpaneViewModel.allowedAddressNames.Contains(cBoxLayer.Text.ToLower()))
        //        {
        //            if (txtTaxlotMap.Text.Length > 10)
        //                await selectTaxlotAddress(txtTaxlotMap.Text.Substring(0, 10));
        //            else
        //                await selectTaxlotAddress(txtTaxlotMap.Text);
        //        }
        //        else { MessageBox.Show("To search on taxlot MAP, the Linn County taxlot or address layer must be selected in the layer dropdown list.", "Taxlot/Address Search Alert"); }
        //    }
        //    else { MessageBox.Show("Please load the taxlots or address layer to the map.", "Taxlot/Address Search Alert"); }                
        //}

        //private async Task selectTaxlotAddress(string mapPIN, int AssessorNum = 0)
        //{
        //    var featLayer = MapView.Active.Map.FindLayers(cBoxLayer.Text).FirstOrDefault() as FeatureLayer;
        //    if (featLayer != null)
        //    {
        //        string selectedLayer;
        //        if (TaxlotSearchDockpaneViewModel.allowedTaxlotNames.Contains(cBoxLayer.Text.ToLower()))
        //            selectedLayer = "taxlots";
        //        else if (TaxlotSearchDockpaneViewModel.allowedAddressNames.Contains(cBoxLayer.Text.ToLower()))
        //            selectedLayer = "address";
        //        else
        //            selectedLayer = "(Not Found)";

        //        Selection featSelection = await QueuedTask.Run<Selection>(() =>
        //        {
        //            // clear all map selections
        //            MapView.Active.Map.SetSelection(null);

        //            // create query filter for selection
        //            QueryFilter queryFilter = new QueryFilter();

        //            if (AssessorNum > 0)
        //            {
        //                if (selectedLayer == "taxlots")
        //                    queryFilter.WhereClause = "ACTNUM = " + AssessorNum.ToString();
        //                else if (selectedLayer == "address")
        //                    queryFilter.WhereClause = "ACT_NUM = " + AssessorNum.ToString();
        //            }
        //            else if (mapPIN.Length == 15)
        //            {
        //                if (selectedLayer == "taxlots")
        //                    queryFilter.WhereClause = "PIN = '" + mapPIN + "'";
        //                else if (selectedLayer == "address")
        //                    queryFilter.WhereClause = "MAP_PIN = '" + mapPIN + "'";
        //            }
        //            else
        //            {
        //                if (selectedLayer == "taxlots")
        //                    queryFilter.WhereClause = "MAP = '" + mapPIN + "'";
        //                else if (selectedLayer == "address")
        //                    queryFilter.WhereClause = "MAP_NUM = '" + mapPIN + "'";
        //            }

        //            Selection resultSel = featLayer.Select(queryFilter, SelectionCombinationMethod.New);

        //            if (resultSel.GetCount() == 0)
        //                if (AssessorNum > 0)                        
        //                    MessageBox.Show("Assessor Number: " + AssessorNum.ToString() + " not found on layer: " + selectedLayer.ToUpper(), "Taxlot/Address Search Alert");
        //                else
        //                    MessageBox.Show("Map/PIN: " + mapPIN + " not found on layer: " + selectedLayer.ToUpper(), "Taxlot/Address Search Alert");
        //            else
        //            {
        //                MapView.Active.ZoomToSelected();
        //                MapView.Active.ZoomOutFixed();
        //            }

        //            return resultSel;
        //        });
        //    }
        //}

        //private async void btnSearchAssessorNum_Click(object sender, RoutedEventArgs e)
        //{
        //    if (!TaxlotSearchDockpaneViewModel.allowedTaxlotNames.Contains(cBoxLayer.Text.ToLower()))
        //        SwitchSelectedMapLayer();

        //    if (cBoxLayer.SelectedValue != null)
        //    {
        //        if (TaxlotSearchDockpaneViewModel.allowedTaxlotNames.Contains(cBoxLayer.Text.ToLower()))
        //        {
        //            if (validateNumber(txtAssessorNum.Text))
        //            {
        //                int assessorNum;
        //                bool success = Int32.TryParse(txtAssessorNum.Text, out assessorNum);
        //                await selectTaxlotAddress("", assessorNum);
        //            }
        //            else { MessageBox.Show("Enter a valid number into the Assessor number box.", "Taxlot/Address Search Alert"); }
        //        }
        //        else { MessageBox.Show("To search on Assessor number, the Linn County taxlot layer must be selected in the layer dropdown list.", "Taxlot/Address Search Alert"); }
        //    }
        //    else { MessageBox.Show("Please load the taxlots or address layer to the map.", "Taxlot/Address Search Alert"); }
        //}

        private async void btnSearchOwner_Click(object sender, RoutedEventArgs e)
        {
            if (!TaxlotSearchDockpaneViewModel.allowedTaxlotNames.Contains(cBoxLayer.Text.ToLower()))
                SwitchSelectedMapLayer();

            if (cBoxLayer.SelectedValue != null)
            {
                if (txtOwnerLastName.Text != "")
                {
                    if (TaxlotSearchDockpaneViewModel.allowedTaxlotNames.Contains(cBoxLayer.Text.ToLower()))
                    {
                        await selectTaxlotByOwner(txtOwnerLastName.Text);
                    }
                    else { MessageBox.Show("To search on owner name, the Linn County taxlot layer must be selected in the layer dropdown list.", "Taxlot/Address Search Alert"); }
                }
                else { MessageBox.Show("Enter an Owner Name into the Owner Last Name textbox.", "Taxlot/Address Search Alert"); }
            }
            else { MessageBox.Show("Please load the taxlots or address layer to the map.", "Taxlot/Address Search Alert"); }
        }

        private async Task selectTaxlotByOwner(string ownerName)
        {
            var featLayer = MapView.Active.Map.FindLayers(cBoxLayer.Text).FirstOrDefault() as FeatureLayer;
            if (featLayer != null)
            {
                string layerName = cBoxLayer.Text;
                Selection featSelection = await QueuedTask.Run<Selection>(() =>
                {
                    // clear all map selections
                    MapView.Active.Map.SetSelection(null);

                    // create query filter for selection
                    QueryFilter queryFilter = new QueryFilter();
                    queryFilter.WhereClause = "OWNER1 LIKE '%" + ownerName.ToUpper() + "%'";

                    Selection resultSel = featLayer.Select(queryFilter, SelectionCombinationMethod.New);

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

        private async void txtNumber_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!TaxlotSearchDockpaneViewModel.allowedAddressNames.Contains(cBoxLayer.Text.ToLower()))
                SwitchSelectedMapLayer();

            if (cBoxLayer.SelectedValue != null)
            {
                if (validateNumber(txtNumber.Text) || txtNumber.Text == "")
                {
                    if (TaxlotSearchDockpaneViewModel.allowedAddressNames.Contains(cBoxLayer.Text.ToLower()))
                    {
                        await selectAddressesAsync(txtNumber.Text, txtStreet.Text, cBoxCity.SelectedValue.ToString());
                    }
                    else { MessageBox.Show("To search for an address, the Linn County Address layer must be selected in the layer dropdown list.", "Taxlot/Address Search Alert"); }
                }
                else { MessageBox.Show("Enter a valid number into the address number field.", "Taxlot/Address Search Alert"); }
            }
            else { MessageBox.Show("Please load the taxlots or address layer to the map.", "Taxlot/Address Search Alert"); }
        }

        private async Task selectAddressesAsync(string numberVal, string streetVal, string cityVal = "")
        {
            var featLayer = MapView.Active.Map.FindLayers(cBoxLayer.Text).FirstOrDefault() as FeatureLayer;
            if (featLayer != null)
            {
                //string layerName = cBoxLayer.Text;
                await QueuedTask.Run(() =>
                {
                    // clear all map selections
                    MapView.Active.Map.SetSelection(null);

                    // create query filter for selection
                    QueryFilter queryFilter = new QueryFilter();
                    //List<string> addressListAsync = new List<string>();

                    if (numberVal != "")
                        if (streetVal != "")
                            if (cityVal != "Entire County")
                                queryFilter.WhereClause = "NUMBER = " + numberVal + " and " + "STREET LIKE '%" + streetVal.ToUpper() + "%'" + " and " + "CITY = '" + cityVal.ToUpper() + "'";
                            else
                                queryFilter.WhereClause = "NUMBER = " + numberVal + " and " + "STREET LIKE '%" + streetVal.ToUpper() + "%'";
                        else
                            if (cityVal != "Entire County")
                            queryFilter.WhereClause = "NUMBER = " + numberVal + " and " + "CITY = '" + cityVal.ToUpper() + "'";
                        else
                            queryFilter.WhereClause = "NUMBER = " + numberVal;
                    else
                        if (streetVal != "")
                            if (cityVal != "Entire County")
                                queryFilter.WhereClause = "STREET LIKE '%" + streetVal.ToUpper() + "%'" + " and " + "CITY = '" + cityVal.ToUpper() + "'";
                            else
                                queryFilter.WhereClause = "STREET LIKE '%" + streetVal.ToUpper() + "%'";
                        else
                            if (cityVal != "Entire County")
                                queryFilter.WhereClause = "CITY = '" + cityVal.ToUpper() + "'";
                            else
                                return;

                    Selection resultSel = featLayer.Select(queryFilter, SelectionCombinationMethod.New);
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

        private async void txtStreet_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!TaxlotSearchDockpaneViewModel.allowedAddressNames.Contains(cBoxLayer.Text.ToLower()))
                SwitchSelectedMapLayer();

            if (cBoxLayer.SelectedValue != null)
            {
                if (validateNumber(txtNumber.Text) || txtNumber.Text == "")
                {
                    if (TaxlotSearchDockpaneViewModel.allowedAddressNames.Contains(cBoxLayer.Text.ToLower()))
                    {
                        await selectAddressesAsync(txtNumber.Text, txtStreet.Text, cBoxCity.SelectedValue.ToString());
                    }
                    else { MessageBox.Show("To search for an address, the Linn County Address layer must be selected in the layer dropdown list.", "Taxlot/Address Search Alert"); }
                }
                else { MessageBox.Show("Enter a valid number into the address number field.", "Taxlot/Address Search Alert"); }
            }
            else { MessageBox.Show("Please load the taxlots or address layer to the map.", "Taxlot/Address Search Alert"); }
        }

        private async void cBoxCity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!TaxlotSearchDockpaneViewModel.allowedAddressNames.Contains(cBoxLayer.Text.ToLower()))
                SwitchSelectedMapLayer();

            if (cBoxLayer.SelectedValue != null)
            {
                if (validateNumber(txtNumber.Text) || txtNumber.Text == "")
                {
                    if (TaxlotSearchDockpaneViewModel.allowedAddressNames.Contains(cBoxLayer.Text.ToLower()))
                    {
                        await selectAddressesAsync(txtNumber.Text, txtStreet.Text, cBoxCity.SelectedValue.ToString());
                    }
                    else { MessageBox.Show("To search for an address, the Linn County Address layer must be selected in the layer dropdown list.", "Taxlot/Address Search Alert"); }
                }
                else { MessageBox.Show("Enter a valid number into the address number field.", "Taxlot/Address Search Alert"); }
            }
            else { MessageBox.Show("Please load the taxlots or address layer to the map.", "Taxlot/Address Search Alert"); }
        }

        private void lBoxPropertyInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // zoom to selected address
            if (lBoxPropertyInfo.SelectedValue != null)
            {
                string layerName = cBoxLayer.SelectedValue.ToString();
                string selectedLayer;
                if (TaxlotSearchDockpaneViewModel.allowedTaxlotNames.Contains(cBoxLayer.Text.ToLower()))
                    selectedLayer = "taxlots";
                else if (TaxlotSearchDockpaneViewModel.allowedAddressNames.Contains(cBoxLayer.Text.ToLower()))
                    selectedLayer = "address";
                else
                    selectedLayer = "(Not Found)";

                string propInfoString = lBoxPropertyInfo.SelectedValue.ToString();
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

        //private void btnClearSearchList_Click(object sender, RoutedEventArgs e)
        //{
        //    ClearAndZoomFull();

        //    txtNumber.TextChanged -= txtNumber_TextChanged;
        //    txtStreet.TextChanged -= txtStreet_TextChanged;
        //    cBoxCity.SelectionChanged -= cBoxCity_SelectionChanged;
        //    txtNumber.Text = "";
        //    txtStreet.Text = "";
        //    cBoxCity.SelectedIndex = 0;
        //    txtNumber.TextChanged += txtNumber_TextChanged;
        //    txtStreet.TextChanged += txtStreet_TextChanged;
        //    cBoxCity.SelectionChanged += cBoxCity_SelectionChanged;

        //    txtTaxlot.Text = "";
        //    cBoxTownship.SelectedItem = null;
        //    cBoxRange.SelectedItem = null;
        //    cBoxSection.SelectedItem = null;
        //    cBoxQtrSec.SelectedItem = null;
        //    cBoxQtrQtrSec.SelectedItem = null;
        //}

        //private void ClearAndZoomFull()
        //{
        //    // clear all map selections
        //    var featLayer = MapView.Active.Map.FindLayers(cBoxLayer.Text).FirstOrDefault() as FeatureLayer;
        //    if (featLayer != null)
        //    {
        //        _ = QueuedTask.Run(() =>
        //        {
        //            MapView.Active.Map.SetSelection(null);
        //            MapView.Active.ZoomTo(featLayer);
        //        });
        //    }
        //    else { _ = QueuedTask.Run(() => MapView.Active.Map.SetSelection(null)); }
        //}

        private void SwitchSelectedMapLayer()
        {
            if (cBoxLayer.SelectedIndex == 0)
                cBoxLayer.SelectedIndex = 1;
            else
                cBoxLayer.SelectedIndex = 0;
        }

        //private void setMapAndPINText()
        //{
        //    string MAPtxt = "";
        //    string PINtxt = "";
        //    string township = "";
        //    string range = "";

        //    if (cBoxTownship.SelectedValue != null)
        //        township = cBoxTownship.SelectedValue.ToString();
        //    if (cBoxRange.SelectedValue != null)
        //        range = cBoxRange.SelectedValue.ToString();

        //    MAPtxt = township + range;
        //    if (cBoxSection.SelectedValue != null)
        //    {
        //        if (cBoxSection.SelectedValue.ToString().Length < 2)
        //        {
        //            MAPtxt += "0" + cBoxSection.SelectedValue.ToString();
        //        }
        //        else
        //        {
        //            MAPtxt += cBoxSection.SelectedValue.ToString();
        //        }
        //    }

        //    PINtxt = MAPtxt;

        //    if (cBoxQtrSec.SelectedValue != null)
        //        if (cBoxQtrSec.SelectedValue.ToString() != " ")
        //            MAPtxt += cBoxQtrSec.SelectedValue.ToString();

        //    if (cBoxQtrQtrSec.SelectedValue != null)
        //        if (cBoxQtrQtrSec.SelectedValue.ToString() != " ")
        //            MAPtxt += cBoxQtrQtrSec.SelectedValue.ToString();

        //    txtTaxlotMap.Text = MAPtxt;

        //    // calculate PIN
        //    if (cBoxQtrSec.SelectedValue is null)
        //        PINtxt += " ";
        //    else
        //        PINtxt += cBoxQtrSec.SelectedValue.ToString();

        //    if (cBoxQtrQtrSec.SelectedValue is null)
        //        PINtxt += " ";
        //    else
        //        PINtxt += cBoxQtrQtrSec.SelectedValue.ToString();

        //    if (txtTaxlot.Text != null)
        //        PINtxt += txtTaxlot.Text.PadLeft(5, '0');

        //    txtTaxlotPIN.Text = PINtxt;
        //}

        //private void cBoxTownship_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    setMapAndPINText();
        //}

        //private void cBoxRange_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    setMapAndPINText();
        //}

        //private void cBoxSection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    setMapAndPINText();
        //}

        //private void cBoxQtrSec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    setMapAndPINText();
        //}

        //private void cBoxQtrQtrSec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    setMapAndPINText();
        //}

    //    private void txtTaxlot_TextChanged(object sender, TextChangedEventArgs e)
    //    {
    //        int number;
    //        bool success = Int32.TryParse(txtTaxlot.Text, out number);

    //        if (txtTaxlot.Text != "")
    //        {
    //            if (success)
    //                setMapAndPINText();
    //            else { MessageBox.Show("Enter a valid number into the Taxlot number box.", "Taxlot/Address Search Alert"); }
    //        }
    //    }
    }
}

