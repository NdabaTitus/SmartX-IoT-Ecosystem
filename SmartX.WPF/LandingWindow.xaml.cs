using System.Windows;
using System.Windows.Input;

namespace SmartX.WPF
{
    
    /// Landing page that presents the three architectural pillars
    /// of the Smart-X ecosystem.
 
    public partial class LandingWindow : Window
    {
        public LandingWindow()
        {
            InitializeComponent();
        }

    
        /// Opens the main dashboard when Pillar 1 is clicked.
    
        private void Pillar1_Click(object sender, MouseButtonEventArgs e)
        {
            var dashboard = new MainWindow();
            dashboard.Show();
            this.Close();
        }
    }
}