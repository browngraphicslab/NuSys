using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LdaLibrary //IT'S TIME TO DO SOME ESTIMATINGGGGGG! RAWRRRRR
{
    public class Estimator
    {
        protected Model trnModel; // now we need to make a model (note that trn stands for training)
        Option option;

        // OUTPUT DAH MODULLLLLL
        public async Task<bool> Init(Option option, List<string> documents )
        {
            this.option = option;
            trnModel = new Model();

            if (option.est)
            {
                bool temp = await trnModel.initNewModel(option, documents);
                if (!temp)
                {
                    return false;
                }
                //await trnModel.data.localDict.writeWordMap(option.wordmap); //wordmap should be the equivalent of wordmapfile
            }
            else if (option.estc)
            {
                bool temp = await trnModel.initEstimatedModel(option);
                if (!temp)
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> estimate() // IT TOOK DAYZZZZ TO GET HEYURRRR >:D
        {
            if (trnModel == null)
                return false;

            Debug.WriteLine("Sampling" + trnModel.niters + " iteration!!");

            int lastIter = trnModel.liter;
            for (trnModel.liter = lastIter + 1; trnModel.liter < trnModel.niters + lastIter; trnModel.liter++) // DIS IS SO CLEVERRRR :D
            {
                Debug.WriteLine("Iteration " + trnModel.liter + " ...");

                // for all z_i :O
                for (int m = 0; m < trnModel.M; m++)
                {
                    for (int n = 0; n < trnModel.data.docs[m].length; n++)
                    {
                        // note that z_i is equivalent to z[m][n]
                        // here, we are sampling from p(z_i|z_-i,w)
                        int topic = sampling(m, n);
                        trnModel.z[m][n] = topic;
                    } // complete each word
                }//complete each doc

                if (option.savestep > 0)
                {
                    if (trnModel.liter % option.savestep == 0)
                    {
                        Debug.WriteLine("Saving the model at iteration " + trnModel.liter + " ...");
                        computeTheta();
                        computePhi();
                        await trnModel.saveModel("model-" + Conversion.ZeroPad(trnModel.liter, 5));
                    }
                }
            } // end of iterations

            Debug.WriteLine("Gibbs sampling completed!\n");
		    Debug.WriteLine("Saving the final model!\n");
		    computeTheta();
		    computePhi();
		    trnModel.liter--;
		    await trnModel.saveModel("model-final");
            return true;
        }

        public List<string> GetWordListOfTopic()
        {
            if (trnModel == null)
                return new List<string>();
            return trnModel.wordList;
        }

        //THREE MORE FUNCTIONS BOOOOOM
        /**
         * Do sampling
         * @param m document number
         * @param n word number
         * @return topic id
         */
        public int sampling(int m, int n)
        {
            // remove z_i from the count variable
            int topic = trnModel.z[m][n];
            int w = trnModel.data.docs[m].words[n];

            trnModel.nw[w,topic] -= 1;
            trnModel.nd[m,topic] -= 1;
            trnModel.nwsum[topic] -= 1;
            trnModel.ndsum[m] -= 1;

            double Vbeta = trnModel.V * trnModel.beta;
            double Kalpha = trnModel.K * trnModel.alpha;

            //do multinominal sampling via cumulative method
            for (int k = 0; k < trnModel.K; k++)
            {
                trnModel.p[k] = (trnModel.nw[w,k] + trnModel.beta) / (trnModel.nwsum[k] + Vbeta) *
                        (trnModel.nd[m,k] + trnModel.alpha) / (trnModel.ndsum[m] + Kalpha);
            }

            // cumulate multinomial parameters
            for (int k = 1; k < trnModel.K; k++)
            {
                trnModel.p[k] += trnModel.p[k - 1];
            }

            // scaled sample because of unnormalized p[]
            Random random = new Random();
            double u = random.NextDouble() * trnModel.p[trnModel.K - 1];

            for (topic = 0; topic < trnModel.K; topic++)
            {
                if (trnModel.p[topic] > u) //sample topic w.r.t distribution p
                    break;
            }

            // add newly estimated z_i to count variables
            trnModel.nw[w,topic] += 1;
            trnModel.nd[m,topic] += 1;
            trnModel.nwsum[topic] += 1;
            trnModel.ndsum[m] += 1;

            return topic;
        }

        public void computeTheta()
        {
            for (int m = 0; m < trnModel.M; m++)
            {
                for (int k = 0; k < trnModel.K; k++)
                {
                    trnModel.theta[m,k] = (trnModel.nd[m,k] + trnModel.alpha) / (trnModel.ndsum[m] + trnModel.K * trnModel.alpha);
                }
            }
        }

        public void computePhi()
        {
            for (int k = 0; k < trnModel.K; k++)
            {
                for (int w = 0; w < trnModel.V; w++)
                {
                    trnModel.phi[k,w] = (trnModel.nw[w,k] + trnModel.beta) / (trnModel.nwsum[k] + trnModel.V * trnModel.beta);
                }
            }
        }
    }
}
