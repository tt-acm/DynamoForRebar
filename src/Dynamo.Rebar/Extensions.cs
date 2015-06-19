using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

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
        public static XYZ[] Divide(this Curve line, int counter)
        {
            XYZ[] points = new XYZ[counter + 1];

            // Walk through the counter and evaluate normalized points
            for (int i = 0; i <= counter; i++)
            {
                points[i] = line.Evaluate(i / counter, true);
            }

            return points;
        }


        /// <summary>
        /// Returns a set of morphed Curves of a face
        /// </summary>
        /// <param name="face"></param>
        /// <param name="curveTop">Top Curve of the Face</param>
        /// <param name="curveBottom">Bottom Curve of the Face</param>
        /// <param name="number">Number of divisions</param>
        /// <param name="offset">Offset along Normal</param>
        /// <returns>List of Curves</returns>
        public static List<Curve> OffsetCurve(this Face face, Curve curveTop, Curve curveBottom, int number, double offset)
        {
            // Mroph between the Top and the bottom curve
            List<Curve> curves = curveTop.MorphTo(curveBottom, number);

            // Walk through results
            foreach (Curve curve in curves)
            {
                // Get the midpoint
                XYZ midPoint = curve.Evaluate(0.5,true);

                // Project it onto the Surface
                IntersectionResult result = face.Project(midPoint);

                if (result != null && result.UVPoint != null)
                {
                    // Create an Offset of the Curve along the Normal of the Face at the midpoint
                    Curve offsetCurve = curve.CreateOffset(offset, face.ComputeNormal(result.UVPoint));
                    curves.Add(offsetCurve);
                }
            }

            return curves;
        }


        /// <summary>
        /// Returns a set of Normal Curves along a Curve of the face
        /// </summary>
        /// <param name="face"></param>
        /// <param name="curve">Curve on the Face</param>
        /// <param name="numberOfLines">Number of Normals to create</param>
        /// <param name="offset">Offset from Curve</param>
        /// <returns>List of Normal Curves</returns>
        public static List<Curve> NormalCurves(this Face face, Curve curve, int numberOfLines, double offset)
        {
            List<Curve> curves = new List<Curve>();

            // Divide the Curve by a number of Lines
            XYZ[] points = curve.Divide(numberOfLines);

            // Create Normals for each Point
            for (int i = 0; i < points.Length; i++)
            {
                // Project the point on the face
                IntersectionResult result = face.Project(points[i]);

                if (result != null && result.UVPoint != null)
                {
                    // Get the Normal at the point on this face
                    XYZ normal = face.ComputeNormal(result.UVPoint);

                    // Create an unbound line from the point using the normals dircetion
                    Line offsetLine = Line.CreateUnbound(points[i], normal);

                    // Evaluate the start point using the offset
                    XYZ startPoint = offsetLine.Evaluate(offset, false);

                    // Create a new Line using the new StartPoint
                    Line barLine = Line.CreateUnbound(startPoint, normal);

                    curves.Add(barLine);
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
        public static List<Curve> MorphTo(this Curve curve1, Curve curve2, int numberOfLines, int precision = 10)
        {
            List<Curve> curves = new List<Curve>();

            // Divide both curves using the precision factor
            XYZ[] pointsCurve1 = curve1.Divide(precision);
            XYZ[] pointsCurve2 = curve2.Divide(precision);

            // Create a Matrix for the morphed points
            XYZ[][] points = new XYZ[precision + 1][];

            // Draw construction lines between the two curves using the division points
            for (int i = 0; i < pointsCurve1.Length; i++)
            {
                Line line = Line.CreateBound(pointsCurve1[i], pointsCurve2[i]);

                // Divide the construction line into the number of curves to create
                // Add the divsion points to the matrix
                points[i] = line.Divide(numberOfLines);
            }

            // Create an empty Array holding the pointWeights
            double[] pointWeights = new double[numberOfLines];

            // Flip the Matrix to create new curves from
            XYZ[][] transposedPoints = points.TransposeRowsAndColumns();

            // Create Curves from the Matrix
            for (int i = 0; i < transposedPoints.Length; i++)
            {
                Curve curve = NurbSpline.Create(transposedPoints[i].ToList(), pointWeights.ToList());
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
