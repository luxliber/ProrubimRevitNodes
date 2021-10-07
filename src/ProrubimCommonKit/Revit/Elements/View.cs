using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;
using RS = Revit.Elements;


namespace Prorubim.Common.Revit.Elements
{
    /// <summary>View class</summary>
    public class View
    {
        internal static Document Document => DocumentManager.Instance.CurrentDBDocument;
        
        /// <summary>Export objects on view as image to file</summary>
        /// <param name="view">View object</param>
        /// <param name="elements">Exported elements list</param>
        /// <param name="imagePath">Image path</param>
        /// <param name="imageSize">Image size in pixels</param>
        /// <param name="loadImage">Is image should be loaded into the project after export</param>
        /// <param name="deleteImageFile">Is image file should be deleted after export</param>
        /// <returns>Image path and (or) ImageType element</returns>
        [MultiReturn("path", "imageType")]
        public static Dictionary<string, object> ExportObjectsAsImage(RS.Views.View view, RS.Element[] elements, string imagePath, int imageSize = 500, bool loadImage = true, bool deleteImageFile = true)
        {
            if (!(view.InternalElement is Autodesk.Revit.DB.View internalElement))
                throw new Exception("Object in view input is ot View element");

            var path1 = Path.GetDirectoryName(imagePath);
            var withoutExtension = Path.GetFileNameWithoutExtension(imagePath);
            var ext = Path.GetExtension(imagePath);

            if (path1 == null)
                throw new Exception("Image path: '" + imagePath + "' is not correct");

            if (string.IsNullOrEmpty(path1))
                path1 = Path.GetTempPath();

            if (string.IsNullOrEmpty(ext) || ext.ToLower() != ".png" || ext.ToLower() != ".jpg" || ext.ToLower() != ".tga" || ext.ToLower() != ".tif" || ext.ToLower() != ".bmp")
                ext = ".png";
            else if (!string.IsNullOrEmpty(ext))
                withoutExtension += ext;

            imagePath = Path.Combine(path1, withoutExtension + ext);

            var imageExportOptions1 = new ImageExportOptions
            {
                ExportRange = (ExportRange)2,
                ZoomType = (ZoomFitType)0,
                PixelSize = imageSize,
                FilePath = imagePath,
                FitDirection = (FitDirectionType)0,
                HLRandWFViewsFileType = View.ImageFileTypeFromExtension(ext),
                ShadowViewsFileType = View.ImageFileTypeFromExtension(ext),
                ImageResolution = (ImageResolution)3,
                ShouldCreateWebSite = false
            };

            var elementIds = new FilteredElementCollector(Document, internalElement.Id).WhereElementIsNotElementType().ToElementIds();

            TransactionManager.Instance.EnsureInTransaction(View.Document);

            internalElement.IsolateElementsTemporary((elements).Select(x => x.InternalElement.Id).ToArray());
            internalElement.ConvertTemporaryHideIsolateToPermanent();

            TransactionManager.Instance.ForceCloseTransaction();
            var imageExportOptions2 = imageExportOptions1;
            var elementIdList = new List<ElementId> { internalElement.Id };
            imageExportOptions2.SetViewsAndSheets(elementIdList);

            Document.ExportImage(imageExportOptions1);

            TransactionManager.Instance.EnsureInTransaction(Document);

            if (elementIds.Count > 0)
                internalElement.UnhideElements(elementIds);

            TransactionManager.Instance.ForceCloseTransaction();

            var str = Path.Combine(path1, withoutExtension);
            var sourceFileName = str + " - " + internalElement.Title.Split(':')[0] + " - " + internalElement.Name + ext;
            var path = str + ext;

            if (File.Exists(path))
                File.Delete(path);

            ImageType imageType = null;
            var destFileName = path;

            File.Move(sourceFileName, destFileName);

            if (loadImage)
                imageType = ImageType.Load(path);

            if (deleteImageFile)
                File.Delete(path);

            return new Dictionary<string, object>
            {
                {"path", path},
                {"imageType", imageType}
            };
        }

        private static ImageFileType ImageFileTypeFromExtension(string ext)
        {
            switch (ext.ToLower())
            {
                case ".jpg":
                    return (ImageFileType)1;
                case ".png":
                    return (ImageFileType)4;
                case ".tga":
                    return (ImageFileType)5;
                case ".tif":
                    return (ImageFileType)6;
                case ".bmp":
                    return (ImageFileType)0;
                default:
                    return (ImageFileType)4;
            }
        }


       

        internal View() { }
    }
}
