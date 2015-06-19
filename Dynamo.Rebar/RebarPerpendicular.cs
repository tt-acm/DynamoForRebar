// TODO: Clarify License Header

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

using Autodesk.DesignScript.Runtime;

namespace Dynamo.Rebar
{
    public class RebarPerpendicular
    {

        [MultiReturn("BarCurves")]
        public static Dictionary<string, object> Perpendicular(Face face, Edge edge, double height, int numberOfBars)
        {
            List<Curve> curves = face.NormalCurves(edge.AsCurve(),numberOfBars,height);

            return new Dictionary<string, object>
            {
                {"BarCurves", curves}
            };
        }


        private RebarPerpendicular()
        {

        }



    }
}