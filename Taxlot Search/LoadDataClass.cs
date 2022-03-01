using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace Taxlot_Search
{
    internal class LoadDataClass
    {
        public static string localGISdirectory = @"C:\GIS\shapefiles";

        public static async Task CheckForMap()
        {
            // check if active map exists and create if necessary
            if (MapView.Active is null)
            {
                IMapPane createMapResult = await QueuedTask.Run<IMapPane>(() =>
                {
                    var map = MapFactory.Instance.CreateMap("Linn County", basemap: Basemap.ProjectDefault);
                    return ProApp.Panes.CreateMapPaneAsync(map);
                });
            }
        }

        public static async void LoadMapServiceToLayer(string serviceURL)
        {
            await CheckForMap();

            // check if layer is already loaded
            var featurelayerCheck = MapView.Active.Map.GetLayersAsFlattenedList().OfType<ServiceLayer>().ToList();
            bool datasetLoaded = false;

            // OfType<TiledServiceLayer> only works on ortho images with cache. The non-cashed verison is probably a DynamicServiceLayer. Just ServiceLayer seems to be more generic.
            foreach (ServiceLayer layerCheckMember in featurelayerCheck)
            {
                if (layerCheckMember.URL == serviceURL)
                {
                    await QueuedTask.Run(() => { layerCheckMember.SetVisibility(true); });
                    datasetLoaded = true;
                }
            }

            if (datasetLoaded == false)
            {
                FeatureLayer loadedLayer = await QueuedTask.Run<FeatureLayer>(() =>
                {
                    try
                    {
                        var imageLayer = LayerFactory.Instance.CreateLayer(new Uri(serviceURL, UriKind.Absolute), MapView.Active.Map, LayerPosition.AddToBottom) as FeatureLayer;

                        return MapView.Active.GetSelectedLayers()[0] as FeatureLayer;
                    }
                    catch
                    {
                        MessageBox.Show("failed. ;(");
                        return null;
                    }
                });
            }
        }

        public static async void LoadDatasetToLayer(string pathToData, string DataSetName)
        {
            await CheckForMap();

            // check if layer is already loaded
            var featurelayerCheck = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().ToList();
            bool datasetLoaded = false;

            foreach (FeatureLayer layerCheckMember in featurelayerCheck)
            {
                if (DataSetName.ToLower() == layerCheckMember.Name.ToLower())
                    datasetLoaded = true;
                else if (DataSetName == "taxlots")
                {
                    if (TaxlotSearchDockpaneViewModel.allowedTaxlotNames.Contains(layerCheckMember.Name.ToLower()))
                        datasetLoaded = true;
                }
                else if (DataSetName == "Address")
                {
                    if (TaxlotSearchDockpaneViewModel.allowedAddressNames.Contains(layerCheckMember.Name.ToLower()))
                        datasetLoaded = true;
                }

                if (datasetLoaded)
                {
                    await QueuedTask.Run(() => { layerCheckMember.SetVisibility(true); });
                    break;
                }
            }

            if (datasetLoaded == false)
            {
                FeatureLayer loadedLayer = await QueuedTask.Run<FeatureLayer>(() =>
                {
                    try
                    {
                        // open shapefile to map layer
                        System.Uri uriPath = new System.Uri(pathToData + "\\" + DataSetName + ".shp");
                        FeatureLayer shp_layer = LayerFactory.Instance.CreateFeatureLayer(uriPath, MapView.Active.Map, layerName: DataSetName, rendererDefinition: getSimpleRendererDef(DataSetName.ToLower()));

                        // set unique values on zoning layer
                        if (DataSetName.ToLower() == "zoning")
                        {
                            //For examination in the debugger...
                            // var renderer = shp_layer.GetRenderer();
                            // string xmlDef = renderer.ToXml();
                            // string xmlDef2 = _layerCimRenderer.ToXml();

                            shp_layer.SetRenderer(CreateUniqueValueRendererForZoning());
                        }

                        // return MapView.Active.GetSelectedLayers()[0] as FeatureLayer;
                        return shp_layer;
                    }
                    catch
                    {
                        MessageBox.Show("C:\\GIS\\Shapefiles load failed for: " + DataSetName + " attempting to load layer from the LinnPublication SDE");
                        return null;
                    }

                    //var uriPath = new Uri(pathToShp);
                    //var FSpath = new FileSystemConnectionPath(uriPath, FileSystemDatastoreType.Shapefile);
                    //using (var shapefileFolder = new FileSystemDatastore(FSpath))
                    //{
                    //    FeatureClass taxLotsFeatureClass = shapefileFolder.OpenDataset<FeatureClass>(shpName);
                    //    int nCount = taxLotsFeatureClass.GetCount();
                    //    MessageBox.Show(String.Format("Feature count: {0}", nCount.ToString()));

                    //}
                });

                if (loadedLayer == null)
                {
                    FeatureLayer loadedLayerSDE = await QueuedTask.Run<FeatureLayer>(() =>
                    {
                        try
                        {
                            using (Geodatabase linnPublicationSDE = new Geodatabase(new DatabaseConnectionFile(new Uri(@"\\lc-gis\data\linn_geodatabase\Publication\Connection to lc-sql2016.sde"))))
                            {
                                // MessageBox.Show("Attempting to open featureclass: " + DataSetName + " from: " + @"\\lc-gis\data\linn_geodatabase\Publication\Connection to lc-sql2016.sde");
                                // MessageBox.Show("Gdb connection string: " + linnPublicationSDE.GetConnectionString());
                                // MessageBox.Show("Gdb type: " + linnPublicationSDE.GetGeodatabaseType().ToString());
                                FeatureClass fcSDE = linnPublicationSDE.OpenDataset<FeatureClass>("LinnPublication.DBO." + DataSetName);
                                // MessageBox.Show("Attempting to create layer from featureclass and set symbology");
                                FeatureLayer SDElayer = LayerFactory.Instance.CreateFeatureLayer(fcSDE, MapView.Active.Map, layerName: DataSetName, rendererDefinition: getSimpleRendererDef(DataSetName.ToLower())) as FeatureLayer;

                                // set unique values on zoning layer
                                if (DataSetName.ToLower() == "zoning")
                                    SDElayer.SetRenderer(CreateUniqueValueRendererForZoning());

                                // FeatureClassDefinition featureClassDefinition = linnPublicationSDE.GetDefinition<FeatureClassDefinition>("LinnPublication.DBO." + DataSetName);


                                // load taxlots off the Portal - Note, this is very slow to load
                                // var by_ref_id = @"https://gis.co.linn.or.us/public/rest/services/baselayers/Pub_taxlots/MapServer/0";
                                //FeatureLayer portalLayer = LayerFactory.Instance.CreateLayer(new Uri(by_ref_id, UriKind.Absolute), MapView.Active.Map, layerName: shpName) as FeatureLayer;

                                return SDElayer;
                            }
                        }
                        catch (GeodatabaseNotFoundOrOpenedException e)
                        {
                            MessageBox.Show("Geodatabase Not Found or Opened: " + e.Message);
                            return null;
                        }
                        catch (InvalidOperationException e)
                        {
                            MessageBox.Show("Invalid Operation: " + e.Message);
                            return null;
                        }
                        catch (GeodatabaseException e)
                        {
                            MessageBox.Show("Geodatabase Exception: " + e.Message);
                            return null;
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Load data from SDE failed for: " + DataSetName + " Exception: " + e.Message);
                            MessageBox.Show("Caught: " + e.GetType().ToString());
                            return null;
                        }

                    });

                    if (loadedLayerSDE != null)
                        await setFeatureLabels(loadedLayerSDE);
                }
                else
                    await setFeatureLabels(loadedLayer);
            }
        }

        private static async Task setFeatureLabels(FeatureLayer featureLayerToLabel)
        {
            if (featureLayerToLabel.Name.ToLower() == "roads")
                await QueuedTask.Run(() =>
                {
                    var lyrDefn = featureLayerToLabel.GetDefinition() as CIMFeatureLayer;
                    //Get the label classes - and check if the correct label class we need to label the line features with exists
                    var listLabelClasses = lyrDefn.LabelClasses.ToList();
                    var theLabelClass = listLabelClasses.Where(l => l.Name == "Class 1").FirstOrDefault();
                    //if (theLabelClass == null) //create label class and add to the collection
                    //{
                    //    theLabelClass = await CreateAndApplyLabelClassAsync(featureLayer);
                    //    listLabelClasses.Add(theLabelClass);
                    //}

                    // set default label
                    theLabelClass.Visibility = true;
                    theLabelClass.Expression = "$feature.STREET";
                    theLabelClass.MaplexLabelPlacementProperties.LineFeatureType = MaplexLineFeatureType.Street;
                    theLabelClass.MaplexLabelPlacementProperties.PrimaryOffset = 2.0;
                    theLabelClass.MinimumScale = 105000;
                    theLabelClass.MaplexLabelPlacementProperties.MinimumFeatureSizeUnit = MaplexUnit.MM;
                    theLabelClass.MaplexLabelPlacementProperties.MinimumSizeForLabeling = 20.0;

                    var textSym = theLabelClass.TextSymbol.Symbol as CIMTextSymbol;
                    textSym.Height = 8;

                    // create new solid fill symbol to use for halo
                    var haloFill = new CIMSolidFill
                    {
                        Enable = true,
                        Color = CIMColor.CreateRGBColor(255, 255, 255),
                        ColorLocked = false
                    };
                    
                    //Define the array of Symbol layers (holds the halo solid fill only)
                    var symbolLyrs = new CIMSymbolLayer[] { haloFill };
                    //Creates the Polygon symbol with the halo solid fill layer
                    var haloSymbol = new CIMPolygonSymbol { SymbolLayers = symbolLyrs };
                    //Assign this halo symbol and its size to the text symbol
                    textSym.HaloSymbol = haloSymbol;

                    // var xmlDef = theLabelClass.StandardLabelPlacementProperties.ToXml();

                    //Apply the label classes back to the layer definition
                    lyrDefn.LabelClasses = listLabelClasses.ToArray();
                    // set the labels to visible
                    lyrDefn.LabelVisibility = true;
                    //Set the layer definition
                    featureLayerToLabel.SetDefinition(lyrDefn);
                    //set the label's visiblity
                    //featureLayerToLabel.SetLabelVisibility(true);
                });
        }

        private static SimpleRendererDefinition getSimpleRendererDef(string DataSetName)
        {
            List<string> polygonLayers = new List<string>
            {
                "taxlots",
                "countyline"
            };

            List<string> lineLayers = new List<string>
            {
                "roads",
                "2fturban",
                "5ftnorth",
                "5ftsouth"
            };

            List<string> pointLayers = new List<string> { "address" };
            // define the renderer object
            SimpleRendererDefinition rendDef = new SimpleRendererDefinition();

            if (lineLayers.Contains(DataSetName))
            {
                CIMLineSymbol lineSymbol = SymbolFactory.Instance.ConstructLineSymbol();

                if (DataSetName == "roads")
                {
                    lineSymbol.SetColor(CIMColor.CreateRGBColor(115, 0, 0));
                    lineSymbol.SetSize(1.0);
                }
                else if (DataSetName == "2fturban" || DataSetName == "5ftnorth" || DataSetName == "5ftsouth")
                {
                    lineSymbol.SetColor(CIMColor.CreateRGBColor(92, 137, 68));
                    lineSymbol.SetSize(0.4);
                }

                //use the symbol object to set the symbol reference of the current renderer
                rendDef.SymbolTemplate = lineSymbol.MakeSymbolReference();
            }
            else if (polygonLayers.Contains(DataSetName))
            {
                CIMPolygonSymbol polySymbol = SymbolFactory.Instance.ConstructPolygonSymbol();

                if (DataSetName == "taxlots")
                {
                    polySymbol.SetOutlineColor(CIMColor.CreateRGBColor(156, 156, 156));
                    polySymbol.SetColor(CIMColor.NoColor());
                    polySymbol.SetSize(0.8);
                }
                else if (DataSetName == "countyline")
                {
                    polySymbol.SetOutlineColor(CIMColor.CreateRGBColor(0, 0, 0));
                    polySymbol.SetColor(CIMColor.NoColor());
                    polySymbol.SetSize(2.0);
                }

                //use the symbol object to set the symbol reference of the current renderer
                rendDef.SymbolTemplate = polySymbol.MakeSymbolReference();
                //Set the current renderer to the target layer
                //layerToSymbolize.SetRenderer(layerToSymbolize.CreateRenderer(rendDef));
            }
            else if (pointLayers.Contains(DataSetName))
            {
                CIMPointSymbol pointSymbol = SymbolFactory.Instance.ConstructPointSymbol();

                if (DataSetName == "address")
                {
                    pointSymbol.SetColor(CIMColor.CreateRGBColor(230, 152, 0));
                    pointSymbol.SetSize(4.0);
                }

                rendDef.SymbolTemplate = pointSymbol.MakeSymbolReference();
            }

            return rendDef;

        }

        /// <summary>
        /// Warning! You must call this method on the MCT!
        /// </summary>
        /// <returns></returns>
        private static CIMRenderer CreateUniqueValueRendererForZoning()
        {
            //All of these methods have to be called on the MCT
            //if (Module1.OnUIThread)
            //    throw new CalledOnWrongThreadException();

            //Create the Unique Value Renderer
            CIMUniqueValueRenderer uniqueValueRenderer = new CIMUniqueValueRenderer()
            {
                // set the value field
                Fields = new string[] { "ZONING" }
            };

            // set outline color and stroke weight for new polygon symbols
            CIMStroke outline = SymbolFactory.Instance.ConstructStroke(CIMColor.CreateRGBColor(105, 105, 105), 0.7, SimpleLineStyle.Solid);

            //Construct the list of UniqueValueClasses
            List<CIMUniqueValueClass> classes = new List<CIMUniqueValueClass>();

            // City level zoning
            List<CIMUniqueValue> ZoningValues = new List<CIMUniqueValue>();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "Albany" } });
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "Brownsville" } });
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "Gates" } });
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "Halsey" } });
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "Harrisburg" } });
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "Lebanon" } });
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "Mill City" } });
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "Millersburg" } });
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "Scio" } });
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "Sodaville" } });
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "Sweet Home" } });
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "Tangent" } });
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "Waterloo" } });

            var ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "City Level Zoning",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(178, 178, 178), SimpleFillStyle.Solid, outline) }
            };

            classes.Add(ZoneClass);

            // AB
            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "AB" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "AB",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(252, 208, 182), SimpleFillStyle.Solid, outline) }
            };

            classes.Add(ZoneClass);

            // ARO
            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "ARO" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "ARO",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(187, 252, 248), SimpleFillStyle.Solid, outline) }
            };

            classes.Add(ZoneClass);

            // EFU
            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "EFU" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "EFU",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(255, 255, 190), SimpleFillStyle.Solid, outline) }
            };

            classes.Add(ZoneClass);

            // F/F
            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { @"F/F" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = @"F/F",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(226, 252, 207), SimpleFillStyle.Solid, outline) }
            };

            classes.Add(ZoneClass);

            // FCM
            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "FCM" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "FCM",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(56, 168, 0), SimpleFillStyle.Solid, outline) }
            };

            classes.Add(ZoneClass);

            // FIC
            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "FIC" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "FIC",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(184, 217, 252), SimpleFillStyle.Solid, outline) }
            };

            classes.Add(ZoneClass);

            // FIC-LUO
            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "FIC-LUO" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "FIC-LUO",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(252, 227, 192), SimpleFillStyle.Solid, outline) }
            };

            classes.Add(ZoneClass);

            // HI
            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "HI" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "HI",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(252, 204, 234), SimpleFillStyle.Solid, outline) }
            };

            classes.Add(ZoneClass);

            // HI-LUO
            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "HI-LUO" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "HI-LUO",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(252, 199, 241), SimpleFillStyle.Solid, outline) }
            };

            classes.Add(ZoneClass);

            // HRO
            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "HRO" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "HRO",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(199, 232, 252), SimpleFillStyle.Solid, outline) }
            };

            classes.Add(ZoneClass);

            // LI
            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "LI" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "LI",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(252, 215, 236), SimpleFillStyle.Solid, outline) }
            };

            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "NR-5" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "NR-5",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(245, 252, 197), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "RCM" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "RCM",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(204, 187, 252), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "RCT-1" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "RCT-1",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(189, 203, 252), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "RCT-2.5" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "RCT-2.5",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(179, 252, 228), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "RCT-5" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "RCT-5",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(239, 252, 179), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "RR-1" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "RR-1",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(252, 179, 183), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "RR-2.5" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "RR-2.5",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(184, 182, 252), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "RR-5" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "RR-5",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(179, 238, 252), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "RR-10" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "RR-10",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(187, 193, 252), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "UD-I" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "UD-I",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(182, 229, 252), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "UD-II" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "UD-II",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(252, 219, 212), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "UGA-AB" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "UGA-AB",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(252, 192, 196), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "UGA-EFU" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "UGA-EFU",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(212, 218, 252), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "UGA-EFU-80" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "UGA-EFU-80",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(192, 252, 232), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { @"UGA-F/F" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = @"UGA-F/F",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(209, 179, 252), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "UGA-HI" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "UGA-HI",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(252, 189, 208), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "UGA-LI" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "UGA-LI",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(179, 252, 207), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "UGA-RCM" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "UGA-RCM",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(252, 208, 194), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "UGA-RR-1" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "UGA-RR-1",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(214, 252, 199), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "UGA-RR-2.5" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "UGA-RR-2.5",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(235, 197, 252), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "UGA-RR-5" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "UGA-RR-5",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(199, 222, 252), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "UGA-UMG-2.5" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "UGA-UMG-2.5",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(230, 215, 252), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "UGA-UMG-5" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "UGA-UMG-5",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(210, 242, 252), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "UGA-UMG-10" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "UGA-UMG-10",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(252, 234, 182), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            ZoningValues.Clear();
            ZoningValues.Add(new CIMUniqueValue() { FieldValues = new string[] { "UGA-UMG-20" } });
            ZoneClass = new CIMUniqueValueClass()
            {
                Values = ZoningValues.ToArray(),
                Label = "UGA-UMG-20",
                Visible = true,
                Editable = true,
                Symbol = new CIMSymbolReference() { Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(CIMColor.CreateRGBColor(191, 252, 179), SimpleFillStyle.Solid, outline) }
            };
            classes.Add(ZoneClass);

            // so on and so forth for all the 51.
            //....

            //Add the classes to a group (by default there is only one group or "symbol level")
            // Unique value groups
            CIMUniqueValueGroup groupOne = new CIMUniqueValueGroup()
            {
                Heading = "County Zoning",
                Classes = classes.ToArray()
            };
            uniqueValueRenderer.Groups = new CIMUniqueValueGroup[] { groupOne };

            //Draw the rest with the default symbol
            uniqueValueRenderer.UseDefaultSymbol = true;
            uniqueValueRenderer.DefaultLabel = "All other values";

            var defaultColor = CIMColor.CreateRGBColor(178, 178, 178);
            uniqueValueRenderer.DefaultSymbol = new CIMSymbolReference()
            {
                Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(defaultColor, SimpleFillStyle.Solid, outline)
            };

            return uniqueValueRenderer as CIMRenderer;
        }

    }
}
