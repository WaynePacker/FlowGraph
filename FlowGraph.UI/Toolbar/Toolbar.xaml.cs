using System.Windows.Controls;

namespace FlowGraph.UI.Toolbar
{ 
    public partial class Toolbar : UserControl
    {
        public Toolbar()
        {
            Model = new ToolbarViewModel();
            DataContext = Model;

            InitializeComponent();
        }

        public ToolbarViewModel Model { get; }
    }
}
