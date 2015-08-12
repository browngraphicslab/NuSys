﻿using System.Collections.Generic;

namespace NuSysApp
{
    public class Node
    {
        public Node(int id)
        {
            StartLines = new List<Link>();
            EndLines = new List<Link>();
            ID = id;
        }
        
        public void ConnectNodes(Node node)
        {
            if (!this.ConnectedNodes.Contains(node))
            {
                var connector = new Link(this, node);
            }
        }

        public void Delete()
        {
            foreach(var connect in this.StartLines) 
            {
                connect.DeleteFromOut();
            }
            foreach(var connect in this.EndLines)
            {
                connect.DeleteFromIn();
            }
        }

        public Content Content { set; get; }

        public List<Link> StartLines { get; }

        public List<Link> EndLines { get; }

        public List<Node> ConnectedNodes { get; }

        public int ID { get; }
    }
}
