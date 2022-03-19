using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Model;
using TSD = Tekla.Structures.Geometry3d;

namespace TeklaImporter
{
    class Command
    {
        public static ArrayList listNeedToOpen;
        public static ModelObject CreateColumns(RevitData revitData, Model myModel)
        {

            Beam Column = new Beam(Beam.BeamTypeEnum.COLUMN);
            try
            {
                Column.Name = revitData.FamilyName;
                Column.Profile.ProfileString = "U200*100*5";
                Column.Material.MaterialString = "S235";
                Column.Class = "2";
                if (revitData.RevitColumn.Profile != null)
                {
                    Column.Profile.ProfileString = revitData.RevitColumn.Profile;
                    Column.Material.MaterialString = "C20";
                    Column.Class = "7";
                }

                Column.StartPoint.X = revitData.PointCoordinates[0].PointX;
                Column.StartPoint.Y = revitData.PointCoordinates[0].PointY;
                Column.StartPoint.Z = revitData.PointCoordinates[0].PointZ;
                Column.EndPoint.X = revitData.PointCoordinates[1].PointX;
                Column.EndPoint.Y = revitData.PointCoordinates[1].PointY; ;
                Column.EndPoint.Z = revitData.PointCoordinates[1].PointZ; ;
                Column.Position.Rotation = Position.RotationEnum.FRONT;
                Column.Position.RotationOffset = revitData.RevitColumn.Rotation;
                Column.Position.Plane = Position.PlaneEnum.MIDDLE;
                Column.Position.Depth = Position.DepthEnum.MIDDLE;


                if (!Column.Insert())
                {
                    Console.WriteLine("Insertion of stiffened base plate failed.");
                }
                Column.SetUserProperty("MemberId", revitData.MemberID);
                myModel.CommitChanges();
            }
            catch { }

            return Column;
        }
        public static ModelObject CreateBeam(RevitData revitData, Model myModel)
        {
            Beam beam = new Beam(Beam.BeamTypeEnum.BEAM);
            try
            {
                beam.Name = revitData.FamilyName;

                if (revitData.RevitBeam.Profile != null)
                {
                    beam.Profile.ProfileString = revitData.RevitBeam.Profile;
                    beam.Material.MaterialString = "C20";
                    beam.Class = "6";

                    beam.StartPoint.X = revitData.RevitBeam.PointCoordinates[1].PointX;
                    beam.StartPoint.Y = revitData.RevitBeam.PointCoordinates[1].PointY;
                    beam.StartPoint.Z = revitData.RevitBeam.PointCoordinates[1].PointZ;
                    beam.EndPoint.X = revitData.RevitBeam.PointCoordinates[0].PointX;
                    beam.EndPoint.Y = revitData.RevitBeam.PointCoordinates[0].PointY;
                    beam.EndPoint.Z = revitData.RevitBeam.PointCoordinates[0].PointZ;

                    beam.Position.Rotation = Position.RotationEnum.FRONT;
                    beam.Position.RotationOffset = revitData.RevitBeam.Rotation;
                    if (revitData.RevitBeam.yJustification == "Origin" || revitData.RevitBeam.yJustification == "Center")
                        beam.Position.Plane = Position.PlaneEnum.MIDDLE;
                    else if (revitData.RevitBeam.yJustification == "Left")
                        beam.Position.Plane = Position.PlaneEnum.LEFT;
                    else if (revitData.RevitBeam.yJustification == "Right")
                        beam.Position.Plane = Position.PlaneEnum.RIGHT;

                    if (revitData.RevitBeam.zJustification == "Origin" || revitData.RevitBeam.zJustification == "Center")
                        beam.Position.Depth = Position.DepthEnum.MIDDLE;
                    else if (revitData.RevitBeam.zJustification == "Top")
                        beam.Position.Depth = Position.DepthEnum.BEHIND;
                    else if (revitData.RevitBeam.zJustification == "Bottom")
                        beam.Position.Depth = Position.DepthEnum.FRONT;
                }
                else
                {
                    beam.Profile.ProfileString = "U200*100*5";
                    beam.Material.MaterialString = "S235";
                    beam.Class = "3";

                    beam.StartPoint.X = revitData.RevitBeam.PointCoordinates[1].PointX;
                    beam.StartPoint.Y = revitData.RevitBeam.PointCoordinates[1].PointY;
                    beam.StartPoint.Z = revitData.RevitBeam.PointCoordinates[1].PointZ;
                    beam.EndPoint.X = revitData.RevitBeam.PointCoordinates[0].PointX;
                    beam.EndPoint.Y = revitData.RevitBeam.PointCoordinates[0].PointY;
                    beam.EndPoint.Z = revitData.RevitBeam.PointCoordinates[0].PointZ;

                    beam.Position.Rotation = Position.RotationEnum.TOP;
                    beam.Position.RotationOffset = revitData.RevitBeam.Rotation;
                    if (revitData.RevitBeam.yJustification == "Origin" || revitData.RevitBeam.yJustification == "Center")
                        beam.Position.Plane = Position.PlaneEnum.MIDDLE;
                    else if (revitData.RevitBeam.yJustification == "Left")
                        beam.Position.Plane = Position.PlaneEnum.LEFT;
                    else if (revitData.RevitBeam.yJustification == "Right")
                        beam.Position.Plane = Position.PlaneEnum.RIGHT;

                    if (revitData.RevitBeam.zJustification == "Origin" || revitData.RevitBeam.zJustification == "Center")
                        beam.Position.Depth = Position.DepthEnum.MIDDLE;
                    else if (revitData.RevitBeam.zJustification == "Top")
                        beam.Position.Depth = Position.DepthEnum.BEHIND;
                    else if (revitData.RevitBeam.zJustification == "Bottom")
                        beam.Position.Depth = Position.DepthEnum.FRONT;
                }


                beam.EndPointOffset.Dy = revitData.RevitBeam.StartYOffsetValue;
                beam.EndPointOffset.Dz = revitData.RevitBeam.StartZOffsetValue;
                beam.StartPointOffset.Dy = revitData.RevitBeam.EndYOffsetValue;
                beam.StartPointOffset.Dz = revitData.RevitBeam.EndZOffsetValue;



                if (!beam.Insert())
                {
                    Console.WriteLine("Insertion of stiffened base plate failed.");
                }
                beam.SetUserProperty("MemberId", revitData.MemberID);
                myModel.CommitChanges();
            }
            catch { }

            return beam;
        }
        public static ModelObject CreatePolyBeam(RevitData revitData, Model myModel)
        {
            PolyBeam polyBeam = new PolyBeam(PolyBeam.PolyBeamTypeEnum.BEAM);
            try
            {
                polyBeam.Name = revitData.FamilyName;

                if (revitData.RevitBeam.Profile != null)
                {
                    polyBeam.Profile.ProfileString = revitData.RevitBeam.Profile;
                    polyBeam.Material.MaterialString = "C20";
                    polyBeam.Class = "6";

                    Contour ct = new Contour();
                    foreach (PointCoordinate p in revitData.RevitBeam.PointCoordinates)
                    {
                        ContourPoint p1 = new ContourPoint(new TSD.Point(p.PointX, p.PointY, p.PointZ), new Chamfer());
                        ct.AddContourPoint(p1);

                    }
                    polyBeam.Contour = ct;


                    polyBeam.Position.Rotation = Position.RotationEnum.FRONT;
                    polyBeam.Position.RotationOffset = revitData.RevitBeam.Rotation;
                    if (revitData.RevitBeam.yJustification == "Origin" || revitData.RevitBeam.yJustification == "Center")
                        polyBeam.Position.Plane = Position.PlaneEnum.MIDDLE;
                    else if (revitData.RevitBeam.yJustification == "Left")
                        polyBeam.Position.Plane = Position.PlaneEnum.LEFT;
                    else if (revitData.RevitBeam.yJustification == "Right")
                        polyBeam.Position.Plane = Position.PlaneEnum.RIGHT;

                    if (revitData.RevitBeam.zJustification == "Origin" || revitData.RevitBeam.zJustification == "Center")
                        polyBeam.Position.Depth = Position.DepthEnum.MIDDLE;
                    else if (revitData.RevitBeam.zJustification == "Top")
                        polyBeam.Position.Depth = Position.DepthEnum.BEHIND;
                    else if (revitData.RevitBeam.zJustification == "Bottom")
                        polyBeam.Position.Depth = Position.DepthEnum.FRONT;
                }
                else
                {
                    polyBeam.Profile.ProfileString = "UB152*89*16";
                    polyBeam.Material.MaterialString = "S235";
                    polyBeam.Class = "3";

                    Contour ct = new Contour();
                    foreach (PointCoordinate p in revitData.RevitBeam.PointCoordinates)
                    {
                        ContourPoint p1 = new ContourPoint(new TSD.Point(p.PointX, p.PointY, p.PointZ), new Chamfer());
                        ct.AddContourPoint(p1);

                    }
                    polyBeam.Contour = ct;

                    polyBeam.Position.Rotation = Position.RotationEnum.TOP;
                    polyBeam.Position.RotationOffset = revitData.RevitBeam.Rotation;
                    if (revitData.RevitBeam.yJustification == "Origin" || revitData.RevitBeam.yJustification == "Center")
                        polyBeam.Position.Plane = Position.PlaneEnum.MIDDLE;
                    else if (revitData.RevitBeam.yJustification == "Left")
                        polyBeam.Position.Plane = Position.PlaneEnum.LEFT;
                    else if (revitData.RevitBeam.yJustification == "Right")
                        polyBeam.Position.Plane = Position.PlaneEnum.RIGHT;

                    if (revitData.RevitBeam.zJustification == "Origin" || revitData.RevitBeam.zJustification == "Center")
                        polyBeam.Position.Depth = Position.DepthEnum.MIDDLE;
                    else if (revitData.RevitBeam.zJustification == "Top")
                        polyBeam.Position.Depth = Position.DepthEnum.BEHIND;
                    else if (revitData.RevitBeam.zJustification == "Bottom")
                        polyBeam.Position.Depth = Position.DepthEnum.FRONT;
                }


                //polyBeam.EndPointOffset.Dy = revitData.RevitBeam.StartYOffsetValue;
                //polyBeam.EndPointOffset.Dz = revitData.RevitBeam.StartZOffsetValue;
                //polyBeam.StartPointOffset.Dy = revitData.RevitBeam.EndYOffsetValue;
                //polyBeam.StartPointOffset.Dz = revitData.RevitBeam.EndZOffsetValue;



                if (!polyBeam.Insert())
                {
                    Console.WriteLine("Insertion of stiffened base plate failed.");
                }
                polyBeam.SetUserProperty("MemberId", revitData.MemberID);
                myModel.CommitChanges();
            }
            catch { }

            return polyBeam;
        }
        public static ModelObject CreateWallStraight(RevitData revitData, RevitWallData revitWallData, Model myModel)
        {
            Beam beam = new Beam(Beam.BeamTypeEnum.PANEL);
            try
            {
                beam.Name = revitData.FamilyName;
                beam.Profile.ProfileString = revitWallData.Profile;
                beam.Material.MaterialString = "S235";
                beam.Class = "1";


                beam.StartPoint.X = revitWallData.ListPointWall[0].PointX;
                beam.StartPoint.Y = revitWallData.ListPointWall[0].PointY;
                beam.StartPoint.Z = revitWallData.ListPointWall[0].PointZ;
                beam.EndPoint.X = revitWallData.ListPointWall[1].PointX;
                beam.EndPoint.Y = revitWallData.ListPointWall[1].PointY;
                beam.EndPoint.Z = revitWallData.ListPointWall[1].PointZ;
                beam.Position.Rotation = Position.RotationEnum.TOP;
                beam.Position.Plane = Position.PlaneEnum.MIDDLE;
                beam.Position.Depth = Position.DepthEnum.FRONT;
                if (!beam.Insert())
                {
                    Console.WriteLine("Insertion of stiffened base plate failed.");
                }
                beam.SetUserProperty("MemberId", revitData.MemberID);
                myModel.CommitChanges();
            }
            catch { }

            return beam;
        }
        public static ModelObject CreatePolyWall(RevitData revitData, RevitWallData revitWallData, Model myModel)
        {
            PolyBeam polyPanel = new PolyBeam(PolyBeam.PolyBeamTypeEnum.PANEL);
            try
            {
                polyPanel.Name = revitData.FamilyName;
                polyPanel.Profile.ProfileString = revitWallData.Profile;
                polyPanel.Material.MaterialString = "S235";
                polyPanel.Class = "1";

                Contour ct = new Contour();


                foreach (PointCoordinate p in revitWallData.ListPointWall)
                {
                    ContourPoint p1 = new ContourPoint(new TSD.Point(p.PointX, p.PointY, p.PointZ), new Chamfer());
                    ct.AddContourPoint(p1);

                }
                polyPanel.Contour = ct;
                polyPanel.Position.Rotation = Position.RotationEnum.TOP;
                polyPanel.Position.Plane = Position.PlaneEnum.MIDDLE;
                polyPanel.Position.Depth = Position.DepthEnum.FRONT;
                if (!polyPanel.Insert())
                {
                    Console.WriteLine("Insertion of stiffened base plate failed.");
                }
                polyPanel.SetUserProperty("MemberId", revitData.MemberID);
                myModel.CommitChanges();
            }
            catch { }
            return polyPanel;
        }
        public static ModelObject CreateModelInPlaceWall(RevitData revitData, RevitWallData revitWallData, List<PointCoordinate> pointCoordinates, Model myModel)
        {
            ContourPlate polyPanel = new ContourPlate();
            try
            {
                polyPanel.Name = revitData.FamilyName;
                polyPanel.Profile.ProfileString = revitWallData.Profile;
                polyPanel.Material.MaterialString = "S235";
                polyPanel.Class = "1";

                Contour ct = new Contour();


                foreach (PointCoordinate p in pointCoordinates)
                {
                    ContourPoint p1 = new ContourPoint(new TSD.Point(p.PointX, p.PointY, p.PointZ), new Chamfer());
                    ct.AddContourPoint(p1);

                }
                polyPanel.Contour = ct;
                //polyPanel.Position.Rotation = Position.RotationEnum.TOP;
                //polyPanel.Position.Plane = Position.PlaneEnum.MIDDLE;
                polyPanel.Position.Depth = Position.DepthEnum.FRONT;
                if (!polyPanel.Insert())
                {
                    Console.WriteLine("Insertion of stiffened base plate failed.");
                }
                polyPanel.SetUserProperty("MemberId", revitData.MemberID);
                listNeedToOpen.Add(polyPanel);
                myModel.CommitChanges();
            }
            catch { }

            return polyPanel;
        }
        public static ModelObject CreateSlab(RevitData revitData, PointCoordinateFloor PointCoordinateFloor, Model myModel)
        {
            ContourPlate PadFooting1 = new ContourPlate();
            try
            {
                if (PointCoordinateFloor.ListPointFloor.Count > 2)
                {
                    PadFooting1.Name = revitData.FamilyName + " " + revitData.MemberID;
                    PadFooting1.Profile.ProfileString = PointCoordinateFloor.Profile;
                    PadFooting1.Material.MaterialString = "C20";
                    PadFooting1.Class = "5";


                    Contour ct = new Contour();
                    var basePointZ = PointCoordinateFloor.ListPointFloor.Min(x => x.PointZ);
                    foreach (PointCoordinate p in PointCoordinateFloor.ListPointFloor)
                    {
                        var chamfer = new Chamfer();
                        //if(PointCoordinateFloor.BasePointFloor!= null)
                        //{
                        //    chamfer.DZ1 = p.PointZ-Convert.ToDouble(PointCoordinateFloor.BasePointFloor) ;
                        //    chamfer.DZ2 = p.PointZ - Convert.ToDouble(PointCoordinateFloor.BasePointFloor);
                        //}

                        ContourPoint p1 = new ContourPoint(new TSD.Point(p.PointX, p.PointY, p.PointZ), chamfer);
                        ct.AddContourPoint(p1);

                    }

                    PadFooting1.Contour = ct;
                    PadFooting1.Position.Depth = Position.DepthEnum.BEHIND;
                    if (!PadFooting1.Insert())
                    {
                        Console.WriteLine("Insertion of stiffened base plate failed.");
                    }
                    listNeedToOpen.Add(PadFooting1);
                    PadFooting1.SetUserProperty("MemberId", revitData.MemberID);
                    myModel.CommitChanges();
                }
            }
            catch { }


            return PadFooting1;
        }


