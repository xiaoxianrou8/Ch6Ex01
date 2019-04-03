using Microsoft.WindowsAzure.Storage;
using System;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using System.IO;

namespace Ch16Ex01
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //创建连接字符串
                StorageCredentials credentials = new StorageCredentials("zxytest1", "YBdka3flRzwkfkWQYiFvR7Akys7l8mJCkeGYefRy2p8WxG5M4cEa2nUpXkr4k4CAAKeYlpcmd135xWrTB0 + GgA ==");
                //创建存储账户
                CloudStorageAccount storageAccount = new CloudStorageAccount(credentials, useHttps: true,endpointSuffix: "core.chinacloudapi.cn");
                //创建blob客户端   
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                //获取特定容器的引用
                CloudBlobContainer container = blobClient.GetContainerReference("carddeck1");
                if(container.CreateIfNotExists())
                {
                    Console.WriteLine($"Created Container '{container.Name}"+$"in storage account '{storageAccount.Credentials.AccountName}'");
                }
                else
                {
                    Console.WriteLine($"Created '{container.Name} already exists" + $"from storage account '{storageAccount.Credentials.AccountName}'");
                }
                //设置容器的可访问性为public
                container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                Console.WriteLine($"Permission foe container '{container.Name}' is public");
                //上传图片
                int numberOfCards = 0;
                //获取图片的目录
                DirectoryInfo dir = new DirectoryInfo(@"cards");
                foreach (var item in dir.GetFiles("*.*"))
                {
                    //为要添加到容器的图片设置引用
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(item.Name);
                    using (var fileStream=File.OpenRead(@"cards\"+item.Name))
                    {
                        blockBlob.UploadFromStream(fileStream);
                        Console.WriteLine($"Uploading:{item.Name} which"+$" is {fileStream.Length} bytes");
                    }
                    numberOfCards++;
                    
                }
                Console.WriteLine($"Uploaded {numberOfCards.ToString()} cards");
                Console.Read();
                //检查图片
                numberOfCards = 0;
                foreach (var item in container.ListBlobs(null,false))
                {
                    if (item.GetType()==typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        Console.WriteLine($"Card image url '{blob.Uri}'" +$" with Length of '{blob.Properties.Length}'");
                    }
                    numberOfCards++;
                }
                Console.WriteLine($"list {numberOfCards.ToString()} cards");
                Console.Read();
                //删除图片
                Console.WriteLine($"enter Y to delete list Cards");
                if (Console.ReadLine()=="Y")
                {
                    numberOfCards = 0;
                    foreach (var item in container.ListBlobs(null,false))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        CloudBlockBlob blobDelete = container.GetBlockBlobReference(blob.Name);
                        blobDelete.Delete();
                        Console.WriteLine($"Delete:'{blob.Name}' which was {blob.Name.Length} bytes");
                        numberOfCards++;
                    }
                    Console.WriteLine($"Deleted {numberOfCards.ToString()} cards");
                }
            }
            catch (StorageException ex)
            {
                Console.WriteLine($"StorageException:{ex.Message}");
            }
            catch (Exception ex)
            {
                throw;
                Console.WriteLine($"Exception:{ex.Message}");
            }
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}
