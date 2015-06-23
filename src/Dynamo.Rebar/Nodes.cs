// TODO: Clarify License Header

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

/// <summary>
/// Rebar Nodes
/// </summary>
public static class Rebar
{
    /// <summary>
    /// Morph Bars from one Curve to another
    /// </summary>
    /// <param name="fromCurve">Source Curve</param>
    /// <param name="toCurve">Target Curve</param>
    /// <param name="numberOfBars">Number of Bars to create</param>
    /// <param name="precision">Morphing precision</param> 
    /// <param name="offset">Offset</param>
    /// <returns>List of morphed bars</returns>
    public static List<Curve> MorphedRebar(Curve fromCurve, Curve toCurve, int numberOfBars, int precision = 10, double offset = 0)
    {
        List<Curve> bars = fromCurve.MorphTo(toCurve, numberOfBars, precision, offset);
        return bars;
    }

    /// <summary>
    /// Create Rebar perpendicular to Face
    /// </summary>
    /// <param name="face">Face to Rebar</param>
    /// <param name="boundary">Boundary Faces</param>
    /// <param name="height">Height Parameter</param>
    /// <param name="numberOfBars">Number of Bars to create</param>
    /// <param name="horizontal">Horizontal or Vertical</param>
    /// <param name="offset">Offset</param>
    /// <returns>List of perpendicular bars</returns>
    public static List<Curve> PerpendicularRebar(Surface face, List<Surface> boundary, double height, int numberOfBars, bool horizontal = true, double offset = 0)
    {
        List<Curve> curves = face.NormalCurves(boundary, numberOfBars, offset, height, horizontal);
        return curves;
    }


}
