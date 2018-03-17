using FlowGraph.UI.NetworkModel.Base;
using System;
using System.Linq;
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
        private ImpObservableCollection<ANodeViewModel> nodes = null;

        /// <summary>
        /// The collection of connections in the network.
        /// </summary>
        private ImpObservableCollection<AConnectionViewModel> connections = null;

        #endregion Internal Data Members

        /// <summary>
        /// The collection of nodes in the network.
        /// </summary>
        public ImpObservableCollection<ANodeViewModel> Nodes
        {
            get
            {
                if (nodes == null)
                {
                    nodes = new ImpObservableCollection<ANodeViewModel>();
                }

                return nodes;
            }
        }

        /// <summary>
        /// The collection of connections in the network.
        /// </summary>
        public ImpObservableCollection<AConnectionViewModel> Connections
        {
            get
            {
                if (connections == null)
                {
                    connections = new ImpObservableCollection<AConnectionViewModel>();
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
            foreach (var connection in e.Items.Cast<AConnectionViewModel>())
            {
                connection.SourceConnector = null;
                connection.DestConnector = null;
            }
        }

        #endregion Private Methods
    }
}
