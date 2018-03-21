using System.Windows;

namespace FlowGraph.UI.NetworkModel.NodeFactory
{
    /// <summary>
    /// A Factory class to help construct different types of nodes
    /// </summary>
    public static class NodeViewModelFactory
    {
        private static NodeViewModel Create()
        {
            return new NodeViewModel();
        }

        private static NodeViewModel Create(string name, Point nodeLocation)
        {
            var node = Create();
            node.Name = name;
            node.X = nodeLocation.X;
            node.Y = nodeLocation.Y;

            return node;
        }

        public static NodeViewModel Create(string name, Point nodeLocation, bool hasLeftPath, bool hasRightPath)
        {
            var node = Create(name, nodeLocation);

            if (hasLeftPath)
            {
                node.LeftNodeConnection = new ConnectorViewModel(NodeViewModel.DefaultLeftNodeConnectorName)
                {
                    ParentNode = node,
                    Type = ConnectorType.Path
                };
            }

            if (hasRightPath)
            {
                node.RightNodeConnection = new ConnectorViewModel(NodeViewModel.DefaultRightNodeConnectorName)
                {
                    ParentNode = node,
                    Type = ConnectorType.Path
                };
            }

            return node;
        }

        public static void AttachInputAndOutputConnectors(NodeViewModel node, int inputCount, int outputCount)
        {
            for (var i = 0; i < inputCount; i++)
            {
                node.AddInputConnector();
            }

            for (var i = 0; i < outputCount; i++)
            {
                node.AddOutputConnector();
            }
        }
    }
}
