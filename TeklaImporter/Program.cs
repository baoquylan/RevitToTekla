using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tekla.Structures;

namespace TeklaImporter
{
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //ResolveEventHandler CurrentDomain_AssemblyResolve = null;
            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            TeklaImporter form1 = new TeklaImporter();
            //form1.Show(TeklaStructures.MainWindow);
            Application.Run(form1);
        }
    }
}
