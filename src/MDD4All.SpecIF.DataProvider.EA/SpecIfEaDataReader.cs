/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.EA.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using EAAPI = EA;

namespace MDD4All.SpecIF.DataProvider.EA
{
	public class SpecIfEaDataReader : AbstractSpecIfDataReader
	{
		private EAAPI.Repository _eaRepository;

        private ISpecIfMetadataReader _metadataReader;

        private static EaUmlToSpecIfConverter _eaUmlToSpecIfConverter;

        //public bool RepositoryIsOpen { get; set; }

        public SpecIfEaDataReader(EAAPI.Repository eaRepository, ISpecIfMetadataReader metadataReader)
		{
			_eaRepository = eaRepository;
            _metadataReader = metadataReader;

            if (_eaUmlToSpecIfConverter == null)
            {
                _eaUmlToSpecIfConverter = new EaUmlToSpecIfConverter(eaRepository, metadataReader);
            }
		}

        public override List<Node> GetAllHierarchies()
        {
            List<Node> result = new List<Node>();


            //EaToSpecIfConverter converter = new EaToSpecIfConverter(_eaRepository);



            //// TODO remove hardcoded test
            //EAAPI.Package fctsysPackage = _eaRepository.GetPackageByGuid("{5D647B1D-D622-4a45-90EB-5FC6ECCD405C}");

            //if (fctsysPackage != null)
            //{
            //    Node hierarchy = converter.ConvertPackageToHierarchy(fctsysPackage);

            //    result.Add(hierarchy);
            //}




            //SpecificationToHierarchyConverter converter = new SpecificationToHierarchyConverter(_eaRepository);

            //result = converter.GetAllHierarchies();

            //_eaUmlToSpecIfConverter = new EaUmlToSpecIfConverter(_eaRepository, _metadataReader);

            Node hierarchy = _eaUmlToSpecIfConverter.GetHierarchy();

            result.Add(hierarchy);


            return result;
        }

       

        public override byte[] GetFile(string filename)
        {
            throw new NotImplementedException();
        }

        public override Node GetHierarchyByKey(Key id)
        {
            Node result = null;

            //EaToSpecIfConverter converter = new EaToSpecIfConverter(_eaRepository);

            //string eaGuid = EaSpecIfGuidConverter.ConvertSpecIfGuidToEaGuid(id.ID);

            //EAAPI.Package fctsysPackage = _eaRepository.GetPackageByGuid(eaGuid);

            //if (fctsysPackage != null)
            //{
            //    Node hierarchy = converter.ConvertPackageToHierarchy(fctsysPackage);

            //    result = hierarchy;
            //}

            //SpecificationToHierarchyConverter converter = new SpecificationToHierarchyConverter(_eaRepository);

            //List<Node> allHierarchies = converter.GetAllHierarchies();

            //result = allHierarchies.Find(hierarchy => hierarchy.ID == id.ID);

            //_eaUmlToSpecIfConverter = new EaUmlToSpecIfConverter(_eaRepository, _metadataReader);

            Node hierarchy = _eaUmlToSpecIfConverter.GetHierarchy();

            result = hierarchy;

            return result;
        }

		//public override Revision GetLatestResourceRevision(string resourceID)
		//{
		//	return Key.FIRST_MAIN_REVISION;
		//}

		public override Resource GetResourceByKey(Key key)
        {
            

            Resource result = null;

            try
            {
                //EaUmlToSpecIfConverter converter = new EaUmlToSpecIfConverter(_eaRepository, _metadataReader);

                //EAAPI.Element eaElement = _eaRepository.GetElementByGuid(EaSpecIfGuidConverter.ConvertSpecIfGuidToEaGuid(key.ID));

                //if (eaElement != null)
                //{
                //    result = converter.ConvertElement(eaElement);
                //}
                //else
                //{

                //}

                result = _eaUmlToSpecIfConverter.GetResourceByKey(key);
            }
            catch(Exception exception)
            {
                Debug.WriteLine("Element not found " + key.ID);
                Debug.WriteLine(exception);
            }

            return result;
         
        }

        public override Statement GetStatementByKey(Key key)
        {
            throw new NotImplementedException();
        }

		public override string GetLatestHierarchyRevision(string hierarchyID)
		{
			return "";
		}

		public override string GetLatestStatementRevision(string statementID)
		{
			return "";
		}

		public override List<Statement> GetAllStatementsForResource(Key resourceKey)
		{
            return _eaUmlToSpecIfConverter.GetAllStatementsForResource(resourceKey);
		}
         
		public override List<Node> GetContainingHierarchyRoots(Key resourceKey)
		{
			throw new NotImplementedException();
		}

        public override List<Node> GetAllHierarchyRootNodes(string projectID = null)
        {
            List<Node> result = new List<Node>();

            //SpecificationToHierarchyConverter converter = new SpecificationToHierarchyConverter(_eaRepository);

            //List<Node> allHierarchies = converter.GetAllHierarchies();

            //foreach(Node rootNode in allHierarchies)
            //{
            //    rootNode.Nodes = new List<Node>();
            //}

            //result = allHierarchies;

            result.Add(_eaUmlToSpecIfConverter.GetHierarchy());

            return result;
        }

        public override List<Resource> GetAllResourceRevisions(string resourceID)
        {
            throw new NotImplementedException();
        }

        public override List<Statement> GetAllStatementRevisions(string statementID)
        {
            throw new NotImplementedException();
        }

        public override List<Statement> GetAllStatements()
        {
            throw new NotImplementedException();
        }

        public override List<Node> GetChildNodes(Key parentNodeKey)
        {
            throw new NotImplementedException();
        }

        public override string GetLatestResourceRevisionForBranch(string resourceID, string branchName)
        {
            throw new NotImplementedException();
        }

        public override Node GetNodeByKey(Key key)
        {
            throw new NotImplementedException();
        }

        public override Node GetParentNode(Key childNode)
        {
            throw new NotImplementedException();
        }

        public override DataModels.SpecIF GetProject(ISpecIfMetadataReader metadataReader, string projectID, List<Key> hierarchyFilter = null, bool includeMetadata = true)
        {
            throw new NotImplementedException();
        }

        public override List<ProjectDescriptor> GetProjectDescriptions()
        {
            throw new NotImplementedException();
        }

        

        
    }
}
