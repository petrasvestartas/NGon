using System;


namespace NGonCore {
	public class Rectangle2d 
	{
		public Vector2d Location { get; set; }

		public Vector2d Size { get; set; }

		public Rectangle2d ()
		{
		}

		public Rectangle2d (Vector2d a, Vector2d c) : this ()
		{
			Location = a;
			Size = c - a;
		}

		public double Area ()
		{
			return Size.X * Size.Y;
		}

		public Vector2d[] Points {
			get {
				return new  [] {
					new Vector2d (Location.X, Location.Y),
					new Vector2d (Location.X + Size.X, Location.Y),
					new Vector2d (Location.X + Size.X, Location.Y + Size.Y),
					new Vector2d (Location.X, Location.Y + Size.Y)
				};
			}
		}

	
	}
}

