using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Moe.ResourcesManager;

/// <summary>
/// A unique resource id
/// </summary>
public sealed class ResourceID : IEquatable<ResourceID>
{
    /// <summary>
    /// The human readable resource name
    /// </summary>
    public string DisplayName { get; init; }

    /// <summary>
    /// the unique uri of the resource
    /// </summary>
    public Uri UniqueUri { get; init; }

    public ResourceID(string displayName, Uri uniqueUri)
    {
        DisplayName = displayName;
        UniqueUri = uniqueUri;
    }

    public ResourceID(string fileName)
    {
        fileName = Path.GetFullPath(fileName);

        if (File.Exists(fileName))
        {
            throw new ArgumentException("the file must exists", nameof(fileName));
        }

        var file = new FileInfo(fileName);

        fileName = file.LinkTarget ?? fileName;

        fileName = Path.GetFullPath(fileName);

        DisplayName = $"file:{fileName}";
        UniqueUri = new Uri($"file://localhost/{Uri.EscapeDataString(fileName)}");
    }

    /// <summary>
    /// Create a unique id,it will return different id in different call.
    /// This is useful when marking generated-in-runtime and in-memory resources.
    /// </summary>
    /// <returns></returns>
    public static ResourceID CreateUnique()
    {
        var random = RandomNumberGenerator.GetHexString(32, true);

        return new ResourceID(
            "unique resource generated in runtime",
            new($"inmemory-resource://{Uri.EscapeDataString(random)}"));
    }

    public bool Equals(ResourceID? other)
    {
        if (other == null)
        {
            return false;
        }

        return ((IEquatable<Uri>)UniqueUri).Equals(other.UniqueUri);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ResourceID);
    }

    public override int GetHashCode()
    {
        return UniqueUri.GetHashCode();
    }
}
