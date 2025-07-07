namespace DotNetCommons.Numerics;

/// <summary>
/// Provides functionality to create and interpret federated ulong keys from 15-bit system keys and 48-bit
/// record keys.
/// </summary>
/// <remarks>
/// A federated key is a composite identifier that consists of three parts: a system key (15 bits), a record key (48 bits),
/// and a parity bit. This class facilitates the creation of such keys, ensuring validity and retaining information necessary
/// to split the key back into its original components.
/// </remarks>
public static class Federated48Key
{
    private const ulong SystemBitMask = 0xFFFE_0000_0000_0000;
	private const ulong RecordBitMask = 0x0001_FFFF_FFFF_FFFE;

	private const uint SystemMax = 0x7FFF;
	private const ulong RecordMax = 0xFFFF_FFFF_FFFF;

	private const int SystemShift = 49;
	private const int RecordShift = 1;

	/// <summary>
	/// Creates a federated 48-bit key by combining a 15-bit system key and a 48-bit record key,
	/// with a parity bit included to ensure validity.
	/// </summary>
	/// <param name="systemKey">The 15-bit system key. Must be a positive integer &gt; 0 and &lt; 32,768.</param>
	/// <param name="recordKey">The 48-bit record key. Must be a positive integer &gt; 0 and &lt; 2^48.</param>
	/// <returns>A 64-bit unsigned long representing the federated key constructed from the input parameters.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown if either key is zero, negative, or exceeds the maximum allowable value.
	/// </exception>
	public static ulong MakeFederatedKey(int systemKey, long recordKey)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(systemKey);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(systemKey, (int)SystemMax);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(recordKey);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(recordKey, (long)RecordMax);

		var sk = (ulong)(uint)systemKey;
		var rk = (ulong)recordKey;

		var result = (sk << SystemShift) | (rk << RecordShift);
		if (result.IsParityOdd())
			result |= 1;

		return result;
	}

	/// <summary>
	/// Splits a federated key into its individual components: the 15-bit system key,
	/// the 48-bit record key, and a validity flag based on the parity bit.
	/// </summary>
	/// <param name="federatedKey">The federated key to split. Must be a 64-bit unsigned long.</param>
	/// <returns>
	/// A tuple containing:
	/// - `systemKey` (int): The extracted 15-bit system key.
	/// - `recordKey` (long): The extracted 48-bit record key.
	/// - `valid` (bool): A flag indicating whether the federated key is valid and the systemKey and recordKey fields are
	/// allowed (greater than zero).
	/// </returns>
	public static (int systemKey, long recordKey, bool valid) SplitFederatedKey(ulong federatedKey)
	{
		var systemKey = (federatedKey & SystemBitMask) >> SystemShift;
		var recordKey = (federatedKey & RecordBitMask) >> RecordShift;

		return ((int)systemKey, (long)recordKey, systemKey > 0 && recordKey > 0 && federatedKey.IsParityEven());
	}
}