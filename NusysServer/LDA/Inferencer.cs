﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LDAUser
{
    public class Inferencer
    {
        // Train model
        public Model trnModel;
        public Dictionary globalDict;
        private Option option;

        private Model newModel;
        public int niters = 100;


        //-----------------------------------------------------
        // Init method
        //-----------------------------------------------------
        public async Task<bool> init(Option option)
        {
            this.option = option;
            trnModel = new Model();
            bool temp = await trnModel.initEstimatedModel(option);
            if (!temp)
                return false;

            globalDict = trnModel.data.localDict;
            computeTrnTheta();
            computeTrnPhi();

            return true;
        }

        //inference new model ~ getting data from a specified dataset
        public async Task<Model> inference(LDADataset newData){
		    //Debug.WriteLine("init new model");
		    Model newModel = new Model();		
		
		    await newModel.initNewModel(option, newData, trnModel);		
		    this.newModel = newModel;		
		
		    //Debug.WriteLine("Sampling " + niters + " iteration for inference!");		
		    for (newModel.liter = 1; newModel.liter <= niters; newModel.liter++){
			    //Debug.WriteLine("Iteration " + newModel.liter + " ...");
			
			    // for all newz_i
			    for (int m = 0; m < newModel.M; ++m){
				    for (int n = 0; n < newModel.data.docs[m].length; n++){
					    // (newz_i = newz[m][n]
					    // sample from p(z_i|z_-1,w)
					    int topic = infSampling(m, n);
					    newModel.z[m][n]= topic;
				    }
			    }//end foreach new doc
			
		    }// end iterations

            //Debug.WriteLine("Gibbs sampling for inference completed!");
		
		    computeNewTheta();
		    computeNewPhi();
		    newModel.liter--;
		    return this.newModel;
	    }

        public async Task<Model> inference(String[] strs)
        {
            //Debug.WriteLine("inference");
            Model newModel = new Model();

            //Debug.WriteLine("read dataset");
            LDADataset dataset = LDADataset.ReadDataSet(strs, globalDict);

            return await inference(dataset);
        }

        //inference new model ~ getting dataset from file specified in option
        /*
        public async Task<Model> inference(){	
		    //Debug.WriteLine("inference");
		
		    newModel = new Model();
            bool temp = await newModel.initNewModel(option, trnModel);
		    if (!temp) return null;
		
		    Debug.WriteLine("Sampling " + niters + " iteration for inference!");
		
		    for (newModel.liter = 1; newModel.liter <= niters; newModel.liter++){
			    //Debug.WriteLine("Iteration " + newModel.liter + " ...");
			
			    // for all newz_i
			    for (int m = 0; m < newModel.M; ++m){
				    for (int n = 0; n < newModel.data.docs[m].length; n++){
					    // (newz_i = newz[m][n]
					    // sample from p(z_i|z_-1,w)
					    int topic = infSampling(m, n);
					    newModel.z[m][n]= topic;
				    }
			    }//end foreach new doc
			
		    }// end iterations
		
		    Debug.WriteLine("Gibbs sampling for inference completed!");		
		    Debug.WriteLine("Saving the inference outputs!");
		
		    computeNewTheta();
		    computeNewPhi();
		    newModel.liter--;
		    await newModel.saveModel(newModel.dfile + "." + newModel.modelName);		
		
		    return newModel;
	    }
        */

        /**
 * do sampling for inference
 * m: document number
 * n: word number?
 */
        protected int infSampling(int m, int n)
        {
            // remove z_i from the count variables
            int topic = newModel.z[m][n];
            int _w = newModel.data.docs[m].words[n];
            int w = newModel.data.lid2gid[_w];
            newModel.nw[_w,topic] -= 1;
            newModel.nd[m,topic] -= 1;
            newModel.nwsum[topic] -= 1;
            newModel.ndsum[m] -= 1;

            double Vbeta = trnModel.V * newModel.beta;
            double Kalpha = trnModel.K * newModel.alpha;

            // do multinomial sampling via cummulative method		
            for (int k = 0; k < newModel.K; k++)
            {
                newModel.p[k] = (trnModel.nw[w,k] + newModel.nw[_w,k] + newModel.beta) / (trnModel.nwsum[k] + newModel.nwsum[k] + Vbeta) *
                        (newModel.nd[m,k] + newModel.alpha) / (newModel.ndsum[m] + Kalpha);
            }

            // cummulate multinomial parameters
            for (int k = 1; k < newModel.K; k++)
            {
                newModel.p[k] += newModel.p[k - 1];
            }

            // scaled sample because of unnormalized p[]
            Random random = new Random();
            double u = random.NextDouble() * newModel.p[newModel.K - 1];

            for (topic = 0; topic < newModel.K; topic++)
            {
                if (newModel.p[topic] > u)
                    break;
            }

            // add newly estimated z_i to count variables
            newModel.nw[_w,topic] += 1;
            newModel.nd[m,topic] += 1;
            newModel.nwsum[topic] += 1;
            newModel.ndsum[m] += 1;

            return topic;
        }

        protected void computeNewTheta()
        {
            for (int m = 0; m < newModel.M; m++)
            {
                for (int k = 0; k < newModel.K; k++)
                {
                    newModel.theta[m,k] = (newModel.nd[m,k] + newModel.alpha) / (newModel.ndsum[m] + newModel.K * newModel.alpha);
                }//end foreach topic
            }//end foreach new document
        }

        protected void computeNewPhi()
        {
            for (int k = 0; k < newModel.K; k++)
            {
                for (int _w = 0; _w < newModel.V; _w++)
                {
                    int id = newModel.data.lid2gid[_w];

                    if (id != null)
                    {
                        newModel.phi[k,_w] = (trnModel.nw[id,k] + newModel.nw[_w,k] + newModel.beta) / (newModel.nwsum[k] + newModel.nwsum[k] + trnModel.V * newModel.beta);
                    }
                }//end foreach word
            }// end foreach topic
        }

        protected void computeTrnTheta()
        {
            for (int m = 0; m < trnModel.M; m++)
            {
                for (int k = 0; k < trnModel.K; k++)
                {
                    trnModel.theta[m,k] = (trnModel.nd[m,k] + trnModel.alpha) / (trnModel.ndsum[m] + trnModel.K * trnModel.alpha);
                }
            }
        }

        protected void computeTrnPhi()
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
