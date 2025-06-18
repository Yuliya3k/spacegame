using UnityEngine;
using UnityEditor;
using System.IO;
using System.Security.Cryptography;

public class AssetBundleEncryptorEditor : EditorWindow
{
    private string sourcePath = "Assets/AssetBundles";
    private string destinationPath = "Assets/EncryptedAssetBundles";
    private string keyString = "12345678901234567890123456789012"; // 32 characters
    private string ivString = "1234567890123456"; // 16 characters

    [MenuItem("Assets/Encrypt Asset Bundles")]
    public static void ShowWindow()
    {
        GetWindow(typeof(AssetBundleEncryptorEditor), false, "Encrypt Asset Bundles");
    }

    void OnGUI()
    {
        GUILayout.Label("Encrypt Asset Bundles", EditorStyles.boldLabel);
        sourcePath = EditorGUILayout.TextField("Source Path", sourcePath);
        destinationPath = EditorGUILayout.TextField("Destination Path", destinationPath);
        keyString = EditorGUILayout.TextField("Encryption Key (32 chars)", keyString);
        ivString = EditorGUILayout.TextField("Initialization Vector (16 chars)", ivString);

        if (GUILayout.Button("Encrypt"))
        {
            if (keyString.Length != 32 || ivString.Length != 16)
            {
                Debug.LogError("Key must be 32 characters and IV must be 16 characters long.");
                return;
            }

            byte[] key = System.Text.Encoding.UTF8.GetBytes(keyString);
            byte[] iv = System.Text.Encoding.UTF8.GetBytes(ivString);

            // Call encryption process
            AssetBundleEncryptor.EncryptAssetBundles(sourcePath, destinationPath, key, iv);

            AssetDatabase.Refresh();
            Debug.Log("Asset Bundles encrypted successfully.");
        }
    }
}

public static class AssetBundleEncryptor
{
    public static void EncryptAssetBundles(string sourcePath, string destinationPath, byte[] key, byte[] iv)
    {
        // Ensure the destination path exists
        Directory.CreateDirectory(destinationPath);

        // Get all files in the source directory (filtering out unwanted files)
        string[] files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
        Debug.Log($"Found {files.Length} files in {sourcePath}");

        int fileIndex = 0;
        foreach (var filePath in files)
        {
            // Skip manifest and meta files
            if (filePath.EndsWith(".manifest") || filePath.EndsWith(".meta")) continue;

            // Display progress bar
            float progress = (float)fileIndex / files.Length;
            EditorUtility.DisplayProgressBar("Encrypting Asset Bundles", $"Encrypting {Path.GetFileName(filePath)}", progress);

            // Read and encrypt the file data
            byte[] data = File.ReadAllBytes(filePath);
            byte[] encryptedData = Encrypt(data, key, iv);

            // Prepare the destination file path
            string relativePath = filePath.Substring(sourcePath.Length + 1); // Preserve relative paths
            string destFilePath = Path.Combine(destinationPath, relativePath);

            // Ensure the destination directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));

            // Write the encrypted data to the destination path
            File.WriteAllBytes(destFilePath, encryptedData);

            Debug.Log($"Encrypted file saved to: {destFilePath}");
            fileIndex++;
        }

        // Clear the progress bar
        EditorUtility.ClearProgressBar();
    }

    private static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Padding = PaddingMode.PKCS7;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();  // Make sure to write everything
                }
                return ms.ToArray();
            }
        }
    }
}
