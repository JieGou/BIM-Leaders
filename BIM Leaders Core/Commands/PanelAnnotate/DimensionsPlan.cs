using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using System.Collections;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class DimensionsPlan : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            // Get Application
            Application uiapp = doc.Application;

            //double toleranceAngle = uiapp.AngleTolerance / 100; // 0.001 grad

            Options opts = new Options()
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = true,
                View = doc.ActiveView
            };

            try
            {
                // INPUT
                double offsetDistanceInternal = 30;
#if VERSION2020
                double offset_distance = UnitUtils.ConvertFromInternalUnits(offsetDistanceInternal, DisplayUnitType.DUT_CENTIMETERS);
#else
                double offset_distance = UnitUtils.ConvertFromInternalUnits(offsetDistanceInternal, UnitTypeId.Centimeters);
#endif
                bool orientationIsExterior = true;
                bool multiDim = false;

                // If the wall is exterior we need to extend the intersect line beyond the exterior face to pick up the intersecting walls
                // if the wall is interior, we don't want to extend as far
                double intersectLineEndExtend = (orientationIsExterior == true)
                    ? 500
                    : 0;

                // User Input
                Reference reference = uidoc.Selection.PickObject(ObjectType.Element, new SelectionFilterByCategory("Walls"), "Select a Wall");
                // Define Target Wall
                Wall targetWall = doc.GetElement(reference) as Wall;

                // Let's go get the walls for finding references
                List<Wall> intersectedWalls = new List<Wall>();
                // Start by adding our target wall
                intersectedWalls.Add(targetWall);
                // Then get all the other walls in the view and test if they intersect the Target Wall
                IList<Element> collectedWalls = new FilteredElementCollector(doc, doc.ActiveView.Id)
                    .OfClass(typeof(Wall))
                    .ToElements();
                foreach (Wall wall in collectedWalls)
                {
                    if (wall.Location is LocationCurve)
                    {
                        LocationCurve lc = wall.Location as LocationCurve;
                        LocationCurve lcTartet = targetWall.Location as LocationCurve;
                        if (lcTartet.Curve.Intersect(lc.Curve) == SetComparisonResult.Overlap)
                            intersectedWalls.Add(wall);
                    }
                    
                }

                // Target Wall external line for intersect check
                Line exLi = MoveLineToWallEdge(doc, targetWall, orientationIsExterior);
                // Extend the line
                if (intersectLineEndExtend > 0)
                    exLi = ExtendLine(exLi, intersectLineEndExtend);
                // Curve where the dimension will be located
                offCrv = geom.Geometry.Translate(exLi.ToProtoType(), wallNormal(targetWall, orientationIsExterior), (offset_distance));

                // Lets get the wall edges we want
                // Only get edges intersecting target side? no this is misleading... we want any reference hitting our external wall
                // The problem is actually that the face itsetlf is registering the intersections we don't want e.g. the wrapping ends
                List<Face> frontFaceIW = new List<Face>();
                List<Edge> vertEdges = new List<Edge>();

                foreach (Wall wallInt in intersectedWalls)
                {
                    foreach (Solid s in wallInt.get_Geometry(opts))
                    {
                        foreach (Face face in s.Faces)
                        {
                            // If face is normal is equal to wall normal it is the external face
                            if (isAlmostEqualTo(wallInt.Orientation, face.ComputeNormal(new UV(0.5, 0.5))))
                                frontFaceIW.Add(face);
                        }
                        foreach (Edge edge in s.Edges)
                        {
                            // Get edges which intersect
                            Curve edgeC = edge.AsCurve();
                            if (edgeC is Line)
                            {
                                Line lineC = edgeC as Line;
                                XYZ edgeCNorm = lineC.Direction.Normalize();
                                // If front face edge and edge intersects line and edge is vertical up or vertical down add to list
                                if (edgeC.Intersect(exLi) != SetComparisonResult.Disjoint && (edgeCNorm.IsAlmostEqualTo(new XYZ(0, 0, 1)) || edgeCNorm.IsAlmostEqualTo(new XYZ(0, 0, -1))))
                                    vertEdges.Add(edge);
                            }
                        }
                    }
                }

                // So we use the X+Y values as a unique identifier of location
                // (we're less interested in the actual unique reference, there may be 2 in the same place)
                // We will use these as filtering and sorting values if we wanted to use this on sections we'd want to use z value?
                List<double> vertEdgesLoc = new List<double>();
                foreach (Edge v in vertEdges)
                {
                    double vLoc = v.AsCurve().GetEndPoint(0).X + v.AsCurve().GetEndPoint(0).Y;
                    // Getting some revit rounding errors, 7dp should be enough!
                    vertEdgesLoc.Add(Math.Round(vLoc, 7));
                }

                // Trying to remove stray intersect edges from adjoining walls to identify them,
                // they are not on an intersecting wall front face
                // Their faces are not both on the target wall so we need all the target wall faces
                FaceArray faceTW;
                foreach (Solid s in targetWall.get_Geometry(opts))
                {
                    faceTW = s.Faces;
                }

                // Create a holding list containing everything in vertEdges
                List<Edge> strayEdges = new List<Edge>();
                foreach (Edge v in vertEdges)
                {
                    strayEdges.Add(v);
                }

                // Start removing things from holding list to leave only the stray ones
                foreach (Face faIW in frontFaceIW)
                {
                    // If edge face 0&1 are both target wall faces we don't want to remove them
                    int i = 0;
                    int length = strayEdges.Count;
                    
                    while (i < length)
                    {

                        if (strayEdges[i].GetFace(0) in faceTW.Cast<List<Face>>)
                        {
                            strayEdges.Remove(strayEdges[i]);
                            length--;
                        }
                        else if (strayEdges[i].GetFace(1) in faceTW)
                        {
                            strayEdges.Remove(strayEdges[i]);
                            length--;
                        }
                        // If our wall is external.... 
                        // If edge face is an intersecting wall's front face, we don't want it
                        else if (strayEdges[i].GetFace(0) == faIW && orientationIsExterior == true)
                        {
                            strayEdges.Remove(strayEdges[i]);
                            length--;
                        }
                        else if (strayEdges[i].GetFace(1) == faIW && orientationIsExterior == true)
                        {
                            strayEdges.Remove(strayEdges[i]);
                            length--;
                            //strayEdges.Remove(ed);
                            // Or if the edge reference is to a non-wall
                            continue;
                        }
                        i++;
                    }
                }
                
                // If the wall is exterior, we want to remove references to internal wall edge
                if (orientationIsExterior == true)
                {
                    int i = 0;
                    int length = vertEdgesLoc.Count;

                    while (i < length)
                    {
                        foreach (Edge stray in strayEdges)
                        {
                            double stLoc = stray.AsCurve().GetEndPoint(0).X + stray.AsCurve().GetEndPoint(0).Y;
                            // Getting eroneous values, Revit accuracy not good enough? round is built in method
                            if (Math.Round(vertEdgesLoc[i], 7) == Math.Round(stLoc, 7))
                            {
                                vertEdges.Remove(vertEdges[i]);
                                vertEdgesLoc.Remove(vertEdgesLoc[i]);
                                length--;
                                continue;
                            }
                        }
                        i++;
                    }
        
                }

                // Sort the edges using the combined XY location value
                List<Edge> vertEdgesSorted = vertEdges.OrderBy(x => vertEdgesLoc.IndexOf(x.Id)).ToList();

                // Only add uniquely located references
                // This is awkward because we test 1 list, then add to the other list
                // We need the Temp list, so we know what should be added to the Sub list

                vertEdgeUniLocTemp = [];
                ReferenceArray vertEdgeSub = new ReferenceArray();
                for (eL, e in zip(vertEdgesLocSorted, vertEdgesSorted))
                {
                    if (eL not in vertEdgeUniLocTemp)
                    {
                        vertEdgeUniLocTemp.append(eL);
                        vertEdgeSub.Add(e.Reference);
                    }
                    
                }
// We want to pair up the references to create unique dims for the brick dim checker
// Convoluted code, ref arrays seem their own beast!
outRefs = []
// Define the overall list length
for i in range(vertEdgeSub.Size-1):
// Create the array here so the list nesting is correct
    vertEdgeAr = ReferenceArray()
// Define the sub list length
    while vertEdgeAr.Size < 2:
// Only get add 2 indices for each sub list
        vertEdgeAr.Append(vertEdgeSub[i])
        vertEdgeAr.Append(vertEdgeSub[i+1])
    outRefs.append(vertEdgeAr)




                using (Transaction trans = new Transaction(doc, "Dimension Plan Walls"))
                {
                    trans.Start();

                // Create dimensions for each pair of referenes
                if (multiDim == true)
                    for reference in outRefs:    

                        dim = doc.Create.NewDimension(doc.ActiveView, offCrv.ToRevitType(), reference)
else
                            dim = doc.Create.NewDimension(doc.ActiveView, offCrv.ToRevitType(), vertEdgeSub)

                    trans.Commit();
                }

                // Show result
                string text = (count == 0)
                    ? "Dimension creating error."
                    : $"Dimension with {count} segments was created.";
                TaskDialog.Show("Dimension Plan Walls", text);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Get list of straight walls visible on active view.
        /// </summary>
        /// <returns>List of straight walls visible on active view.</returns>
        private static List<Wall> GetWallsStraight(Document doc)
        {
            IEnumerable<Wall> wallsAll = new FilteredElementCollector(doc, doc.ActiveView.Id)
                    .OfClass(typeof(Wall))
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<Wall>();

            // Filter line walls
            List<Wall> walls = new List<Wall>();
            foreach (Wall wall in wallsAll)
            {
                if (wall.Location is LocationCurve lc)
                    if (lc.Curve.GetType() == typeof(Line))
                        walls.Add(wall);
            }
            return walls;
        }

        /// <summary>
        /// Filter list of walls to get walls only perpendicular to the given line with the given angle tolerance.
        /// </summary>
        /// <returns>List of walls perpendicular to the line.</returns>
        private static List<Wall> FilterWallsPer(double toleranceAngle, Line line, IEnumerable<Wall> walls)
        {
            List<Wall> wallsPer = new List<Wall>();

            // Get direction of reference plane
            double lineX = Math.Abs(line.Direction.X);
            double lineY = Math.Abs(line.Direction.Y);

            foreach (Wall wall in walls)
            {
                double wallX = Math.Abs(wall.Orientation.X);
                double wallY = Math.Abs(wall.Orientation.Y);

                if ((Math.Abs(wallX) - Math.Abs(lineX) <= toleranceAngle) && (Math.Abs(wallY) - Math.Abs(lineY) <= toleranceAngle))
                    wallsPer.Add(wall);
            }
            return wallsPer;
        }

        /// <summary>
        /// Get columns visible on active view.
        /// </summary>
        /// <returns>List of columns visible on active view.</returns>
        private static List<FamilyInstance> GetColumns(Document doc)
        {
            List<FamilyInstance> columns = new List<FamilyInstance>();

            // Get all structural columns on the view
            List<FamilyInstance> columns_str = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .ToElements()
                .Cast<FamilyInstance>()
                .ToList();
            // Get all columns on the view
            List<FamilyInstance> columns_arc = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Columns)
                .ToElements()
                .Cast<FamilyInstance>()
                .ToList();

            columns.AddRange(columns_str);
            columns.AddRange(columns_arc);

            return columns;
        }

        /// <summary>
        /// Filter list of columns to get columns only parallel or perpendicular to the given line with the given angle tolerance.
        /// </summary>
        /// <returns>List of columns parallel or perpendicular to the line.</returns>
        private static List<FamilyInstance> FilterColumnsPer(double toleranceAngle, Line line, IEnumerable<FamilyInstance> columns)
        {
            List<FamilyInstance> columns_per = new List<FamilyInstance>();

            foreach (FamilyInstance i in columns)
            {
                LocationPoint lp = i.Location as LocationPoint;

                double col_ox = Math.Cos(lp.Rotation);
                double col_oy = Math.Sin(lp.Rotation);
                XYZ col_xy = new XYZ(col_ox, col_oy, 0);

                double col_angle = col_xy.AngleTo(line.Direction);

                // Checking if parallel
                if (Math.Abs(col_angle) <= toleranceAngle)
                    columns_per.Add(i);
                else if (Math.Abs(col_angle - Math.PI) <= toleranceAngle)
                    columns_per.Add(i);
                // Checking if parallel
                else if (Math.Abs(col_angle - Math.PI / 2) <= toleranceAngle)
                    columns_per.Add(i);
            }
            return columns_per;
        }

        private static List<Reference> FindIntersections(Document doc, Line curve, IEnumerable<Element> elements)
        {
            List<Reference> intersections = new List<Reference>();

            Options opts = new Options()
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = false,
                View = doc.ActiveView
            };

            foreach (Element e in elements)
            {
                foreach (Solid s in e.get_Geometry(opts))
                {
                    FaceArray faces = s.Faces;
                    foreach (PlanarFace face in faces)
                    {
                        // Check if faces are vertical
                        if (Math.Round(face.FaceNormal.Z) == 0)
                        {
                            SetComparisonResult intersection = face.Intersect(curve);
                            if (intersection == SetComparisonResult.Overlap)
                                intersections.Add(face.Reference);
                        }
                    }
                }
            }
            return intersections;
        }

        private static bool isParallel(XYZ v1, XYZ v2)
        {
            return v1.CrossProduct(v2).IsAlmostEqualTo(new XYZ(0, 0, 0));
        }
        
        private static bool isAlmostEqualTo(XYZ v1, XYZ v2)
        {
            return v1.IsAlmostEqualTo(v2);
        }

        private static bool isPerpendicular(XYZ v1, XYZ v2)
        {
            if (v1.DotProduct(v2) == 0)
                return true;
            else
                return false;
        }
        private static XYZ CurveToVector(Curve crv)
        {
            Line line = Line.CreateBound(crv.GetEndPoint(0), crv.GetEndPoint(1));
            XYZ vec = line.Direction;
            return vec;
        }

        /// <summary>
        /// Get the wall orientation, depending on whether exterior or interior is selected.
        /// </summary>
        /// <returns>Normal vector of the wall</returns>
        private static XYZ GetWallNormal(Wall wall, bool orientationIsExterior)
        {
            XYZ wallNormal = (orientationIsExterior == true)
                ? wall.Orientation
                : wall.Orientation.Negate();
            return wallNormal;
        }

        /// <summary>
        /// Takes the wall location line, shifts it up the to the view cut plane and moves it out to the external edge of the wall.
        /// </summary>
        /// <returns>Line on the exterior edge of the wall on current plan view height.</returns>
        private static Line MoveLineToWallEdge(Document doc, Wall wall, bool orientationIsExterior)
        {
            if (!(wall.Location is LocationCurve))
                return null;
            LocationCurve lc = wall.Location as LocationCurve;
            Curve wallCurve = lc.Curve;
            if (!(wallCurve is Line))
                return null;
            Line wallLine = wallCurve as Line;

            ViewPlan viewPlan = doc.ActiveView as ViewPlan;
            // Take the level height, subtract the height of the wall base
            double zOf = viewPlan.GenLevel.Elevation - wallLine.GetEndPoint(0).Z;
            // Get the cut plane offset of the view
            double planOffset = viewPlan.GetViewRange().GetOffset(PlanViewPlane.CutPlane); // The CutPlane enumeration is like a property of PBP?

            Transform transformUp = Transform.CreateTranslation(new XYZ(0, 0, zOf + planOffset));
            Transform transformWidth = Transform.CreateTranslation(GetWallNormal(wall, orientationIsExterior) * wall.Width / 2);

            Line wallLineUp = (Line)wallLine.CreateTransformed(transformUp);
            Line wallLineTr = (Line)wallLineUp.CreateTransformed(transformWidth);

            return wallLineTr;
        }

        /// <summary>
        /// Extend given line to both sides via given distance.
        /// </summary>
        /// <returns>Extended line.</returns>
        private static Line ExtendLine(Line line, double distance)
        {
            if (!line.IsBound)
                return line;

            // Vectors from line center to ends
            XYZ lineCenter = line.Evaluate(0.5, true);
            XYZ delta0 = line.GetEndPoint(0) - lineCenter;
            XYZ delta1 = line.GetEndPoint(1) - lineCenter;

            // Unit vectors from line center to ends
            XYZ delta0N = delta0.Normalize() * distance;
            XYZ delta1N = delta1.Normalize() * distance;

            // Needed length vectors from line center to ends
            XYZ p0 = line.GetEndPoint(0) + delta0N;
            XYZ p1 = line.GetEndPoint(1) + delta1N;

            Line lineExtended = Line.CreateBound(p0, p1);

            return lineExtended;
        }
    
        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionsPlan).Namespace + "." + nameof(DimensionsPlan);
        }
    }
}
