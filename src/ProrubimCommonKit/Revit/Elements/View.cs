using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitServices.Persistence;
using RevitServices.Transactions;
using RS = Revit.Elements;


namespace Prorubim.Common.Revit.Elements
{
    internal class ExportExternalEvent : IExternalEventHandler
    {
        internal RS.Views.View view;
        internal int imageSize;
        internal string imagePath;
        internal RS.Element[] elements;


        public void Execute(UIApplication app)
        {
           

            var uv = view.InternalElement as Autodesk.Revit.DB.View;


            var ExportOptions = new ImageExportOptions
            {
                ZoomType = ZoomFitType.FitToPage,
                PixelSize = imageSize,
                FilePath = imagePath,
                FitDirection = FitDirectionType.Horizontal,
                HLRandWFViewsFileType = ImageFileType.PNG,
                ImageResolution = ImageResolution.DPI_600,
            };

           TransactionManager.Instance.ForceCloseTransaction();



            if (View.UIDocument.ActiveView != uv)
                View.UIDocument.ActiveView = uv;


            

            TransactionManager.Instance.EnsureInTransaction(View.Document);
            uv.IsolateElementsTemporary(elements.Select(x => x.InternalElement.Id).ToArray());
            TransactionManager.Instance.ForceCloseTransaction();

            View.Document.ExportImage(ExportOptions);

            TransactionManager.Instance.EnsureInTransaction(View.Document);
            uv.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
            TransactionManager.Instance.ForceCloseTransaction();

            View.isBusyForExport = false;
        }

        public string GetName()
        {
            return "Export External Event";
        }
    }

    public class View
    {
        public static List<ExternalEvent> evs = new List<ExternalEvent>();
        public static bool isBusyForExport;

        internal static Document Document
        {
            get { return DocumentManager.Instance.CurrentDBDocument; }
        }

        internal static UIDocument UIDocument
        {
            get { return DocumentManager.Instance.CurrentUIDocument; }
        }

        /// <summary>
        /// Get a Revit view name by view
        /// </summary>
        /// <param name="view">View object</param>
        /// <returns>The view name</returns>
        public static string ViewName(RS.Views.View view)
        {
            var uv = view.InternalElement as Autodesk.Revit.DB.View;
            return uv==null?null:uv.ViewName;
        }

        /// <summary>
        /// Export objects on view as image to file
        /// </summary>
        /// <param name="view">View object</param>
        /// <returns>The view name</returns>
        public static string ExportObjectsAsImage(RS.Views.View view, RS.Element[] elements, string imagePath, int imageSize)
        {

            var handler = new ExportExternalEvent();
            handler.view = view;
            handler.elements = elements;
            handler.imagePath = imagePath;
            handler.imageSize = imageSize;
            
            var exEvent = ExternalEvent.Create(handler);

            evs.Add(exEvent);
            isBusyForExport = true;
            exEvent.Raise();

        

            return imagePath;
        }
      
        internal View(){}
    }
}
