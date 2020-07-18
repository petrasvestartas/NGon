using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using NGonsCore;

namespace SubD.SubD
{

    namespace Buckminster.Components

    {

        public class Chebychev : GH_Component

        {
            List<Polyline> polylines = new List<Polyline>();

            public Chebychev()  : base("Buckminster's Chebychev Net", "Chebychev",    "Constructs a quad mesh from a NURBS surface using the Chebychev net method. https://github.com/pearswj/buckminster",  "SubD", "Sub")
            {
                this.ValuesChanged();
            }


            protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)  {

                pManager.AddSurfaceParameter("Surface", "S", "Input surface", GH_ParamAccess.item);
                pManager.AddPointParameter("Point", "P", "Starting point (Optional: If none provided, a point in the middle of the parameter space will selected.)", GH_ParamAccess.item);
                pManager[1].Optional = true;
                pManager.AddNumberParameter("Length", "L", "Length of mesh edges", GH_ParamAccess.item, 3.0);
                pManager.AddNumberParameter("Rotation", "R", "Angle of rotation for grid (degrees)", GH_ParamAccess.item, 0.0);
                pManager.AddIntegerParameter("Steps", "B", "Maximum number of steps to walk out from the starting point", GH_ParamAccess.item, 1000);
                pManager.AddBooleanParameter("Extend", "E", "Extend the surface beyond its original boundaries", GH_ParamAccess.item, false);
            }


            protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
                pManager.AddMeshParameter("Mesh", "M", "Quad mesh (Chebychev net)", GH_ParamAccess.item);
                pManager.AddPointParameter("Points", "P", "Direction points", GH_ParamAccess.tree);
                pManager.AddCurveParameter("Arc", "A", "Arcs", GH_ParamAccess.tree);
                pManager.AddCurveParameter("Arc", "A", "Arcs", GH_ParamAccess.list);
            }


            protected override void SolveInstance(IGH_DataAccess DA)
            {

                //Input
                Surface S = null;
                if (!DA.GetData(0, ref S)) { return; }

                Point3d P = Point3d.Unset;

                if (!DA.GetData(1, ref P))
                    P = S.PointAt(S.Domain(0).Mid, S.Domain(1).Mid);

                double R = Rhino.RhinoMath.UnsetValue;
                if (!DA.GetData(2, ref R)) { return; }

                double A = Rhino.RhinoMath.UnsetValue;
                if (!DA.GetData(3, ref A)) { return; }

                int max = 0;
                if (!DA.GetData(4, ref max)) { return; }

                Boolean extend = false;
                if (!DA.GetData(5, ref extend)) { return; }

                if (R <= 0)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Mesh edge length must be a positive, non-zero number.");
                    return;
                }

                Surface Sold = S;

                if (extend) { //Extend more and trim edges?
                    S = S.Extend(IsoStatus.North, R*2, true);
                    S = S.Extend(IsoStatus.East, R*2, true);
                    S = S.Extend(IsoStatus.South, R*2, true);
                    S = S.Extend(IsoStatus.West, R*2, true);
                }

                //----------------------------------------------------------------------------------------------------------//
                //Solution

                // starting point
                double u0, v0;
                S.ClosestPoint(P, out u0, out v0);

                //Create plane on surface by point and surface normal, plane x,y axis are directions for the net
                Plane plane = new Plane(S.PointAt(u0, v0), S.NormalAt(u0, v0));
                plane.Rotate(Rhino.RhinoMath.ToRadians(A), S.NormalAt(u0, v0));
                Vector3d[] dir = new Vector3d[]{plane.XAxis * R, plane.YAxis * R, plane.XAxis * -R, plane.YAxis * -R };

                //Surface
                Curve[] MyNakedEdges = Sold.ToBrep().DuplicateNakedEdgeCurves(true, false);
                Mesh[] meshes = new Mesh[] { new Mesh() , new Mesh() , new Mesh() , new Mesh() };

                //----------------------------------------------------------------------------------------------------------//
                //Create axis
                // for each direction, walk out (and store list of points)

                double u, v;
                List<Point3d>[] axis = new List<Point3d>[4];
                List<Arc>[] arcs = new List<Arc>[4];
                polylines = new List<Polyline>();

                for (int i = 0; i < 4; i++)   {

                    // set u and v to starting point
                    u = u0;
                    v = v0;
                    List<Point3d> pts = new List<Point3d>();
                    List<Arc> arcCurrent = new List<Arc>();


                    for (int j = 0; j < max + 1; j++)  {

                        Point3d pt = S.PointAt(u, v); // get point and normal for uv
                        pts.Add(pt);

                        Vector3d srfNormal = S.NormalAt(u, v) * R;

                        Arc arc = new Arc(pt + srfNormal, pt + dir[i], pt - srfNormal); // create forward facing arc and find intersection point with surface (as uv)
                        arcCurrent.Add(arc);
                        CurveIntersections isct = Intersection.CurveSurface(arc.ToNurbsCurve(), S, 0.01, 0.01);


                        if (isct.Count > 0)
                            isct[0].SurfacePointParameter(out u, out v);
                        else
                            break;

                        // adjust direction vector (new position - old position)
                        dir[i] = S.PointAt(u, v) - pt;

                    }

                    axis[i] = pts;
                    arcs[i] = arcCurrent;
                }





