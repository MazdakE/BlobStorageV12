using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace BlobStorageV12
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Azure Blob Storage V12 - .NET sample");
            string connectionString = Environment.GetEnvironmentVariable("connectionString");
            //Console.WriteLine(connectionString); //here you can see if the environment variable is working

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // container names
            var availableContainersName = await GetAllContainerNames(blobServiceClient);

            Console.Write("Press '1' to create a new blob container\nPress '2' to create a blob in existingblob container\nPress '3' to upload an image to an existing container\nEnter your value: ");
            var options = Console.ReadLine();

            switch (Convert.ToInt32(options))
            {
                case 1:
                    Console.Write("Enter name of your new blob container: ");
                    string containerName = Console.ReadLine();
                    var condition = CheckIfBlobExists(availableContainersName.ToList(), blobServiceClient, containerName);
                    await CreateBlob(blobServiceClient, containerName, condition);
                    break;
                case 2:
                    Console.Write("Enter name of your existing blob container: ");
                    containerName = Console.ReadLine();
                    condition = CheckIfBlobExists(availableContainersName.ToList(), blobServiceClient, containerName);
                    Console.Write("Write a name for your blob: ");
                    string blobName = Console.ReadLine();
                    CreateBlobInExistingContainer(containerName, connectionString, blobName, containerExist: condition);
                    break;
                case 3:
                    Console.Write("Enter name of your existing blob container: ");
                    containerName = Console.ReadLine();
                    BlobContainerClient blobContainerClient = new BlobContainerClient(connectionString, containerName);
                    Console.WriteLine("Which image would you like to upload: ");
                    var image = Console.ReadLine();
                    await UploadImageInContainer(blobContainerClient, image);
                    break;
                default:
                    break;
            }
        }

        private static void CreateBlobInExistingContainer(string blobContainerName, string connectionString, string blobName, bool containerExist)
        {
            if (containerExist)
            {
                // Get a reference to a container named "sample-container" and then create it
                BlobContainerClient container = new BlobContainerClient(connectionString, blobContainerName);

                // Get a reference to a blob named "sample-file" in a container named "sample-container"
                BlobClient blob = container.GetBlobClient(blobName);
            }
        }

        private static async Task CreateBlob(BlobServiceClient blobServiceClient, string containerName, bool containerExist)
        {
            if (!containerExist)
            {
                BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName.ToLower());
            }
        }

        private static bool CheckIfBlobExists(List<string> containerNames, BlobServiceClient blobServiceClient, string containerName)
        {
            if (containerNames.Contains(containerName))
            {
                Console.WriteLine("A blob with this name already exist");
                return true;
            }
            else
            {
                Console.WriteLine("Creating a blob");
                return false;
            }
        }

        private static async Task<IEnumerable<string>> GetAllContainerNames(BlobServiceClient blobServiceClient)
        {
            List<Page<BlobContainerItem>> blobContainers = new List<Page<BlobContainerItem>>();

            var result = blobServiceClient.GetBlobContainersAsync(BlobContainerTraits.Metadata).AsPages();

            await foreach (var container in result)
            {
                blobContainers.Add(container);
            }

            var containerNames = blobContainers.Select(x => x.Values.Select(x => x.Name));

            var names = new List<string>();

            foreach (var name in containerNames)
            {
                foreach (var item in name)
                {
                    names.Add(item);
                    Console.WriteLine(item);
                }
            }

            return names;
        }

        private static async Task UploadImageInContainer(BlobContainerClient container, string image)
        {
            try
            {
                string localPath = @"C:\Users\Mazdak\source\repos\BlobStorageV12\images";
                string fileName = image + ".jpg";

                string localFilePath = Path.Combine(localPath, fileName);

                using FileStream uploadFileStream = File.OpenRead(localFilePath);

                BlobClient blobClient = container.GetBlobClient(fileName);

                var imageUri = blobClient.Uri.AbsoluteUri;

                var blobHttpHeader = new BlobHttpHeaders { ContentType = "image/jpg" };

                var blobUploadOptions = new BlobUploadOptions() { HttpHeaders = blobHttpHeader };

                Console.WriteLine(imageUri);

                await blobClient.UploadAsync(localFilePath, blobUploadOptions);

                Console.WriteLine("Successfully uploaded!");
            }
            catch (Exception)
            {
                Console.WriteLine("Unsuccessfull to upload");
            }
        }
    }
}