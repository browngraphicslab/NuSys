using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserHelper
{
    public class NaiveBayesClassifier
    {
        private Dictionary<double, List<List<double>>> _jointProbabilities;
        public double LastProbability = 0;
        public NaiveBayesClassifier()
        {
        }

        public void Train(List<List<double>> data)
        {
            _jointProbabilities = SummarizeByClass(data);
        }

        public static double Mean(List<double> numbers)
        {
            return numbers.Sum() / numbers.Count;
        }

        public static double StdDev(List<double> numbers)
        {
            var avg = Mean(numbers);
            var variance = numbers.Select(e => Math.Pow(e - avg, 2)).Sum() / (numbers.Count - 1);
            return Math.Sqrt(variance);
        }

        public static Dictionary<double,List<List<double>>> SeperateByClass(List<List<double>> data)
        {
            var seperatedDataSet = new Dictionary<double,List<List<double>>>();
            foreach (var dataPoint in data)
            {
                if (!seperatedDataSet.ContainsKey(dataPoint.Last()))
                {
                    seperatedDataSet.Add(dataPoint.Last(),new List<List<double>>());
                }
                seperatedDataSet[dataPoint.Last()].Add(dataPoint);
            }
            return seperatedDataSet;
        }

        public static List<List<double>> Summarize(List<List<double>> data)
        {
            var summary = new List<List<double>>();
            for(int i = 0; i < data.First().Count - 1; i++)
            {
                var attributes = data.Select(e => e[i]).ToList();
                var summaryContents = new List<double>() {Mean(attributes), StdDev(attributes)};
                summary.Add(summaryContents);
            }
            return summary;
        }

        public static Dictionary<double, List<List<double>>> SummarizeByClass(List<List<double>> data)
        {
            var seperated = SeperateByClass(data);
            var summaries = new Dictionary<double,List<List<double>>>();
            foreach (var keys in seperated.Keys)
            {
                summaries[keys]=Summarize(seperated[keys]);
            }
            return summaries;
        }

        public static double CalculateProbability(double x, double mean, double stdev)
        {
            var exponent = Math.Exp(-(Math.Pow(x - mean, 2) / (2 * Math.Pow(stdev, 2))));
            return (1 / (Math.Sqrt(1 * Math.PI) * stdev)) * exponent;
        }

        public Dictionary<double,double> CalculateClassProbabilities(List<double> input)
        {
            if (_jointProbabilities == null)
            {
                return null;
            }
            var probabilities = new Dictionary<double,double>();
            foreach (var classId in _jointProbabilities.Keys)
            {
                probabilities.Add(classId, 1);
                for (int i = 0; i < input.Count; i++)
                {
                    probabilities[classId] *= CalculateProbability(input[i], _jointProbabilities[classId][i][0],
                        _jointProbabilities[classId][i][1]);
                }
            }
            return probabilities;
        }
        public double Predict(List<double> input)
        {
            var probablilities = CalculateClassProbabilities(input);
            var bestLabel = -1.0;
            var bestProb = -1.0;
            foreach(var classId in probablilities.Keys)
            {
                if (probablilities[classId] > bestProb)
                {
                    bestLabel = classId;
                    bestProb = probablilities[classId];
                }
            }
            LastProbability = bestProb;
            return bestLabel;
        }
    }
}
