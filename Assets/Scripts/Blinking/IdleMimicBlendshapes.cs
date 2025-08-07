using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drives idle facial blendshapes such as small muscle twitches and blinking.
/// Blendshape groups are chosen at random and played for a duration specified on
/// each group. Blinking runs on its own timer and is not affected by the
/// randomisation of other groups.
/// </summary>
public class IdleMimicBlendshapes : MonoBehaviour
{
    [System.Serializable]
    public class BlendshapeEntry
    {
        public string blendshapeName;
        [Range(0f, 100f)]
        public float minWeight = 0f;
        [Range(0f, 100f)]
        public float maxWeight = 100f;
    }

    [System.Serializable]
    public class BlendshapeGroup
    {
        public string groupName;
        public BlendshapeEntry[] blendshapes;

        [Tooltip("Duration in seconds for this group to play (up and down)")]
        public float duration = 2f;
    }

    [System.Serializable]
    public class ConditionalBlendshape
    {
        [Tooltip("Name of the blendshape to control")] public string blendshapeName;
        [Tooltip("Stat in CharacterStats used to drive this blendshape")]
        public string statName;
        [Tooltip("Minimum value of the stat to map from")] public float statMin = 0f;
        [Tooltip("Maximum value of the stat to map from")] public float statMax = 100f;
        [Range(0f, 100f)] public float minWeight = 0f;
        [Range(0f, 100f)] public float maxWeight = 100f;
    }

    [Header("Renderer")]
    public SkinnedMeshRenderer skinnedRenderer;

    [Header("Blink Settings")]
    public string blinkLeft = "Eye_Blink_L";
    public string blinkRight = "Eye_Blink_R";
    public float minBlinkInterval = 3f;
    public float maxBlinkInterval = 6f;
    public float blinkDuration = 0.1f;

    [Header("Random Groups")]
    public BlendshapeGroup[] groups;
    public float minGroupInterval = 2f;
    public float maxGroupInterval = 5f;
    public float minSpeed = 1f;
    public float maxSpeed = 2f;

    [Header("Conditional Blendshapes")]
    public CharacterStats characterStats;
    public ConditionalBlendshape[] conditionalBlendshapes;
    public float conditionUpdateInterval = 0.5f;
    private readonly Dictionary<string, int> _indexCache = new Dictionary<string, int>();
    private readonly Dictionary<string, float> _initialWeights = new Dictionary<string, float>();

    private void Awake()
    {
        if (skinnedRenderer == null)
        {
            skinnedRenderer = GetComponent<SkinnedMeshRenderer>();
        }
        CacheIndices();
        StoreInitialWeights();
    }

    private void OnEnable()
    {
        ApplyInitialWeights();
        StartCoroutine(BlinkRoutine());
        StartCoroutine(RandomGroupRoutine());
        if (conditionalBlendshapes != null && conditionalBlendshapes.Length > 0)
            StartCoroutine(ConditionalRoutine());
    }

