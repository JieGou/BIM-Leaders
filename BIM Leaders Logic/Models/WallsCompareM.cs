﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class WallsCompareM : BaseModel
    {
        private int _countFilledRegions;

        #region PROPERTIES

        private bool _checkOneLink;
        public bool CheckOneLink
        {
            get { return _checkOneLink; }
            set
            {
                _checkOneLink = value;
                OnPropertyChanged(nameof(CheckOneLink));
            }
        }

        private int _materialsSelected;
        public int MaterialsSelected
        {
            get { return _materialsSelected; }
            set
            {
                _materialsSelected = value;
                OnPropertyChanged(nameof(MaterialsSelected));
            }
        }

        private int _fillTypesSelected;
        public int FillTypesSelected
        {
            get { return _fillTypesSelected; }
            set
            {
                _fillTypesSelected = value;
                OnPropertyChanged(nameof(FillTypesSelected));
            }
        }

        #endregion

        public WallsCompareM(
            ExternalCommandData commandData,
            string transactionName,
            Action<RunResult> showResultAction
            ) : base(commandData, transactionName, showResultAction) { }

        #region METHODS

        private protected override void TryExecute()
        {
            string materialName = _doc.GetElement(new ElementId(MaterialsSelected)).Name;
            FilledRegionType fill = _doc.GetElement(new ElementId(FillTypesSelected)) as FilledRegionType;

            double elevation = _doc.ActiveView.GenLevel.Elevation;

            // Links selection.
            List<Wall> walls1 = new List<Wall>();
            List<Wall> walls2 = new List<Wall>();
            Transform transform1 = _doc.ActiveView.CropBox.Transform; // Just new transform, view is dummy
            Transform transform2 = _doc.ActiveView.CropBox.Transform; // Just new transform, view is dummy

            // If only one file is a link
            if (CheckOneLink)
            {
                Reference linkReference = _uidoc.Selection.PickObject(ObjectType.Element, new SelectionFilterByCategory("RVT Links"), "Select Link");
                RevitLinkInstance link1 = _doc.GetElement(linkReference.ElementId) as RevitLinkInstance;

                walls1 = GetWalls(link1.GetLinkDocument(), elevation, materialName);
                walls2 = GetWalls(_doc, elevation, materialName);

                transform1 = link1.GetTotalTransform();
                transform2 = transform1; // Don't need this because its doc file itself
            }
            else
            {
                IList<Reference> linkReferences = _uidoc.Selection.PickObjects(ObjectType.Element, new SelectionFilterByCategory("RVT Links"), "Select 2 Links");
                Reference linkReference1 = linkReferences.First();
                Reference linkReference2 = linkReferences.Last();
                RevitLinkInstance link1 = _doc.GetElement(linkReference1.ElementId) as RevitLinkInstance;
                RevitLinkInstance link2 = _doc.GetElement(linkReference2.ElementId) as RevitLinkInstance;

                walls1 = GetWalls(link1.GetLinkDocument(), elevation, materialName);
                walls2 = GetWalls(link2.GetLinkDocument(), elevation, materialName);

                transform1 = link1.GetTotalTransform();
                transform2 = link2.GetTotalTransform();
            }

            // Getting plan loops of walls
            List<CurveLoop> loops1 = GetWallsLoops(walls1);
            List<CurveLoop> loops2 = GetWallsLoops(walls2);

            // Getting total plan loop of wall set
            Solid solid1 = GetWallsLoop(loops1);
            Solid solid2 = GetWallsLoop(loops2);

            // Transform solids
            Solid solid1Transformed = SolidUtils.CreateTransformed(solid1, transform1);
            Solid solid2Transformed = solid2;
            if (!CheckOneLink)
                solid2Transformed = SolidUtils.CreateTransformed(solid2, transform2);

            if (solid1Transformed.Volume == 0 || solid2Transformed.Volume == 0)
            {
                _result.Result = "No intersections found.";
                return;
            }

            // Get list of curveloops by intersecting two solids.
            List<CurveLoop> loopList = GetCurveLoops(solid1Transformed, solid2Transformed);

            // Drawing filled region
            using (Transaction trans = new Transaction(_doc, TransactionName))
            {
                trans.Start();

                FilledRegion region = FilledRegion.Create(_doc, fill.Id, _doc.ActiveView.Id, loopList);

                trans.Commit();
            }

            _result.Result = GetRunResult();
        }

        /// <summary>
        /// Get walls from document, that have level with given elevation and have material with given name.
        /// </summary>
        /// <param name="doc">Document (current Revit file or link).</param>
        /// <param name="elevation">Elevation of the needed walls, input the current plan view level elevation.</param>
        /// <param name="material">If wall will have material with given name, it will be filtered in.</param>
        /// <returns>A list of walls from the document.</returns>
        private List<Wall> GetWalls(Document doc, double elevation, string material)
        {
            List<Wall> walls = new List<Wall>();

            // Selecting all walls from doc
            IEnumerable<Wall> wallsAll = new FilteredElementCollector(doc)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .Cast<Wall>(); //LINQ function

            // Selecting all levels from doc
            IEnumerable<Level> levelsAll = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .WhereElementIsNotElementType()
                .Cast<Level>(); //LINQ function

            // Getting closest level in the document
            double level0 = levelsAll.First().Elevation;
            foreach (Level level in levelsAll)
                if (Math.Abs(level.ProjectElevation - elevation) < 1)
                    level0 = level.ProjectElevation;

            // Filtering needed walls for materials and height
            foreach (Wall wall in wallsAll)
            {
                CompoundStructure wallStructure = wall.WallType.GetCompoundStructure();
                if (wallStructure != null)
                {
                    IList<CompoundStructureLayer> layers = wallStructure.GetLayers();
                    List<string> materialNames = new List<string>();
                    foreach (CompoundStructureLayer layer in layers)
                        materialNames.Add(doc.GetElement(layer.MaterialId).Name);
                    if (materialNames.Contains(material)) // Filtering  for materials
                    {
                        LocationCurve wallLocation = wall.Location as LocationCurve;
                        double diff = Math.Abs(wallLocation.Curve.GetEndPoint(0).Z - level0);
                        if (diff < 1)
                            walls.Add(wall);
                    }
                }
            }
            return walls;
        }

        /// <summary>
        /// Convert walls contours to list of CurveLoop items.
        /// </summary>
        /// <param name="wall">Wall element.</param>
        /// <returns>List of CurveLoops.</returns>
        private List<CurveLoop> GetWallsLoops(IEnumerable<Wall> walls)
        {
            List<CurveLoop> loops = new List<CurveLoop>();

            Options options = new Options();

            foreach (Wall wall in walls)
            {
                List<CurveLoop> loopsWall = new List<CurveLoop>();

                GeometryElement geometryElement = wall.get_Geometry(options);
                foreach (Solid geometrySolid in geometryElement)
                {
                    foreach (Face face in geometrySolid.Faces)
                    {
                        if (face is PlanarFace)
                        {
                            PlanarFace facePlanar = face as PlanarFace;
                            if (facePlanar.FaceNormal.Z == -1)
                            {
                                double height = facePlanar.Origin.Z;
                                double heightLowest = height;
                                // Get the lowest face
                                if (loopsWall.Count == 0)
                                    loopsWall = facePlanar.GetEdgesAsCurveLoops() as List<CurveLoop>;
                                else
                                {
                                    if (height < heightLowest)
                                    {
                                        loopsWall = facePlanar.GetEdgesAsCurveLoops() as List<CurveLoop>;
                                        heightLowest = height;
                                    }
                                }
                            }
                        }
                    }
                }
                if (loopsWall.Count > 0)
                    loops.AddRange(loopsWall);
            }
            return loops;
        }

        /// <summary>
        /// Convert walls loops to single solid.
        /// </summary>
        /// <returns>Solid</returns>
        private Solid GetWallsLoop(List<CurveLoop> CurveLoopsInput)
        {
            List<Solid> solids = new List<Solid>();
            foreach (CurveLoop curveLoop in CurveLoopsInput)
            {
                List<CurveLoop> CurveLoops = new List<CurveLoop> { curveLoop };
                Solid extrusion = GeometryCreationUtilities.CreateExtrusionGeometry(CurveLoops, new XYZ(0, 0, 1), 10);

                if (solids.Count > 0)
                {
                    Solid solid = BooleanOperationsUtils.ExecuteBooleanOperation(extrusion, solids.First(), BooleanOperationsType.Union);
                    solids.Clear();
                    solids.Add(solid);
                }
                else
                    solids.Add(extrusion);
            }
            return solids.First();
        }

        /// <summary>
        /// Get list of curveloops by intersecting two solids.
        /// </summary>
        /// <returns>List of CurveLoop objects.</returns>
        private List<CurveLoop> GetCurveLoops(Solid solid1, Solid solid2)
        {
            List<CurveLoop> loopList = new List<CurveLoop>();

            Solid intersection = BooleanOperationsUtils.ExecuteBooleanOperation(solid1, solid2, BooleanOperationsType.Intersect);

            // Create CurveLoop from intersection
            foreach (Face face in intersection.Faces)
            {
                try
                {
                    PlanarFace facePlanar = face as PlanarFace;
                    if (facePlanar.FaceNormal.Z == 1)
                    {
                        IList<CurveLoop> loops = face.GetEdgesAsCurveLoops();
                        loopList.AddRange(loops);
                    }
                }
                catch { }
            }

            _countFilledRegions = loopList.Count;

            return loopList;
        }

        private protected override string GetRunResult()
        {
            string text = $"{_countFilledRegions} filled regions created.";

            return text;
        }

        #endregion
    }
}