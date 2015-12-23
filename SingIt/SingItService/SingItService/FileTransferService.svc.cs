using HttpMultipartParser;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace SingItService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "FileTransferService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select FileTransferService.svc or FileTransferService.svc.cs at the Solution Explorer and start debugging.
    public class FileTransferService : IFileTransferService
    {
        public string UploadSong(Stream stream)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Utilities.GetConnectionStringForStorage());
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("singitcontainer");

                MultipartFormDataParser parser = new MultipartFormDataParser(stream);
                string username = parser.Parameters["username"].Data;
                string songtitle = parser.Parameters["songtitle"].Data;
                string songgenre = parser.Parameters["songgenre"].Data;

                FilePart file = parser.Files.First();
                string songfilename = string.Concat(Guid.NewGuid().ToString(), Path.GetExtension(file.FileName));

                CloudBlockBlob blockBlob = container.GetBlockBlobReference(string.Concat(username, "/", songfilename));
                blockBlob.UploadFromStream(file.Data);

                using (var context = new UserSongModel(Utilities.GetConnectionStringForDatabase()))
                {
                    context.users.Where(u => u.username == username).First().songs.Add(new song() { songfilename = songfilename, songtitle = songtitle, songgenre = songgenre });

                    context.SaveChanges();
                }

                stream.Close();
                stream.Dispose();

                return Utilities.SUCCESS;
            }
            catch (Exception)
            {
                return Utilities.EXCEPTION;
            }
        }
    }
}