using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using RevitServices.Elements;
using RevitServices.Persistence;
using System;
using RS = Revit.Elements;


namespace Prorubim.Common.Revit.Elements
{
    [DynamoServices.RegisterForTrace]
    public class LegendView : RS.Views.View
    {
        internal static Document Document
        {
            get { return DocumentManager.Instance.CurrentDBDocument; }
        }

        internal Autodesk.Revit.DB.View InternalViewLegend
        {
            get; private set;
        }
        
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalViewLegend; }
        }

        
        private LegendView(string name)
        {
            SafeInit(() => InitLegendView(name));
        }

        private void InitLegendView(string name)
        {
            var collector = new FilteredElementCollector(Document);
            var view = collector.OfClass(typeof(Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>().First(x => x.ViewType == ViewType.Legend && x.ViewName == name);

            InternalSetLegendView(view);
            ElementBinder.CleanupAndSetElementForTrace(Document, InternalElement);
        }

        private void InternalSetLegendView(Autodesk.Revit.DB.View legend)
        {
            InternalViewLegend = legend;
            InternalElementId = legend.Id;
            InternalUniqueId = legend.UniqueId;
        }

        /// <summary>
        /// Get a Revit LegendView by it's name
        /// </summary>
        /// <param name="name">Name of the legend view</param>
        /// <returns>The legend view</returns>
        public static LegendView ByName( string name )
        {
            if (name == null)
                throw new ArgumentNullException("name");
            return new LegendView( name );
        }

        [IsVisibleInDynamoLibrary(false)]
        public override void Dispose()
        {
            // Do not cleanup Revit elements if we are shutting down Dynamo or
            // closing homeworkspace.
            //if (DisposeLogic.IsShuttingDown || DisposeLogic.IsClosingHomeworkspace)
            //  return;
            //    InternalElementId = null;
        }
    }
}
