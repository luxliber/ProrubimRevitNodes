using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Element = Autodesk.Revit.DB.Element;
using FamilyInstance = Autodesk.Revit.DB.FamilyInstance;
using Line = Autodesk.Revit.DB.Line;
using RS = Revit.Elements;

namespace Prorubim.Common.Revit.Scattering
{
    public class Elements
    {
        internal static Document Document => DocumentManager.Instance.CurrentDBDocument;

        private Elements() { }

        internal static double GetRotationAngleOfInstance(Element fi)
        {
            var viewDirection = Document.ActiveView.ViewDirection;
            var rightDirection = Document.ActiveView.RightDirection;

            if(fi.GetType() == typeof(FamilyInstance))
            {
                var trf = ((FamilyInstance) fi).GetTransform();
                return -trf.BasisX.AngleOnPlaneTo(rightDirection, viewDirection);
            }
            
            if(fi.GetType() == typeof(AssemblyInstance))
            {

                var trf = ((AssemblyInstance) fi).GetTransform();
                return -trf.BasisX.AngleOnPlaneTo(rightDirection, viewDirection);
            }
            
            if (fi.GetType() == typeof(Autodesk.Revit.DB.Group))
            {
                var fii =((Autodesk.Revit.DB.Group)fi).GetMemberIds().Select(x=>Document.GetElement(x)).Where(x => x.Location.GetType() == typeof(LocationPoint));
                if(fii.Any())
                {
                    var loc = fii.First().Location;
                    if (loc.GetType() == typeof (LocationPoint))
                        return ((LocationPoint) loc).Rotation;
                }
            }

            return 0;
        }

        internal static XYZ GetLocationOfInstance(Element fi)
        {
            if (fi.GetType() == typeof(FamilyInstance) || fi.GetType() == typeof(Autodesk.Revit.DB.Group))
            {
                if (fi.Location is LocationPoint loc) return loc.Point;
            }

            if (fi.GetType() == typeof(AssemblyInstance))
            {
                var fii = ((AssemblyInstance)fi).GetMemberIds().Select(x => Document.GetElement(x)).Where(x => x.Location != null);
                if (fii.Any())
                {
                    var loc = fi.Location;
                    if(loc.GetType()==typeof(LocationPoint))
                        return ((LocationPoint)loc).Point;

                    if (loc.GetType() == typeof(LocationCurve))
                        return ((LocationCurve)loc).Curve.GetEndPoint(0);
                }
            }

            return new XYZ();
        }

        /// <summary>
        /// Scatter element, assembly or group in many locations and orientations using reference alignment element and elements array as locations list
        /// </summary>
        /// <param name="scatteredElement">Element, assembly or group for scattering</param>
        /// <param name="alignmentElement">Reference alignment element for base location</param>
        /// <param name="elementsArray">Elements array for new scattered objects locations</param>
        /// <returns>Scattered elements list</returns>
        public static List<RS.Element> ScatterAndAlign(RS.Element scatteredElement, RS.Element alignmentElement, List<RS.Element> elementsArray )
        {
            if (!(scatteredElement.InternalElement.Location is LocationPoint) || !(alignmentElement.InternalElement.Location is LocationPoint))
                throw new Exception("Scatter element or alignment element don`t have point location");

            var res = new List<RS.Element>();
            
            TransactionManager.Instance.EnsureInTransaction(Document);

            foreach (var el in elementsArray)
                if (el.InternalElement.Location is LocationPoint)
                {
                    var transPoint =
                        GetLocationOfInstance(el.InternalElement)
                            .Subtract(GetLocationOfInstance(alignmentElement.InternalElement));

                    var newElId = ElementTransformUtils.CopyElement(Document, scatteredElement.InternalElement.Id,
                        transPoint);
                    var newEl = Document.GetElement(newElId.First());

                    var ang = GetRotationAngleOfInstance(el.InternalElement) -
                              GetRotationAngleOfInstance(alignmentElement.InternalElement);

                    var p1 = GetLocationOfInstance(el.InternalElement);
                    var p2 = p1.Add(new XYZ(0, 0, 1));

                    var axis = Line.CreateBound(p1, p2);

                    ElementTransformUtils.RotateElement(Document, newEl.Id, axis, ang);

                    res.Add(newEl.ToDSType(false));
                }

            Document.Regenerate();
            TransactionManager.Instance.ForceCloseTransaction();

            return res;
        }
    }
}
