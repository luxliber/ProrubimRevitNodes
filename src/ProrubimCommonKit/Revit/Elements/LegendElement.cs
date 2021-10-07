using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using RevitServices.Elements;
using RevitServices.Persistence;
using RS = Revit.Elements;

namespace Prorubim.Common.Revit.Elements
{
    [DynamoServices.RegisterForTrace]
    public class LegendElement : RS.Element
    {
        internal static Document Document => DocumentManager.Instance.CurrentDBDocument;

        private readonly Autodesk.Revit.DB.Element _internalElement;
        private readonly ElementId _elementTypeId;

        private LegendElement(Autodesk.Revit.DB.Element element)
        {
            _internalElement = element;

            SafeInit(() => InitLegendElement(element));
            _elementTypeId = element.get_Parameter(BuiltInParameter.LEGEND_COMPONENT).AsElementId();
        }

        private void InitLegendElement(Autodesk.Revit.DB.Element element)
        {
            InternalElementId = element.Id;
            InternalUniqueId = element.UniqueId;
        }

        /// <summary>
        /// Get LegendElement in some existing LegendView by it`s FamilyType
        /// </summary>
        /// <param name="elementType">FamilyType for searching legend elements</param>
        /// <returns>Found legend elements list</returns>
        public static IList<LegendElement> ByElementType(RS.ElementType elementType)
        {
            var elType = elementType.InternalElement;
            
            if (elType != null)
            {
                var allLegendElements = new FilteredElementCollector(Document).OfCategory(BuiltInCategory.OST_LegendComponents).ToElements();

                var symbolLevelElements =
                    allLegendElements.Where(x => x.get_Parameter(BuiltInParameter.LEGEND_COMPONENT).AsElementId() == elType.Id).ToList();

                return symbolLevelElements.Select(element => new LegendElement(element)).ToList();
            }

            return new List<LegendElement>();
        }

        public override Autodesk.Revit.DB.Element InternalElement
        {
            get
            {
                return _internalElement;
            }
        }

        public override string ToString() => $"Legend Component - FamilyType Id: {_elementTypeId}";
    }
}
