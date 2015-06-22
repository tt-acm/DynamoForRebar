using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;

namespace Dynamo.Rebar
{
    /// <summary>
    /// Revit Extensions
    /// </summary>
    public static class Extensions
    {

        /// <summary>
        /// Divides the Curves and returns the division points
        /// </summary>
        /// <param name="line"></param>
        /// <param name="counter">division factor</param>
        /// <returns>Set of division points</returns>
        public static Point[] PointDivide(this Curve line, int counter)
        {
            Point[] points = new Point[counter + 1];

            // Walk through the counter and evaluate normalized points
            for (int i = 0; i <= counter; i++)
            {
                double parameter = (double)i / (double)counter;
                points[i] = line.PointAtParameter(parameter);
            }

            return points;
        }



        /// <summary>
        /// Returns a set of Normal Curves along a Curve of the face
        /// </summary>
        /// <param name="face"></param>
        /// <param name="curve">Curve on the Face</param>
        /// <param name="numberOfLines">Number of Normals to create</param>
        /// <param name="offset">Offset from Curve</param>
        /// <returns>List of Normal Curves</returns>
        public static List<Curve> NormalCurves(this Surface face, List<Surface> surfaces ,Curve curve, int numberOfLines, double offset)
        {
            List<Curve> curves = new List<Curve>();

            // Divide the Curve by a number of Lines
            Point[] points = curve.PointDivide(numberOfLines);

            // Create Normals for each Point
            for (int i = 0; i < points.Length; i++)
            {
                // Get the Normal at that point
                Vector normal = face.NormalAtPoint(points[i]);

                // Remove Z Value
                Vector optimizedNormal = Vector.ByCoordinates(normal.X, normal.Y, 0).Reverse();
                
                // Get an startpoint offset if it applies
                Point startPoint = points[i];
                if (offset > 0)
                {
                    Line offsetLine = Line.ByStartPointDirectionLength(points[i], optimizedNormal, offset);
                    startPoint = offsetLine.EndPoint;
                }

                // Create an endless line
                Line line = Line.ByStartPointDirectionLength(startPoint, optimizedNormal, 100000000);

                // Check for intersections with boundary surfaces
                foreach (Surface boundarySurface in surfaces)
                {
                    Geometry[] intersections = line.Intersect(boundarySurface);

                    foreach (Geometry geometry in intersections)
                    {
                        if (geometry.GetType() == typeof(Point))
                        {
                            Point intersection = (Point)geometry;

                            // overwrite line by a new line defined by start and endpoint
                            line = Line.ByStartPointEndPoint(startPoint, intersection);

                            curves.Add(line);
                        }

                    }
                }

            }
            return curves;
        }

        /// <summary>
        /// Returns a set of morphed Curves using a precision factor
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2">Target Curve</param>
        /// <param name="numberOfLines">Number of morphed Lines to create</param>
        /// <param name="precision">Precision level</param>
        /// <returns>Morphes Curves including the input curves</returns>
        public static List<Curve> MorphTo(this Curve curve1, Curve curve2, int numberOfLines, int precision = 10, double offset = 0)
        {
            List<Curve> curves = new List<Curve>();

            // Divide both curves using the precision factor
            Point[] pointsCurve1 = curve1.PointDivide(precision);
            Point[] pointsCurve2 = curve2.PointDivide(precision);

            // Create a Matrix for the morphed points
            Point[][] points = new Point[precision + 1][];

            // If the Curves are in different directions, flip them.
            if (pointsCurve1[0].DistanceTo(pointsCurve2[pointsCurve2.Length - 1]) < pointsCurve1[0].DistanceTo(pointsCurve2[0]))
            {
                Array.Reverse(pointsCurve2);
            }


            // Draw construction lines between the two curves using the division points
            for (int i = 0; i < pointsCurve1.Length; i++)
            {
                Line line = Line.ByStartPointEndPoint(pointsCurve1[i], pointsCurve2[i]);
                
                // Divide the construction line into the number of curves to create
                // Add the divsion points to the matrix
                points[i] = line.PointDivide(numberOfLines);
            }

            // Create an empty Array holding the pointWeights
            double[] pointWeights = new double[numberOfLines];

            // Flip the Matrix to create new curves from
            Point[][] transposedPoints = points.TransposeRowsAndColumns();

            // Create Curves from the Matrix
            for (int i = 0; i < transposedPoints.Length; i++)
            {
                NurbsCurve curve = NurbsCurve.ByControlPoints(transposedPoints[i].ToList());
                
                if (offset > 0)
                    curves.Add(curve.Offset(offset));
                else
                    curves.Add(curve);
            }

            return curves;
        }


        /// <summary>
        /// Flips the Array
        /// </summary>
        public static T[][] TransposeRowsAndColumns<T>(this T[][] arr)
        {
            int rowCount = arr.Length;
            int columnCount = arr[0].Length;
            T[][] transposed = new T[columnCount][];
            if (rowCount == columnCount)
            {
                transposed = (T[][])arr.Clone();
                for (int i = 1; i < rowCount; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        T temp = transposed[i][j];
                        transposed[i][j] = transposed[j][i];
                        transposed[j][i] = temp;
                    }
                }
            }
            else
            {
                for (int column = 0; column < columnCount; column++)
                {
                    transposed[column] = new T[rowCount];
                    for (int row = 0; row < rowCount; row++)
                    {
                        transposed[column][row] = arr[row][column];
                    }
                }
            }
            return transposed;
        }
    }


}
