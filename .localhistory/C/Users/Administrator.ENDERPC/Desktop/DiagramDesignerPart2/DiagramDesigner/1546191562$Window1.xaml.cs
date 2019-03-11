
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
using Path = System.Windows.Shapes.Path;

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

        public double width { get; set; }

        public string Data { get; set; }

        public string VisualOffset { get; set; }

        //public DesignerItem DesignerItem { get; set; }

        public Brush Stroke { get; set; }

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
            for (int i = 0; i < dlen; i++)
            {
                var dItem = MyDesignerCanvas.Children[i] as DesignerItem;
                ///TODO: also get the position on canvas before saving the stuff
                var dshape = dItem.Content as Shape;
                var dpath = dshape as Path;
                MyCanvasStore = new Dictionary<int, CanvasItemDetail>
                {
                    {
                        i,
                        new CanvasItemDetail
                        {
                            //DesignerItem = dItem,
                            Fill = dshape.Fill,
                            //Shape = dshape,
                            Index = i,
                            Data = dpath.Data.ToString(),
                            Height = dshape.ActualHeight,
                            width = dshape.ActualWidth,
                            ShapeType = dshape.ToolTip.ToString(),
                            Stroke = dshape.Stroke
                        }
                    }
                };
            }

            canvasDto = new CanvasDto
            {
                MyCanvasStore = MyCanvasStore
            };

            SerializeToJson(); // Binary();

        }
        private void Click_load(object sender, RoutedEventArgs e)
        {
            //DeSerializeFromJson();
            //var dstring = dJsonString;

            MyCanvasStore = canvasDto.MyCanvasStore;

            foreach (var item in MyCanvasStore)
            {

                DesignerItem newItem = null;
                FrameworkElement content = XamlReader.Load(XmlReader.Create(new StringReader(xamlString))) as FrameworkElement;

                if (content != null)
                {
                    newItem = new DesignerItem
                    {
                        Content = content
                    };

                    Point position = e.GetPosition(this);
                    if (content.MinHeight != 0 && content.MinWidth != 0)
                    {
                        newItem.Width = content.MinWidth * 2; ;
                        newItem.Height = content.MinHeight * 2;
                    }
                    else
                    {
                        newItem.Width = 65;
                        newItem.Height = 65;
                    }
                    My.SetLeft(newItem, Math.Max(0, position.X - newItem.Width / 2));
                    DesignerCanvas.SetTop(newItem, Math.Max(0, position.Y - newItem.Height / 2));
                    MyDesignerCanvas.Children.Insert()
                    //Children.Add(newItem);

                    //this.DeselectAll();
                    newItem.IsSelected = true;

                }

            }

            var dChildren = MyToolBox.Children as UIElementCollection;

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
