using System.Collections.Generic;
using NGonsCore.geometry3Sharp.math;
using NGonsCore.geometry3Sharp.solvers;

namespace NGonsCore.geometry3Sharp.approximation
{
    // Ported from WildMagic5 Wm5ApprPlaneFit3
    // Least-squares fit of a plane to (x,y,z) data by using distance measurements
    // orthogonal to the proposed plane.
    public class OrthogonalPlaneFit3
    {
        public Vector3D Origin;
        public Vector3D Normal;
        public bool ResultValid = false;

        public OrthogonalPlaneFit3(IEnumerable<Vector3D> points)
        {
            // Compute the mean of the points.
            Origin = Vector3D.Zero;
            int numPoints = 0;
            foreach (Vector3D v in points) {
                Origin += v;
                numPoints++;
            }
            double invNumPoints = (1.0) / numPoints;
            Origin *= invNumPoints;

            // Compute the covariance matrix of the points.
            double sumXX = (double)0, sumXY = (double)0, sumXZ = (double)0;
            double sumYY = (double)0, sumYZ = (double)0, sumZZ = (double)0;
            foreach (Vector3D p in points) { 
                Vector3D diff = p - Origin;
                sumXX += diff[0] * diff[0];
                sumXY += diff[0] * diff[1];
                sumXZ += diff[0] * diff[2];
                sumYY += diff[1] * diff[1];
                sumYZ += diff[1] * diff[2];
                sumZZ += diff[2] * diff[2];
            }

            sumXX *= invNumPoints;
            sumXY *= invNumPoints;
            sumXZ *= invNumPoints;
            sumYY *= invNumPoints;
            sumYZ *= invNumPoints;
            sumZZ *= invNumPoints;

            double[] matrix = new double[] {
                sumXX, sumXY, sumXZ,
                sumXY, sumYY, sumYZ,
                sumXZ, sumYZ, sumZZ
            };

            // Setup the eigensolver.
            SymmetricEigenSolver solver = new SymmetricEigenSolver(3, 4096);
            int iters = solver.Solve(matrix, SymmetricEigenSolver.SortType.Decreasing);
            ResultValid = (iters > 0 && iters < SymmetricEigenSolver.NO_CONVERGENCE);

            Normal = new Vector3D(solver.GetEigenvector(2));
        }


    }
}
