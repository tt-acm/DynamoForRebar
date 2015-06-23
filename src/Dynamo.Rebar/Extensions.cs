using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

    /// <summary>
    /// Extensions
    /// </summary>
    public static class Extensions
    {
        [IsVisibleInDynamoLibrary(false)]
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

        [IsVisibleInDynamoLibrary(false)]
        public static List<Curve> NormalCurves(this Surface face, List<Surface> surfaces, int numberOfLines, double offset, double height, bool horizontal)
        {
            List<Curve> curves = new List<Curve>();

            Point[] points = new Point[numberOfLines];

            bool VisY = true;

            if (face.PointAtParameter(0, height).X == face.PointAtParameter(1, height).X) VisY = false;

            if (!horizontal) VisY = !VisY;
            
            
            
            // Divide the Curve by a number of Lines
            for (int i = 1; i <= numberOfLines; i++)
            {
                double parameter = (double)i / (double)numberOfLines;

                if (VisY)
                    points[i-1] = face.PointAtParameter(parameter,height);
                else
                    points[i - 1] = face.PointAtParameter(height, parameter);
            }
            

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

                List<double> intersections = line.Insersection(surfaces);

                if (intersections.Count > 0)
                {

                    Curve[] segments = line.ParameterTrimSegments(intersections.ToArray(), false);
                    foreach (Curve segment in segments) { 
                        if (segment.Length < 100000) curves.Add(segment); }
                }

            }
            return curves;
        }

        [IsVisibleInDynamoLibrary(false)]
        public static List<double> Insersection(this Curve curve, List<Surface> surfaces)
        {
            List<double> intersectionParameters = new List<double>();


            foreach (Surface surface in surfaces)
            {
                Geometry[] intersections = curve.Intersect(surface);

                foreach (Geometry geometry in intersections)
                {
                    if (geometry.GetType() == typeof(Point))
                    {
                        intersectionParameters.Add(curve.ParameterAtPoint((Point)geometry));
                    }

                }
            }

            intersectionParameters.Sort();


            return intersectionParameters.RemoveDuplicates();

        }

        [IsVisibleInDynamoLibrary(false)]
        public static List<double> RemoveDuplicates(this List<double> list)
        {
            if (list.Count == 1) return list;
            else if (list.Count == 2) { list.RemoveAt(1); return list; }
            else return list;
        }

        [IsVisibleInDynamoLibrary(false)]
        public static Curve GetUpperCurve(this Surface surface)
        {
            Edge upper = surface.Edges[0];

            foreach (Edge edge in surface.Edges)
            { 
                double elevation = edge.StartVertex.PointGeometry.Y + edge.StartVertex.PointGeometry.Y;
                if (elevation > (upper.StartVertex.PointGeometry.Y + upper.StartVertex.PointGeometry.Y)) upper = edge;           
            }

            return upper.CurveGeometry;
        }

        [IsVisibleInDynamoLibrary(false)]
        public static Curve GetLowerCurve(this Surface surface)
        {
            Edge lower = surface.Edges[0];

            foreach (Edge edge in surface.Edges)
            {
                double elevation = edge.StartVertex.PointGeometry.Y + edge.StartVertex.PointGeometry.Y;
                if (elevation < (lower.StartVertex.PointGeometry.Y + lower.StartVertex.PointGeometry.Y)) lower = edge;
            }

            return lower.CurveGeometry;
        }

        [IsVisibleInDynamoLibrary(false)]
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
        [IsVisibleInDynamoLibrary(false)]
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



