using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace NGonCore
{
    public static class CircleUtil
    {



        //Get circle center from 3 planes and point that lies on a circle
        public static Circle GetCircleCenter(Plane edgePlane, Plane bottomPlane, Plane sectionPlane, List<Point3d> topPoints, double radius, double bottomCut = 0)
        {
            //Get Center Point
            Point3d c;
            Rhino.Geometry.Intersect.Intersection.PlanePlanePlane(edgePlane, bottomPlane, sectionPlane, out c);


            //Choose the farrest point from bottom plane
            double d0 = bottomPlane.ClosestPoint(topPoints[0]).DistanceToSquared(topPoints[0]);
            double d1 = bottomPlane.ClosestPoint(topPoints[1]).DistanceToSquared(topPoints[1]);
            int id = (d0 < d1) ? 1 : 0;
            //topPoints[id].Bake();

            //Project to the side plane and get its distance
            Point3d cpEdge = edgePlane.ClosestPoint(topPoints[id]);
            double x = topPoints[id].DistanceTo(cpEdge);

            //Vector to from cpEdge to center
            Vector3d yv = c - cpEdge;
            double R = yv.Length;
            yv.Unitize();

            //y value to move yv vector
            double v = Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(x, 2));

            //Get new center
            Point3d c_ = cpEdge + yv * v;

            //Get circle
            Plane circlePlane = new Plane(c_, sectionPlane.XAxis, sectionPlane.YAxis);
            Circle circle = new Circle(circlePlane, radius);


            //Assumming we are cutting the same distance from the bottom
            double radiusDiff = radius - c.DistanceTo(c_) - bottomCut;
            if (radiusDiff > 0 && bottomCut > 0) circle.Translate(-yv * radiusDiff);

            return circle;

        }

    }
}
