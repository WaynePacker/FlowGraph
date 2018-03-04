﻿using System;
using Utils;

namespace FlowGraph.UI.NetworkModel
{
    /// <summary>
    /// Defines a network of nodes and connections between the nodes.
    /// </summary>
    public sealed class NetworkViewModel
    {
        #region Internal Data Members

        /// <summary>
        /// The collection of nodes in the network.
        /// </summary>
        private ImpObservableCollection<NodeViewModel> nodes = null;

        /// <summary>
        /// The collection of connections in the network.
        /// </summary>
        private ImpObservableCollection<ConnectionViewModel> connections = null;

        #endregion Internal Data Members

        /// <summary>
        /// The collection of nodes in the network.
        /// </summary>
        public ImpObservableCollection<NodeViewModel> Nodes
        {
            get
            {
                if (nodes == null)
                {
                    nodes = new ImpObservableCollection<NodeViewModel>();
                }

                return nodes;
            }
        }

        /// <summary>
        /// The collection of connections in the network.
        /// </summary>
        public ImpObservableCollection<ConnectionViewModel> Connections
        {
            get
            {
                if (connections == null)
                {
                    connections = new ImpObservableCollection<ConnectionViewModel>();
                    connections.ItemsRemoved += new EventHandler<CollectionItemsChangedEventArgs>(Connections_ItemsRemoved);
                }

                return connections;
            }
        }

        #region Private Methods

        /// <summary>
        /// Event raised then Connections have been removed.
        /// </summary>
        private void Connections_ItemsRemoved(object sender, CollectionItemsChangedEventArgs e)
        {
            foreach (ConnectionViewModel connection in e.Items)
            {
                connection.SourceConnector = null;
                connection.DestConnector = null;
            }
        }

        #endregion Private Methods
    }
}
