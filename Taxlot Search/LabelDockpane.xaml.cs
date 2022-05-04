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
using ArcGIS.Desktop.Mapping;
using System.Collections.ObjectModel;
using ArcGIS.Desktop.Framework.Threading.Tasks;


namespace Taxlot_Search
{
    /// <summary>
    /// Interaction logic for LabelDockpaneView.xaml
    /// </summary>
    public partial class LabelDockpaneView : UserControl
    {
        public LabelDockpaneView()
        {
            InitializeComponent();

            foreach (int fSize in Enumerable.Range(5, 20)) {
                cBoxFontSize.Items.Add(fSize);
            }

            cBoxFontSize.Items.Add(36);
            cBoxFontSize.Items.Add(42);
            cBoxFontSize.Items.Add(48);

            cBoxFontSize.SelectedItem = 10;

            checkBoxPIN.IsChecked = true;
            checkBoxActNum.IsChecked = false;
            checkBoxOwnerName.IsChecked = true;
            checkBoxOwnerAddress.IsChecked = true;
            checkBoxAcres.IsChecked = true;
        }


    }
}
