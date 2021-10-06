using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RS = Revit.Elements;

namespace Prorubim.Common.Revit.Elements
{
    public class Element
    {
        internal static Document Document => DocumentManager.Instance.CurrentDBDocument;


        private Element(){}

        /// <summary>
        /// Find neighbour elements for base element using it`s bounding box with some additional offset
        /// </summary>
        /// <param name="element">Base element for neighbours searching</param>
        /// <param name="offset">Additional offset area for neighbours detecting</param>
        /// <param name="view">View for neighbours searching</param>
        /// <returns>Found neighbour elements</returns>
        public static IList<RS.Element> FindNeighbourElements(RS.Element element, RS.Element view, double offset)
        {
            var uEl = element.InternalElement;
            var uView = view.InternalElement as Autodesk.Revit.DB.View;

            if (uView == null) return new List<RS.Element>();

            var elBbox = uEl.get_BoundingBox(uView);

            if (elBbox == null) return new List<RS.Element>();

            var nEls = new FilteredElementCollector(Document, uView.Id);
            var idsExclude = new List<ElementId> {uEl.Id};

            var nElsComplete = nEls.Excluding(idsExclude).ToElements();

            var minp = elBbox.Min.ToPoint().Add(Vector.ByCoordinates(-offset, -offset, 0));
            var maxp = elBbox.Max.ToPoint().Add(Vector.ByCoordinates(offset, offset, 0));

            var exElBbox = BoundingBox.ByCorners(minp, maxp);

            var resList = new List<RS.Element>();

            foreach (var el in nElsComplete)
            {
                var uBbox = el.get_BoundingBox(uView);
                if (uBbox != null)
                {
                    var bbox = BoundingBox.ByCorners(uBbox.Min.ToPoint(), uBbox.Max.ToPoint());
                    if (exElBbox.Intersects(bbox))
                        resList.Add(el.ToDSType(true));
                }
            }

            return resList;
        }

        /// <summary>
        /// Find similar elements by same category and type as base element
        /// </summary>
        /// <param name="element">Base element for similar elements searching</param>
        /// <returns>Found similar elements</returns>
        public static RS.Element[] FindSimilarElements(RS.Element element)
        {
           var bic = (BuiltInCategory) System.Enum.ToObject(typeof (BuiltInCategory), element.InternalElement.Category.Id.IntegerValue);
           var els =  DocumentManager.Instance.ElementsOfCategory(bic)
               .Where(x=>x.GetTypeId().IntegerValue == element.InternalElement.GetTypeId().IntegerValue)
               .Select(x=>x.ToDSType(true))
               .ToArray();

            return els;
        }
    }
}
