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
        public static Dictionary<string, object> Perpendicular(Autodesk.DesignScript.Geometry.Surface face, List<Autodesk.DesignScript.Geometry.Surface> boundary, Autodesk.DesignScript.Geometry.Curve edge, double height, int numberOfBars)
        {

         


            List<Autodesk.DesignScript.Geometry.Curve> curves = face.NormalCurves(boundary, edge, numberOfBars, height);

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