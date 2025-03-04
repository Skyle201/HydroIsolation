using Autodesk.Revit.DB;
using HydroIsolation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydroIsolation.Model
{
    public class FamilyPlacer
    {
        Document doc {  get; set; } 
        public void Place(FamilySymbol FamSym,List<Face> faces)
        {
            if (!FamSym.IsActive)
            {
                FamSym.Activate();
                doc.Regenerate();
            }
            var faceHelper = new FaceHelper();
            foreach (var face in faces)
            {
                if (face == null) continue;
                var planarFace = face as PlanarFace;
                var centroid = faceHelper.GetCentroidOfFace(face);
                var direction = faceHelper.FindMainDirection(face);

                var familyInstance = doc.Create.NewFamilyInstance(face, centroid, direction, FamSym);
                (double width, double height) = faceHelper.GetRectangularProjectionDimensions(face);
                var a = familyInstance.LookupParameter("a");
                var b = familyInstance.LookupParameter("b");
                var c = familyInstance.LookupParameter("Комментарии");
                a.Set(Math.Min(width, height)); b.Set(Math.Max(width,height));
                c.Set(faceHelper.IsHorizontalFace(face)? "Горизонтальная гидроизоляция" : "Вертикальная гидроизоляция");

            }
        }
        public FamilyPlacer(Document doc)
        {
            this.doc = doc;
        }
    }
}
