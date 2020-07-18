using System;


namespace NGonsCore.geometry3Sharp.math {

	// partially based on WildMagic5 Box3
	public struct Box3d
	{
		// A box has center C, axis directions U[0], U[1], and U[2] (mutually
		// perpendicular unit-length vectors), and extents e[0], e[1], and e[2]
		// (all nonnegative numbers).  A point X = C+y[0]*U[0]+y[1]*U[1]+y[2]*U[2]
		// is inside or on the box whenever |y[i]| <= e[i] for all i.

		public Vector3D Center;
		public Vector3D AxisX;
		public Vector3D AxisY;
		public Vector3D AxisZ;
		public Vector3D Extent;

		public Box3d(Vector3D center) {
			Center = center;
			AxisX = Vector3D.AxisX;
			AxisY = Vector3D.AxisY;
			AxisZ = Vector3D.AxisZ;
			Extent = Vector3D.Zero;
		}
		public Box3d(Vector3D center, Vector3D x, Vector3D y, Vector3D z,
		                 Vector3D extent) {
			Center = center;
			AxisX = x; AxisY = y; AxisZ = z;
			Extent = extent;
		}
		public Box3d(Vector3D center, Vector3D extent) {
			Center = center;
			Extent = extent;
			AxisX = Vector3D.AxisX;
			AxisY = Vector3D.AxisY;
			AxisZ = Vector3D.AxisZ;
		}
		public Box3d(AxisAlignedBox3d aaBox) {
			Extent= 0.5*aaBox.Diagonal;
			Center = aaBox.Min + Extent;
			AxisX = Vector3D.AxisX;
			AxisY = Vector3D.AxisY;
			AxisZ = Vector3D.AxisZ;
		}
        public Box3d(Frame3f frame, Vector3D extent)
        {
            Center = frame.Origin;
            AxisX = frame.X;
            AxisY = frame.Y;
            AxisZ = frame.Z;
            Extent = extent;
        }

        public static readonly Box3d Empty = new Box3d(Vector3D.Zero);
        public static readonly Box3d UnitZeroCentered = new Box3d(Vector3D.Zero, 0.5 * Vector3D.One);
        public static readonly Box3d UnitPositive = new Box3d(0.5 * Vector3D.One, 0.5 * Vector3D.One);


		public Vector3D Axis(int i)
		{
			return (i == 0) ? AxisX : (i == 1) ? AxisY : AxisZ;
		}


		public Vector3D[] ComputeVertices() {
			Vector3D[] v = new Vector3D[8];
			ComputeVertices(v);
			return v;
		}
		public void ComputeVertices (Vector3D[] vertex) {
			Vector3D extAxis0 = Extent.x*AxisX;
			Vector3D extAxis1 = Extent.y*AxisY;
			Vector3D extAxis2 = Extent.z*AxisZ;
			vertex[0] = Center - extAxis0 - extAxis1 - extAxis2;
			vertex[1] = Center + extAxis0 - extAxis1 - extAxis2;
			vertex[2] = Center + extAxis0 + extAxis1 - extAxis2;
			vertex[3] = Center - extAxis0 + extAxis1 - extAxis2;
			vertex[4] = Center - extAxis0 - extAxis1 + extAxis2;
			vertex[5] = Center + extAxis0 - extAxis1 + extAxis2;
			vertex[6] = Center + extAxis0 + extAxis1 + extAxis2;
			vertex[7] = Center - extAxis0 + extAxis1 + extAxis2;			
		}



        public AxisAlignedBox3d ToAABB()
        {
            // [TODO] probably more efficient way to do this...at minimum can move center-shift
            // to after the containments...
 			Vector3D extAxis0 = Extent.x*AxisX;
			Vector3D extAxis1 = Extent.y*AxisY;
			Vector3D extAxis2 = Extent.z*AxisZ;
            AxisAlignedBox3d result = new AxisAlignedBox3d(Center - extAxis0 - extAxis1 - extAxis2);
			result.Contain(Center + extAxis0 - extAxis1 - extAxis2);
            result.Contain(Center + extAxis0 + extAxis1 - extAxis2);
			result.Contain(Center - extAxis0 + extAxis1 - extAxis2);
			result.Contain(Center - extAxis0 - extAxis1 + extAxis2);
			result.Contain(Center + extAxis0 - extAxis1 + extAxis2);
			result.Contain(Center + extAxis0 + extAxis1 + extAxis2);
            result.Contain(Center - extAxis0 + extAxis1 + extAxis2);
            return result;
        }



