using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitToTekla
{
    [Transaction(TransactionMode.Manual)]

    public class RevitToTekla : IExternalCommand
    {
        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Application app = uiApp.Application;
            Document doc = uidoc.Document;


            var form = new RevitExport();
            form.ShowDialog();
            if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                string folder_path = "";
                string filename = doc.PathName;

                if (0 == filename.Length)
                {
                    filename = doc.Title;
                }
                filename = Path.GetFileNameWithoutExtension(filename) + ".json";
                bool check = Command.SelectFile(ref folder_path, ref filename);
                if (check)
                {
                    using (TransactionGroup tg = new TransactionGroup(doc, "Export"))
                    {
                        tg.Start();
                        bool unjoin = Command.UnjoinFloor(doc);
                        if (unjoin)
                        {
                            filename = Path.Combine(folder_path, filename);
                            Command.ExportToTekla(doc, filename);
                        }
                        tg.Assimilate();
                    }
                }         
            }
            return Result.Succeeded;

        }
    }


}
