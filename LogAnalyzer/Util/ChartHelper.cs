using OxyPlot;
using OxyPlot.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LogAnalyzer.Util {
    public static class ChartHelper {
        public static void Save(this PlotModel model, string directory, int width, int height, string name = ""){
            BitmapSource bitmap = PngExporter.ExportToBitmap(model, width, height, OxyColors.White);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            if (string.IsNullOrWhiteSpace(name)) {
                name = System.Guid.NewGuid().ToString();
            }

            String photolocation = System.IO.Path.Combine(directory, name + ".jpg");  //file name 

            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var filestream = new FileStream(photolocation, FileMode.Create))
                encoder.Save(filestream);
        }
    }
}