                //----------------------------------------------------------------------------------------------------------//
                //Build up the mesh quads in between

                GH_PreviewUtil preview = new GH_PreviewUtil(GetValue("Animate", false));

                Mesh mesh = new Mesh(); // target mesh

                for (int k = 0; k < 4; k++) { //Loop through each axis

                    Mesh qmesh = new Mesh(); // local mesh for quadrant

                    int k0 = (k + 1) % 4;//Take neighbour id
                    Point3d[,] quad = new Point3d[axis[k].Count + 10, axis[k0].Count + 10]; // 2d array of points
                    int[,] qindex = new int[axis[k].Count + 10, axis[k0].Count + 10]; // 2d array of points' indices in local mesh

                    int count = 0;

                    for (int i = 0; i < axis[k0].Count; i++) {
                        quad[0, i] = axis[k0][i];//store 2nd axis points in point array
                        qmesh.Vertices.Add(axis[k0][i]);//also add 2nd axis points to mesh 
                        qindex[0, i] = count++;//store indicies
                    }


                    for (int i = 1; i < quad.GetLength(0); i++)   {

                        if (i < axis[k].Count) {// add axis vertex
                            quad[i, 0] = axis[k][i];//store 1st axis points in point array
                            qmesh.Vertices.Add(axis[k][i]);//also add 1st axis points to mesh 
                            qindex[i, 0] = count++;//store indicies
                        }

                        // for each column attempt to locate a new vertex in the grid
                        for (int j = 1; j < quad.GetLength(1); j++)  {

                            // if quad[i - 1, j] doesn't exist, try to add it and continue (or else break the current row)
                            if (quad[i - 1, j] == new Point3d())   {

                                if (j < 2)  break; 

                                CurveIntersections isct = this.ArcIntersect(S, quad[i - 1, j - 1], quad[i - 1, j - 2], R);
                      
                                if (isct.Count > 0)  {

                                    quad[i - 1, j] = isct[0].PointB;
                                    qmesh.Vertices.Add(quad[i - 1, j]);

                                    qindex[i - 1, j] = count++;

                                } else
                                    break;

                            }

                            // if quad[i, j - 1] doesn't exist, try to create quad[i, j] by projection and skip mesh face creation
                            if (quad[i, j - 1] == new Point3d())  {

                                if (i < 2) { break; }

                                CurveIntersections isct = this.ArcIntersect(S, quad[i - 1, j], quad[i - 2, j], R);

                                if (isct.Count > 0) {

                                    quad[i, j] = isct[0].PointB;
                                    qmesh.Vertices.Add(quad[i, j]);
                                    qindex[i, j] = count++;

                                    continue;
                                }

                            }

                            // construct a sphere at each neighbouring vertex ([i,j-1] and [i-1,j]) and intersect

                            Sphere sph1 = new Sphere(quad[i, j - 1], R);
                            Sphere sph2 = new Sphere(quad[i - 1, j], R);
                            Circle cir;

                            if (Intersection.SphereSphere(sph1, sph2, out cir) == SphereSphereIntersection.Circle) {

                                CurveIntersections cin = Intersection.CurveSurface(NurbsCurve.CreateFromCircle(cir), S, 0.01, 0.01);// intersect circle with surface

                                // attempt to find the new vertex (i.e not [i-1,j-1])

                                foreach (IntersectionEvent ie in cin) {

                                    if ((ie.PointA - quad[i - 1, j - 1]).Length > 0.2 * R) {// compare with a tolerance, rather than exact comparison

                                        quad[i, j] = ie.PointA;
                                        qmesh.Vertices.Add(quad[i, j]);
                                        qindex[i, j] = count++;

                                        Point3d[] facePt = new Point3d[] { quad[i, j] , quad[i - 1, j] , quad[i - 1, j - 1] , quad[i, j - 1] };

                                        Sold.ClosestPoint(quad[i, j], out double u1, out double v1);
                                        Sold.ClosestPoint(quad[i - 1, j], out double u2, out double v2);
                                        Sold.ClosestPoint(quad[i - 1, j - 1], out double u3, out double v3);
                                        Sold.ClosestPoint(quad[i, j - 1], out double u4, out double v4);

                                        double tolerance = 0.01;
                                        bool[] flag = new bool[] {
                                        Sold.PointAt(u1, v1).DistanceTo(quad[i, j]) < tolerance,
                                        Sold.PointAt(u2, v2).DistanceTo(quad[i - 1, j]) < tolerance,
                                        Sold.PointAt(u3, v3).DistanceTo(quad[i - 1, j - 1]) < tolerance,
                                        Sold.PointAt(u4, v4).DistanceTo(quad[i, j - 1])< tolerance
                                    };



                                        if (flag[0] && flag[1] && flag[2] && flag[3]) {
                                            qmesh.Faces.AddFace(qindex[i, j], qindex[i - 1, j], qindex[i - 1, j - 1], qindex[i, j - 1]);// create quad-face
                                        } else if (flag[0] || flag[1] || flag[2] || flag[3]) {

                                            Polyline temp = new Polyline();

                                            for (int p = 0; p < 4; p++) {


                                                switch (Convert.ToInt32(flag[p]) * 10 + Convert.ToInt32(flag[(p+1)%4])) {

                                                    case (11):
                                                    temp.Add(facePt[p]);
                                                    break;
                                                    case (10):
                                                    temp.Add(facePt[p]);
                                                    temp.Add(ClosestPointOnNakedEdge(MyNakedEdges, facePt[p], new Line(facePt[p] , facePt[(p + 1) % 4])));
                                                    break;
                                                    case (1):
                                                    temp.Add(ClosestPointOnNakedEdge(MyNakedEdges, facePt[p], new Line(facePt[p], facePt[(p + 1) % 4])));
                                                    break;
                                                    case (0):
                                                    Sold.ClosestPoint(facePt[p],out double u6, out double v6);
                                                    temp.Add(Sold.PointAt(u6,v6));
                                                    break;
                                                }

                                            }
                                           temp.Close();
                                            polylines.Add(temp);

       

                                            //qmesh.Faces.AddFace(qindex[i, j], qindex[i - 1, j], qindex[i - 1, j - 1], qindex[i, j - 1]);// create quad-face
                                        }



                                        //qmesh.Faces.AddFace(qindex[i, j], qindex[i - 1, j], qindex[i - 1, j - 1], qindex[i, j - 1]);// create quad-face
                                        break;
                                    }
                                }

                                if (preview.Enabled)  {
                                    preview.Clear();
                                    preview.AddMesh(mesh);
                                    preview.AddMesh(qmesh);
                                    preview.Redraw();
                                }
                            }
                        }
                    }

                ;

                    mesh.Append(qmesh);// add local mesh to target

                }