        // corners [ (-x,-y), (x,-y), (x,y), (-x,y) ], -z, then +z
        //
        //   7---6     +z       or        3---2     -z
        //   |\  |\                       |\  |\
        //   4-\-5 \                      0-\-1 \
        //    \ 3---2                      \ 7---6   
        //     \|   |                       \|   |
        //      0---1  -z                    4---5  +z
        //
        // Note that in RHS system (which is our default), +z is "forward" so -z in this diagram 
        // is actually the back of the box (!) This is odd but want to keep consistency w/ ComputeVertices(),
        // and the implementation there needs to stay consistent w/ C++ Wildmagic5
        public Vector3D Corner(int i)
        {
            Vector3D c = Center;
            c += (  ((i&1) != 0) ^ ((i&2) != 0) ) ? (Extent.x * AxisX) : (-Extent.x * AxisX);
            c += ( (i / 2) % 2 == 0 ) ? (-Extent.y * AxisY) : (Extent.y * AxisY);
            c += (i < 4) ? (-Extent.z * AxisZ) : (Extent.z * AxisZ);
            return c;
        }


		// g3 extensions
		public double MaxExtent {
			get { return Math.Max(Extent.x, Math.Max(Extent.y, Extent.z) ); }
		}
		public double MinExtent {
			get { return Math.Min(Extent.x, Math.Max(Extent.y, Extent.z) ); }
		}
		public Vector3D Diagonal {
			get { return (Extent.x*AxisX + Extent.y*AxisY + Extent.z*AxisZ) - 
				(-Extent.x*AxisX - Extent.y*AxisY - Extent.z*AxisZ); }
		}
		public double Volume {
			get { return 2*Extent.x + 2*Extent.y * 2*Extent.z; }
		}

		public void Contain( Vector3D v) {
			Vector3D lv = v - Center;
			for (int k = 0; k < 3; ++k) {
				double t = lv.Dot(Axis(k));
				if ( Math.Abs(t) > Extent[k]) {
					double min = -Extent[k], max = Extent[k];
					if ( t < min )
						min = t;
					else if ( t > max )
						max = t;
					Extent[k] = (max-min) * 0.5;
					Center = Center + ((max+min) * 0.5) * Axis(k);
				}
			}			
		}

		// I think this can be more efficient...no? At least could combine
		// all the axis-interval updates before updating Center...
		public void Contain( Box3d o ) {
			Vector3D[] v = o.ComputeVertices();
			for (int k = 0; k < 8; ++k) 
				Contain(v[k]);
		}

		public bool Contained( Vector3D v ) {
			Vector3D lv = v - Center;
			return (Math.Abs(lv.Dot(AxisX)) <= Extent.x) &&
				(Math.Abs(lv.Dot(AxisY)) <= Extent.y) &&
				(Math.Abs(lv.Dot(AxisZ)) <= Extent.z);
		}

		public void Expand(double f) {
			Extent += f;
		}

		public void Translate( Vector3D v ) {
			Center += v;
		}

        public void Scale(Vector3D s)
        {
            Center *= s;
            Extent *= s;
            AxisX *= s; AxisX.Normalize();
            AxisY *= s; AxisY.Normalize();
            AxisZ *= s; AxisZ.Normalize();
        }

        public void ScaleExtents(Vector3D s)
        {
            Extent *= s;
        }

        public static implicit operator Box3d(Box3f v)
        {
            return new Box3d(v.Center, v.AxisX, v.AxisY, v.AxisZ, v.Extent);
        }
        public static explicit operator Box3f(Box3d v)
        {
            return new Box3f((Vector3F)v.Center, (Vector3F)v.AxisX, (Vector3F)v.AxisY, (Vector3F)v.AxisZ, (Vector3F)v.Extent);
        }

	}












	// partially based on WildMagic5 Box3
	public struct Box3f
	{
		// A box has center C, axis directions U[0], U[1], and U[2] (mutually
		// perpendicular unit-length vectors), and extents e[0], e[1], and e[2]
		// (all nonnegative numbers).  A point X = C+y[0]*U[0]+y[1]*U[1]+y[2]*U[2]
		// is inside or on the box whenever |y[i]| <= e[i] for all i.

		public Vector3F Center;
		public Vector3F AxisX;
		public Vector3F AxisY;
		public Vector3F AxisZ;
		public Vector3F Extent;

		public Box3f(Vector3F center) {
			Center = center;
			AxisX = Vector3F.AxisX;
			AxisY = Vector3F.AxisY;
			AxisZ = Vector3F.AxisZ;
			Extent = Vector3F.Zero;
		}
		public Box3f(Vector3F center, Vector3F x, Vector3F y, Vector3F z,
		             Vector3F extent) {
			Center = center;
			AxisX = x; AxisY = y; AxisZ = z;
			Extent = extent;
		}
		public Box3f(Vector3F center, Vector3F extent) {
			Center = center;
			Extent = extent;
			AxisX = Vector3F.AxisX;
			AxisY = Vector3F.AxisY;
			AxisZ = Vector3F.AxisZ;
		}
		public Box3f(AxisAlignedBox3f aaBox) {
			Extent= 0.5f*aaBox.Diagonal;
			Center = aaBox.Min + Extent;
			AxisX = Vector3F.AxisX;
			AxisY = Vector3F.AxisY;
			AxisZ = Vector3F.AxisZ;
		}

