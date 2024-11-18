﻿using NGonCore;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
//using ClipperLib;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using NGonCore.Clipper642;
using NGonCore.Geometry;
//using StudioAvw.Clipper.Components.Helpers;
//using StudioAvw.Geometry;

namespace NGon_RH8.Polylines
{
    public class PolylineBoolean : GH_Component {
        public PolylineBoolean()
          : base("Boolean", "Boolean",
              "Polyline boolean", "NGon",
               "Polygon")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override void RegisterInputParams(GH_InputParamManager pManager) {
            EvenOdd = EvenOdd;
            pManager.AddCurveParameter("A", "A", "The first polyline", GH_ParamAccess.list);
            pManager.AddCurveParameter("B", "B", "The first polyline", GH_ParamAccess.list);
            pManager[1].Optional = true;
            // ctIntersection, ctUnion, ctDifference, ctXor };
            pManager.AddIntegerParameter("BooleanType", "BT", "Type: (0: intersection, 1: union, 2: difference, 3: xor)", GH_ParamAccess.item, 0);

            //pManager.AddPlaneParameter("Plane", "Pln", "Plane to project the polylines to", GH_ParamAccess.item, default(Plane));
            //pManager.AddNumberParameter("Tolerance", "T", "Tolerance: all floating point data beyond this precision will be discarded.", GH_ParamAccess.item, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        /// <param name="pManager">Use the pManager to register new parameters. pManager is never null.</param>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
            pManager.AddGenericParameter("Result", "R", "Simple result", GH_ParamAccess.item);
        }



        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="da">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess da) {

            
            // SETALL INPUT PARAMETERS
        
            List<Curve> curvesA = NGonCore.Clipper.DataAccessHelper.FetchList<Curve>(da,"A");
            List<Curve> curvesB = NGonCore.Clipper.DataAccessHelper.FetchList<Curve>(da,"B");
            try {
                ClipType type = (ClipType)NGonCore.Clipper.DataAccessHelper.Fetch<int>(da,"BooleanType");
                //Plane pln = NGonCore.Clipper.DataAccessHelper.Fetch<Plane>(da,"Plane");
                //double tolerance = NGonCore.Clipper.DataAccessHelper.Fetch<double>(da,"Tolerance");
                double tolerance = 0.001;

                // Convert the curves to polylines
                // This is a crude way of doing this.
                // Should we add some parameters for this perhaps?
                List <Polyline> polylinesA = Polyline3D.ConvertCurvesToPolyline(curvesA).ToList();
                List<Polyline> polylinesB = Polyline3D.ConvertCurvesToPolyline(curvesB).ToList();

                // If we don't have a plane, let's try to create a plane from the first curve.
                //if (pln.Equals(default(Plane)) || !pln.IsValid) {
                // ReSharper disable once PossibleMultipleEnumeration
                Plane pln =polylinesA.First().FitPlane();
                //}

                // do the boolean operation
                List<Polyline> result = Polyline3D.Boolean(type, polylinesA, polylinesB, pln, tolerance, EvenOdd);

                // OUTPUT LOGIC
                da.SetDataList("Result", result);
            } catch (Exception e) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message + ": " + e.StackTrace);
            }
        }
        /// <summary>
        /// The filling type even odd
        /// SEE http://www.angusj.com/delphi/clipper/documentation/Docs/Units/ClipperLib/Types/PolyFillType.htm
        /// </summary>
        public bool FillingTypeEvenOdd = true;

        /// <summary>
        /// Gets or sets a value indicating whether [even odd].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [even odd]; otherwise, <c>false</c>.
        /// </value>
        public bool EvenOdd {
            get { return FillingTypeEvenOdd; }
            set {
                FillingTypeEvenOdd = value;
                if ((FillingTypeEvenOdd)) {
                    Message = "Even/Odd filling";
                } else {
                    Message = "Non-Zero filling";
                }
            }
        }


        /// <summary>
        /// Write all required data for deserialization to an IO archive.
        /// </summary>
        /// <param name="writer">Object to write with.</param>
        /// <returns>
        /// True on success, false on failure.
        /// </returns>
        public override bool Write(GH_IWriter writer) {
            // First add our own field.
            writer.SetBoolean("EvenOdd", EvenOdd);
            // Then call the base class implementation.
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader) {
            // First read our own field.
            EvenOdd = reader.GetBoolean("EvenOdd");
            // Then call the base class implementation.
            return base.Read(reader);
        }

        /// <summary>
        /// Override this function if you want to insert some custom menu items in your derived Component class.
        /// Items will be added between List Matching items and parameter menus.
        /// </summary>
        /// <param name="menu"></param>
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu) {
            // Append the item to the menu, making sure it's always enabled and checked if Absolute is True.
            ToolStripMenuItem item = Menu_AppendItem(menu, "EvenOdd", Menu_AbsoluteClicked, true, EvenOdd);
            // Specifically assign a tooltip text to the menu item.
            item.ToolTipText = @"When checked, even/odd sorting rule is used.";
        }

        /// <summary>
        /// Handles the AbsoluteClicked event of the Menu control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Menu_AbsoluteClicked(object sender, EventArgs e) {
            RecordUndoEvent("EvenOdd");
            EvenOdd = !EvenOdd;
            ExpireSolution(true);
        }



        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.PolylineBoolean;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("a9bf996b-890a-1589-8b61-4b87be64a786"); }
        }
    }
}