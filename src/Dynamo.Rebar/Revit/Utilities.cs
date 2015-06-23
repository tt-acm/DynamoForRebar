using System;
using Autodesk.Revit.DB;
using DynamoServices;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements
{
    [RegisterForTrace]
    public class Rebar : Element
    {
        #region Internal Properties

        /// <summary>
        /// Internal reference to the Revit Element
        /// </summary>
        internal Autodesk.Revit.DB.Structure.Rebar InternalRebar
        {
            get;
            private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalRebar; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Create from an existing Revit Element
        /// </summary>
        /// <param name="wall"></param>
        private Rebar(Autodesk.Revit.DB.Structure.Rebar rebar)
        {
            SafeInit(() => InitRebar(rebar));
        }

        /// <summary>
        /// Create a new instance of WallType, deleting the original
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="wallType"></param>
        /// <param name="baseLevel"></param>
        /// <param name="height"></param>
        /// <param name="offset"></param>
        /// <param name="flip"></param>
        /// <param name="isStructural"></param>
        private Rebar(System.Collections.Generic.List<Curve> curve,
            Autodesk.Revit.DB.Structure.RebarBarType barType,
            Autodesk.Revit.DB.Structure.RebarStyle barStyle,
            Autodesk.Revit.DB.Element host,
            Autodesk.Revit.DB.Structure.RebarHookType startHook,
            Autodesk.Revit.DB.Structure.RebarHookType endHook,
            Autodesk.Revit.DB.Structure.RebarHookOrientation startHookOrientation,
            Autodesk.Revit.DB.Structure.RebarHookOrientation endHookOrientation,
            Autodesk.Revit.DB.XYZ normal,
            bool useExistingShape,
            bool createNewShape)
        {
            SafeInit(() => InitRebar(curve, barType, barStyle,host,startHook,endHook,startHookOrientation,endHookOrientation,normal,useExistingShape,createNewShape));
        }

        #endregion

        #region Helpers for private constructors

        /// <summary>
        /// Initialize a Wall element
        /// </summary>
        /// <param name="wall"></param>
        private void InitRebar(Autodesk.Revit.DB.Structure.Rebar rebar)
        {
            InternalSetRebar(rebar);
        }

        /// <summary>
        /// Initialize a Wall element
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="wallType"></param>
        /// <param name="baseLevel"></param>
        /// <param name="height"></param>
        /// <param name="offset"></param>
        /// <param name="flip"></param>
        /// <param name="isStructural"></param>
        private void InitRebar(System.Collections.Generic.List<Curve> curve, 
            Autodesk.Revit.DB.Structure.RebarBarType barType,
            Autodesk.Revit.DB.Structure.RebarStyle barStyle,
            Autodesk.Revit.DB.Element host, 
            Autodesk.Revit.DB.Structure.RebarHookType startHook,
            Autodesk.Revit.DB.Structure.RebarHookType endHook,
            Autodesk.Revit.DB.Structure.RebarHookOrientation startHookOrientation,
            Autodesk.Revit.DB.Structure.RebarHookOrientation endHookOrientation,
            Autodesk.Revit.DB.XYZ normal,
            bool useExistingShape,
            bool createNewShape)
        {
            Autodesk.Revit.DB.Document Document = DocumentManager.Instance.CurrentDBDocument;

            // This creates a new wall and deletes the old one
            TransactionManager.Instance.EnsureInTransaction(Document);

            //Phase 1 - Check to see if the object exists and should be rebound
            var rebarElem =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Structure.Rebar>(Document);

            bool successfullyUsedExistingWall = false;
            //There was a modelcurve, try and set sketch plane
            // if you can't, rebuild 
            //if (rebarElem != null && rebarElem.Location is Autodesk.Revit.DB.LocationCurve)
            //{
            //    var wallLocation = rebarElem.Location as Autodesk.Revit.DB.LocationCurve;
            //    if ((wallLocation.Curve is Autodesk.Revit.DB.Line == curve is Autodesk.Revit.DB.Line) ||
            //        (wallLocation.Curve is Autodesk.Revit.DB.Arc == curve is Autodesk.Revit.DB.Arc))
            //    {
            //        if (!CurveUtils.CurvesAreSimilar(wallLocation.Curve, curve))
            //            wallLocation.Curve = curve;

            //        Autodesk.Revit.DB.Parameter baseLevelParameter =
            //           rebarElem.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.WALL_BASE_CONSTRAINT);
            //        Autodesk.Revit.DB.Parameter topOffsetParameter =
            //           rebarElem.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.WALL_USER_HEIGHT_PARAM);
            //        Autodesk.Revit.DB.Parameter wallTypeParameter =
            //           rebarElem.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.ELEM_TYPE_PARAM);
            //        if (baseLevelParameter.AsElementId() != baseLevel.Id)
            //            baseLevelParameter.Set(baseLevel.Id);
            //        if (Math.Abs(topOffsetParameter.AsDouble() - height) > 1.0e-10)
            //            topOffsetParameter.Set(height);
            //        if (wallTypeParameter.AsElementId() != barType.Id)
            //            wallTypeParameter.Set(barType.Id);
            //        successfullyUsedExistingWall = true;
            //    }
            //}

            var rebar = successfullyUsedExistingWall ? rebarElem :
                     Autodesk.Revit.DB.Structure.Rebar.CreateFromCurves(Document,barStyle,barType,startHook,endHook,host,normal,curve,startHookOrientation,endHookOrientation,useExistingShape,createNewShape);
            InternalSetRebar(rebar);

            TransactionManager.Instance.TransactionTaskDone();

            // delete the element stored in trace and add this new one
            ElementBinder.CleanupAndSetElementForTrace(Document, InternalRebar);
        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the internal Element, ElementId, and UniqueId
        /// </summary>
        /// <param name="wall"></param>
        private void InternalSetRebar(Autodesk.Revit.DB.Structure.Rebar rebar)
        {
            InternalRebar = rebar;
            InternalElementId = rebar.Id;
            InternalUniqueId = rebar.UniqueId;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit Wall from a guiding Curve, height, Level, and WallType
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="height"></param>
        /// <param name="level"></param>
        /// <param name="wallType"></param>
        /// <returns></returns>
        public static Rebar ByCurve(System.Collections.Generic.List<Autodesk.DesignScript.Geometry.Curve> curves, int hostId)
        {
            if (curves == null) throw new ArgumentNullException("curves");

            System.Collections.Generic.List<Curve> revitCurves = new System.Collections.Generic.List<Curve>();

            foreach (Autodesk.DesignScript.Geometry.Curve curve in curves)
            {
                revitCurves.Add(curve.ToRevitType());
            }


            ElementId id = new ElementId(hostId);
            Autodesk.Revit.DB.Element host = DocumentManager.Instance.CurrentDBDocument.GetElement(id);

            FilteredElementCollector fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument).OfClass(typeof(Autodesk.Revit.DB.Structure.RebarHookType));
            Autodesk.Revit.DB.Structure.RebarHookType hookType = (Autodesk.Revit.DB.Structure.RebarHookType)fec.FirstElement();


            FilteredElementCollector fec2 = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument).OfClass(typeof(Autodesk.Revit.DB.Structure.RebarBarType));
             Autodesk.Revit.DB.Structure.RebarBarType rebarBarType = (Autodesk.Revit.DB.Structure.RebarBarType)fec2.FirstElement();




             return new Rebar(revitCurves, rebarBarType, Autodesk.Revit.DB.Structure.RebarStyle.Standard, host, hookType, hookType, Autodesk.Revit.DB.Structure.RebarHookOrientation.Left, Autodesk.Revit.DB.Structure.RebarHookOrientation.Left, XYZ.BasisZ, false, true);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a Revit Wall from an existing reference
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static Rebar FromExisting(Autodesk.Revit.DB.Structure.Rebar rebar, bool isRevitOwned)
        {
            return new Rebar(rebar)
            {
               // IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }
}
