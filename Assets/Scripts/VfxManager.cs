using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class VfxData
{
    public string vfxName;
    public GameObject vfxPrefab;
    [Tooltip("Seconds before auto-return to pool. 0 = detect from ParticleSystem duration.")]
    public float lifetime = 0f;
}

public class VfxManager : MonoBehaviour
{
    public static VfxManager Instance { get; private set; }

    [SerializeField] private List<VfxData> _vfxList = new();


    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        RegisterAllPrefabs();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public GameObject SpawnVfx(string vfxName, Vector3 position, Quaternion rotation)
    {
        VfxData data = _vfxList.Find(v => v.vfxName.ToLower() == vfxName.ToLower());
        if (data == null || data.vfxPrefab == null)
        {
            Debug.LogWarning($"[VfxManager] VFX '{vfxName}' not found or prefab is null.");
            return null;
        }

        GameObject instance = ObjectPoolManager.Get(data.vfxPrefab, position, rotation);

        float duration = ResolveLifetime(data);
        if (duration > 0f)
            StartCoroutine(ReturnAfterDelay(instance, duration));

        return instance;
    }

    /// <summary>Manual early return — call this to recycle a VFX before its lifetime expires.</summary>
    public void ReturnVfx(GameObject vfxInstance) => ObjectPoolManager.Return(vfxInstance);

    // ── Internal Helpers ──────────────────────────────────────────────────────

    private void RegisterAllPrefabs()
    {
        foreach (var data in _vfxList)
            ObjectPoolManager.RegisterPrefab(data.vfxPrefab);
    }

    private float ResolveLifetime(VfxData data)
    {
        if (data.lifetime > 0f)
            return data.lifetime;

        if (data.vfxPrefab.TryGetComponent(out ParticleSystem ps))
        {
            var main = ps.main;
            return main.duration + main.startLifetime.constantMax + 0.5f;
        }

        return 0f;
    }

    private IEnumerator ReturnAfterDelay(GameObject instance, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (instance != null && instance.activeSelf)
            ObjectPoolManager.Return(instance);
    }
}
