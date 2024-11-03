using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using Amazon;

public class Create3D : MonoBehaviour
{
    [SerializeField] private Button create3DButton;
    [SerializeField] private PDFLoader pdfLoader;
    [SerializeField] private TMP_InputField queryInput;
    [SerializeField] private TMP_InputField dinosaurInput;
    [SerializeField] private string fastApiBaseUrl = "PLACE YOUR SERVER URL HERE";
    [SerializeField] private GameObject loadingContainer;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private ModelLoader modelLoader;
    
    private const string intent = "create_3d";
    private const float apiTimeout = 250f;
    private const float pollInterval = 10f;
    private Coroutine pulseAnimation;

    private const string bucketName = "PLACE YOUR BUCKET NAME HERE";
    private const string accessKey = "PLACE YOUR ACCESS KEY HERE";
    private const string secretKey = "PLACE YOUR SECRET KEY HERE";

    private void Start()
    {
        ServicePointManager.ServerCertificateValidationCallback = TrustCertificate;

        if (pdfLoader == null) pdfLoader = FindObjectOfType<PDFLoader>();
        if (pdfLoader == null) Debug.LogError("PDFLoader not found in the scene!");

        if (modelLoader == null) modelLoader = FindObjectOfType<ModelLoader>();
        if (modelLoader == null) Debug.LogError("ModelLoader not found in the scene!");

        create3DButton.onClick.AddListener(OnCreate3DButtonClick);
        
        if (loadingContainer != null)
        {
            loadingContainer.SetActive(false);
        }
    }

    private bool TrustCertificate(object sender, X509Certificate x509Certificate, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    private IEnumerator PulseAnimation()
    {
        for (int i = 0; i < 3; i++)
        {
            // Scale up (0.2초)
            float t = 0;
            Vector3 startScale = Vector3.one;
            Vector3 endScale = Vector3.one * 1.05f;
            
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                loadingContainer.transform.localScale = Vector3.Lerp(startScale, endScale, t / 0.2f);
                yield return null;
            }

            // Scale down (0.2초)
            t = 0;
            startScale = loadingContainer.transform.localScale;
            endScale = Vector3.one;
            
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                loadingContainer.transform.localScale = Vector3.Lerp(startScale, endScale, t / 0.2f);
                yield return null;
            }
        }

