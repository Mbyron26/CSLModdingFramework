using System;

namespace CSLModdingFramework.Lib.Text;

public struct StringSlice : IEquatable<StringSlice> {
    private readonly string _value;
    private readonly int _offset;

    public int Length { get; private set; }
    public bool IsEmpty => Length == 0;

    public char this[int index] => (uint)index >= (uint)Length ? throw new ArgumentOutOfRangeException(nameof(index)) : _value[_offset + index];

    public StringSlice(string value) {
        _value = value ?? throw new ArgumentNullException(nameof(value));
        _offset = 0;
        Length = value.Length;
    }

    private StringSlice(string value, int offset, int length) {
        _value = value;
        _offset = offset;
        Length = length;
    }

    public override bool Equals(object obj) {
        if (obj is StringSlice slice) return Equals(slice);

        if (obj is string s) return Equals(s);

        return false;
    }


    public override int GetHashCode() {
        unchecked {
            var hash = 17;

            for (var i = 0; i < Length; i++) {
                hash = hash * 31 +
                       _value[_offset + i];
            }

            return hash;
        }
    }


    public override string ToString() {
        if (Length == 0) return string.Empty;

        if (_offset == 0 &&
            Length == _value.Length) {
            return _value;
        }

        var buffer = new char[Length];

        _value.CopyTo(_offset, buffer, 0, Length);

        return new string(buffer);
    }

    public StringSlice Slice(int offset) => Slice(offset, Length - offset);

    public StringSlice Slice(int offset, int length) {
        if ((uint)offset > (uint)Length)
            throw new ArgumentOutOfRangeException(nameof(offset));

        if ((uint)length > (uint)(Length - offset))
            throw new ArgumentOutOfRangeException(nameof(length));

        return new StringSlice(_value, _offset + offset, length);
    }

    public int IndexOf(char value) {
        var pos = _value.IndexOf(value, _offset, Length);

        return pos < 0 ? -1 : pos - _offset;
    }

    public int IndexOf(string value) {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var pos = _value.IndexOf(value, _offset, Length, StringComparison.Ordinal);

        return pos < 0 ? -1 : pos - _offset;
    }

    public bool StartsWith(string value) {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var len = value.Length;

        if (len > Length) return false;

        for (var i = 0; i < len; i++) {
            if (_value[_offset + i] != value[i])
                return false;
        }

        return true;
    }

    public bool EndsWith(string value) {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var len = value.Length;

        if (len > Length) return false;

        var start = Length - len;

        for (var i = 0; i < len; i++) {
            if (_value[_offset + start + i] != value[i])
                return false;
        }

        return true;
    }

    public bool Equals(string value) {
        if (value == null) return false;

        if (value.Length != Length) return false;

        for (var i = 0; i < Length; i++) {
            if (_value[_offset + i] != value[i])
                return false;
        }

        return true;
    }

    public bool Equals(StringSlice other) {
        if (Length != other.Length) return false;

        for (var i = 0; i < Length; i++) {
            if (_value[_offset + i] !=
                other._value[other._offset + i]) {
                return false;
            }
        }

        return true;
    }

    public static StringSlice FromString(string value) => new(value);

    public static implicit operator StringSlice(string value) => new(value);

    public static bool operator ==(StringSlice left, StringSlice right) => left.Equals(right);

    public static bool operator !=(StringSlice left, StringSlice right) => !left.Equals(right);
}