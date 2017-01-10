using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserHelper
{
    class TestClassifier
    {
        void Main()
        {
            var classifier = new NaiveBayesClassifier();
            var data = new List<List<double>>() {new List<double>() {1, 2, 3, 0},new List<double>(){ 1,2,3,0},new List<double>(){ 1,2,3,1}};
            classifier.Train(data);
            Debug.WriteLine(classifier.Predict(new List<double>() {2,1,3}));
        }
    }
}

