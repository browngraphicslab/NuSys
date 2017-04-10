using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class ClusterUtil
    {
        private class Cluster : List<int>
        {
            private double _errorMargin;

            public double MaxDim
            {
                get; set;
            }

            public double MinDim
            {
                get; set;
            }

            private List<Rect> rects = new List<Rect>();

            private double _average = 0;
            private bool dirty = true;
            public double Average
            {
                get
                {
                    if (dirty)
                    {
                        _average = 0;
                        foreach (Rect r in rects)
                        {
                            double v = getDim(r);
                            _average += v;
                        }
                        _average /= Count;
                        dirty = false;
                    }
                    return _average;
                }
            }

            private double getDim(Rect r)
            {
                switch (_side)
                {
                    case Side.Top:
                        return r.Top;
                    case Side.Left:
                        return r.Left;
                    case Side.Right:
                        return r.Right;
                    case Side.Bottom:
                        return r.Bottom;
                    default:
                        return 0;
                }
            }

            private bool added = false;

            public enum Side
            {
                Top, Bottom, Left, Right
            }

            private Side _side;

            public Cluster(double errorMargin, Side side)
            {
                _errorMargin = errorMargin;
                _side = side;
            }

            public bool AddRect(int index, Rect r)
            {
                double v = getDim(r);
                if (!added || (v - MinDim <= 2 * _errorMargin && MaxDim - v <= 2 * _errorMargin))
                {
                    Add(index);
                    rects.Add(r);
                    if (added)
                    {
                        MinDim = v < MinDim ? v : MinDim;
                        MaxDim = v > MaxDim ? v : MinDim;
                    }
                    else
                    {
                        MinDim = v;
                        MaxDim = v;
                    }
                    added = true;
                    dirty = true;
                    return true;
                }
                return false;
            }
        }

        private List<Rect> FitSide(List<Rect> baseRects, double errorMargin, Cluster.Side side)
        {
            List<Cluster> clusters = new List<Cluster>();
            for (int i = 0; i < baseRects.Count; ++i)
            {
                Rect r = baseRects[i];
                bool added = false;
                for (int j = 0; j < clusters.Count; ++j)
                {
                    if (clusters[j].AddRect(i, r))
                    {
                        added = true;
                        break;
                    }
                }
                if (!added)
                {
                    Cluster c = new Cluster(errorMargin, side);
                    c.AddRect(i, r);
                    clusters.Add(c);
                }
            }

            List<Rect> newRects = new List<Rect>(baseRects);
            for (int i = 0; i < clusters.Count; ++i)
            {
                Cluster c = clusters[i];
                for (int j = 0; j < c.Count; ++j)
                {
                    Rect r = baseRects[c[j]];
                    switch (side)
                    {
                        case Cluster.Side.Right:
                            r.Width = c.Average - r.X;
                            break;
                        case Cluster.Side.Left:
                            r.Width = r.Right - c.Average;
                            r.X = c.Average;
                            break;
                        case Cluster.Side.Top:
                            r.Height = r.Bottom - c.Average;
                            r.Y = c.Average;
                            break;
                        case Cluster.Side.Bottom:
                            r.Height = c.Average - r.Y;
                            break;
                    }
                    newRects[c[j]] = r;
                }
            }
            return newRects;
        }
        public List<Rect> FitRects(List<Rect> baseRects, int errorMargin, int border)
        {
            List<Rect> newRects = baseRects;
            newRects = FitSide(newRects, errorMargin, Cluster.Side.Left);
            newRects = FitSide(newRects, errorMargin, Cluster.Side.Right);
            newRects = FitSide(newRects, errorMargin, Cluster.Side.Top);
            newRects = FitSide(newRects, errorMargin, Cluster.Side.Bottom);

            newRects = HorizontalBorders(newRects, errorMargin, border);
            newRects = VerticalBorders(newRects, errorMargin, border);
            return newRects;
        }

        private List<Rect> VerticalBorders(List<Rect> rects, double errorMargin, double border)
        {
            List<Rect> brects = new List<Rect>(rects);
            Dictionary<double, List<int>> leftDict = new Dictionary<double, List<int>>();
            Dictionary<double, List<int>> rightDict = new Dictionary<double, List<int>>();
            for (int i = 0; i < rects.Count; ++i)
            {
                Rect r = rects[i];
                if (!leftDict.ContainsKey(r.Left))
                {
                    leftDict[r.Left] = new List<int>();
                }
                leftDict[r.Left].Add(i);

                if (!rightDict.ContainsKey(r.Right))
                {
                    rightDict[r.Right] = new List<int>();
                }
                rightDict[r.Right].Add(i);
            }

            bool[] leftBools = new bool[brects.Count];
            bool[] rightBools = new bool[brects.Count];
            foreach (double lc in leftDict.Keys.ToArray())
            {
                foreach (double rc in rightDict.Keys.ToArray())
                {
                    double l = brects[leftDict[lc][0]].Left;
                    double r = brects[rightDict[rc][0]].Right;
                    if (Math.Abs(l - r - border) <= 2*errorMargin)
                    {
                        double newL = ((l + r)/2) + (border/2);
                        double newR = ((l + r)/2) - (border/2);
                        List<int> lrs = leftDict[l];
                        List<int> rrs = rightDict[r];
                        foreach (int lr in lrs)
                        {
                            if (leftBools[lr])
                            {
                                continue;
                            }
                            Rect newLR = brects[lr];
                            newLR.Width = newLR.Right - newL;
                            newLR.X = newL;
                            brects[lr] = newLR;
                            leftBools[lr] = true;
                        }
                        foreach (int rr in rrs)
                        {
                            if (rightBools[rr])
                            {
                                continue;
                            }
                            Rect newRR = brects[rr];
                            newRR.Width = newR - newRR.X;
                            brects[rr] = newRR;
                            rightBools[rr] = true;
                        }
                    }
                }
            }
            return brects;
        }

        private List<Rect> HorizontalBorders(List<Rect> rects, double errorMargin, double border)
        {
            List<Rect> brects = new List<Rect>(rects);
            Dictionary<double, List<int>> topDict = new Dictionary<double, List<int>>();
            Dictionary<double, List<int>> bottomDict = new Dictionary<double, List<int>>();
            for (int i = 0; i < rects.Count; ++i)
            {
                Rect r = rects[i];
                if (!topDict.ContainsKey(r.Top))
                {
                    topDict[r.Top] = new List<int>();
                }
                topDict[r.Top].Add(i);

                if (!bottomDict.ContainsKey(r.Bottom))
                {
                    bottomDict[r.Bottom] = new List<int>();
                }
                bottomDict[r.Bottom].Add(i);
            }

            bool[] topBools = new bool[brects.Count];
            bool[] bottomBools = new bool[brects.Count];
            foreach (double tc in topDict.Keys.ToArray())
            {
                foreach (double bc in bottomDict.Keys.ToArray())
                {
                    double t = brects[topDict[tc][0]].Top;
                    double b = brects[bottomDict[bc][0]].Bottom;
                    if (Math.Abs(t - b - border) <= 2*errorMargin)
                    {
                        double newT = ((t + b)/2) + (border/2);
                        double newB = ((t + b)/2) - (border/2);
                        List<int> trs = topDict[t];
                        List<int> brs = bottomDict[b];
                        foreach (int tr in trs)
                        {
                            if (topBools[tr])
                            {
                                continue;
                            }
                            Rect newTR = brects[tr];
                            newTR.Height = newTR.Bottom - newT;
                            newTR.Y = newT;
                            brects[tr] = newTR;
                            topBools[tr] = true;
                        }
                        foreach (int br in brs)
                        {
                            if (bottomBools[br])
                            {
                                continue;
                            }
                            Rect newBR = brects[br];
                            newBR.Height = newB - newBR.Y;
                            brects[br] = newBR;
                            bottomBools[br] = true;
                        }
                    }
                }
            }
            return brects;
        }
    }
}
