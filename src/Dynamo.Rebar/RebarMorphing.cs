// TODO: Clarify License Header

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

using Autodesk.DesignScript.Runtime;

namespace Dynamo.Rebar
{
    public class RebarMorphing
    {
        /// <summary>
        /// Morph between two curves
        /// </summary>
        /// <param name="edge1"></param>
        /// <param name="edge2"></param>
        /// <param name="precision"></param>
        /// <param name="numberOfBars"></param>
        /// <returns></returns>
        [MultiReturn("BarCurves")]
        public static Dictionary<string, object> Morph(Curve edge1, Curve edge2, int precision, int numberOfBars)
        {          
            List<Curve> bars = edge1.MorphTo(edge2, numberOfBars, precision);

            return new Dictionary<string, object>
            {
                {"BarCurves", bars}
            };
        }


        private RebarMorphing() 
        {

        }



    }
}