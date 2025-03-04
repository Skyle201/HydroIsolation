using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydroIsolation.Helpers
{
    public class FaceHelper
    {
        public XYZ FindMainDirection(Face face)
        {
            EdgeArray edgeArray = face.EdgeLoops.get_Item(0);
            List<Edge> edges = edgeArray.Cast<Edge>().ToList();

            // Находим самое длинное ребро
            Edge longestEdge = edges.OrderByDescending(edge => edge.AsCurve().Length).FirstOrDefault();

            if (longestEdge == null)
            {
                throw new InvalidOperationException("Не удалось найти рёбра на грани.");
            }

            // Получаем вектор самого длинного ребра
            Curve longestCurve = longestEdge.AsCurve();
            XYZ longestDirection = (longestCurve.GetEndPoint(1) - longestCurve.GetEndPoint(0)).Normalize();

            return longestDirection;
        }

        public (double width, double height) GetRectangularProjectionDimensions(Face face)
        {
            BoundingBoxUV bbox = face.GetBoundingBox();
            if (bbox == null) throw new ArgumentException("shit");
            double width = bbox.Max.U - bbox.Min.U;
            double height = bbox.Max.V - bbox.Min.V;
            return (width, height);
        }
        public XYZ GetCentroidOfFace(Face face)
        {
            Mesh mesh = face.Triangulate();
            XYZ centroid = new XYZ(0, 0, 0);
            int numVertices = 0;

            foreach (XYZ vertex in mesh.Vertices)
            {
                centroid += vertex;
                numVertices++;
            }

            if (numVertices > 0)
            {
                centroid /= numVertices; // Делим сумму координат вершин на их количество
            }

            return centroid;
        }

        public Face GetFaceFromReference(Document doc, Reference reference)
        {
            Element element = doc.GetElement(reference);
            if (element == null)
            {
                throw new InvalidOperationException("Элемент не найден.");
            }

            Options options = new Options
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Fine
            };

            GeometryElement geometryElement = element.get_Geometry(options);
            return FindFaceRecursive(geometryElement, reference, doc);
        }

        public Face FindFaceRecursive(GeometryElement geometryElement, Reference reference, Document doc)
        {
            foreach (GeometryObject geomObj in geometryElement)
            {
                if (geomObj is Face face && face.Reference != null && reference.ConvertToStableRepresentation(doc).Equals(face.Reference.ConvertToStableRepresentation(doc)))
                {
                    return face;
                }
                else if (geomObj is GeometryInstance instance)
                {
                    GeometryElement subGeometry = instance.GetInstanceGeometry();
                    Face result = FindFaceRecursive(subGeometry, reference, doc);
                    if (result != null) return result;
                }
                else if (geomObj is Solid solid)
                {
                    foreach (Face subFace in solid.Faces)
                    {
                        if (subFace.Reference != null && reference.ConvertToStableRepresentation(doc).Equals(subFace.Reference.ConvertToStableRepresentation(doc)))
                        {
                            return subFace;
                        }
                    }

                }
            }

                return null;
        }
        public bool IsHorizontalFace(Face face)
        {
            XYZ normal = face.ComputeNormal(new UV(0.5, 0.5)).Normalize();

            return Math.Abs(normal.Z) > 0.9;
        }
    }
}

