using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DeceptDCP
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() == 0) {
                Console.WriteLine("カメラの機種名を引数に指定して、実行してください。");
                Console.Read();
                return;
            }
            var cameraModel = args[0];
            var dcpList = GetDCP();

            dcpList.ForEach(dcpFile =>
            {
                var xmlFile = DecompileToXML(dcpFile);
                ModifyXML(xmlFile, cameraModel);
                CompileToDCP(xmlFile, dcpFile);
                File.Delete(xmlFile);
            });

            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Adobe\CameraRaw\CameraProfiles";
            Process.Start(path);
        }

        private static void CompileToDCP(string xmlFile, string dcpFile)
        {
            ExecuteDcpTool("-c", xmlFile, dcpFile); 
        }

        private static void ModifyXML(string xmlFile, string cameraModel)
        {
            var text = File.ReadAllText(xmlFile);
            var re = new Regex("<UniqueCameraModelRestriction>.+</UniqueCameraModelRestriction>");
            var replaceText = "<UniqueCameraModelRestriction>" + cameraModel + "</UniqueCameraModelRestriction>";
            File.WriteAllText(xmlFile, re.Replace(text, replaceText));
        }

        private static string DecompileToXML(string dcpFile)
        {
            var xmlFile = Path.GetFileNameWithoutExtension(dcpFile) + ".xml";
            ExecuteDcpTool("-d", dcpFile, xmlFile);
            return xmlFile;
        }

        private static List<string> GetDCP()
        {
            return Directory.GetFiles(Environment.CurrentDirectory,"*.dcp").ToList();
        }

        private static void ExecuteDcpTool(string option,string dcpFile,string xmlFile)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = @"dcpTool.exe";
            psi.Arguments = option + " \"" + dcpFile + "\" \"" + xmlFile + "\"";

            var p = Process.Start(psi);
            p.WaitForExit();
        }

    }
}
