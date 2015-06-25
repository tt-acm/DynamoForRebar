//
// Copyright 2015 Autodesk, Inc.
// Author: Thornton Tomasetti Ltd, CORE Studio
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DSRevitNodesUI;
using RVT = Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;
using DSCoreNodesUI;
using Dynamo.Utilities;
using Dynamo.Models;

using RevitServices.Persistence;

using Category = Revit.Elements.Category;
using Element = Autodesk.Revit.DB.Element;
using Family = Autodesk.Revit.DB.Family;
using FamilyInstance = Autodesk.Revit.DB.FamilyInstance;
using FamilySymbol = Autodesk.Revit.DB.FamilySymbol;
using Level = Autodesk.Revit.DB.Level;
using Parameter = Autodesk.Revit.DB.Parameter;

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
    /// <param name="coverParameter">Offset</param>
    /// <returns>List of morphed bars</returns>
    public static List<Curve> MorphedRebar(Curve fromCurve, Curve toCurve, int numberOfBars, double coverParameter = 0)
    {
        return fromCurve.MorphTo(toCurve, numberOfBars, 20, coverParameter);
    }

    /// <summary>
    /// Create Rebar perpendicular to Face
    /// </summary>
    /// <param name="face">Face to Rebar</param>
    /// <param name="boundary">Boundary Faces</param>
    /// <param name="height">Height Parameter</param>
    /// <param name="numberOfBars">Number of Bars to create</param>
    /// <param name="horizontal">Horizontal or Vertical</param>
    /// <param name="coverParameter">Offset</param>
    /// <returns>List of perpendicular bars</returns>
    public static List<Curve> PerpendicularRebar(Surface face, List<Surface> boundary, double height, int numberOfBars, bool horizontal = true, double coverParameter = 0)
    {
        return face.NormalCurves(boundary, numberOfBars, coverParameter, height, horizontal);
    }

    /// <summary>
    /// Create Rebar following a selected surface
    /// </summary>
    /// <param name="face">Surface to rebar</param>
    /// <param name="numberOfBars">Define number of bars or distance between bars</param>
    /// <param name="distanceBetweenRebar">Define distance between bars or number of bars</param>
    /// <param name="flip">Flip orientation</param>
    /// <param name="coverParameter">Rebar cover parameter</param>
    /// <returns>List of rebar</returns>
    public static List<Curve> FollowingSurface(Surface face, int numberOfBars, double distanceBetweenRebar = 0, bool flip = true, double coverParameter = 0)
    {
       return face.Follow(20, coverParameter, distanceBetweenRebar, numberOfBars, flip);
    }




    /// <summary>
    /// Get all available Rebar Hook Types
    /// </summary>
    //[NodeName("Get Rebar Hook Types")]
    //[NodeCategory("MyCat")]
    //[NodeDescription("GetFamilyParameterDescription")]
    //[IsDesignScriptCompatible]
    //public class RevitRebarHookType : RevitDropDownBase
    //{
    //    private const string noTypes = "No Types available.";

    //    public RevitRebarHookType() : base("Rebar Hook Type") { }

    //    public override void PopulateItems()
    //    {
    //        Items.Clear();

    //        var fec = new RVT.FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
    //        fec.OfClass(typeof(Autodesk.Revit.DB.Structure.RebarHookType));

    //        if (fec.ToElements().Count == 0)
    //        {
    //            Items.Add(new DSCoreNodesUI.DynamoDropDownItem(noTypes, null));
    //            SelectedIndex = 0;
    //            return;
    //        }

    //        foreach (var ft in fec.ToElements())
    //        {
    //            Items.Add(new DSCoreNodesUI.DynamoDropDownItem(ft.Name, ft));
    //        }

    //        Items = Items.OrderBy(x => x.Name).ToObservableCollection();
    //    }

    //    public override IEnumerable<ProtoCore.AST.AssociativeAST.AssociativeNode> BuildOutputAst(List<ProtoCore.AST.AssociativeAST.AssociativeNode> inputAstNodes)
    //    {
    //        if (Items.Count == 0 ||
    //            Items[0].Name == noTypes ||
    //            SelectedIndex == -1)
    //        {
    //            return new[] { ProtoCore.AST.AssociativeAST.AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), ProtoCore.AST.AssociativeAST.AstFactory.BuildNullNode()) };
    //        }

    //        var args = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>
    //        {
    //            ProtoCore.AST.AssociativeAST.AstFactory.BuildStringNode(((Autodesk.Revit.DB.FloorType) Items[SelectedIndex].Item).Name)
    //        };

    //        var functionCall = ProtoCore.AST.AssociativeAST.AstFactory.BuildFunctionCall
    //            <System.String, Revit.Elements.FloorType>
    //            (Revit.Elements.FloorType.ByName, args);

    //        return new[] { ProtoCore.AST.AssociativeAST.AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
    //    }
    //}

    ///// <summary>
    ///// Get all available Rebar Bar Types
    ///// </summary>
    //public class RebarBarType : RevitDropDownBase
    //{
    //    private const string noTypes = "No Types available.";

    //    public RebarBarType() : base("Rebar Bar Type") { }

    //    public override void PopulateItems()
    //    {
    //        Items.Clear();

    //        var fec = new RVT.FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
    //        fec.OfClass(typeof(Autodesk.Revit.DB.Structure.RebarBarType));

    //        if (fec.ToElements().Count == 0)
    //        {
    //            Items.Add(new DSCoreNodesUI.DynamoDropDownItem(noTypes, null));
    //            SelectedIndex = 0;
    //            return;
    //        }

    //        foreach (var ft in fec.ToElements())
    //        {
    //            Items.Add(new DSCoreNodesUI.DynamoDropDownItem(ft.Name, ft));
    //        }

    //        // Items = Items.OrderBy(x => x.Name).ToObservableCollection();
    //    }

    //    public override IEnumerable<ProtoCore.AST.AssociativeAST.AssociativeNode> BuildOutputAst(List<ProtoCore.AST.AssociativeAST.AssociativeNode> inputAstNodes)
    //    {
    //        if (Items.Count == 0 ||
    //            Items[0].Name == noTypes ||
    //            SelectedIndex == -1)
    //        {
    //            return new[] { ProtoCore.AST.AssociativeAST.AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), ProtoCore.AST.AssociativeAST.AstFactory.BuildNullNode()) };
    //        }

    //        var args = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>
    //        {
    //            ProtoCore.AST.AssociativeAST.AstFactory.BuildStringNode(((Autodesk.Revit.DB.FloorType) Items[SelectedIndex].Item).Name)
    //        };

    //        var functionCall = ProtoCore.AST.AssociativeAST.AstFactory.BuildFunctionCall
    //            <System.String, Revit.Elements.FloorType>
    //            (Revit.Elements.FloorType.ByName, args);

    //        return new[] { ProtoCore.AST.AssociativeAST.AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
    //    }
    //}

    ///// <summary>
    ///// Get all available Rebar Hook Orientations
    ///// </summary>
    //public class RebarHookOrientation : RevitDropDownBase
    //{
    //    public RebarHookOrientation() : base("Rebar Hook Orientation") { }

    //    public override void PopulateItems()
    //    {
    //        Items.Clear();

    //        foreach (string name in Enum.GetNames(typeof(Autodesk.Revit.DB.Structure.RebarHookOrientation)))
    //        {
    //            Items.Add(new DSCoreNodesUI.DynamoDropDownItem(name, Enum.Parse(typeof(Autodesk.Revit.DB.Structure.RebarHookOrientation), name)));
    //        }

    //        // Items = Items.OrderBy(x => x.Name).ToObservableCollection();
    //    }

    //    public override IEnumerable<ProtoCore.AST.AssociativeAST.AssociativeNode> BuildOutputAst(List<ProtoCore.AST.AssociativeAST.AssociativeNode> inputAstNodes)
    //    {
    //        if (Items.Count == 0 ||
    //            SelectedIndex == -1)
    //        {
    //            return new[] { ProtoCore.AST.AssociativeAST.AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), ProtoCore.AST.AssociativeAST.AstFactory.BuildNullNode()) };
    //        }

    //        var args = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>
    //        {
    //            ProtoCore.AST.AssociativeAST.AstFactory.BuildStringNode(((Autodesk.Revit.DB.FloorType) Items[SelectedIndex].Item).Name)
    //        };

    //        var functionCall = ProtoCore.AST.AssociativeAST.AstFactory.BuildFunctionCall
    //            <System.String, Revit.Elements.FloorType>
    //            (Revit.Elements.FloorType.ByName, args);

    //        return new[] { ProtoCore.AST.AssociativeAST.AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
    //    }
    //}

    ///// <summary>
    ///// Get all available Rebar Styles
    ///// </summary>
    //public class RebarStyle : RevitDropDownBase
    //{
    //    public RebarStyle() : base("Rebar Style") { }

    //    public override void PopulateItems()
    //    {
    //        Items.Clear();

    //        foreach (string name in Enum.GetNames(typeof(Autodesk.Revit.DB.Structure.RebarStyle)))
    //        {
    //            Items.Add(new DSCoreNodesUI.DynamoDropDownItem(name, Enum.Parse(typeof(Autodesk.Revit.DB.Structure.RebarStyle), name)));
    //        }

    //        //Items = Items.OrderBy(x => x.Name).ToObservableCollection();
    //    }

    //    public override IEnumerable<ProtoCore.AST.AssociativeAST.AssociativeNode> BuildOutputAst(List<ProtoCore.AST.AssociativeAST.AssociativeNode> inputAstNodes)
    //    {
    //        if (Items.Count == 0 ||
    //            SelectedIndex == -1)
    //        {
    //            return new[] { ProtoCore.AST.AssociativeAST.AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), ProtoCore.AST.AssociativeAST.AstFactory.BuildNullNode()) };
    //        }

    //        var args = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>
    //        {
    //            ProtoCore.AST.AssociativeAST.AstFactory.BuildStringNode(((Autodesk.Revit.DB.FloorType) Items[SelectedIndex].Item).Name)
    //        };

    //        var functionCall = ProtoCore.AST.AssociativeAST.AstFactory.BuildFunctionCall
    //            <System.String, Revit.Elements.FloorType>
    //            (Revit.Elements.FloorType.ByName, args);

    //        return new[] { ProtoCore.AST.AssociativeAST.AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
    //    }
    //}
}
