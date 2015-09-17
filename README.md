# DynamoForRebar
A Dynamo package for authoring geometrically complex rebar models in Revit 2016.

Dynamo for Rebar is an Open-Source project available on github and [Dynamo’s package manager](http://dynamobim.org/). The library contains a set of nodes helping you to create bars and containers in Revit, and provides a set of nodes for creating the base curvature of single bars or entire rebar containers.

The project is being developed in C# using Visual Studio, and will work with Dynamo 0.8.0, and Revit 2016.  The project consists of two libraries; one is a [zero-touch library](https://github.com/DynamoDS/Dynamo/wiki/Zero-Touch-Plugin-Development) containing most of the nodes, the other is a UI library containing a few nodes with dropdown elements.  

###Rebar Nodes
The nodes in this group are specific to the [Revit](http://www.autodesk.com/products/revit-family/overview) 2016 Rebar API. They are the core nodes in the package that allow for parametric rebar design in Dynamo. The utility nodes and nodes for curve generation (outlined below) are designed to work well with these rebar nodes.

####Create Rebar 
Creates one single bar element in Revit from a curve and and a series of rebar properties.

####Create Rebar Container 
Creates a rebar container element from a list of curves and a series of rebar properties.  The use of containers is highly encouraged as Revit can get bogged down by thousands of rebar family instances in your model.  Containers are like groups of rebars in a single family instance. 

####Rebar Property Dropdown Nodes
Select Rebar Style - Select available Rebar Styles from the Revit document
Select Rebar Hook Type - Select available Rebar Hook Types from the Revit document
Select Rebar Hook Orientation - Select available Rebar Hook Orientations from the Revit document
Select Rebar Bar Type - Select available Rebar Bar Types from the Revit document

###Nodes for Curve Generation
The nodes in this package for creating curves are powerful tools on their own; they allow the user to parameterize any surface in Dynamo, and create curves along it for any use downstream.  Of course one good downstream use is the creation of rebar containers, but it’s up to you!

####Curves following a surface
This node creates a set of curves following the geometry of a selected surface (most polysurfaces will also work). It divides the surface in one dimension - either U or V - regularly. You can define the number of divisions (or optionally, a distance to divide the surface by), and the direction of the curves.
 
####Curves morphing between two curves
This node creates a set of morphed curves between two border curves. It requires two curves to blend between, and creates either a fixed number of curves between them or divides by a defined distance.

####Curves perpendicular to one surface
This node creates a set of  linear curves normal to a surface. It requires the selection of a driving surface and a set of bounding faces to define the end of the projection. According to a selected height, the node will divide the surface along this height into a selected number points. It will then draw lines along the normals at this points, break the line at any obstacle and continue until the bounding surfaces.

###Utility Nodes
These nodes in this group are mostly designed for use downstream of the rebar nodes.  

####Cut Rebar Container by Plane
The cut rebar node cuts a selected rebar container at a selected surface. The result will be either the left or the right side of the division.

####Shorten Curve from both ends
This node shortens a selected curve from both ends by the same distance.

####Tag (any) Revit Element
The tag element node creates a tag of any taggable revit element in the current Revit view. It requires a revit element as an input and if the tag should be horizontal or vertical or having a leader or not. 

####Select Nodes
This set of nodes also comes with a very generic one: A node to select multiple edges. This allows you to select any number of edges from your Revit model and use them in Dynamo to create bars or even place adaptive components along them (see Image).

Dynamo For Rebar is developed and maintained by [Thornton Tomasetti](http://www.thorntontomasetti.com/)’s [CORE studio](http://core.thorntontomasetti.com/).  The main developers are:
- [Maximilian Thumfart](https://github.com/moethu)
