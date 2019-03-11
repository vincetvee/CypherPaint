﻿
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;

namespace DiagramDesigner
{

    public class SelectCriteria
    {
        public string Value { get; set; }
        public string Content { get; set; }
    }

    public class CanvasDto
    {
        public Dictionary<int, CanvasItemDetail> MyCanvasStore { get; set; }
    }

    public class CanvasItemDetail
    {
        public string ShapeType { get; set; }
        public int Index { get; set; }

        //public Shape Shape { get; set; }

        public double Height { get; set; }

        public double Width { get; set; }

        public double MinHeight { get; set; }

        public double MinWidth { get; set; }

        public string Data { get; set; }

        public double[] VisualOffset { get; set; }

        //public DesignerItem DesignerItem { get; set; }

        public Brush Stroke { get; set; }

        public string FillString { get; set; }

        public Brush Fill { get; set; }

    }

    public partial class Window1 : Window
    {
        public bool SampleBoolInParent { get; set; }

        public string[] NamesOfItemsOnCanvas;
        public List<string> NamesOfItemsAddedToCanvas;

        public List<SelectCriteria> selectCriterias { get; set; }

        private ArrayList selectionCriteriaList = new ArrayList();
        private Dictionary<string, string> MyCanvasDictionary;
        private Dictionary<int, CanvasItemDetail> MyCanvasStore;
        private CanvasDto canvasDto;

        private string dJsonString { get; set; }

        public IEnumerable<DesignerItem> SelectedItems { get; set; }

        // store shape  in the object
        public Shape CurrentSelectedToolBoxItem { get; set; }

        public Window1()
        {
            var myViewModel = new MainViewModel();
            InitializeComponent();
            DataContext = this;
            // creating a list
            selectCriterias = new List<SelectCriteria>
            {
                new SelectCriteria { Value = "firstItem", Content = "Select First Item" },
                new SelectCriteria { Value = "lastItem", Content = "Select Last Item" },
                new SelectCriteria { Value = "middleItem", Content = "Select Middle Item" },
                new SelectCriteria { Value = "allItem", Content = "Select All Item" }
            };
            MyCanvasDictionary = new Dictionary<string, string>();
            MyCanvasStore = new Dictionary<int, CanvasItemDetail>();
            canvasDto = new CanvasDto();
            //this.DataContext = myViewModel;
            if (MyDesignerCanvas != null && MyDesignerCanvas.Children.Count == 0)
            {
                NamesOfItemsOnCanvas = new string[8];
                NamesOfItemsAddedToCanvas = new List<string>();
            }

        }

        public void WriteDictionaryToCanvas()
        {

        }

        private void Rotate(double angle)
        {
            foreach (DesignerItem item in MyDesignerCanvas.SelectedItems)
            {
                FrameworkElement element = item.Content as FrameworkElement;
                if (element != null)
                {
                    RotateTransform rotateTransform = element.LayoutTransform as RotateTransform;
                    if (rotateTransform == null)
                    {
                        rotateTransform = new RotateTransform();
                        element.LayoutTransform = rotateTransform;
                    }

                    rotateTransform.Angle = (rotateTransform.Angle + angle) % 360;
                    Canvas.SetLeft(item, Canvas.GetLeft(item) - (item.Height - item.Width) / 2);
                    Canvas.SetTop(item, Canvas.GetTop(item) - (item.Width - item.Height) / 2);
                    double width = item.Width;
                    item.Width = item.Height;
                    item.Height = width;
                }
            }
        }

