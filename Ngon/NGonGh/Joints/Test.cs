using System;
using System.Collections.Generic;

//using bRigid;
//using bRigid.util;
//using bRigid.test;


//using javax.vecmath;


using Grasshopper.Kernel;


namespace SubD.Joints {
    public class Test : GH_Component {

        //BPhysics physics;
       // BBox box;

        public Test()
          : base("Test", "Nickname",
              "Description",
              "NGon", "test") {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA) {

            //setup();
            //Vector3f pos = new Vector3f(15,-150,0.5f);
            //base.Message = pos.x.ToString();
            ////BObject r = new BObject(this,100,box,pos,true);
            ////physics.addBody(r);

            ////for (int i = 0; i < 100; i++)
            ////    physics.update();

            ////physics.display();






        }

        //void setup() {
        //    Vector3f min = new Vector3f(-120, -250, -120);
        //    Vector3f max = new Vector3f(120, 250, 120);

        //    physics = new BPhysics(min, max);
        //    physics.world.setGravity(new Vector3f(0, 500, 0));

        //    box = new BBox(new PApplet p, 1, 15, 60, 15);
        //}

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon {
            get {
                
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid {
            get { return new Guid("bb1074eb-8737-4bb1-8f0e-044f7014c5eb"); }
        }
    }
}