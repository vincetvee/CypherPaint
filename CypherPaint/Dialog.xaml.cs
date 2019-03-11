using System.Windows;

namespace DiagramDesigner
{
    /// <summary>
    /// Interaction logic for ProjectName.xaml
    /// </summary>
    public partial class ProjectName : Window
    {
        public ProjectName()
        {
            InitializeComponent();
        }

      

        private void Apply(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
           this. Close();
        }

      
    }
}
