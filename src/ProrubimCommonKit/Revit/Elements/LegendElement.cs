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
        internal static Document Document
        {
            get { return DocumentManager.Instance.CurrentDBDocument; }
        }

        private readonly Autodesk.Revit.DB.Element _internalElement;
        private readonly ElementId _familySymbolId;

        private LegendElement(Autodesk.Revit.DB.Element element)
        {
            _internalElement = element;

            SafeInit(() => InitLegendElement(element));
            _familySymbolId = element.get_Parameter(BuiltInParameter.LEGEND_COMPONENT).AsElementId();
        }

        private void InitLegendElement(Autodesk.Revit.DB.Element element)
        {
            InternalElementId = element.Id;
            InternalUniqueId = element.UniqueId;
        }

        /// <summary>
        /// Get LegendElement in some existing LegendView by it`s FamilyType
        /// </summary>
        /// <param name="familyType">FamilyType for searching legend elements</param>
        /// <returns>Found legend elements list</returns>
        public static IList<LegendElement> ByFamilyType(RS.FamilyType familyType)
        {
            var fs = familyType.InternalElement as FamilySymbol;
            
            if (fs != null)
            {
                var allLegendElements = new FilteredElementCollector(Document).OfCategory(BuiltInCategory.OST_LegendComponents).ToElements();

                var symbolLevelElements =
                    allLegendElements.Where(x => x.get_Parameter(BuiltInParameter.LEGEND_COMPONENT).AsElementId() == fs.Id).ToList();

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

        public override string ToString()
        {
            return String.Format("Legend Component - FamilyType Id: {0}", _familySymbolId);
        }


    }
}
