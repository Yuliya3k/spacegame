
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using LLama;
using LLama.Common;

public class LocalLLM : MonoBehaviour
{
    [Tooltip("File name inside StreamingAssets/Models/ or remote server")]
    public string modelFile = "tinyllama-1.1b-chat.q4_0.gguf";

    [Tooltip("Optional URL to download the model if it is missing")]
    public string downloadUrl = "https://yourserver.com/models/";

    private LLamaContext context;
    private InteractiveExecutor executor;
    public bool Ready { get; private set; }

    async void Awake()
    {
        // Ensure the model is present
        string modelDir = Path.Combine(Application.streamingAssetsPath, "Models");
        Directory.CreateDirectory(modelDir);
        string modelPath = Path.Combine(modelDir, modelFile);

        if (!File.Exists(modelPath) && !string.IsNullOrEmpty(downloadUrl))
        {
            using UnityWebRequest www = UnityWebRequest.Get(downloadUrl + modelFile);
            await www.SendWebRequest();
            File.WriteAllBytes(modelPath, www.downloadHandler.data);
        }

        // Load model on a background thread so the main thread stays responsive
        await Task.Run(() =>
        {
            var p = new ModelParams(modelPath) { ContextSize = 2048 };
            context = new LLamaContext(p);
            executor = new InteractiveExecutor(context);
        });

        Ready = true;
        Debug.Log("Local LLM ready");
    }

    public async Task<string> ReplyAsync(string prompt)
    {
        if (!Ready) return "(model not ready)";
        return await Task.Run(() =>
        {
            var sb = new StringBuilder();
            foreach (var token in executor.Infer(prompt))
                sb.Append(token);
            return sb.ToString();
        });
    }

    void OnDestroy()
    {
        executor?.Dispose();
        context?.Dispose();
    }
}
