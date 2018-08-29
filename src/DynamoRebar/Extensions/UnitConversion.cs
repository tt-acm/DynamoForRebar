using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RVT = Autodesk.Revit.DB;
using RevitServices.Persistence;
using Autodesk.DesignScript.Runtime;

namespace DynamoRebar
{
    [IsVisibleInDynamoLibrary(false)]
    public static class UnitConversion
    {
        [IsVisibleInDynamoLibrary(false)]
        public static double ToRvtLength(this double length)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            var getDocUnits = doc.GetUnits();
            var getDisplayUnits = getDocUnits.GetFormatOptions(RVT.UnitType.UT_Length).DisplayUnits;
            return RVT.UnitUtils.ConvertToInternalUnits(length, getDisplayUnits);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static double FromRvtLength(this double length)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            var getDocUnits = doc.GetUnits();
            var getDisplayUnits = getDocUnits.GetFormatOptions(RVT.UnitType.UT_Length).DisplayUnits;
            return RVT.UnitUtils.ConvertFromInternalUnits(length, getDisplayUnits);
        }
    }
}