        #region OpenFloor
        public static Dictionary<ModelObject, ModelObject> listIntersect;
        private static ClashCheckHandler _clashCheckHandler;
        protected static Events TsEvent;
        private static ArrayList _clashes;
        private static bool _clashCheckDone;
        private static readonly object _eventLock = new object();
        private const int MAXTIME = 30;
        private const int WAITTIME = 1500;
        public static void OpenFloor(Model myModel)
        {

            listIntersect = new Dictionary<ModelObject, ModelObject>();
            ArrayList objectsToSelect = new ArrayList();
            _clashes = new ArrayList();
            Tekla.Structures.Model.UI.ModelObjectSelector _selector = new Tekla.Structures.Model.UI.ModelObjectSelector();
            //ModelObjectEnumerator modelObjectEnumerator = myModel.GetModelObjectSelector().GetAllObjects();
            //foreach (ModelObject modelObject in modelObjectEnumerator)
            //{
            //    objectsToSelect.Add(modelObject);
            //}

            _selector.Select(listNeedToOpen);
            RunClashCheck(myModel);

            if (TsEvent != null)
            {
                TsEvent.UnRegister();
                int n = listIntersect.Count;
                string tilte = "{0} of " + n.ToString() + " terminals processed...";
                string caption = "Open Floor Progress ....";
                using (Progress_Form progress = new Progress_Form(caption, tilte, n))
                {
                    foreach (var e in listIntersect)
                    {
                        Part e1 = e.Key as Part;
                        Part e2 = e.Value as Part;
                        e2.Class = BooleanPart.BooleanOperativeClassName;
                        BooleanPart cut = new BooleanPart { Father = e1 };

                        cut.SetOperativePart(e2);
                        if (!cut.Insert())
                            Console.WriteLine("Insert failed!");
                        e2.Delete();

                        progress.Increment();
                    }
                }

            }
        }
        private static void RunClashCheck(Model myModel)
        {
            _clashCheckDone = false;
            _clashes.Clear();
            try
            {
                _clashCheckHandler = myModel.GetClashCheckHandler();
                TsEvent = new Events();
                TsEvent.ClashDetected += TsEventOnClashDetected;
                TsEvent.ClashCheckDone += TsEventOnClashCheckDone;
                TsEvent.Register();
            }
            catch (ApplicationException Exc)
            {
                Console.WriteLine("Exception: " + Exc.ToString());
            }

            DateTime start = DateTime.Now;
            _clashCheckHandler.RunClashCheck();

            TimeSpan span = new TimeSpan();

            while (!_clashCheckDone && span.TotalSeconds < MAXTIME)
            {
                System.Threading.Thread.Sleep(WAITTIME);
                DateTime end = DateTime.Now;
                span = end.Subtract(start);
            }

        }
        private static void TsEventOnClashDetected(ClashCheckData clashCheckData)
        {
            lock (_eventLock)
            {
                _clashes.Add("Clash: " + clashCheckData.Object1.Identifier.ID + " <-> " + clashCheckData.Object2.Identifier.ID + ".");
                ModelObject Object1 = clashCheckData.Object1;
                ModelObject Object2 = clashCheckData.Object2;
                double volume1 = 0;
                double volume2 = 0;
                Object1.GetReportProperty("VOLUME", ref volume1);
                Object2.GetReportProperty("VOLUME", ref volume2);
                string id1 = "";
                string id2 = "";
                Object1.GetUserProperty("MemberId", ref id1);
                Object2.GetUserProperty("MemberId", ref id2);
                if (id1 == id2)
                {
                    if (volume1 > volume2)
                    {
                        listIntersect.Add(Object1, Object2);
                    }
                    else if (volume1 < volume2)
                    {
                        listIntersect.Add(Object2, Object1);
                    }
                }
            }
        }
        private static void TsEventOnClashCheckDone(int numberClashes)
        {
            lock (_eventLock)
            {
                System.Threading.Thread.Sleep(WAITTIME);
                _clashCheckDone = true;
            }
        }
        #endregion
    }
}
