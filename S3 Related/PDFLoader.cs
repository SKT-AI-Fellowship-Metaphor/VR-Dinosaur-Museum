using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;

public class PDFLoader : MonoBehaviour
{
    public string outputFolder = "Assets/Images/";
    public Renderer quadRenderer;
    public GameObject quadObject;
    public Button leftPageButton;
    public Button rightPageButton;
    public TextMeshProUGUI pageNumberText;

    public int currentPage = 0;
    private List<string> pageFiles = new List<string>();

    void Start()
    {
        quadRenderer = quadObject.GetComponent<Renderer>();
        leftPageButton.onClick.AddListener(PreviousPage);
        rightPageButton.onClick.AddListener(NextPage);
    }

    // Update 함수 - 키보드 입력 처리
    void Update()
    {
        // Input Manager
        if (!InputManager.Instance.IsInputAllowed())
        {
            return;
        }

        // 왼쪽 대괄호 키([)를 눌렀을 때
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            PreviousPage();
        }
        // 오른쪽 대괄호 키(])를 눌렀을 때
        else if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            NextPage();
        }
    }

    public IEnumerator ConvertPDFToImagesAndDisplay(string pdfPath)
    {
        yield return StartCoroutine(ConvertPDFToImages(pdfPath));
        UnityEngine.Debug.Log("CONVERTED PDF to IMAGE!");
        
        pageFiles = new List<string>(Directory.GetFiles(outputFolder, "page-*.png"));
        pageFiles.Sort();

        LoadAndDisplayImage(currentPage);
        UpdatePageNumberDisplay();
    }

    private IEnumerator ConvertPDFToImages(string pdfPath)
    {
        Process process = new Process();
        UnityEngine.Debug.Log("CONVERTING PDF to IMAGE...");
        process.StartInfo.FileName = "/opt/homebrew/bin/convert";
        process.StartInfo.Arguments = $"{pdfPath} {outputFolder}page-%03d.png";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;

        process.Start();
        process.WaitForExit();

        yield return null;
    }

    private void LoadAndDisplayImage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < pageFiles.Count)
        {
            UnityEngine.Debug.Log($"LoadAndDisplayImage: {pageIndex}");
            string pagePath = pageFiles[pageIndex];
            StartCoroutine(LoadImageAndApply(pagePath));
            UpdatePageNumberDisplay();
        }
        else
        {
            UnityEngine.Debug.LogError("Invalid page index: " + pageIndex);
        }
    }

    private IEnumerator LoadImageAndApply(string path)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + Path.GetFullPath(path)))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.LogError("Failed to load image: " + uwr.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                quadRenderer.material.mainTexture = texture;
                ResizeQuad(texture.width, texture.height);
                UnityEngine.Debug.Log("Image loaded successfully!");
            }
        }
    }

    private void ResizeQuad(float width, float height)
    {
        float aspectRatio = width / height;
        Vector3 newScale = new Vector3(width * 1, height * 1, 1);
        quadObject.transform.localScale = newScale;
        UnityEngine.Debug.Log($"Resized Quad to: {newScale}");
    }

    public void NextPage()
    {
        UnityEngine.Debug.Log("==================================");
        UnityEngine.Debug.Log("Next Page");
        if (currentPage < pageFiles.Count - 1)
        {
            currentPage++;
            LoadAndDisplayImage(currentPage);
        }
    }

    public void PreviousPage()
    {
        UnityEngine.Debug.Log("==================================");
        UnityEngine.Debug.Log("Previous Page");
        if (currentPage > 0)
        {
            currentPage--;
            LoadAndDisplayImage(currentPage);
        }
    }

    private void UpdatePageNumberDisplay()
    {
        if (pageNumberText != null)
        {
            pageNumberText.text = $"Page {currentPage + 1}/{pageFiles.Count}";
        }
    }
}