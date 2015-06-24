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
        /// <summary>
        /// Divides a Curve into equal pieces
        /// </summary>
        /// <param name="curve">Curve to divide</param>
        /// <param name="counter">Number of pieces</param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static Point[] PointDivide(this Curve curve, int counter)
        {
            Point[] points = new Point[counter + 1];

            // Walk through the counter and evaluate normalized points
            for (int i = 0; i <= counter; i++)
            {
                double parameter = (double)i / (double)counter;
                points[i] = curve.PointAtParameter(parameter);
            }

            return points;
        }

        /// <summary>
        /// Create a set of horizontal Normal Curves on a line along a surface
        /// </summary>
        /// <param name="face">Surface</param>
        /// <param name="surfaces">Boundary conditions (length of normals)</param>
        /// <param name="numberOfLines">Number of normals to create</param>
        /// <param name="offset">Offset from surface</param>
        /// <param name="height">Height parameter of the line along the surface</param>
        /// <param name="horizontal">Horizontal or Vertical position</param>
        /// <returns>List of Curves</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static List<Curve> NormalCurves(this Surface face, List<Surface> surfaces, int numberOfLines, double offset, double height, bool horizontal)
        {
            // List of return curves
            List<Curve> curves = new List<Curve>();

            numberOfLines++;

            // Array of points for a set of points to create the curve from
            Point[] points = new Point[numberOfLines - 1];

            // assume parameter V is in Y direction
            bool VisY = true;

            // Check if start and endpoint of the curve along the surface is vertical
            // If yes, V follows X direction
            if (face.PointAtParameter(0, height).X == face.PointAtParameter(1, height).X) VisY = false;

            // Invert V direction if the user wants a vertical direction
            if (!horizontal) VisY = !VisY;
                       
            // Get points along the surface forming a set of points to create a new curve from
            for (int i = 1; i < numberOfLines; i++)
            {
                // get the parameter to use
                double parameter = (double)i / (double)numberOfLines;

                // Get the point according to the matching U or V setting
                if (VisY)
                    points[i - 1] = face.PointAtParameter(parameter,height);
                else
                    points[i - 1] = face.PointAtParameter(height, parameter);
            }
            
            // Create Normals for each Point
            for (int i = 0; i < points.Length; i++)
            {
                // Get the Normal at that point
                Vector normal = face.NormalAtPoint(points[i]);

                // Remove Z Value in order to create horizontal normals
                Vector optimizedNormal = Vector.ByCoordinates(normal.X, normal.Y, 0).Reverse();
                
                // Get an startpoint offset if it applies
                Point startPoint = points[i];
                if (offset > 0)
                {
                    Line offsetLine = Line.ByStartPointDirectionLength(points[i], optimizedNormal, offset);
                    startPoint = offsetLine.EndPoint;
                }

                // Create an almost endless line
                Line line = Line.ByStartPointDirectionLength(startPoint, optimizedNormal, 100000000);
                
                // Get intersection points with boundary surfaces
                List<double> intersections = line.Insersection(surfaces);

                // If there are any intersections
                if (intersections.Count > 1)
                {
                    // trim the curve into segments if there are gaps or holes
                    Curve[] segments = line.ParameterTrimSegments(intersections.ToArray(), false);

                    // Walk through trimmed curves and add only those to the return collection
                    // which are of a reasonable length
                    foreach (Curve segment in segments)                  
                        //if (segment.Length < 100000) 
                        curves.Add(segment); 
                }
                else if (intersections.Count == 1)
                {
                    Curve[] segments = line.ParameterSplit(intersections[0]);
                    curves.Add(segments[0]);
                }
               

            }
            return curves;
        }

        /// <summary>
        /// Creates a set of Curves following a Surface
        /// </summary>
        /// <param name="face">Surface to follow</param>
        /// <param name="precision">Precision facotr for curves</param>
        /// <param name="offset">Offset from surface</param>
        /// <param name="distanceBetweenLinesInMM">Distance between curves</param>
        /// <param name="numberOfLines">Number of curves to create</param>
        /// <param name="flip">Flip direction</param>
        /// <returns>Set of Curves</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static List<Curve> Follow(this Surface face, int precision, double offset, double distanceBetweenLinesInMM, int numberOfLines, bool flip)
        {
            // Create return value collection
            List<Curve> curves = new List<Curve>();

            // Get a reference curve from the surface
            Curve referenceCurve = face.GetUpperCurve();

            // If there is a distance applied use it to determine the number of lines to create
            if (distanceBetweenLinesInMM > 0) numberOfLines = (int)(referenceCurve.Length / (distanceBetweenLinesInMM / 1000));

            numberOfLines++;

            // Walk thru the amount of lines to create
            for (int j = 1; j < numberOfLines; j++)
            {
                // Create a set of points for createing a curve
                Point[] points = new Point[precision+1];

                // Get the height parameter
                double height = (double)j / (double)numberOfLines;

                // Get Points along this height
                for (int i = 0; i <= precision; i++)
                {
                    // Get the parameter
                    double parameter = (double)i / (double)precision;

                    // Get the point according to tthe UV setting
                    if (flip)
                        points[i] = face.PointAtParameter(parameter, height);
                    else
                        points[i] = face.PointAtParameter(height, parameter);
                }

                // Create a nurbs curve from the points
                NurbsCurve nurbsCurve = NurbsCurve.ByPoints(points);

                // Offset if applies
                Curve curve = (offset > 0) ? nurbsCurve.Offset(offset) : nurbsCurve;

                curves.Add(curve);
            }

            return curves;
        }

        /// <summary>
        /// Intersect a Curve with a set of surfaces
        /// </summary>
        /// <param name="curve">Curve</param>
        /// <param name="surfaces">Boundary to test</param>
        /// <returns>List of sorted parameters along the curve</returns>
        [IsVisibleInDynamoLibrary(false)]
        public static List<double> Insersection(this Curve curve, List<Surface> surfaces)
        {
            List<double> intersectionParameters = new List<double>();

            // Walk through all surfaces and check for intersections
            // The IntersectAll Method doesnt seem to work correctly
            // So we get the intersection geometry for each surface
            foreach (Surface surface in surfaces)
            {
                // Get intersection geometry
                Geometry[] intersections = curve.Intersect(surface);

                // Get geometries from the intersection result
                foreach (Geometry geometry in intersections)
                {
                    // If its a point, get its parameter and add it to the result collection
                    if (geometry.GetType() == typeof(Point))           
                        intersectionParameters.Add(curve.ParameterAtPoint((Point)geometry));
                }
            }

            // Sort the parameters
            intersectionParameters.Sort();

            // Return a list without duplicates
            return intersectionParameters.RemoveDuplicates();
        }

        /// <summary>
        /// Remove Duplicates from double List
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static List<double> RemoveDuplicates(this List<double> list)
        {
            // If the list contains one element return it
            if (list.Count == 1)
                return list;

            // If it contains two points return only one of them
            else if (list.Count == 2)
            { 
                list.RemoveAt(1);
                return list;
            }

            else return list;
        }

        #region GetCurves

        /// <summary>
        /// Get the Upper Curve of an Surface
        /// </summary>
        /// <param name="surface"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get the Lower Curve of a Surface
        /// </summary>
        /// <param name="surface"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get the Left Curve of a Surface
        /// </summary>
        /// <param name="surface"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static Curve GetLeftCurve(this Surface surface)
        {
            Edge left = surface.Edges[0];
            foreach (Edge edge in surface.Edges)
            {
                double x = edge.StartVertex.PointGeometry.X + edge.StartVertex.PointGeometry.X;
                if (x < (left.StartVertex.PointGeometry.X + left.StartVertex.PointGeometry.X)) left = edge;
            }
            return left.CurveGeometry;
        }

        /// <summary>
        /// Get the Right Curve of a Surface
        /// </summary>
        /// <param name="surface"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static Curve GetRightCurve(this Surface surface)
        {
            Edge right = surface.Edges[0];
            foreach (Edge edge in surface.Edges)
            {
                double x = edge.StartVertex.PointGeometry.X + edge.StartVertex.PointGeometry.X;
                if (x > (right.StartVertex.PointGeometry.X + right.StartVertex.PointGeometry.X)) right = edge;
            }
            return right.CurveGeometry;
        }

        #endregion

        /// <summary>
        /// Morphes form one Curve to another
        /// </summary>
        /// <param name="curve1">Curve to start from</param>
        /// <param name="curve2">Curve to end</param>
        /// <param name="numberOfLines">Number of morphed curves to create</param>
        /// <param name="precision">Morphing precision</param>
        /// <param name="offset">Offset</param>
        /// <returns>List of morphed Curves</returns>
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
            for (int i = 1; i < transposedPoints.Length-1; i++)
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



