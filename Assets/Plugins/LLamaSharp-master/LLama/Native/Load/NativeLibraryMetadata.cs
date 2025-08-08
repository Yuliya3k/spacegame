
namespace LLama.Native
{
    /// <summary>
    /// Information of a native library file.
    /// </summary>
    public record class NativeLibraryMetadata
    {
        public NativeLibraryName NativeLibraryName { get; init; }
        public bool UseCuda { get; init; }
        public bool UseVulkan { get; init; }
        public AvxLevel AvxLevel { get; init; }

        public NativeLibraryMetadata(NativeLibraryName nativeLibraryName, bool useCuda, bool useVulkan, AvxLevel avxLevel)
        {
            NativeLibraryName = nativeLibraryName;
            UseCuda = useCuda;
            UseVulkan = useVulkan;
            AvxLevel = avxLevel;
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"(NativeLibraryName: {NativeLibraryName}, UseCuda: {UseCuda}, UseVulkan: {UseVulkan}, AvxLevel: {AvxLevel})";
        }
    }

    /// <summary>
    /// Avx support configuration
    /// </summary>
    public enum AvxLevel
    {
        /// <summary>
        /// No AVX
        /// </summary>
        None,

        /// <summary>
        /// Advanced Vector Extensions (supported by most processors after 2011)
        /// </summary>
        Avx,

        /// <summary>
        /// AVX2 (supported by most processors after 2013)
        /// </summary>
        Avx2,

        /// <summary>
        /// AVX512 (supported by some processors after 2016, not widely supported)
        /// </summary>
        Avx512,
    }
}
