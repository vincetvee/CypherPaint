
using DbAcceess;
using DiagramDesigner;
using Microsoft.Win32;
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

namespace CypherPaint
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

   

    public partial class Window1 : Window
    {
        public bool SampleBoolInParent { get; set; }

        public string[] NamesOfItemsOnCanvas;
        public List<string> NamesOfItemsAddedToCanvas;

        public CanvasWork MyCurrentCanvasWork;

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

            InitializeComponent();
            DataContext = this;
            // creating a list object
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

            MyCurrentCanvasWork = null;

        }

        //private void Rotate(double angle)
        //{
        //    foreach (DesignerItem item in MyDesignerCanvas.SelectedItems)
        //    {
        //        FrameworkElement element = item.Content as FrameworkElement;
        //        if (element != null)
        //        {
        //            RotateTransform rotateTransform = element.LayoutTransform as RotateTransform;
        //            if (rotateTransform == null)
        //            {
        //                rotateTransform = new RotateTransform();
        //                element.LayoutTransform = rotateTransform;
        //            }

        //            rotateTransform.Angle = (rotateTransform.Angle + angle) % 360;
        //            Canvas.SetLeft(item, Canvas.GetLeft(item) - (item.Height - item.Width) / 2);
        //            Canvas.SetTop(item, Canvas.GetTop(item) - (item.Width - item.Height) / 2);
        //            double width = item.Width;
        //            item.Width = item.Height;
        //            item.Height = width;
        //        }
        //    }
        //}

        //public void SerializeToBinary()
        //{

        //    var bf = new BinaryFormatter();

        //    FileStream fsout = new FileStream("dummy.binary", FileMode.Create, FileAccess.Write, FileShare.None);
        //    try
        //    {
        //        using (fsout)
        //        {
        //            bf.Serialize(fsout, canvasDto);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public void DeSerializeFromBinary()
        //{
        //    var bf = new BinaryFormatter();
        //    var fsin = new FileStream("dummy.binary", FileMode.Open, FileAccess.Read, FileShare.None);

        //    try
        //    {
        //        using (fsin)
        //        {
        //            canvasDto = (CanvasDto)bf.Deserialize(fsin);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public void SerializeToJson()
        {
            string json = JsonConvert.SerializeObject(canvasDto);
            dJsonString = json;

            var sFileDialog = new SaveFileDialog();
            if (sFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(sFileDialog.FileName, json);
            }
            else
            {
                // save to file
                File.WriteAllText("dummy.text", json);
            }
        }

        public void DeSerializeFromJson()
        {
            canvasDto = JsonConvert.DeserializeObject<CanvasDto>(dJsonString);
        }

        public void SaveToMSSQLDB(string projectname)
        {

            MyCurrentCanvasWork = null;
            using (var db = new AppDbContext())
            {
                // TO check if the Name of project is already used before
               // var mySavedCanvasWork = db.CanvasWorks.FirstOrDefault(t => t.Name == projectname);

                    MyCurrentCanvasWork = new CanvasWork()
                    {
                        Name = projectname,
                        DateStored = DateTime.Now                       
                    };

                    var dlen = MyDesignerCanvas.Children.Count;
                    MyCanvasStore = new Dictionary<int, CanvasItemDetail>();

                    for (int i = 0; i < dlen; i++)
                    {

                        var dItem = MyDesignerCanvas.Children[i] as DesignerItem;
                        var dvt = VisualTreeHelper.GetOffset(dItem);
                        ///TODO: also get the position on canvas before saving the stuff
                        var dshape = dItem.Content as Shape;
                        var dpath = dshape as System.Windows.Shapes.Path;
                        var dfill = dpath.Fill.ToString().Split('.').Length == 1 ? dpath.Fill.ToString() : new SolidColorBrush(Colors.Transparent).ToString();
                        MyCurrentCanvasWork.CanvasItems.Add(
                            new CanvasItemDetail
                            {
                                FillString = dpath.Fill.ToString(),
                                Fill = dfill,
                                Index = i,
                                Data = dpath.Data.ToString(),
                                Height = dshape.ActualHeight,
                                MinHeight = dshape.MinHeight,
                                MinWidth = dshape.MinWidth,
                                //VisualOffset = new double[2] { dvt.X, dvt.Y },
                                XOffSet = dvt.X,
                                YOffSet = dvt.Y,
                                Width = dshape.ActualWidth,
                                ShapeType = dshape.ToolTip.ToString(),
                                Stroke = dshape.Stroke.ToString()
                            });               
                    }

                db.CanvasWorks.Add(MyCurrentCanvasWork);

                    db.SaveChanges();
                    MessageBox.Show("Project saved successfully!");
            }
        }

        public void ReadFromMSSQLDB(string projectname)
        {
          
            using (var db = new AppDbContext())
            {
                MyCanvasDictionary.Clear();
                var mySavedCanvasWork = db.CanvasWorks.FirstOrDefault(t => t.Name == projectname);
                if (mySavedCanvasWork != null)
                {
                    MyCurrentCanvasWork = mySavedCanvasWork;

                    MyCanvasStore = canvasDto.MyCanvasStore;
                    var dChildren = MyToolBox.Children as UIElementCollection;
                    var dEle = VisualTreeHelper.GetChildrenCount(MyToolBox);

                    foreach (var item in MyCurrentCanvasWork.CanvasItems)
                    {

                        var newItem = new DesignerItem();

                        FrameworkElement content = XamlReader.Load(XmlReader.Create(new StringReader(PathMaker(item.ShapeType, item.Stroke.ToString(), item.Fill.ToString(), item.Data)))) as FrameworkElement;

                        newItem = new DesignerItem
                        {
                            Content = content,
                        };

                        newItem.Width = item.Width;
                        newItem.Height = item.Height;

                        DesignerCanvas.SetLeft(newItem, Math.Max(0, item.XOffSet - newItem.Width / 2));
                        DesignerCanvas.SetTop(newItem, Math.Max(0, item.YOffSet - newItem.Height / 2));
                        MyDesignerCanvas.Children.Insert(item.Index, newItem);

                        newItem.IsSelected = true;

                        CurrentSelectedToolBoxItem = null;
                        var dKey = item.ShapeType;

                        var dshape = new System.Windows.Shapes.Path()
                        {
                            Fill = new SolidColorBrush(Colors.Transparent),
                            Stroke = new SolidColorBrush(Colors.White),
                            StrokeThickness = 1,
                            Height = 25,
                            Width = 25
                        };

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
                            var data = string.Empty;

                            switch (dKey)
                            {
                                case "Circle":
                                    data = "M 10,20 A 20,20 0 1 1 50,20 A 20,20 0 1 1 10,20";
                                    break;
                                case "Hexagon":
                                    data = "M0 8 L8 0 L18 0 L26 8 L18 15 L8 15 Z";
                                    break;
                                case "Pentagon":
                                    data = "M0 10 L15 0 L30 10 L30 10 L27 25 L3 25 Z";
                                    break;
                                case "Triangle":
                                    data = "M 0,10 5,0 10,10 Z ";
                                    break;
                                case "SlantLine":
                                    data = "M20 20 L0 0 Z";
                                    break;
                                case "Cross":
                                    data = "F1 M 35,19L 41,19L 41,35L 57,35L 57,41L 41,41L 41,57L 35,57L 35,41L 19,41L 19,35L 35,35L 35,19 Z ";
                                    break;
                                case "Rectangle":
                                    data = "M 0,0 H 60 V40 H 0 Z";
                                    break;

                                case "Diamond":
                                    data = "M0 8 L12 0 L12 0 L24 8 L12 16 Z";
                                    break;
                            }
                            FrameworkElement dBtncontent = XamlReader.Load(
                                XmlReader.Create(
                                    new StringReader(
                                    PathMaker(dKey, new SolidColorBrush(Colors.White).ToString(), new SolidColorBrush(Colors.Transparent).ToString(), data)
                                    )
                                    )) as FrameworkElement;
                            //dshape.Data = Geometry.Parse(data);
                            button.Click += Button_Click;
                            button.Content = dBtncontent;
                            myItemsHolder.Children.Insert(item.Index, button);

                            NamesOfItemsAddedToCanvas.Insert(item.Index, item.ShapeType);


                            MyCanvasDictionary.Add(dKey, item.Index.ToString());
                        }
                        else
                        {
                            var dCurrentPosition = MyDesignerCanvas.Children.Count;
                            var oldvalue = MyCanvasDictionary[dKey];
                            MyCanvasDictionary[dKey] += "," + item.Index.ToString();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Invalid project name entered ");
                        return;
                }
            }

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
                var dfill = dpath.Fill.ToString().Split('.').Length == 1 ? dpath.Fill.ToString() : new SolidColorBrush(Colors.Transparent).ToString();
                MyCanvasStore.Add(i, new CanvasItemDetail
                {
                    FillString = dpath.Fill.ToString(),
                    Fill = dfill,
                    Index = i,
                    Data = dpath.Data.ToString(),
                    Height = dshape.ActualHeight,
                    MinHeight = dshape.MinHeight,
                    MinWidth = dshape.MinWidth,
                    //VisualOffset = new double[2] { dvt.X, dvt.Y },
                    XOffSet = dvt.X,
                    YOffSet = dvt.Y,
                    Width = dshape.ActualWidth,
                    ShapeType = dshape.ToolTip.ToString(),
                    Stroke = dshape.Stroke.ToString()
                });
            }

            canvasDto = new CanvasDto
            {
                MyCanvasStore = MyCanvasStore
            };

            SerializeToJson();
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
            return "<Path MinWidth=\"35\" MinHeight=\"20\" ToolTip='" + tooltip + "' xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:s=\"clr-namespace:System;assembly=mscorlib\" " +
                "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:dd=\"clr-namespace:CypherPaint;assembly=DiagramDesigner\">" +
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
            

            var oPenFile = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };
            if (oPenFile.ShowDialog() == true)
            {
                canvasDto = JsonConvert.DeserializeObject<CanvasDto>(File.ReadAllText(oPenFile.FileName));
            }
            else
            {
                DeSerializeFromJson();

            }

            //var dpk = GetByUid(MyToolBox, "hex");

            MyCanvasStore = canvasDto.MyCanvasStore;
            var dChildren = MyToolBox.Children as UIElementCollection;
            var dEle = VisualTreeHelper.GetChildrenCount(MyToolBox);

            foreach (var item in MyCanvasStore)
            {

                var newItem = new DesignerItem();

                FrameworkElement content = XamlReader.Load(XmlReader.Create(new StringReader(PathMaker(item.Value.ShapeType, item.Value.Stroke.ToString(), item.Value.Fill.ToString(), item.Value.Data)))) as FrameworkElement;

                newItem = new DesignerItem
                {
                    Content = content,
                };

                newItem.Width = item.Value.Width;
                newItem.Height = item.Value.Height;

                DesignerCanvas.SetLeft(newItem, Math.Max(0, item.Value.XOffSet - newItem.Width / 2));
                DesignerCanvas.SetTop(newItem, Math.Max(0, item.Value.YOffSet - newItem.Height / 2));
                MyDesignerCanvas.Children.Insert(item.Value.Index, newItem);

                newItem.IsSelected = true;

                CurrentSelectedToolBoxItem = null;
                var dKey = item.Value.ShapeType;

                var dshape = new System.Windows.Shapes.Path()
                {
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Stroke = new SolidColorBrush(Colors.White),
                    StrokeThickness = 1,
                    Height = 25,
                    Width = 25
                };

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
                    var data = string.Empty;

                    switch (dKey)
                    {
                        case "Circle":
                            data = "M 10,20 A 20,20 0 1 1 50,20 A 20,20 0 1 1 10,20";
                            break;
                        case "Hexagon":
                            data = "M0 8 L8 0 L18 0 L26 8 L18 15 L8 15 Z";
                            break;
                        case "Pentagon":
                            data = "M0 10 L15 0 L30 10 L30 10 L27 25 L3 25 Z";
                            break;
                        case "Triangle":
                            data = "M 0,10 5,0 10,10 Z ";
                            break;
                        case "SlantLine":
                            data = "M20 20 L0 0 Z";
                            break;
                        case "Cross":
                            data = "F1 M 35,19L 41,19L 41,35L 57,35L 57,41L 41,41L 41,57L 35,57L 35,41L 19,41L 19,35L 35,35L 35,19 Z ";
                            break;
                        case "Rectangle":
                            data = "M 0,0 H 60 V40 H 0 Z";
                            break;

                        case "Diamond":
                            data = "M0 8 L12 0 L12 0 L24 8 L12 16 Z";
                            break;
                    }
                    FrameworkElement dBtncontent = XamlReader.Load(
                        XmlReader.Create(
                            new StringReader(
                            PathMaker(dKey, new SolidColorBrush(Colors.White).ToString(), new SolidColorBrush(Colors.Transparent).ToString(), data)
                            )
                            )) as FrameworkElement;
                    //dshape.Data = Geometry.Parse(data);
                    button.Click += Button_Click;
                    button.Content = dBtncontent;
                    myItemsHolder.Children.Insert(item.Value.Index, button);

                    NamesOfItemsAddedToCanvas.Insert(item.Value.Index, item.Value.ShapeType);


                    MyCanvasDictionary.Add(dKey, item.Value.Index.ToString());
                }
                else
                {
                    var dCurrentPosition = MyDesignerCanvas.Children.Count;
                    var oldvalue = MyCanvasDictionary[dKey];
                    MyCanvasDictionary[dKey] += "," + item.Value.Index.ToString();
                }
            }


        }


        private void ComboColors2_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (MyDesignerCanvas != null)
            {
                foreach (DesignerItem item in MyDesignerCanvas.SelectedItems)
                {
                    item.Background = new SolidColorBrush(e.NewValue.Value);
                    var dshape = item.Content as Shape;
                    dshape.Fill = new SolidColorBrush(e.NewValue.Value);
                   // item.BorderBrush = new SolidColorBrush(e.NewValue.Value);
                }
            }
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
            CurrentSelectedToolBoxItem = odbj.Content as Shape;
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
            myItemsHolder.Children.Clear();
            NamesOfItemsAddedToCanvas.Clear();
        }

        private void FromDbButon_Click(object sender, RoutedEventArgs e)
        {
            var myNewWindow = new ProjectName();

            var myDialogResult = myNewWindow.ShowDialog();

            if (string.IsNullOrEmpty(myNewWindow.Projectname.Text))
            {
                MessageBox.Show("Please enter a valid project name");
            }
            ReadFromMSSQLDB(myNewWindow.Projectname.Text);
        }

        private void ToDbButton_Click(object sender, RoutedEventArgs e)
        {
            var myNewWindow = new ProjectName();

          

            var myDialogResult = myNewWindow.ShowDialog();


            if (string.IsNullOrEmpty(myNewWindow.Projectname.Text))
            {
                MessageBox.Show("Please enter a valid project name");
            }
            SaveToMSSQLDB(myNewWindow.Projectname.Text);
        }


    }
}
