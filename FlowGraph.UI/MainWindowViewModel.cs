using FlowGraph.UI.NetworkModel;
using FlowGraph.UI.NetworkModel.NodeFactory;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Utils;

namespace FlowGraph.UI
{
    public class MainWindowViewModel : AbstractModelBase
    {
        #region Internal Data Members

        public NetworkViewModel network = null;
        private double contentScale = 1;
        private double contentOffsetX = 0;
        private double contentOffsetY = 0;
        private double contentWidth = 1600;
        private double contentHeight = 900;
        private double contentViewportWidth = 0;
        private double contentViewportHeight = 0;

        #endregion Internal Data Members

        public MainWindowViewModel()
        {
            // Add some test data to the view-model.
            PopulateWithTestData();
        }

        /// <summary>
        /// This is the network that is displayed in the window.
        /// It is the main part of the view-model.
        /// </summary>
        public NetworkViewModel Network
        {
            get { return network; }
            set { SetAndNotify(ref network, value); }
        }

        ///
        /// The current scale at which the content is being viewed.
        /// 
        public double ContentScale
        {
            get { return contentScale; }
            set { SetAndNotify(ref contentScale, value); }
        }

        ///
        /// The X coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        public double ContentOffsetX
        {
            get { return contentOffsetX; }
            set { SetAndNotify(ref contentOffsetX, value); }
        }

        ///
        /// The Y coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        public double ContentOffsetY
        {
            get { return contentOffsetY; }
            set { SetAndNotify(ref contentOffsetY, value); }
        }

        ///
        /// The width of the content (in content coordinates).
        /// 
        public double ContentWidth
        {
            get { return contentWidth; }
            set { SetAndNotify(ref contentWidth, value); }
        }

        ///
        /// The heigth of the content (in content coordinates).
        /// 
        public double ContentHeight
        {
            get { return contentHeight; }
            set { SetAndNotify(ref contentHeight, value); }
        }

        ///
        /// The width of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
        /// view-model so that the value can be shared with the overview window.
        /// 
        public double ContentViewportWidth
        {
            get { return contentViewportWidth; }
            set { SetAndNotify(ref contentViewportWidth, value); }
        }

        ///
        /// The heigth of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
        /// view-model so that the value can be shared with the overview window.
        /// 
        public double ContentViewportHeight
        {
            get { return contentViewportHeight; }
            set { SetAndNotify(ref contentViewportHeight, value); }
        }

        /// <summary>
        /// Called when the user has started to drag out a connector, thus creating a new connection.
        /// </summary>
        public AConnectionViewModel ConnectionDragStarted(ConnectorViewModel draggedOutConnector, Point curDragPoint)
        {
            //
            // Create a new connection to add to the view-model.
            //
            AConnectionViewModel connection;

            if (draggedOutConnector.Type == ConnectorType.Path)
                connection = new PathConnectionViewModel();
            else
                connection = new StandardConnectionViewModel();

            switch (draggedOutConnector.Type)
            {
                case ConnectorType.Undefined:
                    break;
                case ConnectorType.Input:
                    {
                        //
                        // The user is dragging out a destination connector (an input) and will connect it to a source connector (an output).
                        //
                        connection.DestConnector = draggedOutConnector;
                        connection.SourceConnectorHotspot = curDragPoint;
                    }
                    break;
                case ConnectorType.Output:
                    {
                        //
                        // The user is dragging out a source connector (an output) and will connect it to a destination connector (an input).
                        //
                        connection.SourceConnector = draggedOutConnector;
                        connection.DestConnectorHotspot = curDragPoint;
                    }
                    break;
                case ConnectorType.Path:
                    {
                        //
                        // The user is dragging out a path connector.
                        //
                        if (draggedOutConnector.Name == NodeViewModel.DefaultLeftNodeConnectorName)
                        {
                            connection.DestConnector = draggedOutConnector;
                            connection.SourceConnectorHotspot = curDragPoint;
                        }
                        else
                        {
                            connection.SourceConnector = draggedOutConnector;
                            connection.DestConnectorHotspot = curDragPoint;
                        }

                    }
                    break;
                default:
                    break;
            }

            //
            // Add the new connection to the view-model.
            //
            this.Network.Connections.Add(connection);

            return connection;
        }

