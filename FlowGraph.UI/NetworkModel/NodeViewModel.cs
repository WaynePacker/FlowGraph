using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Utils;

namespace FlowGraph.UI.NetworkModel
{
    /// <summary>
    /// Defines a node in the view-model.
    /// Nodes are connected to other nodes through attached connectors (aka anchor/connection points).
    /// </summary>
    public sealed class NodeViewModel : AbstractModelBase
    {
        #region Private Data Members

        private const string defaultInputConnectorName = "Input";
        private const string defaultOutputConnectorName = "Output";

        private bool isRootNode = false;
        private bool isBranch = false;

        //Each node will have one parent and one child nodes
        // exceptions to this will be starting nodes which will only have a child node
        // and also nodes whos' output connectors are also Nodes like a branch( e.g an if then else statement i.e there are two paths)
        private ConnectorViewModel parentNodeConnection;
        private ConnectorViewModel childNodeConnection;

        /// <summary>
        /// The name of the node.
        /// </summary>
        private string name = string.Empty;

        /// <summary>
        /// The X coordinate for the position of the node.
        /// </summary>
        private double x = 0;

        /// <summary>
        /// The Y coordinate for the position of the node.
        /// </summary>
        private double y = 0;

        /// <summary>
        /// The Z index of the node.
        /// </summary>
        private int zIndex = 0;

        /// <summary>
        /// The size of the node.
        /// 
        /// Important Note: 
        ///     The size of a node in the UI is not determined by this property!!
        ///     Instead the size of a node in the UI is determined by the data-template for the Node class.
        ///     When the size is computed via the UI it is then pushed into the view-model
        ///     so that our application code has access to the size of a node.
        /// </summary>
        private Size size = Size.Empty;

        /// <summary>
        /// List of input connectors (connections points) attached to the node.
        /// </summary>
        private ImpObservableCollection<ConnectorViewModel> inputConnectors = null;

        /// <summary>
        /// List of output connectors (connections points) attached to the node.
        /// </summary>
        private ImpObservableCollection<ConnectorViewModel> outputConnectors = null;

        /// <summary>
        /// Set to 'true' when the node is selected.
        /// </summary>
        private bool isSelected = false;

        #endregion Private Data Members

        public NodeViewModel() {

            ParentNodeConnection = new ConnectorViewModel("Parent");
            ChildNodeConnection = new ConnectorViewModel("Child");

            ParentNodeConnection.ParentNode = this;
            ParentNodeConnection.Type = ConnectorType.Parent;

            ChildNodeConnection.ParentNode = this;
            ChildNodeConnection.Type = ConnectorType.Child;
        }

        public NodeViewModel(string name): this()
        {
            this.name = name;
        }

        public NodeViewModel(string name, Point nodeLocation)
            : this(name)
        {
            this.X = nodeLocation.X;
            this.Y = nodeLocation.Y;
        }

        /// <summary>
        /// The name of the node.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetAndNotify(ref name, value); }
        }

        /// <summary>
        /// The X coordinate for the position of the node.
        /// </summary>
        public double X
        {
            get { return x; }
            set { SetAndNotify(ref x, value); }
        }

        /// <summary>
        /// The Y coordinate for the position of the node.
        /// </summary>
        public double Y
        {
            get { return y; }
            set { SetAndNotify(ref y, value); }
        }

        /// <summary>
        /// The Z index of the node.
        /// </summary>
        public int ZIndex
        {
            get { return zIndex; }
            set { SetAndNotify(ref zIndex, value); }
        }

