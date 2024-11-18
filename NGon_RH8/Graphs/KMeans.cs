using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Accord.MachineLearning;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;

namespace NGon_RH8.Graphs {
    public class KMeans_Component : GH_Component,IGH_VariableParameterComponent
    {

        public KMeans_Component()
            : base("KMeans", "KMeans", "KMeans", "NGon", "Graph")
        {
        }


        protected override System.Drawing.Bitmap Icon => Properties.Resources.KMeans;


        public override Guid ComponentGuid
        {
            get { return new Guid("cba71457-5610-75a7-8cf8-6ebf7b0e14ef"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Number of Clusters", "N", "Number of Clusters", GH_ParamAccess.item, 2);
            pManager.AddNumberParameter("W", "W", "W", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager.AddNumberParameter("D", "D", "D", GH_ParamAccess.list);
            pManager.AddNumberParameter("D", "D", "D", GH_ParamAccess.list);
            pManager.AddNumberParameter("D", "D", "D", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Index", "I", "Indices of Clusters", GH_ParamAccess.list);
            pManager.AddIntegerParameter("DataTree", "T", "Use list item", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int n = 0;
            DA.GetData(0, ref n);

            List<List<double>> data = new List<List<double>>();

            for (int i = 2; i < Params.Input.Count; i++)
            {
                List<double> d = new List<double>();
                DA.GetDataList(i, d);
                if(d.Count > 0)
                    data.Add(d);
            }

            // Declare some observations
            double[][] observations = new double[data[0].Count][];

            for (int i = 0; i < data[0].Count; i++)
            {
                List<double> num = new List<double>();
                for (int j = 0; j < data.Count; j++)
                    num.Add(data[j][i]);
                observations[i] = num.ToArray();
            }

            //Get Weights
            List<double> weights = new List<double>();
            DA.GetDataList(1, weights);

            if (weights.Count != data[0].Count)
                weights = Enumerable.Repeat(1.0, data[0].Count).ToList();


            //Seed
            Accord.Math.Random.Generator.Seed = 0;

            // Create a new K-Means algorithm with n clusters 
            Accord.MachineLearning.KMeans kmeans = new Accord.MachineLearning.KMeans(n);

            KMeansClusterCollection clusters = kmeans.Learn(observations,weights.ToArray());

            int[] labels = clusters.Decide(observations);

            //Message
            base.Message = "Weights " + weights.Count.ToString() + "\r\n" + "Dimensions " + observations.Length.ToString() + " of length " + observations[0].Length.ToString();
            
            //Output
            DA.SetDataList(0, labels.ToList());

            DataTree<int> dataTree = new DataTree<int>();
            for (int i = 0; i < labels.Length; i++)
                dataTree.Add(i, new GH_Path(labels[i]));

            DA.SetDataTree(1, dataTree);

           

        }


        #region Methods of IGH_VariableParameterComponent interface

        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
            //We only let input parameters to be added (output number is fixed at one)
            if (side == GH_ParameterSide.Input)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            //We can only remove from the input
            if (side == GH_ParameterSide.Input && Params.Input.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
        {
            Param_Number param = new Param_Number();
            param.Access = GH_ParamAccess.list;
            param.Optional = true;
            param.Name = "D";
            param.NickName = param.Name;
            param.Description = "Dimension" + (Params.Input.Count + 1);

            return param;
        }

        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            //Nothing to do here by the moment
            return true;
        }

        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {
            //Nothing to do here by the moment
        }

        #endregion


    }
}
