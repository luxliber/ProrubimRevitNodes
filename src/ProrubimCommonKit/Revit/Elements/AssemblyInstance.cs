using System.Linq;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using RevitServices.Elements;
using RevitServices.Persistence;
using System;
using System.Collections.Generic;
using Revit.Elements;
using RevitServices.Transactions;
using RS = Revit.Elements;


namespace Prorubim.Common.Revit.Elements
{
    [DynamoServices.RegisterForTrace]
    public class AssemblyInstance : RS.Element
    {
        internal static Document Document
        {
            get { return DocumentManager.Instance.CurrentDBDocument; }
        }

        internal Autodesk.Revit.DB.AssemblyInstance InternalAssemblyInstance
        {
            get; private set;
        }
        
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalAssemblyInstance; }
        }


        internal AssemblyInstance(Autodesk.Revit.DB.AssemblyInstance el)
        {
            SafeInit(() => InitAssemblyInstance(el));
        }

        private AssemblyInstance(RS.Element[] elements)
        {
            SafeInit(() => InitAssemblyInstance(elements));
        }

        private void InitAssemblyInstance(Autodesk.Revit.DB.AssemblyInstance el)
        {
            InternalSetAssemblyInstance(el);
        }

        private void InitAssemblyInstance(RS.Element[] elements)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);
            
            var ass = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.AssemblyInstance>(Document);

            if (ass == null)
                ass = Autodesk.Revit.DB.AssemblyInstance.Create(Document,
                    elements.Select(x => x.InternalElement.Id).ToList(), elements[0].InternalElement.Category.Id);
            else
                ass.SetMemberIds(elements.Select(x => x.InternalElement.Id).ToList());

            InternalSetAssemblyInstance(ass);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.CleanupAndSetElementForTrace(Document, InternalElement);
        }

        private void InternalSetAssemblyInstance(Autodesk.Revit.DB.AssemblyInstance el)
        {
            InternalAssemblyInstance = el;
            InternalElementId = el.Id;
            InternalUniqueId = el.UniqueId;
        }

        /// <summary>
        /// Rename assembly type (this node is like AssemblyType.Rename)
        /// </summary>
        /// <param name="newName">New name for AssemblyType</param>
        /// <returns>Passing AssemblyInstance</returns>
        public AssemblyInstance RenameAssemblyType(string newName)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);
            InternalAssemblyInstance.AssemblyTypeName = newName;
            TransactionManager.Instance.TransactionTaskDone();

            return this;
        }

        /// <summary>
        /// Change assembly type for current assembly instance
        /// </summary>
        /// <param name="newAssemblyType">AssemblyType object for assigning</param>
        /// <returns>Passing AssemblyInstance</returns>
        public AssemblyInstance ChangeAssemblyType(AssemblyType newAssemblyType)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);
            InternalAssemblyInstance.ChangeTypeId(newAssemblyType.InternalAssemblyType.Id);
            TransactionManager.Instance.TransactionTaskDone();

            return this;
        }

        /// <summary>
        /// Replace assembly members in all same assembly instances with option for deleting or detaching old elements. If option is "false" then old elements will be only detached from assembly. If "true" then old elements will be deleted from project.
        /// </summary>
        /// <param name="elements">New members for replacement</param>
        /// <param name="deleteOldElements">Deleting old elements from project option.</param>
        /// <returns>Passing AssemblyInstance</returns>
        public AssemblyInstance ReplaceMembersInAllInstances(List<RS.Element> elements, bool deleteOldElements)
        {
            var assName = InternalAssemblyInstance.AssemblyTypeName;
            var assType = Document.GetElement(InternalAssemblyInstance.GetTypeId());
            
            assType.Name = "_" + assName;

            ReplaceMembers(elements, deleteOldElements);
            
            TransactionManager.Instance.EnsureInTransaction(Document);
            
            InternalAssemblyInstance.AssemblyTypeName = assName;

            var allAss = DocumentManager.Instance.ElementsOfType<Autodesk.Revit.DB.AssemblyInstance>().Where( x=> x.AssemblyTypeName=="_"+assName);

            foreach( var a in allAss)
                a.ChangeTypeId(InternalAssemblyInstance.GetTypeId());

            TransactionManager.Instance.ForceCloseTransaction();

            return this;
        }

        /// <summary>
        /// Remove assembly members in all same assembly instances. Elements will stay in the project.
        /// </summary>
        /// <param name="elements">Members for removing</param>
        /// <returns>Passing AssemblyInstance</returns>
        public AssemblyInstance RemoveMembersInAllInstances(List<RS.Element> elements)
        {
            var assName = InternalAssemblyInstance.AssemblyTypeName;
            var assType = Document.GetElement(InternalAssemblyInstance.GetTypeId());

            assType.Name = "_" + assName;

            RemoveMembers(elements);

            TransactionManager.Instance.EnsureInTransaction(Document);

            InternalAssemblyInstance.AssemblyTypeName = assName;

            var allAss = DocumentManager.Instance.ElementsOfType<Autodesk.Revit.DB.AssemblyInstance>().Where(x => x.AssemblyTypeName == "_" + assName);

            foreach (var a in allAss)
                a.ChangeTypeId(InternalAssemblyInstance.GetTypeId());
            
            TransactionManager.Instance.ForceCloseTransaction();

            return this;
        }

        /// <summary>
        /// Add new assembly members in all same assembly instances.
        /// </summary>
        /// <param name="elements">New members for adding</param>
        /// <returns>Passing AssemblyInstance</returns>
        public AssemblyInstance AddMembersInAllInstances(List<RS.Element> elements)
        {
            var assName = InternalAssemblyInstance.AssemblyTypeName;
            var assType = Document.GetElement(InternalAssemblyInstance.GetTypeId());

            assType.Name = "_" + assName;

            AddMembers(elements);

            TransactionManager.Instance.EnsureInTransaction(Document);

            InternalAssemblyInstance.AssemblyTypeName = assName;

            var allAss = DocumentManager.Instance.ElementsOfType<Autodesk.Revit.DB.AssemblyInstance>().Where(x => x.AssemblyTypeName == "_" + assName);

            foreach (var a in allAss)
                a.ChangeTypeId(InternalAssemblyInstance.GetTypeId());
            
            TransactionManager.Instance.ForceCloseTransaction();

            return this;
        }

        /// <summary>
        /// Change assembly type in target assembly instance. Will be assigned assembly type from current assembly instance.
        /// </summary>
        /// <param name="synchedInstance">Target assembly instance for type changing</param>
        /// <returns>Passing AssemblyInstance</returns>
        public AssemblyInstance SyncAssemblyInstance(AssemblyInstance synchedInstance)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);
            
            synchedInstance.InternalAssemblyInstance.ChangeTypeId(InternalAssemblyInstance.GetTypeId());

            TransactionManager.Instance.TransactionTaskDone();

            return this;
        }

        /// <summary>
        /// Convert Element object to AssemblyInstance object if it`s possible for further using 
        /// </summary>
        /// <param name="element">Casting Element</param>
        /// <returns>Converted AssemblyInstance object</returns>
        public static AssemblyInstance CastFromElement(RS.Element element)
        {
            if(element.InternalElement.GetType()==typeof(Autodesk.Revit.DB.AssemblyInstance))
                return new AssemblyInstance(element.InternalElement as Autodesk.Revit.DB.AssemblyInstance);
            throw new Exception("Element is not AssemblyInstance");
        }

        /// <summary>
        /// Replace assembly members in current assembly instance with option for deleting or detaching old elements. If option is "false" then old elements will be only detached from assembly. If "true" then old elements will be deleted from project.
        /// </summary>
        /// <param name="elements">New members for replacement</param>
        /// <param name="deleteOldElements">Deleting old elements from project option.</param>
        /// <returns>Passing AssemblyInstance</returns>
        public AssemblyInstance ReplaceMembers(List<RS.Element> elements, bool deleteOldElements)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            var oldElementIds = InternalAssemblyInstance.GetMemberIds();
           
            InternalAssemblyInstance.SetMemberIds(elements.Select(x=>x.InternalElement.Id).ToList());

            if (deleteOldElements)
                Document.Delete(oldElementIds);

            TransactionManager.Instance.ForceCloseTransaction();

            return this;
        }

        /// <summary>
        /// Add new assembly members in current assembly instance
        /// </summary>
        /// <param name="elements">New members for adding</param>
        /// <returns>Passing AssemblyInstance</returns>
        public AssemblyInstance AddMembers(List<RS.Element> elements)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);
            InternalAssemblyInstance.AddMemberIds(elements.Select(x => x.InternalElement.Id).ToList());
            TransactionManager.Instance.ForceCloseTransaction();

            return this;
        }

        /// <summary>
        /// Remove assembly members in current assembly instance
        /// </summary>
        /// <param name="members">Мembers for removing</param>
        /// <returns>Passing AssemblyInstance</returns>
        public AssemblyInstance RemoveMembers(List<RS.Element> members)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);
            InternalAssemblyInstance.RemoveMemberIds(members.Select(x=>x.InternalElement.Id).ToList());
            TransactionManager.Instance.ForceCloseTransaction();

            return this;
        }

        /// <summary>
        /// Disassemble all members
        /// </summary>
        /// <returns>Disassembled members</returns>
        public RS.Element[] Disassemble()
        {
            TransactionManager.Instance.EnsureInTransaction(Document);
            var els = InternalAssemblyInstance.Disassemble().Select(x=>Document.GetElement(x).ToDSType(true)).ToArray();
            TransactionManager.Instance.TransactionTaskDone();

            return els;
        }

        /// <summary>
        /// Create new assembly in the project from existing elements list
        /// </summary>
        /// <param name="elements">Elements list for new assembly</param>
        /// <returns>New AssemblyInstance object</returns>
        public static AssemblyInstance Create(RS.Element[] elements)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);
            TransactionManager.Instance.TransactionTaskDone();

            return new AssemblyInstance(elements);
        }

        /// <summary>
        /// Get assembly type for current assembly instance
        /// </summary>
        public AssemblyType AssemblyType
        {
            get { return new AssemblyType(Document.GetElement(InternalAssemblyInstance.GetTypeId()) as Autodesk.Revit.DB.AssemblyType); }
        }

        /// <summary>
        /// Get members in current assembly
        /// </summary>
        public RS.Element[] Members
        {
            get { return InternalAssemblyInstance.GetMemberIds().Select(x => Document.GetElement(x).ToDSType(true)).ToArray(); }
        }
    }
}
