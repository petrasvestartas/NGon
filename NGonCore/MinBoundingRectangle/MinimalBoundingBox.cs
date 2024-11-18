using Rhino.Geometry;
using System;
using System.Linq;


namespace NGonCore
{
	public static class MinimalBoundingBox
	{
		public static Polyline BoundingRectangle(this Polyline pline,Line XAxis, Vector3d YAxis, double offset = 0) {
			Box bbox = NGonCore.BoundingBoxUtil.GetBoundingBoxAligned(pline.ToList(),	new Plane(XAxis.From, XAxis.Direction,YAxis), offset);
			return new Polyline { bbox.PointAt(0, 0, 0.5), bbox.PointAt(0, 1, 0.5), bbox.PointAt(1, 1, 0.5), bbox.PointAt(1, 0, 0.5), bbox.PointAt(0, 0, 0.5) };

		}
			
			public static Polyline BoundingRectangle(this Polyline pline, double offset = 0) {
		

			Polyline pline_ = new Polyline(pline);
			pline_ = Geometry.Polyline3D.Offset(new Polyline[] { pline }, offset)[0];
			Transform xform = Transform.PlaneToPlane(pline.plane(), Plane.WorldXY);
			xform.TryGetInverse(out Transform xformInverse);
			pline_.Transform(xform);

			//if (pline_.IsClosed)
				//pline_.RemoveAt(pline_.Count-1);

			Vector2d[] pts = new Vector2d[pline_.Count];
            for (int i = 0; i < pts.Length; i++) {
				pts[i] = new Vector2d(pline_[i].X, pline_[i].Y);
            }

			pts = Calculate(pts).Points.ToArray();



			Polyline rect = new Polyline(pts.Length);
			for (int i = 0; i < pts.Length; i++) {
				rect.Add(new Point3d(pts[i].X, pts[i].Y,0));
			}

			rect.Close();
			//rect = Geometry.Polyline3D.Offset(new Polyline[] { rect }, 50)[0];

			rect.Transform(xformInverse);
			

			return rect;
        }
		/// <summary>
		/// Calculates the minimum bounding box.
		/// </summary>
		/// <param name="points">Bounding Box.</param>
		public static Polygon2d Calculate (Vector2d[] points)
		{
			//calculate the convex hull
			var hullPoints = GeoAlgos.MonotoneChainConvexHull (points);

			//check if no bounding box available
			if (hullPoints.Length <= 1)
				return new Polygon2d{ Points = hullPoints.ToList () };

			Rectangle2d minBox = null;
			var minAngle = 0d;

			//foreach edge of the convex hull
			for (var i = 0; i < hullPoints.Length; i++) {
				var nextIndex = i + 1;

				var current = hullPoints [i];
				var next = hullPoints [nextIndex % hullPoints.Length];

				var segment = new Segment2d (current, next);

				//min / max points
				var top = double.MinValue;
				var bottom = double.MaxValue;
				var left = double.MaxValue;
				var right = double.MinValue;

				//get angle of segment to x axis
				var angle = AngleToXAxis (segment);

				//rotate every point and get min and max values for each direction
				foreach (var p in hullPoints) {
					var rotatedPoint = RotateToXAxis (p, angle);

					top = Math.Max (top, rotatedPoint.Y);
					bottom = Math.Min (bottom, rotatedPoint.Y);

					left = Math.Min (left, rotatedPoint.X);
					right = Math.Max (right, rotatedPoint.X);
				}

				//create axis aligned bounding box
				var box = new Rectangle2d (new Vector2d (left, bottom), new Vector2d (right, top));

				if (minBox == null || minBox.Area () > box.Area ()) {
					minBox = box;
					minAngle = angle;
				}
			}

			//rotate axis algined box back
			var minimalBoundingBox = new Polygon2d
			{
				Points = minBox.Points.Select(p => RotateToXAxis(p, -minAngle)).ToList()
			};

			return minimalBoundingBox;
		}

		/// <summary>
		/// Calculates the angle to the X axis.
		/// </summary>
		/// <returns>The angle to the X axis.</returns>
		/// <param name="s">The segment to get the angle from.</param>
		static double AngleToXAxis (Segment2d s)
		{
			var delta = s.A - s.B;
			return -Math.Atan (delta.Y / delta.X);
		}

		/// <summary>
		/// Rotates vector by an angle to the x-Axis
		/// </summary>
		/// <returns>Rotated vector.</returns>
		/// <param name="v">Vector to rotate.</param>
		/// <param name="angle">Angle to trun by.</param>
		static Vector2d RotateToXAxis (Vector2d v, double angle)
		{
			var newX = v.X * Math.Cos (angle) - v.Y * Math.Sin (angle);
			var newY = v.X * Math.Sin (angle) + v.Y * Math.Cos (angle);

			return new Vector2d (newX, newY);
		}
	}
}