        /// <summary>
        /// Called to query the application for feedback while the user is dragging the connection.
        /// Only allow connections from output connector to input connector (ie each
        /// connector must have a different type or different name).
        /// Also only allocation from one node to another, never one node back to the same node.
        /// </summary>
        public void QueryConnnectionFeedback(ConnectorViewModel draggedOutConnector, ConnectorViewModel draggedOverConnector, out object feedbackIndicator, out bool connectionOk)
        {
            connectionOk = IsValidPathConnection(draggedOutConnector, draggedOverConnector) ||
                            IsValidInputOutputConnection(draggedOutConnector, draggedOverConnector);

            if (connectionOk)
                feedbackIndicator = new ConnectionOkIndicator();
            else
                feedbackIndicator = new ConnectionBadIndicator();
        }

        /// <summary>
        /// Called as the user continues to drag the connection.
        /// </summary>
        public void ConnectionDragging(Point curDragPoint, AConnectionViewModel connection)
        {
            if (connection.DestConnector == null)
                connection.DestConnectorHotspot = curDragPoint;
            else
                connection.SourceConnectorHotspot = curDragPoint;
        }

        /// <summary>
        /// Called when the user has finished dragging out the new connection.
        /// Only one left- right connection is allowed
        /// invalid connections are removed
        /// </summary>
        /// 
        public void ConnectionDragCompleted(AConnectionViewModel newConnection, ConnectorViewModel connectorDraggedOut, ConnectorViewModel connectorDraggedOver)
        {
            if (connectorDraggedOver == null)
            {
                //
                // The connection was unsuccessful.
                // Maybe the user dragged it out and dropped it in empty space.
                //
                Network.Connections.Remove(newConnection);
                return;
            }


            var validPathConnection = IsValidPathConnection(connectorDraggedOut, connectorDraggedOver);
            bool connectionOk = validPathConnection ||
                                IsValidInputOutputConnection(connectorDraggedOut, connectorDraggedOver);

            if (!connectionOk)
            {
                Network.Connections.Remove(newConnection);
                return;
            }

            if (validPathConnection)
            {
                var isConnectingFromLeftToRight = connectorDraggedOut.Name == NodeViewModel.DefaultRightNodeConnectorName;

                var sourceConnector = isConnectingFromLeftToRight ? connectorDraggedOut : connectorDraggedOver;
                var destConnector = isConnectingFromLeftToRight ? connectorDraggedOver : connectorDraggedOut;

                var sourceRightConnection = sourceConnector.ParentNode.RightNodeConnection;
                var destinationLeftConnection = destConnector.ParentNode.LeftNodeConnection;


                if (sourceRightConnection != null && sourceRightConnection.IsConnected)
                {
                    var existingConnection = sourceRightConnection.AttachedConnections.Single(c => c.DestConnector != null);
                    Network.Connections.Remove(existingConnection);
                }

                if (destinationLeftConnection != null && destinationLeftConnection.IsConnected)
                {
                    var existingConnection = destinationLeftConnection.AttachedConnections.Single(c => c.SourceConnector != null);
                    Network.Connections.Remove(existingConnection);
                }
            }
            else
            {
                //
                // Remove any existing connection between the same two connectors.
                //
                var existingConnection = FindConnection(connectorDraggedOut, connectorDraggedOver);
                if (existingConnection != null)
                {
                    Network.Connections.Remove(existingConnection);
                }
            }

            //
            // Finalize the connection by attaching it to the connector
            // that the user dragged the mouse over.
            //
            if (newConnection.DestConnector == null)
                newConnection.DestConnector = connectorDraggedOver;
            else
                newConnection.SourceConnector = connectorDraggedOver;
        }

        private bool IsValidPathConnection(ConnectorViewModel connector1, ConnectorViewModel connector2)
        {
            if (connector1 == connector2)
                return false;

            var isNotSameName = connector1.Name != connector2.Name;
            var isValidPath = connector1.Type == ConnectorType.Path && connector2.Type == ConnectorType.Path;

            return isNotSameName && isValidPath;
        }

        private bool IsValidInputOutputConnection(ConnectorViewModel connector1, ConnectorViewModel connector2)
        {
            if (connector1 == connector2)
                return false;

            var validInputToOutput = connector1.Type == ConnectorType.Output && connector2.Type == ConnectorType.Input;
            var validOutputToInput = connector1.Type == ConnectorType.Input && connector2.Type == ConnectorType.Output;

            return validInputToOutput || validOutputToInput;
        }

