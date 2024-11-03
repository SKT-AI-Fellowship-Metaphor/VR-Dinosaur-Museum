using UnityEngine;
using System.Collections;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using Amazon;
using System.Threading.Tasks;

public class S3ImageLoader : MonoBehaviour
{
    private const string fileName = "dinopdf.pdf";

    private const string bucketName = "PLACE YOUR BUCKET NAME HERE";
    private const string accessKey = "PLACE YOUR ACCESS KEY HERE";
    private const string secretKey = "PLACE YOUR SECRET KEY HERE";

    void Start()
    {
        StartCoroutine(DownloadFileFromS3Coroutine());
    }

    IEnumerator DownloadFileFromS3Coroutine()
    {
        yield return new WaitForEndOfFrame();
        
        Task downloadTask = DownloadFileFromS3();
        while (!downloadTask.IsCompleted)
        {
            yield return null;
        }

        if (downloadTask.IsFaulted)
        {
            Debug.LogError($"Error downloading file: {downloadTask.Exception}");
        }
        else
        {
            Debug.Log("File downloaded successfully");
        }
    }

    async Task DownloadFileFromS3()
    {
        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        var config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.APSoutheast2
        };

        using (var client = new AmazonS3Client(credentials, config))
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = fileName
            };

            try
            {
                using (GetObjectResponse response = await client.GetObjectAsync(request))
                {
                    string localFilePath = Path.Combine(Application.dataPath, "PDF", fileName);

                    if (!Directory.Exists(Path.GetDirectoryName(localFilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));
                    }

                    using (var responseStream = response.ResponseStream)
                    using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write))
                    {
                        await responseStream.CopyToAsync(fileStream);
                    }

                    Debug.Log($"File downloaded successfully: {localFilePath}");
                }
            }
            catch (AmazonS3Exception e)
            {
                Debug.LogError($"Error downloading file from S3: {e.Message}");
                throw;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Unknown error: {e.Message}");
                throw;
            }
        }
    }
}