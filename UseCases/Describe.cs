using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class Describe : MonoBehaviour
{
    [SerializeField] private Button describeButton;
    [SerializeField] private Button educationButton;
    [SerializeField] private PDFLoader pdfLoader;
    [SerializeField] private TMP_InputField queryInput;
    [SerializeField] private TMP_InputField dinosaurInput;
    [SerializeField] private string fastApiBaseUrl = "PLACE YOUR SERVER URL HERE";

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image descriptionImage;
    [SerializeField] private TextMeshProUGUI sourceText;
    [SerializeField] private TextMeshProUGUI loadingText;
    
    [SerializeField] private GameObject contentContainer;

    private const string intent = "describe";
    private const float apiTimeout = 30f;
    private const float pollInterval = 2f;

    private void Start()
    {
        ServicePointManager.ServerCertificateValidationCallback = TrustCertificate;

        if (pdfLoader == null) pdfLoader = FindObjectOfType<PDFLoader>();
        if (pdfLoader == null) Debug.LogError("PDFLoader not found in the scene!");

        describeButton.onClick.AddListener(OnDescribeButtonClick);

        ShowContent(false);
        if (loadingText != null) loadingText.gameObject.SetActive(false);
    }

    private bool TrustCertificate(object sender, X509Certificate x509Certificate, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    private void ShowContent(bool show)
    {
        if (contentContainer != null)
        {
            contentContainer.SetActive(show);
        }
        else
        {
            if (titleText != null) titleText.gameObject.SetActive(show);
            if (subtitleText != null) subtitleText.gameObject.SetActive(show);
            if (descriptionText != null) descriptionText.gameObject.SetActive(show);
            if (descriptionImage != null) descriptionImage.gameObject.SetActive(show);
            if (sourceText != null) sourceText.gameObject.SetActive(show);
        }
    }

    public async void OnDescribeButtonClick()
    {
        Debug.Log("Describe Button Clicked!");        

        if (educationButton != null)
        {
            educationButton.gameObject.SetActive(true);
            Debug.Log("Education button activated");
        }

        ShowContent(false);
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = "로딩 중...";
        }

        int currentPage = pdfLoader.currentPage;

        // Upload current page image to S3 first
        bool uploadSuccess = await UploadToS3.Instance.UploadPageImage(currentPage);
        if (!uploadSuccess)
        {
            Debug.LogError("Failed to upload page image to S3");
            if (loadingText != null) loadingText.gameObject.SetActive(false);
            return;
        }

        string pageName = $"page-{currentPage:D3}.png";
        string query = queryInput.text;
        string dinosaurName = dinosaurInput.text;

        StartCoroutine(ProcessDescribeRequest(pageName, query, dinosaurName));
    }

    public async void OnDescribeIntent(string[] entityValues)
    {
        Debug.Log("Describe Intent Detected!");        

        if (educationButton != null)
        {
            educationButton.gameObject.SetActive(true);
            Debug.Log("Education button activated");
        }

        ShowContent(false);
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = "로딩 중...";
        }

        int currentPage = pdfLoader.currentPage;

        // Upload current page image to S3 first
        bool uploadSuccess = await UploadToS3.Instance.UploadPageImage(currentPage);
        if (!uploadSuccess)
        {
            Debug.LogError("Failed to upload page image to S3");
            if (loadingText != null) loadingText.gameObject.SetActive(false);
            return;
        }

        string pageName = $"page-{currentPage:D3}.png";
        string query = entityValues[0];
        string dinosaurName = entityValues[1];

        StartCoroutine(ProcessDescribeRequest(pageName, query, dinosaurName));
    }

    private IEnumerator ProcessDescribeRequest(string pageName, string query, string dinosaurName)
    {
        var requestData = new Dictionary<string, string>
        {
            { "page_name", pageName },
            { "query", query },
            { "dinosaur_name", dinosaurName },
            { "intent", "describe" }
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

            if (isTimeout)
            {
                Debug.LogError("Request timed out");
                if (loadingText != null) loadingText.gameObject.SetActive(false);
                yield break;
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to send request: {www.error}");
                if (loadingText != null) loadingText.gameObject.SetActive(false);
                yield break;
            }

            string jsonResponse = www.downloadHandler.text;
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
                    var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);

                    if (response.ContainsKey("message") && response["message"].Contains("still processing"))
                    {
                        if (loadingText != null) 
                        {
                            loadingText.gameObject.SetActive(true);
                            loadingText.text = "로딩 중...";
                        }
                        yield return new WaitForSeconds(pollInterval);
                        continue;
                    }

                    if (loadingText != null) loadingText.gameObject.SetActive(false);
                    ProcessFastAPIResponse(jsonResponse);
                    
                    if (response.ContainsKey("image_url") && !string.IsNullOrEmpty(response["image_url"]))
                    {
                        yield return StartCoroutine(LoadImageFromUrl(response["image_url"]));
                    }
                    break;
                }
                else
                {
                    Debug.LogError($"Failed to get results: {www.error}");
                    if (loadingText != null) loadingText.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }

    private void ProcessFastAPIResponse(string jsonResponse)
    {
        try
        {
            Dictionary<string, string> response = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);

            titleText.text = response["title"];
            subtitleText.text = response["subtitle"];
            descriptionText.text = response["description"];
            sourceText.text = response["source"];

            ShowContent(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error processing API response: {ex.Message}");
            if (loadingText != null) loadingText.gameObject.SetActive(false);
        }
    }

    private IEnumerator LoadImageFromUrl(string imageUrl)
    {
        bool success = false;
        yield return UrlImageLoader.Instance.LoadImageFromUrl(imageUrl, descriptionImage).ContinueWith(task =>
        {
            success = task.Result;
        });

        if (!success)
        {
            Debug.LogError($"Failed to load image from URL: {imageUrl}");
        }
    }
}