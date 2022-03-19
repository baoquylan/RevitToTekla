using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

namespace TeklaImporter
{
    public partial class TeklaImporter : Form
    {
        public Model myModel;

        public TeklaImporter()
        {
            InitializeComponent();
        }
        private void TeklaImporter_Load(object sender, EventArgs e)
        {
            myModel = new Model();
            if (!myModel.GetConnectionStatus())
            {
                MessageBox.Show("Tekla Structures not connected");
                return;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //var selection = new Picker().PickObjects(Picker.PickObjectsEnum.PICK_N_OBJECTS, "Pick Objects");


            //return;
            using (OpenFileDialog ofd = new OpenFileDialog() { Multiselect = false, ValidateNames = true, Filter = "JSON|*.json" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using (StreamReader file = File.OpenText(ofd.FileName))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        CoordinateRevit coordinateRevit = (CoordinateRevit)serializer.Deserialize(file, typeof(CoordinateRevit));
                        Command.listNeedToOpen = new System.Collections.ArrayList();
                        if (coordinateRevit == null) return;
                        if (coordinateRevit.RevitDatas.Count != 0)
                        {
                            int n = coordinateRevit.RevitDatas.Count;
                            string tilte = "{0} of " + n.ToString() + " terminals processed...";
                            string caption = "Import Revit Data Progress ....";
                            using (Progress_Form progress = new Progress_Form(caption, tilte, n))
                            {
                                foreach (RevitData revitData in coordinateRevit.RevitDatas)
                                {
                                    if (revitData.CategoryName == "Structural Columns" || revitData.CategoryName == "Columns")
                                    {
                                        if (revitData.PointCoordinates.Count == 2)
                                        {
                                            Command.CreateColumns(revitData, myModel);
                                        }
                                    }
                                    else if (revitData.CategoryName == "Structural Framing")
                                    {
                                        if (revitData.RevitBeam.PolyBeam == false)
                                        {
                                            if (revitData.RevitBeam.PointCoordinates.Count == 2)
                                            {
                                                Command.CreateBeam(revitData, myModel);
                                            }

                                        }
                                        else
                                        {
                                            Command.CreatePolyBeam(revitData, myModel);
                                        }

                                    }
                                    else if (revitData.CategoryName == "Walls")
                                    {
                                        if (revitData.RevitWall != null)
                                        {
                                            foreach (RevitWallData revitWallData in revitData.RevitWall)
                                            {
                                                if (revitWallData.ModelInPlace == false)
                                                {
                                                    if (revitWallData.ListPointWall.Count == 2)
                                                    {
                                                        Command.CreateWallStraight(revitData, revitWallData, myModel);
                                                    }
                                                    else if (revitWallData.ListPointWall.Count > 2)
                                                    {
                                                        Command.CreatePolyWall(revitData, revitWallData, myModel);
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (List<PointCoordinate> pointCoordinates in revitWallData.ListPointWallInPlace)
                                                        Command.CreateModelInPlaceWall(revitData, revitWallData, pointCoordinates, myModel);
                                                }
                                            }
                                        }

                                    }
                                    else if (revitData.CategoryName == "Floors" || revitData.CategoryName == "Structural Foundations")
                                    {
                                        if (revitData.PointCoordinatesFloor != null)
                                        {
                                            foreach (var pointCoordinatesFloor in revitData.PointCoordinatesFloor)
                                            {
                                                if (pointCoordinatesFloor.ListPointFloor.Count != 0)
                                                    Command.CreateSlab(revitData, pointCoordinatesFloor, myModel);

                                            }
                                        }


                                    }
                                    progress.Increment();
                                }

                            }

                        }
                        Command.OpenFloor(myModel);
                        myModel.CommitChanges();
                    }

                }
            }
        }


    }
    
}