		public static readonly Box3f Empty = new Box3f(Vector3F.Zero);


		public Vector3F Axis(int i)
		{
			return (i == 0) ? AxisX : (i == 1) ? AxisY : AxisZ;
		}


		public Vector3F[] ComputeVertices() {
			Vector3F[] v = new Vector3F[8];
			ComputeVertices(v);
			return v;
		}
		public void ComputeVertices (Vector3F[] vertex) {
			Vector3F extAxis0 = Extent.x*AxisX;
			Vector3F extAxis1 = Extent.y*AxisY;
			Vector3F extAxis2 = Extent.z*AxisZ;
			vertex[0] = Center - extAxis0 - extAxis1 - extAxis2;
			vertex[1] = Center + extAxis0 - extAxis1 - extAxis2;
			vertex[2] = Center + extAxis0 + extAxis1 - extAxis2;
			vertex[3] = Center - extAxis0 + extAxis1 - extAxis2;
			vertex[4] = Center - extAxis0 - extAxis1 + extAxis2;
			vertex[5] = Center + extAxis0 - extAxis1 + extAxis2;
			vertex[6] = Center + extAxis0 + extAxis1 + extAxis2;
			vertex[7] = Center - extAxis0 + extAxis1 + extAxis2;			
		}


        public AxisAlignedBox3f ToAABB()
        {
            // [TODO] probably more efficient way to do this...at minimum can move center-shift
            // to after the containments...
 			Vector3F extAxis0 = Extent.x*AxisX;
			Vector3F extAxis1 = Extent.y*AxisY;
			Vector3F extAxis2 = Extent.z*AxisZ;
            AxisAlignedBox3f result = new AxisAlignedBox3f(Center - extAxis0 - extAxis1 - extAxis2);
			result.Contain(Center + extAxis0 - extAxis1 - extAxis2);
            result.Contain(Center + extAxis0 + extAxis1 - extAxis2);
			result.Contain(Center - extAxis0 + extAxis1 - extAxis2);
			result.Contain(Center - extAxis0 - extAxis1 + extAxis2);
			result.Contain(Center + extAxis0 - extAxis1 + extAxis2);
			result.Contain(Center + extAxis0 + extAxis1 + extAxis2);
            result.Contain(Center - extAxis0 + extAxis1 + extAxis2);
            return result;
        }



		// g3 extensions
		public double MaxExtent {
			get { return Math.Max(Extent.x, Math.Max(Extent.y, Extent.z) ); }
		}
		public double MinExtent {
			get { return Math.Min(Extent.x, Math.Max(Extent.y, Extent.z) ); }
		}
		public Vector3F Diagonal {
			get { return (Extent.x*AxisX + Extent.y*AxisY + Extent.z*AxisZ) - 
				(-Extent.x*AxisX - Extent.y*AxisY - Extent.z*AxisZ); }
		}
		public double Volume {
			get { return 2*Extent.x + 2*Extent.y * 2*Extent.z; }
		}

		public void Contain( Vector3F v) {
			Vector3F lv = v - Center;
			for (int k = 0; k < 3; ++k) {
				double t = lv.Dot(Axis(k));
				if ( Math.Abs(t) > Extent[k]) {
					double min = -Extent[k], max = Extent[k];
					if ( t < min )
						min = t;
					else if ( t > max )
						max = t;
					Extent[k] = (float)(max-min) * 0.5f;
					Center = Center + ((float)(max+min) * 0.5f) * Axis(k);
				}
			}			
		}

		// I think this can be more efficient...no? At least could combine
		// all the axis-interval updates before updating Center...
		public void Contain( Box3f o ) {
			Vector3F[] v = o.ComputeVertices();
			for (int k = 0; k < 8; ++k) 
				Contain(v[k]);
		}

		public bool Contained( Vector3F v ) {
			Vector3F lv = v - Center;
			return (Math.Abs(lv.Dot(AxisX)) <= Extent.x) &&
				(Math.Abs(lv.Dot(AxisY)) <= Extent.y) &&
				(Math.Abs(lv.Dot(AxisZ)) <= Extent.z);
		}

		public void Expand(float f) {
			Extent += f;
		}

		public void Translate( Vector3F v ) {
			Center += v;
		}

        public void Scale(Vector3F s)
        {
            Center *= s;
            Extent *= s;
            AxisX *= s; AxisX.Normalize();
            AxisY *= s; AxisY.Normalize();
            AxisZ *= s; AxisZ.Normalize();
        }

        public void ScaleExtents(Vector3F s)
        {
            Extent *= s;
        }

	}




}
