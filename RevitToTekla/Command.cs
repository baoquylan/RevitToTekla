using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitToTekla
{
    class Command
    {
        public static bool UnjoinFloor(Document doc)
        {
            View3D view = doc.ActiveView as View3D;

            if (null == view)
            {
                TaskDialog.Show("Revit",
                  "You must be in a 3D view to export.");
                return false;
            }
            IList<Element> elements = new FilteredElementCollector(doc, doc.ActiveView.Id).OfClass(typeof(Floor)).ToElements();

            using (Transaction t = new Transaction(doc, "temp"))
            {
                foreach (Element element in elements)
                {
                    try
                    {
                        FailureHandlingOptions failureHandlingOptions = t.GetFailureHandlingOptions();
                        failureHandlingOptions.SetFailuresPreprocessor(new ErrorPreprocessor());
                        t.SetFailureHandlingOptions(failureHandlingOptions);
                        t.Start();
                        var listJoin = JoinGeometryUtils.GetJoinedElements(doc, element);
                        foreach (var join in listJoin)
                        {
                            JoinGeometryUtils.UnjoinGeometry(doc, element, doc.GetElement(join));
                        }
                        t.Commit();
                    }
                    catch { }
                }

            }

            return true;
        }


        public static void ExportToTekla(Document doc, string path)
        {

            IList<Element> listElement = new FilteredElementCollector(doc, doc.ActiveView.Id).ToElements();


            CoordinateRevit coordinateRevit = new CoordinateRevit();
            coordinateRevit.RevitDatas = new List<RevitData>();
            foreach (Element element in listElement)
            {
                RevitData revitData = new RevitData();
                revitData.MemberID = element.UniqueId;
                revitData.RevitID = element.Id.IntegerValue.ToString();
                revitData.FamilyName = element.Name;
                revitData.PointCoordinates = new List<PointCoordinate>();
                if (element.Category != null && element.Category.Name != null)
                {
                    revitData.CategoryName = element.Category.Name;
                    if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns ||
                       element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Columns)
                    {
                        ElementColumn(doc, element, revitData);
                        coordinateRevit.RevitDatas.Add(revitData);
                    }
                    else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFraming)
                    {
                        ElementBeam(doc, element, revitData);
                        coordinateRevit.RevitDatas.Add(revitData);
                    }
                    else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Walls)
                    {
                        ElementWall(doc, element, revitData);
                        coordinateRevit.RevitDatas.Add(revitData);
                    }
                    else if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Floors ||
                        element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFoundation)
                    {
                        ElementFloorFoundation(doc, element, revitData);
                        coordinateRevit.RevitDatas.Add(revitData);
                    }
                }

            }



            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, coordinateRevit);
            }


        }
        public static bool SelectFile(ref string folder_path, ref string filename)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "Select JSON Output File";
            dlg.Filter = "JSON files|*.js";

            if (null != folder_path
              && 0 < folder_path.Length)
            {
                dlg.InitialDirectory = folder_path;
            }

            dlg.FileName = filename;

            bool rc = DialogResult.OK == dlg.ShowDialog();

            if (rc)
            {
                filename = Path.Combine(dlg.InitialDirectory,
                  dlg.FileName);

                folder_path = Path.GetDirectoryName(
                  filename);
            }
            return rc;
        }
        private  static void ElementColumn(Document doc, Element element, RevitData revitData)
        {
            try
            {
                revitData.RevitColumn = new RevitColumnData();
                var structuralType = (element as FamilyInstance).StructuralMaterialType.ToString();
                revitData.StructuralType = structuralType;
                //Check Material
                if (structuralType == "Concrete")
                {
                    Element elementType = doc.GetElement(element.GetTypeId());
                    var b = elementType.LookupParameter("b");
                    var h = elementType.LookupParameter("h");
                    if (b != null && h != null)
                    {
                        revitData.RevitColumn.Profile = Math.Round(elementType.LookupParameter("b").AsDouble() * 304.8, 0) + "*"
                     + Math.Round(elementType.LookupParameter("h").AsDouble() * 304.8, 0);
                    }
                    else
                    {
                        revitData.RevitColumn.Profile = "D" + Math.Round(elementType.LookupParameter("b").AsDouble() * 304.8, 0);
                    }

                }
                //Check vetical
                if (element.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE) != null)
                {
                    revitData.RevitColumn.Rotation = Math.Round(element.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).AsDouble() * 180 / Math.PI, 2);
                    var basetExtension = element.get_Parameter(BuiltInParameter.SLANTED_COLUMN_BASE_EXTENSION).AsDouble();
                    var topExtension = element.get_Parameter(BuiltInParameter.SLANTED_COLUMN_TOP_EXTENSION).AsDouble();
                    Curve elementCurve = CurveFamilyColumnInstance(doc, element);
                    if (elementCurve.Tessellate().Count == 2)
                    {
                        Line line1 = Line.CreateBound(elementCurve.Tessellate()[0], elementCurve.Tessellate()[1]);
                        XYZ vecter1 = line1.Direction.Negate();
                        XYZ pointExtend1 = elementCurve.Tessellate()[0] + basetExtension * vecter1;
                        PointCoordinate pointCoordinate1 = new PointCoordinate();
                        pointCoordinate1.PointX = Math.Round(pointExtend1.X * 304.8, 0);
                        pointCoordinate1.PointY = Math.Round(pointExtend1.Y * 304.8, 0);
                        pointCoordinate1.PointZ = Math.Round(pointExtend1.Z * 304.8, 0);
                        revitData.PointCoordinates.Add(pointCoordinate1);

                        XYZ vecter2 = line1.Direction;
                        XYZ pointExtend2 = elementCurve.Tessellate()[1] + topExtension * vecter2;
                        PointCoordinate pointCoordinate2 = new PointCoordinate();
                        pointCoordinate2.PointX = Math.Round(pointExtend2.X * 304.8, 0);
                        pointCoordinate2.PointY = Math.Round(pointExtend2.Y * 304.8, 0);
                        pointCoordinate2.PointZ = Math.Round(pointExtend2.Z * 304.8, 0);
                        revitData.PointCoordinates.Add(pointCoordinate2);
                    }

                }
                else
                {
                    Transform transform = (element as Instance).GetTransform();
                    revitData.RevitColumn.Rotation = 90 - Math.Round(transform.BasisX.AngleOnPlaneTo(new XYZ(1, 0, 0), new XYZ(0, 0, 1)) * 180 / Math.PI, 0);
                    Curve elementCurve = CurveFamilyColumnInstance(doc, element);
                    foreach (XYZ pointXYZ in elementCurve.Tessellate())
                    {
                        PointCoordinate pointCoordinate1 = new PointCoordinate();
                        pointCoordinate1.PointX = Math.Round(pointXYZ.X * 304.8, 0);
                        pointCoordinate1.PointY = Math.Round(pointXYZ.Y * 304.8, 0);
                        pointCoordinate1.PointZ = Math.Round(pointXYZ.Z * 304.8, 0);
                        revitData.PointCoordinates.Add(pointCoordinate1);
                    }
                }

            }
            catch { }
        }
        private static void ElementBeam(Document doc, Element element, RevitData revitData)
        {
            if (element.Location as LocationCurve != null)
            {
                try
                {
                    revitData.RevitBeam = new RevitBeamData();
                    revitData.RevitBeam.PointCoordinates = new List<PointCoordinate>();
                    var structuralType = (element as FamilyInstance).StructuralMaterialType.ToString();
                    revitData.StructuralType = structuralType;
                    if (structuralType == "Concrete")
                    {
                        Element elementType = doc.GetElement(element.GetTypeId());
                        revitData.RevitBeam.Profile = Math.Round(elementType.LookupParameter("b").AsDouble() * 304.8, 0) + "*"
                       + Math.Round(elementType.LookupParameter("h").AsDouble() * 304.8, 0);
                        revitData.RevitBeam.Rotation = Math.Round(element.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).AsDouble() * 180 / Math.PI, 2);

                        Curve elementCurve = CurveFamilyBeamInstance(doc, element);
                        revitData.RevitBeam.PolyBeam = true;
                        if (elementCurve is Line)
                            revitData.RevitBeam.PolyBeam = false;
                        foreach (XYZ pointXYZ in elementCurve.Tessellate())
                        {
                            PointCoordinate pointCoordinate1 = new PointCoordinate();
                            pointCoordinate1.PointX = Math.Round(pointXYZ.X * 304.8, 0);
                            pointCoordinate1.PointY = Math.Round(pointXYZ.Y * 304.8, 0);
                            pointCoordinate1.PointZ = Math.Round(pointXYZ.Z * 304.8, 0);
                            revitData.RevitBeam.PointCoordinates.Add(pointCoordinate1);
                        }
                    }
                    else
                    {
                        var startExtension = element.get_Parameter(BuiltInParameter.START_EXTENSION).AsDouble();
                        var endExtension = element.get_Parameter(BuiltInParameter.END_EXTENSION).AsDouble();

                        Curve elementCurve = CurveFamilyBeamInstance(doc, element);
                        if (elementCurve.Tessellate().Count == 2)
                        {
                            Line line1 = Line.CreateBound(elementCurve.Tessellate()[0], elementCurve.Tessellate()[1]);
                            XYZ vecter1 = line1.Direction.Negate();
                            XYZ pointExtend1 = elementCurve.Tessellate()[0] + startExtension * vecter1;
                            PointCoordinate pointCoordinate1 = new PointCoordinate();
                            pointCoordinate1.PointX = Math.Round(pointExtend1.X * 304.8, 0);
                            pointCoordinate1.PointY = Math.Round(pointExtend1.Y * 304.8, 0);
                            pointCoordinate1.PointZ = Math.Round(pointExtend1.Z * 304.8, 0);
                            revitData.RevitBeam.PointCoordinates.Add(pointCoordinate1);

                            XYZ vecter2 = line1.Direction;
                            XYZ pointExtend2 = elementCurve.Tessellate()[1] + endExtension * vecter2;
                            PointCoordinate pointCoordinate2 = new PointCoordinate();
                            pointCoordinate2.PointX = Math.Round(pointExtend2.X * 304.8, 0);
                            pointCoordinate2.PointY = Math.Round(pointExtend2.Y * 304.8, 0);
                            pointCoordinate2.PointZ = Math.Round(pointExtend2.Z * 304.8, 0);
                            revitData.RevitBeam.PointCoordinates.Add(pointCoordinate2);
                        }
                    }

                    if (element.get_Parameter(BuiltInParameter.YZ_JUSTIFICATION).AsValueString() == "Independent")
                    {
                        revitData.RevitBeam.yJustification = element.get_Parameter(BuiltInParameter.START_Y_JUSTIFICATION).AsValueString();
                        revitData.RevitBeam.StartYOffsetValue = Math.Round(element.get_Parameter(BuiltInParameter.START_Y_OFFSET_VALUE).AsDouble() * 304.8, 0);
                        revitData.RevitBeam.EndYOffsetValue = Math.Round(element.get_Parameter(BuiltInParameter.END_Y_OFFSET_VALUE).AsDouble() * 304.8, 0);

                        revitData.RevitBeam.zJustification = element.get_Parameter(BuiltInParameter.START_Z_JUSTIFICATION).AsValueString();
                        revitData.RevitBeam.StartZOffsetValue = Math.Round(element.get_Parameter(BuiltInParameter.START_Z_OFFSET_VALUE).AsDouble() * 304.8, 0);
                        revitData.RevitBeam.EndZOffsetValue = Math.Round(element.get_Parameter(BuiltInParameter.END_Z_OFFSET_VALUE).AsDouble() * 304.8, 0);
                    }
                    else
                    {
                        revitData.RevitBeam.yJustification = element.get_Parameter(BuiltInParameter.Y_JUSTIFICATION).AsValueString();
                        revitData.RevitBeam.StartYOffsetValue = Math.Round(element.get_Parameter(BuiltInParameter.Y_OFFSET_VALUE).AsDouble() * 304.8, 0);
                        revitData.RevitBeam.EndYOffsetValue = Math.Round(element.get_Parameter(BuiltInParameter.Y_OFFSET_VALUE).AsDouble() * 304.8, 0);

                        revitData.RevitBeam.zJustification = element.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).AsValueString();
                        revitData.RevitBeam.StartZOffsetValue = Math.Round(element.get_Parameter(BuiltInParameter.Z_OFFSET_VALUE).AsDouble() * 304.8, 0);
                        revitData.RevitBeam.EndZOffsetValue = Math.Round(element.get_Parameter(BuiltInParameter.Z_OFFSET_VALUE).AsDouble() * 304.8, 0);
                    }


                }
                catch { }
            }
        }
        private static void ElementWall(Document doc, Element element, RevitData revitData)
        {
            try
            {
                WallType elementType = doc.GetElement(element.GetTypeId()) as WallType;
                if (element.Location as LocationCurve != null)
                {
                    revitData.RevitWall = new List<RevitWallData>();
                    RevitWallData revitWallData = new RevitWallData();
                    List<PointCoordinate> pointCoordinates = new List<PointCoordinate>();
                    revitWallData.ModelInPlace = false;
                    Curve elementCurve = (element.Location as LocationCurve).Curve;
                    #region
                    //double baseOffset = element.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();
                    //double wallHeight = element.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                    //double topOffset = element.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).AsDouble();
                    //double wallHeightNeed = 0;
                    //if (element.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).AsValueString() == "Unconnected")
                    //{
                    //    wallHeightNeed = wallHeight + baseOffset + topOffset;
                    //}
                    #endregion
                    var bbox = element.get_BoundingBox(doc.ActiveView);
                    revitWallData.Profile = Math.Round((bbox.Max.Z - bbox.Min.Z) * 304.8, 0).ToString() + "*" + Math.Round(elementType.Width * 304.8, 0).ToString();
                    foreach (XYZ pointXYZ in elementCurve.Tessellate())
                    {
                        PointCoordinate pointCoordinate1 = new PointCoordinate();
                        pointCoordinate1.PointX = Math.Round(pointXYZ.X * 304.8, 0);
                        pointCoordinate1.PointY = Math.Round(pointXYZ.Y * 304.8, 0);
                        pointCoordinate1.PointZ = Math.Round(bbox.Min.Z * 304.8, 0);
                        pointCoordinates.Add(pointCoordinate1);
                    }
                    revitWallData.ListPointWall = pointCoordinates;
                    revitData.RevitWall.Add(revitWallData);
                }
                else
                {

                    if (!(elementType is WallType))
                    {
                        revitData.RevitWall = new List<RevitWallData>();
                        List<Solid> listSolid = SolidWallModelInPlace(doc, element);
                        foreach (Solid solid in listSolid)
                        {

                            RevitWallData revitWallData = new RevitWallData();
                            revitWallData.ListPointWallInPlace = new List<List<PointCoordinate>>();
                            List<List<PointCoordinate>> listPointCoordinates = new List<List<PointCoordinate>>();

                            revitWallData.ModelInPlace = true;

                            Face bottomFaceFloor = BottomFaceWallModelInPlace(doc, solid);
                            var bbox = solid.GetBoundingBox();
                            revitWallData.Profile = Math.Round((bbox.Max.Z - bbox.Min.Z) * 304.8, 0).ToString();
                            foreach (EdgeArray edgeArray in bottomFaceFloor.EdgeLoops)
                            {
                                List<PointCoordinate> pointCoordinates = new List<PointCoordinate>();
                                foreach (Edge edge in edgeArray)
                                {
                                    foreach (XYZ pointXYZ in edge.Tessellate())
                                    {
                                        PointCoordinate pointCoordinate1 = new PointCoordinate();
                                        pointCoordinate1.PointX = Math.Round(pointXYZ.X * 304.8, 0);
                                        pointCoordinate1.PointY = Math.Round(pointXYZ.Y * 304.8, 0);
                                        pointCoordinate1.PointZ = Math.Round(pointXYZ.Z * 304.8, 0);
                                        pointCoordinates.Add(pointCoordinate1);
                                    }
                                }
                                revitWallData.ListPointWallInPlace.Add(pointCoordinates);
                            }
                            revitData.RevitWall.Add(revitWallData);
                        }
                    }
                }


            }
            catch { }
        }
        private static void ElementFloorFoundation(Document doc, Element element, RevitData revitData)
        {
            try
            {

                Face topFaceFloor = TopFaceFloor(doc, element);
                Floor floor = element as Floor;
                if (!floor.SlabShapeEditor.IsEnabled)
                {
                    List<PointCoordinateFloor> listPointCoordinatesFloor = new List<PointCoordinateFloor>();
                    foreach (EdgeArray edgeArray in topFaceFloor.EdgeLoops)
                    {
                        PointCoordinateFloor pointCoordinateFloor = new PointCoordinateFloor();
                        var thick = element.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM)?.AsValueString();
                        if(thick == "0")
                        {
                            var type = doc.GetElement(element.GetTypeId());
                            thick = type.get_Parameter(BuiltInParameter.FLOOR_ATTR_DEFAULT_THICKNESS_PARAM)?.AsValueString();
                        }
                        pointCoordinateFloor.Profile = element.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsValueString();
                        List<PointCoordinate> listPointCoordinates = new List<PointCoordinate>();
                        foreach (Edge edge in edgeArray)
                        {
                            foreach (XYZ pointXYZ in edge.Tessellate())
                            {
                                PointCoordinate pointCoordinate1 = new PointCoordinate();
                                pointCoordinate1.PointX = Math.Round(pointXYZ.X * 304.8, 0);
                                pointCoordinate1.PointY = Math.Round(pointXYZ.Y * 304.8, 0);
                                pointCoordinate1.PointZ = Math.Round(pointXYZ.Z * 304.8, 0);
                                listPointCoordinates.Add(pointCoordinate1);
                            }
                        }
                        pointCoordinateFloor.ListPointFloor = listPointCoordinates;
                        listPointCoordinatesFloor.Add(pointCoordinateFloor);
                    }
                    revitData.PointCoordinatesFloor = listPointCoordinatesFloor;
                }
                else
                {
                    List<PointCoordinateFloor> listPointCoordinatesFloor = new List<PointCoordinateFloor>();
                    List<PointCoordinate> listPointCoordinates = new List<PointCoordinate>();
                    PointCoordinateFloor pointCoordinateFloor = new PointCoordinateFloor();
                    pointCoordinateFloor.Profile = element.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsValueString();
                    pointCoordinateFloor.BasePointFloor = Math.Round((GetBasePointFloor(doc, element).Z +
                        element.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble()) * 304.8, 0).ToString();
                    foreach (SlabShapeVertex shape in floor.SlabShapeEditor.SlabShapeVertices)
                    {
                        if (shape.VertexType == SlabShapeVertexType.Corner)
                        {
                            PointCoordinate pointCoordinate1 = new PointCoordinate();
                            pointCoordinate1.PointX = Math.Round(shape.Position.X * 304.8, 0);
                            pointCoordinate1.PointY = Math.Round(shape.Position.Y * 304.8, 0);
                            pointCoordinate1.PointZ = Math.Round(shape.Position.Z * 304.8, 0);
                            listPointCoordinates.Add(pointCoordinate1);
                        }
                    }
                    pointCoordinateFloor.ListPointFloor = listPointCoordinates;
                    listPointCoordinatesFloor.Add(pointCoordinateFloor);
                    revitData.PointCoordinatesFloor = listPointCoordinatesFloor;
                }
            }
            catch { }
        }
        public static Face TopFaceFloor(Document doc, Element f)
        {
            Face topFaceFloor = null;
            try
            {
                Options opt = new Options();
                opt.View = doc.ActiveView;
                GeometryElement floorGeometryElement = f.get_Geometry(opt);
                foreach (var geometryObject in floorGeometryElement)
                {
                    if (geometryObject is Solid)
                    {
                        try
                        {
                            if ((geometryObject as Solid).Volume > 0)
                            {
                                Solid floorSolid = geometryObject as Solid;
                                foreach (Face face in floorSolid.Faces)
                                {
                                    if (face is PlanarFace)
                                    {
                                        if ((face as PlanarFace).FaceNormal.IsAlmostEqualTo(new XYZ(0, 0, 1)))
                                        {
                                            topFaceFloor = face;
                                        }
                                    }
                                }
                            }

                        }
                        catch { }

                    }
                }
            }
            catch { }
            return topFaceFloor;
        }

        public static XYZ GetBasePointFloor(Document doc, Element f)
        {
            XYZ basePoint = null;
            List<ElementId> _deleted = null;
            using (Transaction t = new Transaction(doc, "temp"))
            {
                t.Start();
                _deleted = doc.Delete(f.Id).ToList();
                t.RollBack();
            }
            foreach (var id in _deleted)
            {
                // find modelcurves. these are the lines in the bounderysketch
                ModelCurve mc = doc.GetElement(id) as ModelCurve;
                if (mc != null)
                {
                    basePoint = mc.GeometryCurve.GetEndPoint(0);

                }

            }
            return basePoint;
        }
        public static Face BottomFaceWallModelInPlace(Document doc, Solid solid)
        {
            Face topFace = null;
            try
            {
                foreach (Face face in solid.Faces)
                {
                    if (face is PlanarFace)
                    {
                        if ((face as PlanarFace).FaceNormal.IsAlmostEqualTo(new XYZ(0, 0, -1)))
                        {
                            topFace = face;
                            break;
                        }
                    }
                }
            }
            catch { }
            return topFace;
        }
        public static List<Solid> SolidWallModelInPlace(Document doc, Element e)
        {
            List<Solid> listSolid = new List<Solid>();
            try
            {
                Options opt = new Options();
                opt.View = doc.ActiveView;
                GeometryElement floorGeometryElement = e.get_Geometry(opt);
                foreach (GeometryInstance geometryObject in floorGeometryElement)
                {
                    foreach (var geometry in geometryObject.GetInstanceGeometry())
                    {
                        if (geometry is Solid)
                        {
                            try
                            {
                                if ((geometry as Solid).Volume > 0)
                                {
                                    Solid floorSolid = geometry as Solid;
                                    listSolid.Add(floorSolid);
                                }

                            }
                            catch { }

                        }
                    }

                }
            }
            catch { }
            return listSolid;
        }
        public static Curve CurveFamilyBeamInstance(Document doc, Element e)
        {
            Curve curveFamily = null;
            try
            {
                Options opt = new Options();
                opt.View = doc.ActiveView;
                opt.ComputeReferences = true;
                opt.IncludeNonVisibleObjects = true;
                GeometryElement floorGeometryElement = e.get_Geometry(opt);
                foreach (var geometryObject in floorGeometryElement)
                {
                    if ((geometryObject as Curve) is Curve)
                    {
                        GraphicsStyle graphicStyle = doc.GetElement(geometryObject.GraphicsStyleId) as GraphicsStyle;
                        if (graphicStyle.Name == "Structural Beams" || graphicStyle.Name == "Location Lines")
                            curveFamily = geometryObject as Curve;

                    }

                }
            }
            catch { }
            return curveFamily;
        }
        public static Curve CurveFamilyColumnInstance(Document doc, Element e)
        {
            Curve curveFamily = null;
            try
            {
                Options opt = new Options();
                opt.View = doc.ActiveView;
                opt.ComputeReferences = true;
                opt.IncludeNonVisibleObjects = true;
                GeometryElement floorGeometryElement = e.get_Geometry(opt);
                foreach (var geometryObject in floorGeometryElement)
                {
                    if ((geometryObject as Line) is Line)
                    {
                        GraphicsStyle graphicStyle = doc.GetElement(geometryObject.GraphicsStyleId) as GraphicsStyle;
                        if (graphicStyle.Name == "Structural Columns" || graphicStyle.Name == "Location Lines")
                            curveFamily = geometryObject as Curve;

                    }

                }
            }
            catch { }
            return curveFamily;
        }
    }

    public class ErrorPreprocessor : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();
            if (failureMessages.Count == 0)
            {
                return FailureProcessingResult.Continue;
            }
            bool isResolved = false;
            foreach (FailureMessageAccessor current in failureMessages)
            {
                var ss = current.GetDescriptionText();
                if (current.HasResolutions())
                {
                    failuresAccessor.ResolveFailure(current);
                    isResolved = true;
                }
                else
                {
                    failuresAccessor.DeleteWarning(current);
                }
            }
            if (isResolved)
            {
                return FailureProcessingResult.ProceedWithCommit;
            }
            return FailureProcessingResult.Continue;
        }
    }

}


