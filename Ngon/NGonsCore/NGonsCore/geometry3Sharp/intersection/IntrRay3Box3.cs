﻿using System;
using NGonsCore.geometry3Sharp.math;
using NGonsCore.geometry3Sharp.queries;

namespace NGonsCore.geometry3Sharp.intersection
{
	// ported from WildMagic5 
	public class IntrRay3Box3
	{
		Ray3d ray;
		public Ray3d Ray
		{
			get { return ray; }
			set { ray = value; Result = IntersectionResult.NotComputed; }
		}

		Box3d box;
		public Box3d Box
		{
			get { return box; }
			set { box = value; Result = IntersectionResult.NotComputed; }
		}

		public int Quantity = 0;
		public IntersectionResult Result = IntersectionResult.NotComputed;
		public IntersectionType Type = IntersectionType.Empty;

		public bool IsSimpleIntersection {
			get { return Result == IntersectionResult.Intersects && Type == IntersectionType.Point; }
		}

		public double RayParam0, RayParam1;
		public Vector3D Point0 = Vector3D.Zero;
		public Vector3D Point1 = Vector3D.Zero;

		public IntrRay3Box3(Ray3d r, Box3d b)
		{
			ray = r; box = b;
		}

		public IntrRay3Box3 Compute()
		{
			Find();
			return this;
		}


		public bool Find()
		{
			if (Result != IntersectionResult.NotComputed)
				return (Result == IntersectionResult.Intersects);

			// [RMS] if either line direction is not a normalized vector, 
			//   results are garbage, so fail query
			if ( ray.Direction.IsNormalized == false )  {
				Type = IntersectionType.Empty;
				Result = IntersectionResult.InvalidQuery;
				return false;
			}

			RayParam0 = 0.0;
			RayParam1 = double.MaxValue;
			IntrLine3Box3.DoClipping(ref RayParam0, ref RayParam1, ray.Origin, ray.Direction, box,
			          true, ref Quantity, ref Point0, ref Point1, ref Type);

			Result = (Type != IntersectionType.Empty) ?
				IntersectionResult.Intersects : IntersectionResult.NoIntersection;
			return (Result == IntersectionResult.Intersects);
		}




		public bool Test ()
		{
			Vector3D WdU = Vector3D.Zero;
			Vector3D AWdU = Vector3D.Zero;
			Vector3D DdU = Vector3D.Zero;
			Vector3D ADdU = Vector3D.Zero;
			Vector3D AWxDdU = Vector3D.Zero;
			double RHS;

			Vector3D diff = ray.Origin - box.Center;

			WdU[0] = ray.Direction.Dot(box.AxisX);
			AWdU[0] = Math.Abs(WdU[0]);
			DdU[0] = diff.Dot(box.AxisX);
			ADdU[0] = Math.Abs(DdU[0]);
			if (ADdU[0] > box.Extent.x && DdU[0]*WdU[0] >= (double)0)
			{
				return false;
			}

			WdU[1] = ray.Direction.Dot(box.AxisY);
			AWdU[1] = Math.Abs(WdU[1]);
			DdU[1] = diff.Dot(box.AxisY);
			ADdU[1] = Math.Abs(DdU[1]);
			if (ADdU[1] > box.Extent.y && DdU[1]*WdU[1] >= (double)0)
			{
				return false;
			}

			WdU[2] = ray.Direction.Dot(box.AxisZ);
			AWdU[2] = Math.Abs(WdU[2]);
			DdU[2] = diff.Dot(box.AxisZ);
			ADdU[2] = Math.Abs(DdU[2]);
			if (ADdU[2] > box.Extent.z && DdU[2]*WdU[2] >= (double)0)
			{
				return false;
			}

			Vector3D WxD = ray.Direction.Cross(diff);

			AWxDdU[0] = Math.Abs(WxD.Dot(box.AxisX));
			RHS = box.Extent.y*AWdU[2] + box.Extent.z*AWdU[1];
			if (AWxDdU[0] > RHS)
			{
				return false;
			}

			AWxDdU[1] = Math.Abs(WxD.Dot(box.AxisY));
			RHS = box.Extent.x*AWdU[2] + box.Extent.z*AWdU[0];
			if (AWxDdU[1] > RHS)
			{
				return false;
			}

			AWxDdU[2] = Math.Abs(WxD.Dot(box.AxisZ));
			RHS = box.Extent.x*AWdU[1] + box.Extent.y*AWdU[0];
			if (AWxDdU[2] > RHS)
			{
				return false;
			}

			return true;
		}



	}
}
