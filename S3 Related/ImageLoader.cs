using UnityEngine;
using UnityEngine.UI;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using System;
using System.IO;
using System.Threading.Tasks;

public class ImageLoader : MonoBehaviour
{
    public static ImageLoader Instance { get; private set; }

    private const string bucketName = "PLACE YOUR BUCKET NAME HERE";
    private const string accessKey = "PLACE YOUR ACCESS KEY HERE";
    private const string secretKey = "PLACE YOUR SECRET KEY HERE";

    private IAmazonS3 _s3Client;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // S3 클라이언트 초기화
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            _s3Client = new AmazonS3Client(credentials, RegionEndpoint.APSoutheast2);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task<bool> LoadImageFromS3(string imageKey, Image targetImage)
    {
        try
        {
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = imageKey
            };

            using (GetObjectResponse response = await _s3Client.GetObjectAsync(request))
            using (MemoryStream ms = new MemoryStream())
            {
                await response.ResponseStream.CopyToAsync(ms);
                byte[] data = ms.ToArray();

                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(data))
                {
                    targetImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    Debug.Log($"Image '{imageKey}' loaded from S3 and applied to UI Image");
                    return true;
                }
                else
                {
                    Debug.LogError($"Failed to load image data for '{imageKey}'");
                    return false;
                }
            }
        }
        catch (AmazonS3Exception e)
        {
            Debug.LogError($"S3 error encountered for '{imageKey}'. Message: {e.Message}");
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"Unknown error encountered for '{imageKey}'. Message: {e.Message}");
            return false;
        }
    }
}