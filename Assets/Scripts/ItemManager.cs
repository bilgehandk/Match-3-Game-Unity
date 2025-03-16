using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class ItemManager
{
    public static List<Item> Items = new List<Item>();

    public static async Task Initialize()
    {
        AsyncOperationHandle<IList<Item>> handle = Addressables.LoadAssetsAsync<Item>("Item", null);

        await handle.Task; 

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Items.AddRange(handle.Result);
            Debug.Log($"{Items.Count} uploaded");
        }
        else
        {
            Debug.LogError("Addressable asset have a error.");
        }
    }
}