using System;

namespace NGonsCore.geometry3Sharp.math {

	// partially based on WildMagic5 Box3
	public struct Box2d
	{
		// A box has center C, axis directions U[0] and U[1] (perpendicular and
		// unit-length vectors), and extents e[0] and e[1] (nonnegative numbers).
		// A/ point X = C+y[0]*U[0]+y[1]*U[1] is inside or on the box whenever
		// |y[i]| <= e[i] for all i.

		public Vector2D Center;
		public Vector2D AxisX;
		public Vector2D AxisY;
		public Vector2D Extent;

		public Box2d(Vector2D center) {
			Center = center;
			AxisX = Vector2D.AxisX;
			AxisY = Vector2D.AxisY;
			Extent = Vector2D.Zero;
		}
		public Box2d(Vector2D center, Vector2D x, Vector2D y, Vector2D extent) {
			Center = center;
			AxisX = x; AxisY = y;
			Extent = extent;
		}
		public Box2d(Vector2D center, Vector2D extent) {
			Center = center;
			Extent = extent;
			AxisX = Vector2D.AxisX;
			AxisY = Vector2D.AxisY;
		}
		public Box2d(AxisAlignedBox2d aaBox) {
			Extent= 0.5*aaBox.Diagonal;
			Center = aaBox.Min + Extent;
			AxisX = Vector2D.AxisX;
			AxisY = Vector2D.AxisY;
		}

		public static readonly Box2d Empty = new Box2d(Vector2D.Zero);


		public Vector2D Axis(int i)
		{
			return (i == 0) ? AxisX : AxisY;
		}


		public Vector2D[] ComputeVertices() {
			Vector2D[] v = new Vector2D[4];
			ComputeVertices(v);
			return v;
		}
		public void ComputeVertices (Vector2D[] vertex) {
			Vector2D extAxis0 = Extent.x*AxisX;
			Vector2D extAxis1 = Extent.y*AxisY;
			vertex[0] = Center - extAxis0 - extAxis1;
			vertex[1] = Center + extAxis0 - extAxis1;
			vertex[2] = Center + extAxis0 + extAxis1;
			vertex[3] = Center - extAxis0 + extAxis1;		
		}


		// g3 extensions
		public double MaxExtent {
			get { return Math.Max(Extent.x, Extent.y); }
		}
		public double MinExtent {
			get { return Math.Min(Extent.x, Extent.y); }
		}
		public Vector2D Diagonal {
			get { return (Extent.x*AxisX + Extent.y*AxisY) - 
				(-Extent.x*AxisX - Extent.y*AxisY); }
		}
		public double Area {
			get { return 2*Extent.x + 2*Extent.y; }
		}

		public void Contain( Vector2D v) {
			Vector2D lv = v - Center;
			for (int k = 0; k < 2; ++k) {
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
		public void Contain( Box2d o ) {
			Vector2D[] v = o.ComputeVertices();
			for (int k = 0; k < 4; ++k) 
				Contain(v[k]);
		}

		public bool Contained( Vector2D v ) {
			Vector2D lv = v - Center;
			return (Math.Abs(lv.Dot(AxisX)) <= Extent.x) &&
				(Math.Abs(lv.Dot(AxisY)) <= Extent.y);
		}

		public void Expand(double f) {
			Extent += f;
		}

		public void Translate( Vector2D v ) {
			Center += v;
		}


        public static implicit operator Box2d(Box2f v)
        {
            return new Box2d(v.Center, v.AxisX, v.AxisY, v.Extent);
        }
        public static explicit operator Box2f(Box2d v)
        {
            return new Box2f((Vector2F)v.Center, (Vector2F)v.AxisX, (Vector2F)v.AxisY, (Vector2F)v.Extent);
        }


	}












	// partially based on WildMagic5 Box3
	public struct Box2f
	{
		// A box has center C, axis directions U[0] and U[1] (perpendicular and
		// unit-length vectors), and extents e[0] and e[1] (nonnegative numbers).
		// A/ point X = C+y[0]*U[0]+y[1]*U[1] is inside or on the box whenever
		// |y[i]| <= e[i] for all i.

		public Vector2F Center;
		public Vector2F AxisX;
		public Vector2F AxisY;
		public Vector2F Extent;

		public Box2f(Vector2F center) {
			Center = center;
			AxisX = Vector2F.AxisX;
			AxisY = Vector2F.AxisY;
			Extent = Vector2F.Zero;
		}
		public Box2f(Vector2F center, Vector2F x, Vector2F y, Vector2F extent) {
			Center = center;
			AxisX = x; AxisY = y;
			Extent = extent;
		}
		public Box2f(Vector2F center, Vector2F extent) {
			Center = center;
			Extent = extent;
			AxisX = Vector2F.AxisX;
			AxisY = Vector2F.AxisY;
		}
		public Box2f(AxisAlignedBox2f aaBox) {
			Extent= 0.5f*aaBox.Diagonal;
			Center = aaBox.Min + Extent;
			AxisX = Vector2F.AxisX;
			AxisY = Vector2F.AxisY;
		}

		public static readonly Box2f Empty = new Box2f(Vector2F.Zero);


		public Vector2F Axis(int i)
		{
			return (i == 0) ? AxisX : AxisY;
		}


		public Vector2F[] ComputeVertices() {
			Vector2F[] v = new Vector2F[4];
			ComputeVertices(v);
			return v;
		}
		public void ComputeVertices (Vector2F[] vertex) {
			Vector2F extAxis0 = Extent.x*AxisX;
			Vector2F extAxis1 = Extent.y*AxisY;
			vertex[0] = Center - extAxis0 - extAxis1;
			vertex[1] = Center + extAxis0 - extAxis1;
			vertex[2] = Center + extAxis0 + extAxis1;
			vertex[3] = Center - extAxis0 + extAxis1;
		}


		// g3 extensions
		public double MaxExtent {
			get { return Math.Max(Extent.x, Extent.y); }
		}
		public double MinExtent {
			get { return Math.Min(Extent.x, Extent.y); }
		}
		public Vector2F Diagonal {
			get { return (Extent.x*AxisX + Extent.y*AxisY) - 
				(-Extent.x*AxisX - Extent.y*AxisY); }
		}
		public double Area {
			get { return 2*Extent.x + 2*Extent.y; }
		}

		public void Contain( Vector2F v) {
			Vector2F lv = v - Center;
			for (int k = 0; k < 2; ++k) {
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
		public void Contain( Box2f o ) {
			Vector2F[] v = o.ComputeVertices();
			for (int k = 0; k < 4; ++k) 
				Contain(v[k]);
		}

		public bool Contained( Vector2F v ) {
			Vector2F lv = v - Center;
			return (Math.Abs(lv.Dot(AxisX)) <= Extent.x) &&
				(Math.Abs(lv.Dot(AxisY)) <= Extent.y);
		}

		public void Expand(float f) {
			Extent += f;
		}

		public void Translate( Vector2F v ) {
			Center += v;
		}

	}




}