        /// <summary>
        /// Retrieve a connection between the two connectors.
        /// Returns null if there is no connection between the connectors.
        /// </summary>
        public AConnectionViewModel FindConnection(ConnectorViewModel connector1, ConnectorViewModel connector2)
        {
            Trace.Assert(connector1.Type != connector2.Type);

            //
            // Figure out which one is the source connector and which one is the
            // destination connector based on their connector types.
            //
            var sourceConnector = connector1.Type == ConnectorType.Output ? connector1 : connector2;
            var destConnector = connector1.Type == ConnectorType.Output ? connector2 : connector1;

            //
            // Now we can just iterate attached connections of the source
            // and see if it each one is attached to the destination connector.
            //

            foreach (var connection in sourceConnector.AttachedConnections)
            {
                if (connection.DestConnector == destConnector)
                {
                    //
                    // Found a connection that is outgoing from the source connector
                    // and incoming to the destination connector.
                    //
                    return connection;
                }
            }

            return null;
        }

        /// <summary>
        /// Delete the currently selected nodes from the view-model.
        /// </summary>
        public void DeleteSelectedNodes()
        {
            // Take a copy of the selected nodes list so we can delete nodes while iterating.
            var nodesCopy = Network.Nodes.ToArray();
            foreach (var node in nodesCopy)
            {
                if (node.IsSelected)
                {
                    DeleteNode(node);
                }
            }
        }

        /// <summary>
        /// Delete the node from the view-model.
        /// Also deletes any connections to or from the node.
        /// </summary>
        public void DeleteNode(NodeViewModel node)
        {
            Network.Connections.RemoveRange(node.AttachedConnections);
            Network.Nodes.Remove(node);
        }

        /// <summary>
        /// Creates the node according to the type using the factory method
        /// and add it to the view-model.
        /// </summary>
        /// <param name="name">Name of the the node</param>
        /// <param name="nodeLocation"></param>
        /// <param name="centerNode">Should the node be centered</param>
        /// <returns></returns>
        public NodeViewModel CreateNode(string name, Point nodeLocation, bool centerNode, bool hasLeftPath, bool hasRightPath)
        {
            var node = NodeViewModelFactory.Create(name, nodeLocation, hasLeftPath, hasRightPath);

            NodeViewModelFactory.AttachInputAndOutputConnectors(node, 2, 2);//This is preliminary to test things out

            if (centerNode)
                CenterNode(node);

            Network.Nodes.Add(node);

            return node;
        }

        /// <summary>
        /// Utility method to delete a connection from the view-model.
        /// </summary>
        public void DeleteConnection(AConnectionViewModel connection)
        {
            this.Network.Connections.Remove(connection);
        }


        #region Private Methods



        /// <summary>
        /// We want to center the node.
        /// For this to happen we need to wait until the UI has determined the 
        /// size based on the node's data-template.
        /// 
        /// So we define an anonymous method to handle the SizeChanged event for a node.
        /// 
        /// Note: If you don't declare sizeChangedEventHandler before initializing it you will get
        ///       an error when you try and unsubscribe the event from within the event handler.
        /// 
        /// </summary>
        /// <param name="node"></param>
        private void CenterNode(NodeViewModel node)
        {
            EventHandler<EventArgs> sizeChangedEventHandler = null;
            sizeChangedEventHandler =
                delegate (object sender, EventArgs e)
                {
                    // This event handler will be called after the size of the node has been determined.
                    // So we can now use the size of the node to modify its position.
                    node.X -= node.Size.Width / 2;
                    node.Y -= node.Size.Height / 2;

                    // Unhook the event, after the initial centering of the node
                    // we don't need to be notified again of any size changes.
                    node.SizeChanged -= sizeChangedEventHandler;
                };

            node.SizeChanged += sizeChangedEventHandler;
        }

        /// <summary>
        /// A function to conveniently populate the view-model with test data.
        /// </summary>
        private void PopulateWithTestData()
        {
            //
            // Create a network, the root of the view-model.
            //
            this.Network = new NetworkViewModel();

            //
            // Create some nodes and add them to the view-model.
            //
            var node1 = CreateNode("Test Node1", new Point(100, 60), false, true, true);
            var node2 = CreateNode("Test Node2", new Point(350, 80), false, true, true);

            //
            // Create a connection between the nodes.
            //
            AConnectionViewModel connection = new StandardConnectionViewModel
            {
                SourceConnector = node1.OutputConnectors[0],
                DestConnector = node2.InputConnectors[0]
            };

            AConnectionViewModel pathconnection = new PathConnectionViewModel
            {
                SourceConnector = node1.RightNodeConnection,
                DestConnector = node2.LeftNodeConnection
            };

            //
            // Add the connection to the view-model.
            //
            this.Network.Connections.Add(connection);
            this.Network.Connections.Add(pathconnection);
        }

        #endregion Private Methods
    }
}
