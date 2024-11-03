using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

public class UrlImageLoader : MonoBehaviour
{
    public static UrlImageLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task<bool> LoadImageFromUrl(string imageUrl, Image targetImage)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            Debug.LogError("Image URL is empty");
            return false;
        }

        try
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl))
            {
                var operation = www.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to load image from URL: {www.error}");
                    return false;
                }

                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                targetImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                Debug.Log($"Image loaded from URL: {imageUrl}");
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading image from URL: {e.Message}");
            return false;
        }
    }
}