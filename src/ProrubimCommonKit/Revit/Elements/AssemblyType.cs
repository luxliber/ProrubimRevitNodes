using System.Linq;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using System;
using RevitServices.Transactions;
using RS = Revit.Elements;


namespace Prorubim.Common.Revit.Elements
{
    [DynamoServices.RegisterForTrace]
    public class AssemblyType : RS.Element
    {
        internal static Document Document
        {
            get { return DocumentManager.Instance.CurrentDBDocument; }
        }

        internal Autodesk.Revit.DB.AssemblyType InternalAssemblyType
        {
            get; private set;
        }
        
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalAssemblyType; }
        }

        
        private AssemblyType(string name)
        {
            SafeInit(() => InitAssemblyType(name));
        }

        internal AssemblyType(Autodesk.Revit.DB.AssemblyType el)
        {
            SafeInit(() => InitAssemblyType(el));
        }

        private void InitAssemblyType(string name)
        {
            var els = DocumentManager.Instance.ElementsOfType<Autodesk.Revit.DB.AssemblyType>().First(x => x.Name == name);
            InternalSetAssemblyType(els);
        }

        private void InitAssemblyType(Autodesk.Revit.DB.AssemblyType el)
        {
            InternalSetAssemblyType(el);
        }

        private void InternalSetAssemblyType(Autodesk.Revit.DB.AssemblyType legend)
        {
            InternalAssemblyType = legend;
            InternalElementId = legend.Id;
            InternalUniqueId = legend.UniqueId;
        }

        /// <summary>
        /// Get existing AssemblyType by it`s name
        /// </summary>
        /// <param name="name">AssemblyType name</param>
        /// <returns>AssemblyType object</returns>
        public static AssemblyType ByName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            return new AssemblyType(name);
        }

        /// <summary>
        /// Get all assembly instances objects of this assemply type
        /// </summary>
        public AssemblyInstance[] AllInstances
        {
            get
            {
                return DocumentManager.Instance.ElementsOfType<Autodesk.Revit.DB.AssemblyInstance>()
                    .Where(x => x.GetTypeId() == InternalAssemblyType.Id)
                    .Select(x => new AssemblyInstance(x)).ToArray();
            }
        }

        /// <summary>
        /// Rename AssemblyType
        /// </summary>
        /// <param name="newName">Assembly type new name</param>
        /// <returns>Renamed assemblyType object</returns>
        public AssemblyType Rename(string newName)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);
            InternalAssemblyType.Name = newName;
            TransactionManager.Instance.TransactionTaskDone();

            return this;
        }

        /// <summary>
        /// Convert Element object to AssemblyType object if it`s possible for further using 
        /// </summary>
        /// <param name="element">Casting Element</param>
        /// <returns>Converted AssemblyType object</returns>
        public static AssemblyType CastFromElement(RS.Element element)
        {
            if (element.InternalElement.GetType() == typeof(Autodesk.Revit.DB.AssemblyType))
                return new AssemblyType(element.InternalElement as Autodesk.Revit.DB.AssemblyType);
            throw new Exception("Element is not AssemblyType");
        }
  }
}
