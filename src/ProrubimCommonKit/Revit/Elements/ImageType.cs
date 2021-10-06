using System;
using System.IO;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using RevitServices.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
using RS = Revit.Elements;

namespace Prorubim.Common.Revit.Elements
{
    public class ImageType : RS.Element
    {
        internal static Document Document
        {
            get { return DocumentManager.Instance.CurrentDBDocument; }
        }

        internal Autodesk.Revit.DB.ImageType InternalImageType
        {
            get;
            private set;
        }

        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalImageType; }
        }

        private ImageType(string path)
        {
            SafeInit(() => InitImageType(path));
        }

        private ImageType(Autodesk.Revit.DB.ImageType imageType)
        {
            SafeInit(() => InitImageType(imageType));
        }

        private void InitImageType(Autodesk.Revit.DB.ImageType imageType)
        {
            InternalSetImageType(imageType);
            ElementBinder.CleanupAndSetElementForTrace(Document, InternalElement);
        }

        private void InitImageType(string path)
        {
            var imName = new FileInfo(path).Name;

            var res =
                new FilteredElementCollector(Document).OfClass(typeof(Autodesk.Revit.DB.ImageType))
                    .Cast<Autodesk.Revit.DB.ImageType>()
                    .Where(x => new FileInfo(x.Path).Name == imName);

            TransactionManager.Instance.EnsureInTransaction(Document);

            Autodesk.Revit.DB.ImageType im;

            var imPOpts = new ImageTypeOptions(path, false, ImageTypeSource.Import);

            if (File.Exists(path) && res.Any())
            {
                res.First().ReloadFrom(imPOpts);
                im = res.First();
            }
            else
            {
                im = Autodesk.Revit.DB.ImageType.Create(Document, imPOpts);
            }

            TransactionManager.Instance.TransactionTaskDone();
            TransactionManager.Instance.ForceCloseTransaction();

            InternalSetImageType(im);
            ElementBinder.CleanupAndSetElementForTrace(Document, InternalElement);
        }

        private void InternalSetImageType(Autodesk.Revit.DB.ImageType im)
        {
            InternalImageType = im;
            InternalElementId = im.Id;
            InternalUniqueId = im.UniqueId;
        }

        /// <summary>
        /// Get ImageType in current project by it`s source path
        /// </summary>
        /// <param name="path">ImageType source path</param>
        /// <returns>ImageType object</returns>
        public static ImageType ByPath(string path)
        {
            var imName = new FileInfo(path).Name;

            var res =
                new FilteredElementCollector(Document).OfClass(typeof(Autodesk.Revit.DB.ImageType))
                    .Cast<Autodesk.Revit.DB.ImageType>()
                    .Where(x => new FileInfo(x.Path).Name == imName);

            if (res.Any())
                return new ImageType(res.First());

            return null;
        }

        /// <summary>
        /// Load image using it`s path into current project
        /// </summary>
        /// <param name="path">Image source path</param>
        /// <returns>ImageType object</returns>
        public static ImageType Load(string path)
        {
            return new ImageType(path);
        }

        public override string ToString()
        {
            return String.Format("ImageType - {0}", InternalImageType.Path);
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
