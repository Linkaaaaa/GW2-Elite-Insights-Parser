using System.Runtime.CompilerServices;
using static GW2EIEvtcParser.ParserHelper;

namespace GW2EIEvtcParser.ParserHelpers;
public static class StringExtensions
{
    internal readonly ref struct SplitResult
    {
        /// The left part of the split.
        public readonly ReadOnlySpan<char> Tail;
        /// The right part of the split. If the split was not found this may be empty.
        public readonly ReadOnlySpan<char> Head;

        public SplitResult(ReadOnlySpan<char> tail, ReadOnlySpan<char> head)
        {
            Tail = tail;
            Head = head;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SplitResult SplitOnce(this in ReadOnlySpan<char> str, char split)
    {
        var pivot = str.IndexOf(split);
        if (pivot == -1) { return new(str, ReadOnlySpan<char>.Empty); }
        return new(str[..pivot], str[(pivot + 1)..]);
    }

    internal static string ToHexString(ReadOnlySpan<byte> bytes)
    {
        using var buffer = new ArrayPoolReturner<char>(bytes.Length * 2);
        AppendHexString(buffer, bytes);
        return new String(buffer);
    }

    internal static void AppendHexString(Span<char> destination, ReadOnlySpan<byte> bytes)
    {
        const string CHARSET = "0123456789ABCDEF";
        int offset = 0;
        foreach (var c in bytes)
        {
            destination[offset++] = CHARSET[(c & 0xf0) >> 4];
            destination[offset++] = CHARSET[c & 0x0f];
        }
    }

    public static string ToDurationString(long duration)
    {
        var durationTimeSpan = TimeSpan.FromMilliseconds(Math.Abs(duration));
        string durationString = durationTimeSpan.ToString("mm") + "m " + durationTimeSpan.ToString("ss") + "s " + durationTimeSpan.Milliseconds + "ms";
        if (durationTimeSpan.Hours > 0)
        {
            durationString = durationTimeSpan.ToString("hh") + "h " + durationString;
        }
        if (duration < 0)
        {
            durationString = "-" + durationString;
        }
        return durationString;
    }
}
