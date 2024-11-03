using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using GLTFast;

public class ModelLoader : MonoBehaviour
{
    public async Task LoadModelAsync(string modelPath)
    {
        string absolutePath = GetAbsolutePath(modelPath);
        string extension = Path.GetExtension(absolutePath).ToLower();

        if (extension == ".glb" || extension == ".gltf")
        {
            await LoadGLTFModelAsync(absolutePath);
        }
        else if (extension == ".fbx")
        {
            await LoadFBXModelAsync(absolutePath);
        }
        else
        {
            Debug.LogError("지원되지 않는 파일 형식입니다: " + extension);
        }
    }

    private string GetAbsolutePath(string path)
    {
        if (Path.IsPathRooted(path))
        {
            return path;
        }
        
        // 상대 경로를 절대 경로로 변환
        string absolutePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));
        
        // file:// 프로토콜 추가
        return "file://" + absolutePath.Replace("\\", "/");
    }

    private async Task LoadGLTFModelAsync(string modelPath)
    {
        // MainDinosaur 태그를 가진 오브젝트를 찾아서 숨깁니다.
        GameObject mainDinosaur = GameObject.FindGameObjectWithTag("MainDinosaur");
        if (mainDinosaur != null)
        {
            mainDinosaur.SetActive(false);
            Debug.Log("MainDinosaur 오브젝트를 숨겼습니다.");
        }
        else
        {
            Debug.LogWarning("MainDinosaur 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }

        var gltf = new GltfImport();
        
        // URI 형식으로 변환
        string uri = new Uri(modelPath).AbsoluteUri;
        
        bool success = await gltf.Load(uri);

        if (success)
        {
            var instantiator = new GameObjectInstantiator(gltf, transform);
            await gltf.InstantiateMainSceneAsync(instantiator);
            Debug.Log("GLB/GLTF 모델 로드 완료: " + modelPath);
        }
        else
        {
            Debug.LogError("GLB/GLTF 모델을 로드하지 못했습니다: " + modelPath);
            
            // 로드 실패 시 MainDinosaur를 다시 보이게 합니다.
            if (mainDinosaur != null)
            {
                mainDinosaur.SetActive(true);
                Debug.Log("GLB/GLTF 모델 로드 실패로 MainDinosaur 오브젝트를 다시 보이게 했습니다.");
            }
        }
    }

    private async Task LoadFBXModelAsync(string modelPath)
    {
        #if UNITY_EDITOR
        await Task.Yield();

        // file:// 프로토콜 제거
        string assetPath = modelPath.Replace("file://", "").Replace("/", "\\");
        
        GameObject modelPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (modelPrefab != null)
        {            
            Instantiate(modelPrefab, transform.position, transform.rotation, transform);
            Debug.Log("FBX 모델 로드 완료: " + modelPath);
        }
        else
        {
            Debug.LogError("FBX 모델을 찾을 수 없습니다: " + modelPath);
        }
        #else
        Debug.LogWarning("FBX 로딩은 Unity 에디터에서만 작동합니다.");
        await Task.CompletedTask;
        #endif
    }
}