using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitToTekla
{
    public class CoordinateRevit
    {
        public List<RevitData> RevitDatas { get; set; }
    }
    public class RevitData
    {
        public string MemberID { get; set; }
        public string RevitID { get; set; }
        public string CategoryName { get; set; }
        public string StructuralType { get; set; }
        public string FamilyName { get; set; }
        public RevitColumnData RevitColumn { get; set; }
        public RevitBeamData RevitBeam { get; set; }
        public List<RevitWallData> RevitWall { get; set; }
        public List<PointCoordinate> PointCoordinates { get; set; }
        public List<PointCoordinateFloor> PointCoordinatesFloor { get; set; }

    }
    public class RevitColumnData
    {
        public double Rotation { get; set; }
        public string Profile { get; set; }
    }
    public class RevitBeamData
    {
        public double Rotation { get; set; }
        public string Profile { get; set; }
        public string yJustification { get; set; }
        public double StartYOffsetValue { get; set; }
        public double EndYOffsetValue { get; set; }
        public string zJustification { get; set; }
        public double StartZOffsetValue { get; set; }
        public double EndZOffsetValue { get; set; }
        public bool PolyBeam { get; set; }
        public List<PointCoordinate> PointCoordinates { get; set; }

    }
    public class RevitWallData
    {
        public string Profile { get; set; }
        public List<PointCoordinate> ListPointWall { get; set; }
        public List<List<PointCoordinate>> ListPointWallInPlace { get; set; }
        public bool ModelInPlace { get; set; }
    }
    public class PointCoordinateFloor
    {
        public string Profile { get; set; }
        public string BasePointFloor { get; set; }
        public List<PointCoordinate> ListPointFloor { get; set; }
        public List<PointCoordinate> ListPointOpen { get; set; }
    }
    public class PointCoordinate
    {
        public double PointX { get; set; }
        public double PointY { get; set; }
        public double PointZ { get; set; }
    }
}
