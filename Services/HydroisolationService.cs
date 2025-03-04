using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using HydroIsolation.Helpers;
using HydroIsolation.Model;
using System.Collections.Generic;
using System.Linq;

namespace HydroIsolation.Services
{
    public class HydroisolationService
    {
        public void Create(Document doc, UIDocument uidoc)
        {
            var faceHelper = new FaceHelper();
            var faces = new List<Face>();
            Options geomOptions = new Options() { ComputeReferences = true, IncludeNonVisibleObjects = true };

            IList<Reference> refs = uidoc.Selection.PickObjects(ObjectType.Face);

            foreach (Reference reference in refs) 
            {
               var face = faceHelper.GetFaceFromReference(doc, reference);
               faces.Add(face);
            }

            var Family = new FilteredElementCollector(doc)
            .OfClass(typeof(Family))
            .Cast<Family>()
            .FirstOrDefault(f => f.Name == "GI");
            var famSym = doc.GetElement(Family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;


            var familyPlacer = new FamilyPlacer(doc);
            using (var trans = new Transaction(doc, "Установка гидроизоляции"))
            {
                trans.Start();
                familyPlacer.Place(famSym, faces);

                trans.Commit();
            }
            }
    }
}
