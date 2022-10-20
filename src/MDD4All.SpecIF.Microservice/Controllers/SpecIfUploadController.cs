using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.Microservice.Models;
using MDD4All.SpecIF.ViewModels.IntegrationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MDD4All.SpecIF.Microservice.Controllers
{
    public class SpecIfUploadController : Controller
    {
        private ISpecIfMetadataReader _metadataReader;
        private ISpecIfMetadataWriter _metadataWriter;
        private ISpecIfDataReader _dataReader;
        private ISpecIfDataWriter _dataWriter;

        public SpecIfUploadController(ISpecIfMetadataReader metadataReader,
                                      ISpecIfMetadataWriter metadataWriter,
                                      ISpecIfDataReader dataReader,
                                      ISpecIfDataWriter dataWriter)
        {
            _metadataReader = metadataReader;
            _metadataWriter = metadataWriter;
            _dataWriter = dataWriter;
            _dataReader = dataReader;
        }



        ////[HttpGet("[controller]/Index/files_and_images/{id}")]
        ////[HttpGet("{id}")]
        //public FileResult Get(string id)
        //{
        //    byte[] data = _dataReader.GetFile(id);

        //    string contentType = "application/octet-stream";

        //    if (id.EndsWith(".svg", true, CultureInfo.InvariantCulture))
        //    {
        //        contentType = "image/svg+xml";
        //    }
        //    else if (id.EndsWith(".png", true, CultureInfo.InvariantCulture))
        //    {
        //        contentType = "image/png";
        //    }
        //    else if (id.EndsWith(".jpg", true, CultureInfo.InvariantCulture))
        //    {
        //        contentType = "image/jpeg";
        //    }

        //    return new FileContentResult(data, contentType);

        //}

        [HttpGet()]
        public IActionResult Index()
        {
            return View(new BaseViewModel());
        }

        [HttpPost]
        [Authorize(Policy = "unregisteredReader")]
        public async Task<IActionResult> UploadFile(IFormCollection collection)
        {
            IActionResult result = null;

            List<IFormFile> files = collection.Files.ToList();

            string overrideExistingData = collection["overrideData"];

            long size = files.Sum(f => f.Length);

            // full path to file in temp location
            string filePath = Path.GetTempFileName();

            SpecIfUploadViewModel fileViewModel = new SpecIfUploadViewModel(_metadataReader, 
                                                                            _metadataWriter, 
                                                                            _dataReader, 
                                                                            _dataWriter);

            fileViewModel.TempFileName = filePath;

            fileViewModel.FileName = "<UNNAMED>";

            if (files.Count == 1)
            {
                IFormFile formFile = files[0];

                if (formFile.Length > 0)
                {
                    fileViewModel.FileName = formFile.FileName;

                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                    fileViewModel.UploadFileCommand.Execute(null);
                }
            }

            result = View("UploadFinished", fileViewModel);

            return result;
        }

    }
}
