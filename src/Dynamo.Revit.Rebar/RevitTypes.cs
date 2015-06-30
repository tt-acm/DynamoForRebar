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
using ProtoCore.AST.AssociativeAST;



[NodeName("Rebar Hook Type")]
[NodeCategory("Revit.Rebar")]
[NodeDescription("Select Rebar Hook Type")]
[IsDesignScriptCompatible]
public class RevitRebarHookType : RevitDropDownBase
{
    private const string noTypes = "No Types available.";

    public RevitRebarHookType() : base("Rebar Hook Type") { }


    public override void PopulateItems()
    {
        Items.Clear();

        var fec = new RVT.FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
        fec.OfClass(typeof(Autodesk.Revit.DB.Structure.RebarHookType));

        if (fec.ToElements().Count == 0)
        {
            Items.Add(new DSCoreNodesUI.DynamoDropDownItem(noTypes, null));
            SelectedIndex = 0;
            return;
        }

        foreach (var ft in fec.ToElements())
        {
            Items.Add(new DSCoreNodesUI.DynamoDropDownItem(ft.Name, ft));
        }

        Items = Items.OrderBy(x => x.Name).ToObservableCollection();
    }


    public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
    {
        if (Items.Count == 0 ||
            SelectedIndex == -1)
        {
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
        }

        var node = AstFactory.BuildFunctionCall(
            "Revit.Elements.ElementSelector",
            "ByElementId",
            new List<AssociativeNode>
                {
                    AstFactory.BuildIntNode(((Autodesk.Revit.DB.Structure.RebarHookType)Items[SelectedIndex].Item).Id.IntegerValue)
                });

        return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
    }



}

[NodeName("Rebar Bar Type")]
[NodeCategory("Revit.Rebar")]
[NodeDescription("Select Rebar Bar Type")]
[IsDesignScriptCompatible]
public class RebarBarType : RevitDropDownBase
{
    private const string noTypes = "No Types available.";

    public RebarBarType() : base("Rebar Bar Type") { }

    public override void PopulateItems()
    {
        Items.Clear();

        var fec = new RVT.FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
        fec.OfClass(typeof(Autodesk.Revit.DB.Structure.RebarBarType));

        if (fec.ToElements().Count == 0)
        {
            Items.Add(new DSCoreNodesUI.DynamoDropDownItem(noTypes, null));
            SelectedIndex = 0;
            return;
        }

        foreach (var ft in fec.ToElements())
        {
            Items.Add(new DSCoreNodesUI.DynamoDropDownItem(ft.Name, ft));
        }

        Items = Items.OrderBy(x => x.Name).ToObservableCollection();
    }

    public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
    {
        if (Items.Count == 0 ||
            SelectedIndex == -1)
        {
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
        }

        var node = AstFactory.BuildFunctionCall(
            "Revit.Elements.ElementSelector",
            "ByElementId",
            new List<AssociativeNode>
                {
                    AstFactory.BuildIntNode(((Autodesk.Revit.DB.Structure.RebarBarType)Items[SelectedIndex].Item).Id.IntegerValue)
                });

        return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
    }
}


[NodeName("Rebar Hook Orientation")]
[NodeCategory("Revit.Rebar")]
[NodeDescription("Select Rebar Hook Orientation")]
[IsDesignScriptCompatible]
public class RebarHookOrientation : RevitDropDownBase
{
    public RebarHookOrientation() : base("Rebar Hook Orientation") { }

    public override void PopulateItems()
    {
        Items.Clear();

        foreach (string name in Enum.GetNames(typeof(Autodesk.Revit.DB.Structure.RebarHookOrientation)))
        {
            Items.Add(new DSCoreNodesUI.DynamoDropDownItem(name, Enum.Parse(typeof(Autodesk.Revit.DB.Structure.RebarHookOrientation), name)));
        }

        Items = Items.OrderBy(x => x.Name).ToObservableCollection();
    }

    public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
    {
        if (Items.Count == 0 || Items.Count == -1)
        {
            PopulateItems();
        }

        var stringNode = AstFactory.BuildStringNode((string)Items[SelectedIndex].Name);
        var assign = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), stringNode);

        return new List<AssociativeNode> { assign };
    }
}


[NodeName("Rebar Style")]
[NodeCategory("Revit.Rebar")]
[NodeDescription("Select Rebar Style")]
[IsDesignScriptCompatible]
public class RebarStyle : RevitDropDownBase
{
    public RebarStyle() : base("Rebar Style") { }

    public override void PopulateItems()
    {
        Items.Clear();

        foreach (string name in Enum.GetNames(typeof(Autodesk.Revit.DB.Structure.RebarStyle)))
        {
            Items.Add(new DSCoreNodesUI.DynamoDropDownItem(name, Enum.Parse(typeof(Autodesk.Revit.DB.Structure.RebarStyle), name)));
        }

        Items = Items.OrderBy(x => x.Name).ToObservableCollection();
    }

    public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
    {
        if (Items.Count == 0 || Items.Count == -1)
        {
            PopulateItems();
        }

        var stringNode = AstFactory.BuildStringNode((string)Items[SelectedIndex].Name);
        var assign = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), stringNode);

        return new List<AssociativeNode> { assign };
    }
}