        // 크기를 1로 고정
        loadingContainer.transform.localScale = Vector3.one;
    }

    private void ShowLoadingScreen(bool show, string message = null)
    {
        if (loadingContainer != null)
        {
            loadingContainer.SetActive(show);
            if (show)
            {
                loadingContainer.transform.localScale = Vector3.one;
                if (loadingText != null && message != null)
                {
                    loadingText.text = message;
                }
                if (pulseAnimation != null)
                {
                    StopCoroutine(pulseAnimation);
                }
                pulseAnimation = StartCoroutine(PulseAnimation());
            }
            else
            {
                if (pulseAnimation != null)
                {
                    StopCoroutine(pulseAnimation);
                    pulseAnimation = null;
                }
            }
        }
    }

    private IEnumerator ShowSuccessMessage()
    {
        if (loadingText != null)
        {
            // 성공 메시지 표시
            loadingText.text = "새로운 공룡이 박물관을 찾아왔어요!";
            
            // 3초 대기
            yield return new WaitForSeconds(3f);
            
            // 페이드아웃을 위한 CanvasGroup 확인/추가
            CanvasGroup canvasGroup = loadingContainer.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = loadingContainer.AddComponent<CanvasGroup>();
            
            // 1초간 페이드아웃
            float elapsedTime = 0f;
            canvasGroup.alpha = 1f;
            
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = 1f - (elapsedTime / 1f);
                yield return null;
            }
            
            // 완전히 숨기기
            ShowLoadingScreen(false);
        }
    }

    public async void OnCreate3DButtonClick()
    {
        Debug.Log("Create 3D Button Clicked!");
        
        if (pdfLoader == null)
        {
            Debug.LogError("PDFLoader reference is missing!");
            return;
        }

        ShowLoadingScreen(true, "3D 모델이 로딩 중입니다... 기다리는 동안 박물관을 둘러보세요!");

        int currentPage = pdfLoader.currentPage;
        
        // Upload current page image to S3 first
        bool uploadSuccess = await UploadToS3.Instance.UploadPageImage(currentPage);
        if (!uploadSuccess)
        {
            Debug.LogError("Failed to upload page image to S3");
            ShowLoadingScreen(false);
            return;
        }

        string pageName = $"page-{currentPage:D3}.png";
        string query = queryInput.text;
        string dinosaurName = dinosaurInput.text;

        StartCoroutine(ProcessCreate3DRequest(pageName, query, dinosaurName));
    }

    public async void OnCreate3DIntent(string[] entityValues)
    {
        Debug.Log("Create 3D Intent Detected!");
        
        if (pdfLoader == null)
        {
            Debug.LogError("PDFLoader reference is missing!");
            return;
        }

        ShowLoadingScreen(true, "3D 모델이 로딩 중입니다... 기다리는 동안 박물관을 둘러보세요!");

        int currentPage = pdfLoader.currentPage;
        
        // Upload current page image to S3 first
        bool uploadSuccess = await UploadToS3.Instance.UploadPageImage(currentPage);
        if (!uploadSuccess)
        {
            Debug.LogError("Failed to upload page image to S3");
            ShowLoadingScreen(false);
            return;
        }

        string pageName = $"page-{currentPage:D3}.png";
        string query = entityValues[0];
        string dinosaurName = entityValues[1];

        StartCoroutine(ProcessCreate3DRequest(pageName, query, dinosaurName));
    }

    private IEnumerator ProcessCreate3DRequest(string pageName, string query, string dinosaurName)
    {
        // Check if model already exists locally
        string localModelPath = Path.Combine(Application.dataPath, "Files", $"{dinosaurName}.glb");
        bool modelExists = File.Exists(localModelPath);

        if (modelExists)
        {
            Debug.Log($"Model {dinosaurName}.glb already exists locally. Loading cached version...");
            
            // Wait for 5 seconds to simulate loading
            yield return new WaitForSeconds(5f);
            
            string relativePath = ConvertToRelativePath(localModelPath);
            var loadModelTask = modelLoader.LoadModelAsync(relativePath);
            
            while (!loadModelTask.IsCompleted)
            {
                yield return null;
            }

            if (loadModelTask.Exception != null)
            {
                ShowLoadingScreen(false);
                yield break;
            }

            yield return StartCoroutine(ShowSuccessMessage());
            yield break;
        }

        var requestData = new Dictionary<string, string>
        {
            { "page_name", pageName },
            { "query", query },
            { "dinosaur_name", dinosaurName },
            { "intent", intent }
        };
        
        string jsonRequestData = JsonConvert.SerializeObject(requestData);

        using (UnityWebRequest www = new UnityWebRequest($"{fastApiBaseUrl}/process", "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonRequestData);
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.certificateHandler = new BypassCertificate();

            float elapsedTime = 0f;
            bool isTimeout = false;

            www.SendWebRequest();

            while (!www.isDone)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime > apiTimeout)
                {
                    isTimeout = true;
                    break;
                }
                yield return null;
            }

            if (isTimeout || www.result != UnityWebRequest.Result.Success)
            {
                ShowLoadingScreen(false);
                yield break;
            }

            string jsonResponse = www.downloadHandler.text;
            Debug.Log($"Raw POST response: {jsonResponse}");
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);
            string taskId = response["task_id"];

            yield return StartCoroutine(PollForResults(taskId));
        }
    }

    private IEnumerator PollForResults(string taskId)
    {
        bool isTimeout = false;

        while (!isTimeout)
        {
            using (UnityWebRequest www = UnityWebRequest.Get($"{fastApiBaseUrl}/result/{taskId}"))
            {
                www.certificateHandler = new BypassCertificate();

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = www.downloadHandler.text;
                    Debug.Log($"Raw GET response: {jsonResponse}");
                    var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);

                    if (response.ContainsKey("message") && response["message"].Contains("still processing"))
                    {
                        yield return new WaitForSeconds(pollInterval);
                        continue;
                    }

                    if (response.ContainsKey("model_name"))
                    {
                        string modelName = response["model_name"];
                        Debug.Log($"Retrieved model name: {modelName}");
                        yield return StartCoroutine(DownloadAndLoadModel(modelName));
                    }
                    break;
                }
                else
                {
                    ShowLoadingScreen(false);
                    break;
                }
            }
        }
    }

    private IEnumerator DownloadAndLoadModel(string modelName)
    {
        Task downloadTask = DownloadModelFromS3(modelName);
        while (!downloadTask.IsCompleted)
        {
            yield return null;
        }

        if (downloadTask.IsFaulted)
        {
            ShowLoadingScreen(false);
            yield break;
        }

        string localFilePath = Path.Combine(Application.dataPath, "Files", modelName);
        string relativePath = ConvertToRelativePath(localFilePath);

        yield return new WaitForSeconds(0.5f);

        var loadModelTask = modelLoader.LoadModelAsync(relativePath);
            
        while (!loadModelTask.IsCompleted)
        {
            yield return null;
        }

        if (loadModelTask.Exception != null)
        {
            ShowLoadingScreen(false);
            yield break;
        }

        yield return StartCoroutine(ShowSuccessMessage());
    }

    private async Task DownloadModelFromS3(string fileName)
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
            catch (Exception e)
            {
                Debug.LogError($"Error downloading file from S3: {e.Message}");
                throw;
            }
        }
    }

    private string ConvertToRelativePath(string absolutePath)
    {
        string assetsPath = Path.GetFullPath(Application.dataPath);
        if (absolutePath.StartsWith(assetsPath))
        {
            string relativePath = "Assets" + absolutePath.Substring(assetsPath.Length);
            return relativePath.Replace('\\', '/');
        }
        return absolutePath;
    }
}

public class BypassCertificate : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}