        /// <summary>
        /// The size of the node.
        /// 
        /// Important Note: 
        ///     The size of a node in the UI is not determined by this property!!
        ///     Instead the size of a node in the UI is determined by the data-template for the Node class.
        ///     When the size is computed via the UI it is then pushed into the view-model
        ///     so that our application code has access to the size of a node.
        /// </summary>
        public Size Size
        {
            get { return size; }
            set
            {
                if (SetAndNotify(ref size, value))
                {
                    SizeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Event raised when the size of the node is changed.
        /// The size will change when the UI has determined its size based on the contents
        /// of the nodes data-template.  It then pushes the size through to the view-model
        /// and this 'SizeChanged' event occurs.
        /// </summary>
        public event EventHandler<EventArgs> SizeChanged;

        /// <summary>
        /// List of input connectors (connections points) attached to the node.
        /// </summary>
        public ImpObservableCollection<ConnectorViewModel> InputConnectors
        {
            get
            {
                if (inputConnectors == null)
                {
                    inputConnectors = new ImpObservableCollection<ConnectorViewModel>();
                    inputConnectors.ItemsAdded += new EventHandler<CollectionItemsChangedEventArgs>(InputConnectors_ItemsAdded);
                    inputConnectors.ItemsRemoved += new EventHandler<CollectionItemsChangedEventArgs>(InputConnectors_ItemsRemoved);
                }

                return inputConnectors;
            }
        }

        /// <summary>
        /// List of output connectors (connections points) attached to the node.
        /// </summary>
        public ImpObservableCollection<ConnectorViewModel> OutputConnectors
        {
            get
            {
                if (outputConnectors == null)
                {
                    outputConnectors = new ImpObservableCollection<ConnectorViewModel>();
                    outputConnectors.ItemsAdded += new EventHandler<CollectionItemsChangedEventArgs>(OutputConnectors_ItemsAdded);
                    outputConnectors.ItemsRemoved += new EventHandler<CollectionItemsChangedEventArgs>(OutputConnectors_ItemsRemoved);
                }

                return outputConnectors;
            }
        }

        /// <summary>
        /// A helper property that retrieves a list (a new list each time) of all connections attached to the node. 
        /// </summary>
        public ICollection<ConnectionViewModel> AttachedConnections
        {
            get
            {
                List<ConnectionViewModel> attachedConnections = new List<ConnectionViewModel>();

                foreach (var connector in this.InputConnectors)
                {
                    attachedConnections.AddRange(connector.AttachedConnections);
                }

                foreach (var connector in this.OutputConnectors)
                {
                    attachedConnections.AddRange(connector.AttachedConnections);
                }

                if (!isRootNode)
                    attachedConnections.AddRange(ParentNodeConnection.AttachedConnections);

                if(!isBranch)
                    attachedConnections.AddRange(ChildNodeConnection.AttachedConnections);

                return attachedConnections;
            }
        }

        /// <summary>
        /// Set to 'true' when the node is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetAndNotify(ref isSelected, value); }
        }

    

        public ConnectorViewModel ParentNodeConnection
        {
            get { return parentNodeConnection; }
            set { SetAndNotify(ref parentNodeConnection, value); }
        }


        public ConnectorViewModel ChildNodeConnection
        {
            get { return childNodeConnection; }
            set { SetAndNotify(ref childNodeConnection, value); }
        }


        #region Private Methods

        /// <summary>
        /// Event raised when connectors are added to the node.
        /// </summary>
        private void InputConnectors_ItemsAdded(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectorViewModel connector in e.Items)
            {
                connector.ParentNode = this;
                connector.Type = ConnectorType.Input;
            }
        }

        /// <summary>
        /// Event raised when connectors are removed from the node.
        /// </summary>
        private void InputConnectors_ItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectorViewModel connector in e.Items)
            {
                connector.ParentNode = null;
                connector.Type = ConnectorType.Undefined;
            }
        }

        /// <summary>
        /// Event raised when connectors are added to the node.
        /// </summary>
        private void OutputConnectors_ItemsAdded(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectorViewModel connector in e.Items)
            {
                connector.ParentNode = this;
                connector.Type = ConnectorType.Output;
            }
        }

        /// <summary>
        /// Event raised when connectors are removed from the node.
        /// </summary>
        private void OutputConnectors_ItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectorViewModel connector in e.Items)
            {
                connector.ParentNode = null;
                connector.Type = ConnectorType.Undefined;
            }
        }

        #endregion Private Methods

        #region Public Methods

        public void AddInputConnector()
        {
            var defaultNameWithNumber = defaultInputConnectorName + (this.InputConnectors.Count(c => c.Name == defaultInputConnectorName) + 1);
            this.InputConnectors.Add(new ConnectorViewModel(defaultNameWithNumber));
        }

        public void AddOutputConnector()
        {
            var defaultNameWithNumber = defaultOutputConnectorName + (this.OutputConnectors.Count(c => c.Name == defaultOutputConnectorName) + 1);
            this.OutputConnectors.Add(new ConnectorViewModel(defaultNameWithNumber));
        }

        #endregion Public Methods
    }
}
