using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Autodesk.Revit.DB;

namespace Dynamo.Rebar.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Curve curve1 = (Curve)Line.CreateBound(new XYZ(0,0,0), new XYZ(10,0,0));
            Curve curve2 = (Curve)Line.CreateBound(new XYZ(0,10,0), new XYZ(10,10,0));

            XYZ[] points = curve1.Divide(4);

            curve1.MorphTo(curve2,4);

            Assert.IsNull(points);
        }
    }
}
