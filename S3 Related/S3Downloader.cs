using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using Amazon;
using System.Threading.Tasks;

public class S3Downloader : MonoBehaviour
{
    public List<string> fileNames = new List<string> { "DinosaurFacts.pdf" };
    private const string bucketName = "PLACE YOUR BUCKET NAME HERE";
    private const string accessKey = "PLACE YOUR ACCESS KEY HERE";
    private const string secretKey = "PLACE YOUR SECRET KEY HERE";

    private PDFLoader pdfLoader;
    // private ImageLoader imageLoader;
    private ModelLoader modelLoader;

    void Start()
    {
        // Debug.Log("==================================");
        // Debug.Log("Starting");
        // Debug.Log("==================================");
        pdfLoader = GameObject.Find("PDF Loader").GetComponent<PDFLoader>();
        // imageLoader = GameObject.Find("Image Loader").GetComponent<ImageLoader>();
        modelLoader = GameObject.Find("Model Loader").GetComponent<ModelLoader>();

        if (pdfLoader == null) Debug.LogError("PDFLoader component not found!");
        // if (imageLoader == null) Debug.LogError("ImageLoader component not found!");
        if (modelLoader == null) Debug.LogError("ModelLoader component not found!");

        StartCoroutine(DownloadAndProcessFiles());
    }

    IEnumerator DownloadAndProcessFiles()
    {
        foreach (string fileName in fileNames)
        {
            Debug.Log($"Starting download of {fileName}");
            yield return StartCoroutine(DownloadFileFromS3Coroutine(fileName));
        }
        Debug.Log("All files processed");
    }

    IEnumerator DownloadFileFromS3Coroutine(string fileName)
    {
        Task downloadTask = DownloadFileFromS3(fileName);
        while (!downloadTask.IsCompleted)
        {
            yield return null;
        }

        if (downloadTask.IsFaulted)
        {
            Debug.LogError($"Error downloading {fileName}: {downloadTask.Exception}");
        }
        else
        {
            Debug.Log($"{fileName} downloaded successfully");
            string localFilePath = Path.Combine(Application.dataPath, "Files", fileName);
            yield return StartCoroutine(ProcessFile(fileName, localFilePath));
        }
    }

    IEnumerator ProcessFile(string fileName, string localFilePath)
    {
        string fileExtension = Path.GetExtension(fileName).ToLower();
        string relativePath = ConvertToRelativePath(localFilePath);

        if (fileExtension == ".pdf" && pdfLoader != null)
        {
            Debug.Log($"Processing PDF: {fileName}");
            yield return StartCoroutine(pdfLoader.ConvertPDFToImagesAndDisplay(relativePath));
        }
        // else if ((fileExtension == ".png" || fileExtension == ".jpg" || fileExtension == ".jpeg") && imageLoader != null)
        // {
        //     Debug.Log($"Processing Image: {fileName}");
        //     yield return StartCoroutine(imageLoader.LoadAndDisplayImage(relativePath));
        // }
        else if ((fileExtension == ".fbx" || fileExtension == ".glb") && modelLoader != null)
        {
            Debug.Log($"Processing 3D Model: {fileName}");
            LoadModelAsyncWrapper(relativePath);
        }
        else
        {
            Debug.LogWarning($"Unsupported file format or loader not set for {fileName}");
        }
    }

    async Task DownloadFileFromS3(string fileName)
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
                    string localFilePath = Path.Combine(Application.dataPath, "Files", fileName);

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
            catch (Exception e)
            {
                Debug.LogError($"Unknown error: {e.Message}");
                throw;
            }
        }
    }

    public static string ConvertToRelativePath(string absolutePath)
    {
        string assetsPath = Path.GetFullPath(Application.dataPath);

        if (absolutePath.StartsWith(assetsPath))
        {
            string relativePath = "Assets" + absolutePath.Substring(assetsPath.Length);
            relativePath = relativePath.Replace('\\', '/');
            return relativePath;
        }
        else
        {
            Debug.LogWarning("제공된 경로가 프로젝트의 Assets 폴더 내에 있지 않습니다.");
            return absolutePath;
        }
    }

    private async void LoadModelAsyncWrapper(string path)
    {
        try
        {
            await modelLoader.LoadModelAsync(path);
            Debug.Log($"모델 로딩 완료: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"모델 로딩 중 오류 발생: {e.Message}");
        }
    }
}