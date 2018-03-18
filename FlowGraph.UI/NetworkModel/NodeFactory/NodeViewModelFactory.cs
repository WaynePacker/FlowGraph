using FlowGraph.UI.Interfaces;
using FlowGraph.UI.NetworkModel.Base;
using System;
using System.Windows;

namespace FlowGraph.UI.NetworkModel.NodeFactory
{
    /// <summary>
    /// A Factory class to help construct different types of nodes
    /// </summary>
    public static class NodeViewModelFactory
    {
        public static ANodeViewModel Create<T>()
           where T : INodeViewModel
        {
            return Activator.CreateInstance<T>() as ANodeViewModel;
        }

        public static ANodeViewModel Create<T>(string name, Point nodeLocation)
           where T : INodeViewModel
        {
            var nodeViewModel = Create<T>();
            nodeViewModel.Name = name;
            nodeViewModel.X = nodeLocation.X;
            nodeViewModel.Y = nodeLocation.Y;

            return nodeViewModel;
        }

        public static ANodeViewModel Create<T>(string name, Point nodeLocation, int inputCount, int outputCount)
           where T : INodeViewModel
        {
            var nodeViewModel = Create<T>(name, nodeLocation);

            for(var i = 0; i < inputCount; i++)
            {
                nodeViewModel.AddInputConnector();
            }

            for (var i = 0; i < outputCount; i++)
            {
                nodeViewModel.AddOutputConnector();
            }

            return nodeViewModel;
        }

    }
}