                //----------------------------------------------------------------------------------------------------------//
                //Output
                mesh.Weld(Math.PI);
                mesh.Compact();
                mesh.Normals.ComputeNormals();

                DA.SetData(0, mesh);
                DA.SetDataTree(1, NGonsCore.GrasshopperUtil.IEOfIEToTree(axis, 0));
                DA.SetDataList(2, polylines);

                preview.Clear();
            }

            // A helper function to find the next point by walking out in a direction and pivoting down onto the surface.
            // Similar approach to initial axis generation.
            private CurveIntersections ArcIntersect(Surface s, Point3d pt, Point3d pt_prev, double rad) {
                double u, v;
                s.ClosestPoint(pt, out u, out v);// get uv             
                Vector3d n = s.NormalAt(u, v);// get normal for uv
                n *= rad;// scale normal by desired length
                Arc arc = new Arc(pt + n, pt + (pt - pt_prev), pt - n); // create forward facing arc and find intersection point with surface (as uv) use pt_prev to get direction (middle parameter)

                return Intersection.CurveSurface(arc.ToNurbsCurve(), s, 0.001, 0.001);
            }

            private Point3d ClosestPointOnNakedEdge(Curve[] MyNakedEdges, Point3d pt, Line line) {

                double minDist = double.MaxValue;
                Curve closestEdge = null;
                foreach (Curve ne in MyNakedEdges) {
                    double t;
                    ne.ClosestPoint(pt, out t);
                    double dist = pt.DistanceTo(ne.PointAt(t));
                    if (dist < minDist) {
                        minDist = dist;
                        closestEdge = ne;
                    }
                }

                closestEdge.ClosestPoints(line.ToNurbsCurve(), out Point3d ptA, out Point3d ptB);

                return ptB;
            }

            protected override System.Drawing.Bitmap Icon =>  Properties.Resources.chebyshev;


            public override Guid ComponentGuid => new Guid("{2cfe6201-8101-465c-8739-fefe111f66d2}");




            public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
            {
                //base.AppendAdditionalMenuItems(menu);
                ToolStripMenuItem toolStripMenuItem = Menu_AppendItem(menu, "Animate", new EventHandler(this.Menu_AnimateClicked), true, GetValue("Animate", false));
                toolStripMenuItem.ToolTipText = "Preview the mesh construction process.";
            }



            private void Menu_AnimateClicked(Object sender, EventArgs e)
            {
                RecordUndoEvent("AnimateChebychev");
                SetValue("Animate", !this.GetValue("Animate", false));
                ExpireSolution(true);
            }



            protected override void ValuesChanged()

            {
                if (this.GetValue("Animate", false))
                    this.Message = "Animate";
                else
                    this.Message = null;
            }




        }

    }
}