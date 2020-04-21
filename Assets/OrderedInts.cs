using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderedInts : MonoBehaviour
{
	ushort LastPacketNumber = 0;

	void OnNumber(ushort NextNumber)
	{
		var Step = NextNumber - LastPacketNumber;
		if (Step != 1 )
		{
			Debug.LogWarning("Packet out of order; " + NextNumber + " last=" + LastPacketNumber + " lost=" + (Step-1) );
		}
		LastPacketNumber = NextNumber;
	}

	public void OnPacket(byte[] Data)
	{
		var Alignment = Data.Length % sizeof(ushort);
		if ( Alignment != 0 )
		{
			Debug.LogWarning("Alignment of incoming data x" + Data.Length + " is misaligned...");
		}

		for (var i = 0; i < Data.Length;	i+=2)
		{
			var a = Data[i + 0];
			var b = Data[i + 1];
			var n = (a << 8) | b;
			var s = (ushort)n;
			OnNumber(s);
		}
	}

}
