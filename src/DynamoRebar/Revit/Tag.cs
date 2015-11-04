//
// Copyright 2015 Autodesk, Inc.
// Author: Thornton Tomasetti Ltd, CORE Studio (Maximilian Thumfart)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using System;
using Autodesk.Revit.DB;
using DynamoServices;
using Autodesk.DesignScript.Runtime;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using System.Collections.Generic;


namespace Revit.Elements
{
    [DynamoServices.RegisterForTrace]
    public class Tag : Element
    {
        #region Internal Properties

        /// <summary>
        /// Internal reference to the Revit Element
        /// </summary>
        internal Autodesk.Revit.DB.IndependentTag InternalTag
        {
            get;
            private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalTag; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Create from an existing Revit Element
        /// </summary>
        /// <param name="rebar"></param>
        private Tag(Autodesk.Revit.DB.IndependentTag tag)
        {
            SafeInit(() => InitTag(tag));
        }


        private Tag(
            Autodesk.Revit.DB.View view,
            Autodesk.Revit.DB.Element host,
            Autodesk.Revit.DB.TagOrientation orientation,
            Autodesk.Revit.DB.TagMode mode,
            bool addLeader,
            Autodesk.Revit.DB.XYZ point)
        {
            SafeInit(() => InitTag(view, host,orientation,mode,addLeader,point));
        }

        #endregion

        #region Helpers for private constructors

        /// <summary>
        /// Initialize a Rebar element
        /// </summary>
        /// <param name="rebar"></param>
        private void InitTag(Autodesk.Revit.DB.IndependentTag tag)
        {
            InternalSetTag(tag);
        }


        private void InitTag(
            Autodesk.Revit.DB.View view,
            Autodesk.Revit.DB.Element host,
            Autodesk.Revit.DB.TagOrientation orientation,
            Autodesk.Revit.DB.TagMode mode,
            bool addLeader,
            Autodesk.Revit.DB.XYZ point)
        {
            Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(document);

            var tagElem = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.IndependentTag>(document);


            if (tagElem == null || 
                view.Id != tagElem.OwnerViewId ||
                (tagElem.TaggedElementId.HostElementId != host.Id && tagElem.TaggedElementId.LinkedElementId != host.Id))

                tagElem = document.Create.NewTag(view, host, addLeader, mode, orientation, point);
            else
            {
                tagElem.TagOrientation = orientation;
                tagElem.HasLeader = addLeader;
                tagElem.TagHeadPosition = point;
            }

            InternalSetTag(tagElem);

            TransactionManager.Instance.TransactionTaskDone();


            if (tagElem != null)
            {
                ElementBinder.CleanupAndSetElementForTrace(document, this.InternalElement);
            }
            else
            {
                ElementBinder.SetElementForTrace(this.InternalElement);
            }

        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the internal Element, ElementId, and UniqueId
        /// </summary>
        /// <param name="rebar"></param>
        private void InternalSetTag(Autodesk.Revit.DB.IndependentTag tag)
        {
            InternalTag = tag;
            InternalElementId = tag.Id;
            InternalUniqueId = tag.UniqueId;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create an element based Tag
        /// </summary>
        /// <param name="view">View to Tag</param>
        /// <param name="element">Element to tag</param>
        /// <param name="horizontal">Horizontal alignment</param>
        /// <param name="addLeader">Add a leader</param>
        /// <param name="offset">Offset Vector</param>
        /// <param name="horizontalAlignment">Horizontal Alignment</param>
        /// <param name="verticalAlignment">Vertical Alignment</param>
        /// <returns></returns>
        public static Tag ByElement(Revit.Elements.Views.View view, Element element, bool horizontal, bool addLeader, Autodesk.DesignScript.Geometry.Vector offset = null, string horizontalAlignment = "Center", string verticalAlignment = "Middle")
        {
            if (offset == null) offset = Autodesk.DesignScript.Geometry.Vector.ByCoordinates(0, 0, 0);

            Autodesk.Revit.DB.HorizontalAlignmentStyle alignHorizontal = Autodesk.Revit.DB.HorizontalAlignmentStyle.Center;
            Enum.TryParse<Autodesk.Revit.DB.HorizontalAlignmentStyle>(horizontalAlignment, out alignHorizontal);

            Autodesk.Revit.DB.VerticalAlignmentStyle alignVertical = Autodesk.Revit.DB.VerticalAlignmentStyle.Middle;
            Enum.TryParse<Autodesk.Revit.DB.VerticalAlignmentStyle>(verticalAlignment, out alignVertical);

            //Autodesk.Revit.DB.Document document = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.View revitView = (Autodesk.Revit.DB.View)view.InternalElement;
            Autodesk.Revit.DB.XYZ point = null;
            Autodesk.Revit.DB.TagMode tagMode = TagMode.TM_ADDBY_CATEGORY;
            Autodesk.Revit.DB.TagOrientation orientation = (horizontal)? TagOrientation.Horizontal : TagOrientation.Vertical;
            
            if (revitView.ViewType != ViewType.FloorPlan &&
                revitView.ViewType != ViewType.EngineeringPlan &&
                revitView.ViewType != ViewType.Detail &&
                revitView.ViewType != ViewType.Section &&
                revitView.ViewType != ViewType.Elevation &&
                revitView.ViewType != ViewType.CeilingPlan)
                throw new ArgumentException("Cannot place a Tag on active View");


                //if (element.InternalElement.Location.GetType() == typeof(LocationPoint))
                //{
                //    LocationPoint locationPoint = (LocationPoint)element.InternalElement.Location;
                //    point = locationPoint.Point;
                //}
                //else if (element.InternalElement.Location.GetType() == typeof(LocationCurve))
                //{
                //    LocationCurve locationCurve = (LocationCurve)element.InternalElement.Location;
                //    point = locationCurve.Curve.GetEndPoint(0);
                //}
                //else
                //{
                    BoundingBoxXYZ box = element.InternalElement.get_BoundingBox(revitView);
                    if (box == null) box = element.InternalElement.get_BoundingBox(null);
                    if (box != null)
                    {
                        double Y, X = 0;

                        switch (alignVertical)
                        {
                            case VerticalAlignmentStyle.Bottom: Y = box.Min.Y; break;
                            case VerticalAlignmentStyle.Top: Y = box.Max.Y; break;
                            default: Y = box.Min.Y + ((box.Max.Y - box.Min.Y) / 2); break;                     
                        }

                        switch (alignHorizontal)
                        {
                            case HorizontalAlignmentStyle.Left: X = box.Min.X; break;
                            case HorizontalAlignmentStyle.Right: X = box.Max.X; break;
                            default: X = box.Min.X + ((box.Max.X - box.Min.X) / 2); break;
                        }

                        point = new XYZ(X + offset.X, Y + offset.Y, 0 + offset.Z);
                    }
                    else throw new ArgumentNullException("Cannot determine location");
                //}


                return new Tag(revitView, element.InternalElement, orientation, tagMode, addLeader, point);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a Rebar from an existing reference
        /// </summary>
        /// <param name="rebar"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static Tag FromExisting(Autodesk.Revit.DB.IndependentTag tag, bool isRevitOwned)
        {
            return new Tag(tag)
            {
                // Cannot access base classes internal bool IsRevitOwned
                //IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }

}
