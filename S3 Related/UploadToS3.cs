using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;

public class UploadToS3 : MonoBehaviour
{
    private const string bucketName = "PLACE YOUR BUCKET NAME HERE";
    private const string region = "PLACE YOUR REGION HERE";
    private const string accessKey = "PLACE YOUR ACCESS KEY HERE";
    private const string secretKey = "PLACE YOUR SECRET KEY HERE";

    private static IAmazonS3 s3Client;

    // Singleton instance
    private static UploadToS3 _instance;
    public static UploadToS3 Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UploadToS3>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("UploadToS3");
                    _instance = go.AddComponent<UploadToS3>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // S3 클라이언트 초기화
        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        s3Client = new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(region));
    }

    public async Task<bool> UploadPageImage(int currentPage)
    {
        string fileName = $"page-{currentPage:D3}.png";
        string filePath = Path.Combine(Application.dataPath, "Images", fileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return false;
        }

        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName,
                    InputStream = fileStream,
                    ContentType = "image/png"
                };

                PutObjectResponse response = await s3Client.PutObjectAsync(putRequest);
                Debug.Log($"Successfully uploaded {fileName} to S3 bucket {bucketName}");
                return true;
            }
        }
        catch (AmazonS3Exception e)
        {
            Debug.LogError($"Error uploading to S3: {e.Message}");
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Unknown error: {e.Message}");
            return false;
        }
    }
}