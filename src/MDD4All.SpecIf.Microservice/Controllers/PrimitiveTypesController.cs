/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */

//namespace MDD4All.SpecIf.Microservice.Controllers
//{
//    [ApiVersion("1.0")]
//    [Produces("application/json")]
//    [Route("specif/v{version:apiVersion}/primitiveTypes")]
//    [ApiController]
//    //[SwaggerTag("01 Primitive Types")]
//    public class PrimitiveTypesController : Controller
//    {
//		private ISpecIfMetadataReader _metadataReader;

//		public PrimitiveTypesController(ISpecIfMetadataReader metadataReader)
//		{
//			_metadataReader = metadataReader;
//		}

//        /// <summary>
//        /// Returns a list with the names of the available primitive types.
//        /// </summary>
//        /// <returns>A list of all available primitive types.</returns>
//        /// <response code="200">List of primitive types suceessfull returned.</response>
//        [HttpGet]
//		public List<string> GetPrimitiveTypeList()
//		{
//			return _metadataReader.GetDataTypeTypes();
//		}

//	}
//}