using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
//using UnityEngine.AddressableAssets;
//using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace DrawParkour
{
    using System.Linq;
    using UIManager;
    public class AssetBundleManager : SingletonMono<AssetBundleManager>
    {
        // private Dictionary<string, SceneInstance> _sceneInstances = new Dictionary<string, SceneInstance>();

        /// <summary>
        /// Use this method to download asset bundle in loading screen. The assets after download will be release to save memory
        /// </summary>
        /// <param name="key"></param>
        //public async UniTask DownloadThenReleaseAssets<T>(IList<string> keys, IProgress<float> progress, float startProgressValue, float progressWeight = 1)
        //{
        //    var downloadSize = await Addressables.GetDownloadSizeAsync(keys);
        //    Debug.Log("Download size: " + downloadSize);
        //    if (downloadSize > 0)
        //    {
        //        foreach (var key in keys)
        //        {
        //            Debug.Log("Loading key: " + key);
        //            var keyDownloadSize = await Addressables.GetDownloadSizeAsync(key);
        //            var handler = Addressables.LoadAssetsAsync<T>(key, null);
        //            while (!handler.IsDone)
        //            {
        //                progress?.Report(startProgressValue + progressWeight * handler.PercentComplete * keyDownloadSize / downloadSize);
        //                await UniTask.WaitForEndOfFrame();
        //            }

        //            progress?.Report(startProgressValue + progressWeight * keyDownloadSize / (float)downloadSize);
        //            Addressables.Release(handler);
        //        }

        //        progress?.Report(startProgressValue + progressWeight);
        //    }
        //}

        //public async UniTask<T> LoadAsset<T>(object key)
        //{
        //    var asset = await Addressables.LoadAssetAsync<T>(key);
        //    return asset;
        //}
        //public async UniTask<IList<T>> LoadAssets<T>(object key)
        //{
        //    var asset = await Addressables.LoadAssetsAsync<T>(key, null);
        //    return asset;
        //}

        public async UniTask LoadScene(string sceneName, LoadSceneMode mode, IProgress<float> progress = null)
        {
            await SceneManager.LoadSceneAsync(sceneName, mode);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            // if (_sceneInstances.ContainsKey(sceneName)) return;
            // _sceneInstances.Add(sceneName, new SceneInstance());
            // var sceneInstance = await Addressables.LoadSceneAsync(sceneName, mode);
            // SceneManager.SetActiveScene(sceneInstance.Scene);
            // _sceneInstances[sceneName] = sceneInstance;
        }

        public async UniTask UnloadScene(string sceneName)
        {
            // UIManager.Instance.HideAllPopup();

            try
            {
                await SceneManager.UnloadSceneAsync(sceneName);
            }catch
            {
#pragma warning disable CS0618 // Type or member is obsolete
                List<Scene> scenes = SceneManager.GetAllScenes().ToList();
                scenes.ForEach(x => Debug.LogError(x.name));

                Debug.LogError("UnLoading Fail ??? " + sceneName + " --- " + SceneManager.GetActiveScene().name);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            // if (_sceneInstances.ContainsKey(sceneName))
            // {
            //     await Addressables.UnloadSceneAsync(_sceneInstances[sceneName]);
            //     _sceneInstances.Remove(sceneName);
            //     await Resources.UnloadUnusedAssets();
            // }
        }
    }
}