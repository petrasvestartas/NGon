using System.Collections.Generic;
using System.Collections.ObjectModel;
using NGonsCore.geometry3Sharp.math;

namespace NGonsCore.geometry3Sharp.curve
{

	public interface IParametricCurve3d
	{
		bool IsClosed {get;}

		// can call SampleT in range [0,ParamLength]
		double ParamLength {get;}
		Vector3D SampleT(double t);
		Vector3D TangentT(double t);        // returns normalized vector

		bool HasArcLength {get;}
		double ArcLength {get;}
		Vector3D SampleArcLength(double a);

		void Reverse();

		IParametricCurve3d Clone();		
	}




    public interface ISampledCurve3d
    {
        int VertexCount { get; }
        bool Closed { get; }

        Vector3D GetVertex(int i);

        IEnumerable<Vector3D> Vertices { get; }
    }





	public interface IParametricCurve2d
	{
		bool IsClosed {get;}

		// can call SampleT in range [0,ParamLength]
		double ParamLength {get;}
		Vector2D SampleT(double t);
        Vector2D TangentT(double t);        // returns normalized vector

		bool HasArcLength {get;}
		double ArcLength {get;}
		Vector2D SampleArcLength(double a);

		void Reverse();

        IParametricCurve2d Clone();
	}


    public interface IMultiCurve2d
    {
        ReadOnlyCollection<IParametricCurve2d> Curves { get; }
    }

}
