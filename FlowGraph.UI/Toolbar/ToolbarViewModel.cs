using FlowGraph.UI.Toolbar.ToolbarResources.Help;
using System.Windows.Input;
using Utils;

namespace FlowGraph.UI.Toolbar
{
    public class ToolbarViewModel
    {
        public ToolbarViewModel()
        {
            HelpCommand = new DelegateCommand(HelpCommandExecute);
        }

        public ICommand HelpCommand { get; }

        private void HelpCommandExecute()
        {
            var window = new HelpTextWindow();
            window.Show();
        }
    }
}