    private void CacheIndices()
    {
        if (skinnedRenderer == null || skinnedRenderer.sharedMesh == null)
            return;

        _indexCache.Clear();
        Mesh mesh = skinnedRenderer.sharedMesh;
        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            string name = mesh.GetBlendShapeName(i);
            if (!_indexCache.ContainsKey(name))
                _indexCache.Add(name, i);
        }
    }

    private void StoreInitialWeights()
    {
        _initialWeights.Clear();
        if (skinnedRenderer == null || skinnedRenderer.sharedMesh == null)
            return;

        Mesh mesh = skinnedRenderer.sharedMesh;
        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            string name = mesh.GetBlendShapeName(i);
            float weight = skinnedRenderer.GetBlendShapeWeight(i);
            if (!_initialWeights.ContainsKey(name))
                _initialWeights.Add(name, weight);
        }
    }

    private void ApplyInitialWeights()
    {
        if (skinnedRenderer == null)
            return;

        foreach (var kvp in _initialWeights)
        {
            SetBlendshape(kvp.Key, kvp.Value);
        }
    }

    public void ResetToInitialWeights()
    {
        ApplyInitialWeights();
    }
    
    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            float wait = Random.Range(minBlinkInterval, maxBlinkInterval);
            yield return new WaitForSeconds(wait);
            yield return StartCoroutine(PlayBlink());
        }
    }

    private IEnumerator PlayBlink()
    {
        float timer = 0f;
        while (timer < blinkDuration)
        {
            float t = timer / blinkDuration;
            SetBlendshape(blinkLeft, Mathf.Lerp(0f, 100f, t));
            SetBlendshape(blinkRight, Mathf.Lerp(0f, 100f, t));
            timer += Time.deltaTime * 1f;
            yield return null;
        }
        timer = 0f;
        while (timer < blinkDuration)
        {
            float t = timer / blinkDuration;
            SetBlendshape(blinkLeft, Mathf.Lerp(100f, 0f, t));
            SetBlendshape(blinkRight, Mathf.Lerp(100f, 0f, t));
            timer += Time.deltaTime * 1f;
            yield return null;
        }
        SetBlendshape(blinkLeft, 0f);
        SetBlendshape(blinkRight, 0f);
    }

    private IEnumerator RandomGroupRoutine()
    {
        if (groups == null || groups.Length == 0)
            yield break;

        while (true)
        {
            float wait = Random.Range(minGroupInterval, maxGroupInterval);
            yield return new WaitForSeconds(wait);
            BlendshapeGroup group = groups[Random.Range(0, groups.Length)];
            yield return StartCoroutine(PlayGroup(group));
        }
    }

    private IEnumerator PlayGroup(BlendshapeGroup group)
    {
        if (group.duration <= 0f)
            yield break;

        float halfDuration = group.duration * 0.5f;

        // Cache the starting and target weights for each blendshape in the group
        float[] startWeights = new float[group.blendshapes.Length];
        float[] targetWeights = new float[group.blendshapes.Length];
        for (int i = 0; i < group.blendshapes.Length; i++)
        {
            var b = group.blendshapes[i];
            startWeights[i] = GetBlendshape(b.blendshapeName);
            targetWeights[i] = Random.Range(b.minWeight, b.maxWeight);
        }

        float timer = 0f;
        while (timer < halfDuration)
        {
            float t = timer / halfDuration;
            for (int i = 0; i < group.blendshapes.Length; i++)
            {
                var b = group.blendshapes[i];
                float weight = Mathf.Lerp(startWeights[i], targetWeights[i], t);
                SetBlendshape(b.blendshapeName, weight);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0f;
        while (timer < halfDuration)
        {
            float t = timer / halfDuration;
            for (int i = 0; i < group.blendshapes.Length; i++)
            {
                var b = group.blendshapes[i];
                float weight = Mathf.Lerp(targetWeights[i], startWeights[i], t);
                SetBlendshape(b.blendshapeName, weight);
            }
            timer += Time.deltaTime;
            yield return null;
        }
        for (int i = 0; i < group.blendshapes.Length; i++)
            SetBlendshape(group.blendshapes[i].blendshapeName, startWeights[i]);
    }

    private IEnumerator ConditionalRoutine()
    {
        while (true)
        {
            UpdateConditionalBlendshapes();
            yield return new WaitForSeconds(conditionUpdateInterval);
        }
    }

    private void UpdateConditionalBlendshapes()
    {
        if (characterStats == null || conditionalBlendshapes == null)
            return;

        foreach (var c in conditionalBlendshapes)
        {
            float stat = characterStats.GetStat(c.statName);
            float t = 0f;
            if (Mathf.Approximately(c.statMax, c.statMin))
                t = 0f;
            else
                t = Mathf.InverseLerp(c.statMin, c.statMax, stat);
            float weight = Mathf.Lerp(c.minWeight, c.maxWeight, t);
            SetBlendshape(c.blendshapeName, weight);
        }
    }

    private void SetBlendshape(string name, float weight)
    {
        if (!_indexCache.TryGetValue(name, out int index))
            return;
        skinnedRenderer.SetBlendShapeWeight(index, weight);
    }

    private float GetBlendshape(string name)
    {
        if (!_indexCache.TryGetValue(name, out int index))
            return 0f;
        return skinnedRenderer.GetBlendShapeWeight(index);
    }
    
}