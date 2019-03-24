/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using EAAPI = EA;

namespace MDD4All.SpecIF.DataProvider.EA
{
	public class SpecIfEaDataReader : AbstractSpecIfDataReader
	{
		private EAAPI.Repository _eaRepository;

        public bool RepositoryIsOpen { get; set; }

        public SpecIfEaDataReader(EAAPI.Repository eaRepository)
		{
			_eaRepository = eaRepository;
		}

        public override List<Node> GetAllHierarchies()
        {
            List<Node> result = new List<Node>();

            if (RepositoryIsOpen)
            {
                EaToSpecIfConverter converter = new EaToSpecIfConverter(_eaRepository);



                // TODO remove hardcoded test
                EAAPI.Package fctsysPackage = _eaRepository.GetPackageByGuid("{5D647B1D-D622-4a45-90EB-5FC6ECCD405C}");

                if (fctsysPackage != null)
                {
                    Node hierarchy = converter.ConvertPackageToHierarchy(fctsysPackage);

                    result.Add(hierarchy);
                }
            }

            return result;
        }

       

        public override byte[] GetFile(string filename)
        {
            throw new NotImplementedException();
        }

        public override Node GetHierarchyByKey(Key id)
        {
            Node result = null;

            EaToSpecIfConverter converter = new EaToSpecIfConverter(_eaRepository);

            string eaGuid = EaSpecIfGuidConverter.ConvertSpecIfGuidToEaGuid(id.ID);

            EAAPI.Package fctsysPackage = _eaRepository.GetPackageByGuid(eaGuid);

            if (fctsysPackage != null)
            {
                Node hierarchy = converter.ConvertPackageToHierarchy(fctsysPackage);

                result = hierarchy;
            }

            return result;
        }

		public override int GetLatestResourceRevision(string resourceID)
		{
			return 1;
		}

		public override Resource GetResourceByKey(Key key)
        {
            

            Resource result = null;

            try
            {
                EaToSpecIfConverter converter = new EaToSpecIfConverter(_eaRepository);

                EAAPI.Element eaElement = _eaRepository.GetElementByGuid(EaSpecIfGuidConverter.ConvertSpecIfGuidToEaGuid(key.ID));

                if (eaElement != null)
                {
                    result = converter.ConvertElementToResource(eaElement);
                }
                else
                {
                    
                }
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

		public override int GetLatestHierarchyRevision(string hierarchyID)
		{
			return 1;
		}

		public override int GetLatestStatementRevision(string statementID)
		{
			return 1;
		}
	}
}
