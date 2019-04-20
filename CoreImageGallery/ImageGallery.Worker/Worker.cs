using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ImageGallery.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Watermarker;

namespace ImageGallery.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private const string ImagePrefix = "img_";
        private readonly CloudStorageAccount _account;
        private readonly CloudBlobClient _client;
        private readonly string _connectionString;
        private CloudBlobContainer _uploadContainer;
        private CloudBlobContainer _publicContainer;
        public Worker(ILogger<Worker> logger, IConfiguration config)
        {
            _connectionString = config.GetValue<string>("ConnectionStrings:DefaultConnection");
            _account = CloudStorageAccount.Parse(_connectionString);
            _client = _account.CreateCloudBlobClient();
            _uploadContainer = _client.GetContainerReference(Config.UploadContainer);
            _publicContainer = _client.GetContainerReference(Config.WatermarkedContainer);
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation($"Worker running at: {DateTime.Now}");
                await GetImagesAsync();
                await Task.Delay(1000, stoppingToken);
            }
        }
        public async Task GetImagesAsync()
        {

            var imageList = new List<UploadedImage>();
            var token = new BlobContinuationToken();
            var blobList = await _uploadContainer.ListBlobsSegmentedAsync(ImagePrefix, true, BlobListingDetails.All, 100, token, null, null);

            foreach (var blob in blobList.Results)
            {
                CloudBlockBlob thing = (CloudBlockBlob)blob;
                string name = thing.Name;
                _logger.LogInformation($"Blob name is : {name}");
                Stream inputFile = await thing.OpenReadAsync();
                Stream outputFile = new MemoryStream();
                WaterMarker.WriteWatermark("i am so hungry", inputFile, outputFile);
                await AddImageAsync(outputFile, name);
                await DeleteImageAsync(name);
            }
        }

        public async Task AddImageAsync(Stream stream, string fileName)
        {
            var imageBlob = _publicContainer.GetBlockBlobReference(fileName);
            await imageBlob.UploadFromStreamAsync(stream);
        }

        public async Task DeleteImageAsync(string fileName)
        {
            CloudBlockBlob blockBlob = _uploadContainer.GetBlockBlobReference(fileName);
            await blockBlob.DeleteIfExistsAsync();
        }


    }
}