        public void SerializeToBinary()
        {

            var bf = new BinaryFormatter();

            FileStream fsout = new FileStream("dummy.binary", FileMode.Create, FileAccess.Write, FileShare.None);
            try
            {
                using (fsout)
                {
                    bf.Serialize(fsout, canvasDto);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeSerializeFromBinary()
        {
            var bf = new BinaryFormatter();
            var fsin = new FileStream("dummy.binary", FileMode.Open, FileAccess.Read, FileShare.None);

            try
            {
                using (fsin)
                {
                    canvasDto = (CanvasDto)bf.Deserialize(fsin);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SerializeToJson()
        {
            string json = JsonConvert.SerializeObject(canvasDto);
            dJsonString = json;
        }

        public void DeSerializeFromJson()
        {
            canvasDto = JsonConvert.DeserializeObject<CanvasDto>(dJsonString);
        }

        private void Click_save(object sender, RoutedEventArgs e)
        {
            var dlen = MyDesignerCanvas.Children.Count;
            MyCanvasStore = new Dictionary<int, CanvasItemDetail>();


            for (int i = 0; i < dlen; i++)
            {
                var dItem = MyDesignerCanvas.Children[i] as DesignerItem;
                var dvt = VisualTreeHelper.GetOffset(dItem);
                ///TODO: also get the position on canvas before saving the stuff
                var dshape = dItem.Content as Shape;
                var dpath = dshape as System.Windows.Shapes.Path;
                var dfill = dpath.Fill.ToString().Split('.').Count() == 0 ? dpath.Fill : new SolidColorBrush(Colors.Transparent);
                MyCanvasStore.Add(i, new CanvasItemDetail
                {
                    //DesignerItem = dItem,
                    FillString = dpath.Fill.ToString(),
                    //Shape = dshape,
                    Fill = dfill,
                    Index = i,
                    Data = dpath.Data.ToString(),
                    Height = dshape.ActualHeight,
                    MinHeight = dshape.MinHeight,
                    MinWidth = dshape.MinWidth,
                    VisualOffset = new double[2] { dvt.X, dvt.Y },
                    Width = dshape.ActualWidth,
                    ShapeType = dshape.ToolTip.ToString(),
                    Stroke = dshape.Stroke
                });
            }

            canvasDto = new CanvasDto
            {
                MyCanvasStore = MyCanvasStore
            };

            SerializeToJson(); // Binary();

        }



        //public static UIElement GetByUid(DependencyObject rootElement, string uid)
        //{
        //    foreach (UIElement element in LogicalTreeHelper.GetChildren(rootElement).OfType<UIElement>())
        //    {
        //        if (element.Uid == uid)
        //            return element;
        //        UIElement resultChildren = GetByUid(element, uid);
        //        if (resultChildren != null)
        //            return resultChildren;
        //    }
        //    return null;
        //}


        public string PathMaker(string tooltip, string stroke, string fill, string data)
        {
            return "<Path MinWidth=\"35\" MinHeight=\"20\" ToolTip=\"Rectangle\" " +
                "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" " +
                "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:dd=\"clr-namespace:DiagramDesigner;assembly=DiagramDesigner\">" +
                "<Path.Style><Style TargetType=\"Path\"><Style.BasedOn><Style TargetType=\"Path\"><Style.Resources><ResourceDictionary />" +
                "</Style.Resources>" +
                "<Setter Property=\"Shape.Fill\"><Setter.Value><SolidColorBrush>" + fill + "</SolidColorBrush></Setter.Value></Setter>" +
                "<Setter Property=\"Shape.Stroke\"><Setter.Value><SolidColorBrush>" + stroke + "</SolidColorBrush></Setter.Value></Setter>" +
                "<Setter Property=\"Shape.StrokeThickness\"><Setter.Value><s:Double>1.5</s:Double></Setter.Value></Setter>" +
                "<Setter Property=\"Shape.StrokeLineJoin\"><Setter.Value><x:Static Member=\"PenLineJoin.Round\" /></Setter.Value></Setter>" +
                "<Setter Property=\"Shape.Stretch\"><Setter.Value><x:Static Member=\"Stretch.Fill\" /></Setter.Value></Setter>" +
                "<Setter Property=\"FrameworkElement.Margin\"><Setter.Value><Thickness>1,1,1,1</Thickness></Setter.Value></Setter>" +
                "<Setter Property=\"FrameworkElement.MinWidth\"><Setter.Value><s:Double>35</s:Double></Setter.Value></Setter>" +
                "<Setter Property=\"FrameworkElement.MinHeight\"><Setter.Value><s:Double>20</s:Double></Setter.Value></Setter>" +
                "<Setter Property=\"UIElement.IsHitTestVisible\"><Setter.Value><s:Boolean>False</s:Boolean></Setter.Value></Setter>" +
                "<Setter Property=\"UIElement.SnapsToDevicePixels\"><Setter.Value><s:Boolean>True</s:Boolean></Setter.Value></Setter></Style></Style.BasedOn>" +
                "<Style.Resources>" +
                "<ResourceDictionary /></Style.Resources><Setter Property=\"Path.Data\"><Setter.Value>" +
                "<StreamGeometry>" + data + "</StreamGeometry></Setter.Value></Setter></Style>" +
                "</Path.Style><dd:DesignerItem.MoveThumbTemplate><ControlTemplate><Path><Path.Style>" +
                "<Style TargetType=\"Path\"><Style.BasedOn><Style TargetType=\"Path\"><Style.BasedOn>" +
                "<Style TargetType=\"Path\"><Style.Resources><ResourceDictionary /></Style.Resources>" +
                "<Setter Property=\"Shape.Fill\"><Setter.Value><SolidColorBrush>" + fill + "</SolidColorBrush></Setter.Value></Setter>" +
                "<Setter Property=\"Shape.Stroke\">" +
                "<Setter.Value><SolidColorBrush>" + stroke + "</SolidColorBrush></Setter.Value></Setter>" +
                "<Setter Property=\"Shape.StrokeThickness\">" +
                "<Setter.Value><s:Double>1.5</s:Double></Setter.Value></Setter>" +
                "<Setter Property=\"Shape.StrokeLineJoin\"><Setter.Value>" +
                "<x:Static Member=\"PenLineJoin.Round\" /></Setter.Value></Setter>" +
                "<Setter Property=\"Shape.Stretch\"><Setter.Value><x:Static Member=\"Stretch.Fill\" /></Setter.Value></Setter>" +
                "<Setter Property=\"FrameworkElement.Margin\">" +
                "<Setter.Value><Thickness>1,1,1,1</Thickness></Setter.Value></Setter><Setter Property=\"FrameworkElement.MinWidth\">" +
                "<Setter.Value><s:Double>35</s:Double></Setter.Value></Setter><Setter Property=\"FrameworkElement.MinHeight\">" +
                "<Setter.Value><s:Double>20</s:Double></Setter.Value></Setter><Setter Property=\"UIElement.IsHitTestVisible\">" +
                "<Setter.Value><s:Boolean>False</s:Boolean></Setter.Value></Setter><Setter Property=\"UIElement.SnapsToDevicePixels\">" +
                "<Setter.Value><s:Boolean>True</s:Boolean></Setter.Value></Setter></Style></Style.BasedOn>" +
                "<Style.Resources><ResourceDictionary /></Style.Resources><Setter Property=\"Path.Data\">" +
                "<Setter.Value><StreamGeometry>" + data + "</StreamGeometry></Setter.Value></Setter></Style></Style.BasedOn>" +
                "<Style.Resources>" +
                "<ResourceDictionary /></Style.Resources><Setter Property=\"UIElement.IsHitTestVisible\">" +
                "<Setter.Value><s:Boolean>True</s:Boolean></Setter.Value></Setter><Setter Property=\"Shape.Fill\">" +
                "<Setter.Value><SolidColorBrush>" + fill + "</SolidColorBrush></Setter.Value></Setter>" +
                "<Setter Property=\"Shape.Stroke\"><Setter.Value><SolidColorBrush>" + stroke + "</SolidColorBrush></Setter.Value></Setter></Style>" +
                "</Path.Style></Path></ControlTemplate></dd:DesignerItem.MoveThumbTemplate></Path>";
        }


        private void Click_load(object sender, RoutedEventArgs e)
        {
            DeSerializeFromJson();
            //var dstring = dJsonString;
            //var dpk = GetByUid(MyToolBox, "hex");

            MyCanvasStore = canvasDto.MyCanvasStore;
            var dChildren = MyToolBox.Children as UIElementCollection;
            var dEle = VisualTreeHelper.GetChildrenCount(MyToolBox);



            foreach (var item in MyCanvasStore)
            {

                var newItem = new DesignerItem();
                //var dxaml = "<Path Style= '{StaticResource Process}' MinWidth= '35'  MinHeight= '20' ToolTip='Rectangle' ><s:DesignerItem.MoveThumbTemplate><ControlTemplate>";

                //dxaml += "<Path Style='{StaticResource Process_DragThumb} /></ControlTemplate>";

                //dxaml += "</s:DesignerItem.MoveThumbTemplate></Path>";
                //
                FrameworkElement content = XamlReader.Load(XmlReader.Create(new StringReader(PathMaker(item.Value.ShapeType, item.Value.Stroke.ToString(), item.Value.FillString, item.Value.Data)))) as FrameworkElement;
                // var dElement = AutomationElement.GetElement();
                /// TODO: get the original content then load it back into the FrameworkElement at this point
                var dpath = new System.Windows.Shapes.Path
                {
                    Data = Geometry.Parse(item.Value.Data),
                    Stroke = item.Value.Stroke,
                    Height = item.Value.Height,
                    Width = item.Value.Width
                };


                //if (content != null)
                //{
                newItem = new DesignerItem
                {
                    Content = content,
                };

                // Point position = e.GetPosition(this);
                //if (content.MinHeight != 0 && content.MinWidth != 0)
                //{
                //    newItem.Width = content.MinWidth * 2; ;
                //    newItem.Height = content.MinHeight * 2;
                //}
                //else
                //{
                newItem.Width = item.Value.Width;
                newItem.Height = item.Value.Height;
                //}
                DesignerCanvas.SetLeft(newItem, Math.Max(0, item.Value.VisualOffset[0] - newItem.Width / 2));
                DesignerCanvas.SetTop(newItem, Math.Max(0, item.Value.VisualOffset[1] - newItem.Height / 2));
                MyDesignerCanvas.Children.Insert(item.Value.Index, newItem);
                //Children.Add(newItem);

                //this.DeselectAll();
                newItem.IsSelected = true;

                //}

            }


            //var dContent = dChildren as 

            // load the items back to the canvas

            ////load one item as a representative per unique item in the canvas
            //var newItem = new DesignerItem();
            //FrameworkElement content = XamlReader.Load(XmlReader.Create(new StringReader(xamlString))) as FrameworkElement;
            //if (content != null)
            //{
            //    CurrentSelectedToolBoxItem = null;
            //    var dKey = content.ToolTip.ToString();
            //    ///TODO: the need to reset the selection criteria combobox has to be implemented later
            //    if (NamesOfItemsAddedToCanvas.FindIndex(x => x == dKey) == -1)
            //    {
            //        var button = new Button
            //        {
            //            Height = 45,
            //            Width = 45,
            //            Margin = new Thickness(1),
            //            Background = new SolidColorBrush(Colors.Transparent),
            //            BorderThickness = new Thickness(0),
            //            BorderBrush = new SolidColorBrush(Colors.Transparent)
            //        };
            //        button.Click += Button_Click;
            //        button.Content = content;
            //        myItemsHolder.Children.Add(button);

            //        NamesOfItemsAddedToCanvas.Add(content.ToolTip.ToString());

            //        var dCurrentPosition = MyDesignerCanvas.Children.Count;
            //        MyCanvasDictionary.Add(dKey, dCurrentPosition.ToString());
            //    }
            //    else
            //    {
            //        var dCurrentPosition = MyDesignerCanvas.Children.Count;
            //        var oldvalue = MyCanvasDictionary[dKey];
            //        MyCanvasDictionary[dKey] += "," + dCurrentPosition.ToString();   //.Add(dKey, dCurrentPosition.ToString());
            //    }
            //}

            //var path = new Path();
            //path.Data = Geometry.Parse("M 100,200 C 100,25 400,350 400,175 H 280");

        }


        //Action emptyDelegate = delegate { };

        private void ComboColors2_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {

            if (MyDesignerCanvas != null)
            {
                //for (int i = 0; i < MyDesignerCanvas.Children.Count; i++)
                //{
                //    var ichild = MyDesignerCanvas.Children[i];
                //    var kchild = MyDesignerCanvas.SelectedItems;
                //    var dchild = ichild as DesignerItem;
                //    dchild.Background = new SolidColorBrush(e.NewValue.Value);
                //    dchild.UpdateLayout();
                //}

                foreach (DesignerItem item in MyDesignerCanvas.SelectedItems)
                {

                    item.Background = new SolidColorBrush(e.NewValue.Value);

                    var dshape = item.Content as Shape;
                    dshape.Fill = new SolidColorBrush(e.NewValue.Value);
                    item.BorderBrush = new SolidColorBrush(e.NewValue.Value);
                    // var mys = new Style();
                    //mys.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(e.NewValue.Value)));
                    //ele.Style = mys; // new Style{Setters =  } = new SolidColorBrush(e.NewValue.Value);
                }

                //MyDesignerCanvas.Dispatcher.Invoke(emptyDelegate, DispatcherPriority.Render);
                //foreach (var item in MyDesignerCanvas.Children)
                //{
                //    var ditem = item as DesignerItem;
                //var dIndex = MyDesignerCanvas.Children.IndexOf(ditem);
                //MyDesignerCanvas.Children.Remove(ditem);
                //    ditem.Background = new SolidColorBrush(e.NewValue.Value);
                //MyDesignerCanvas.Children.Insert(dIndex, ditem);
                //}

            }
            //var itemlist = MyDesignerCanvas.SelectedItems;
        }

        private void StrokeColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (MyDesignerCanvas != null)
            {
                foreach (DesignerItem item in MyDesignerCanvas.SelectedItems)
                {
                    var dshape = item.Content as Shape;
                    dshape.Stroke = new SolidColorBrush(e.NewValue.Value);
                }
            }
        }

        private void MyDesignerCanvas_PreviewDrop(object sender, DragEventArgs e)
        {
            string xamlString = e.Data.GetData("DESIGNER_ITEM") as string;
            if (!String.IsNullOrEmpty(xamlString))
            {
                var newItem = new DesignerItem();
                FrameworkElement content = XamlReader.Load(XmlReader.Create(new StringReader(xamlString))) as FrameworkElement;
                if (content != null)
                {
                    CurrentSelectedToolBoxItem = null;
                    var dKey = content.ToolTip.ToString();
                    ///TODO: the need to reset the selection criteria combobox has to be implemented later
                    if (NamesOfItemsAddedToCanvas.FindIndex(x => x == dKey) == -1)
                    {
                        var button = new Button
                        {
                            Height = 45,
                            Width = 45,
                            Margin = new Thickness(1),
                            Background = new SolidColorBrush(Colors.Transparent),
                            BorderThickness = new Thickness(0),
                            BorderBrush = new SolidColorBrush(Colors.Transparent)
                        };
                        button.Click += Button_Click;
                        button.Content = content;
                        myItemsHolder.Children.Add(button);

                        NamesOfItemsAddedToCanvas.Add(content.ToolTip.ToString());

                        var dCurrentPosition = MyDesignerCanvas.Children.Count;
                        MyCanvasDictionary.Add(dKey, dCurrentPosition.ToString());
                    }
                    else
                    {
                        var dCurrentPosition = MyDesignerCanvas.Children.Count;
                        var oldvalue = MyCanvasDictionary[dKey];
                        MyCanvasDictionary[dKey] += "," + dCurrentPosition.ToString();   //.Add(dKey, dCurrentPosition.ToString());
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var odbj = sender as Button;
            SelectionCriteriaComboBox.SelectedItem = null;
            CurrentSelectedToolBoxItem = odbj.Content as Shape; // odbj.Content as DesignerItem;
        }



        private void SelectionCriteriaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentSelectedToolBoxItem != null)
            {
                foreach (DesignerItem item in MyDesignerCanvas.SelectedItems.Where(t => t.IsSelected))
                {
                    item.IsSelected = false;
                }

                var dKey = CurrentSelectedToolBoxItem.ToolTip.ToString();
                var dArray = MyCanvasDictionary[dKey].Split(',');
                var dLen = dArray.Length;
                switch (SelectionCriteriaComboBox.SelectedValue)
                {
                    case "firstItem":
                        var dPosition = dArray[0];

                        var ditem = MyDesignerCanvas.Children[Convert.ToInt32(dPosition)] as DesignerItem;
                        ditem.IsSelected = true;
                        break;
                    case "lastItem":
                        dPosition = dArray[dLen - 1];

                        ditem = MyDesignerCanvas.Children[Convert.ToInt32(dPosition)] as DesignerItem;
                        ditem.IsSelected = true;
                        break;
                    case "middleItem":
                        dLen = dArray.Length;
                        var dCheck = dLen % 2 == 0;
                        if (dCheck)
                        {

                            ditem = MyDesignerCanvas.Children[Convert.ToInt32(dArray[dLen / 2])] as DesignerItem;
                            ditem.IsSelected = true;
                            ditem = MyDesignerCanvas.Children[Convert.ToInt32(dArray[(dLen / 2) - 1])] as DesignerItem;
                            ditem.IsSelected = true;
                        }
                        else
                        {
                            dPosition = dArray[dLen / 2];
                            ditem = MyDesignerCanvas.Children[Convert.ToInt32(dPosition)] as DesignerItem;
                            ditem.IsSelected = true;
                        }
                        break;
                    case "allItem":
                        for (int i = 0; i < dLen; i++)
                        {
                            ditem = MyDesignerCanvas.Children[Convert.ToInt32(dArray[i])] as DesignerItem;
                            ditem.IsSelected = true;
                        }
                        break;
                    default:

                        break;
                }
            }
            else
            {
                SelectionCriteriaComboBox.SelectedItem = null;
                MessageBox.Show("Please select a shape from the itemsbox first!");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MyDesignerCanvas.Children.Clear();
        }
    }
}
