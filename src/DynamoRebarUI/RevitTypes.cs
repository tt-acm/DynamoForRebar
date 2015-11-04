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
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace DynamoRebarUI
{

    [NodeName("Rebar Hook Type")]
    [NodeCategory("Revit.Rebar")]
    [NodeDescription("Select Rebar Hook Type")]
    [IsDesignScriptCompatible]
    public class RevitRebarHookType : CustomRevitElementDropDown
    {
        public RevitRebarHookType() : base("Rebar Hook Type", typeof(Autodesk.Revit.DB.Structure.RebarHookType)) { }
    }

    [NodeName("Rebar Bar Type")]
    [NodeCategory("Revit.Rebar")]
    [NodeDescription("Select Rebar Bar Type")]
    [IsDesignScriptCompatible]
    public class RebarBarType : CustomRevitElementDropDown
    {
        public RebarBarType() : base("Rebar Bar Type", typeof(Autodesk.Revit.DB.Structure.RebarBarType)) { }
    }


    [NodeName("Rebar Hook Orientation")]
    [NodeCategory("Revit.Rebar")]
    [NodeDescription("Select Rebar Hook Orientation")]
    [IsDesignScriptCompatible]
    public class RebarHookOrientation : CustomGenericEnumerationDropDown
    {
        public RebarHookOrientation() : base("Rebar Hook Orientation", typeof(Autodesk.Revit.DB.Structure.RebarHookOrientation)) { }
    }


    [NodeName("Rebar Style")]
    [NodeCategory("Revit.Rebar")]
    [NodeDescription("Select Rebar Style")]
    [IsDesignScriptCompatible]
    public class RebarStyle : CustomGenericEnumerationDropDown
    {
        public RebarStyle() : base("Rebar Style", typeof(Autodesk.Revit.DB.Structure.RebarStyle)) { }
    }


    [NodeName("Horizontal Text Alignment Style")]
    [NodeCategory("Revit.Rebar")]
    [NodeDescription("Horizontal Text Alignment Style")]
    [IsDesignScriptCompatible]
    public class HorizontalAlignment : CustomGenericEnumerationDropDown
    {
        public HorizontalAlignment() : base("Horizontal Alignment", typeof(Autodesk.Revit.DB.HorizontalAlignmentStyle)) { }
    }

    [NodeName("Vertical Text Alignment Style")]
    [NodeCategory("Revit.Rebar")]
    [NodeDescription("Vertical Text Alignment Style")]
    [IsDesignScriptCompatible]
    public class VerticalAlignment : CustomGenericEnumerationDropDown
    {
        public VerticalAlignment() : base("Vertical Alignment", typeof(Autodesk.Revit.DB.VerticalAlignmentStyle)) { }
    }

}