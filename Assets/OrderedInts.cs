using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderedInts : MonoBehaviour
{
	//	handle broken up packets
	List<byte> PendingData = new List<byte>();
	uint LastPacketNumber = 0;
	public bool FlushOnMainThread = false;
	[Range(1, 10000)]
	public int MaxNumbersPerFrame = 1000;
	public int MaxBytesPerFrame { get { return MaxNumbersPerFrame * sizeof(ushort); } }

	void OnNumber(uint NextNumber)
	{
		var Step = NextNumber - LastPacketNumber;
		if (Step != 1 && Step != 256)	//	255 when we drop an odd numbered packet and the buffer goes out of sync
		{
			Debug.LogWarning("Packet out of order; " + NextNumber + " last=" + LastPacketNumber + " lost=" + (Step-1) );
		}
		LastPacketNumber = NextNumber;
	}

	void FlushData()
	{
		//	for speed, pop the array
		byte[] Data;
		lock (PendingData)
		{
			var DataLength = PendingData.Count - (PendingData.Count % sizeof(ushort));
			DataLength = Mathf.Min(MaxBytesPerFrame, DataLength);
			//Debug.Log("Flushing " + DataLength + "bytes");
			Data = new byte[DataLength];
			PendingData.CopyTo(0,Data,0,Data.Length);
			PendingData.RemoveRange(0, Data.Length);
		}

		for (var i = 0; i < Data.Length; i += sizeof(ushort))
		{
			var a = Data[i + 1];
			var b = Data[i + 0];
			int n = a;
			n = (n << 8) | b;
			//n = (n << 8) | c;
			//n = (n << 8) | d;

			var s = (ushort)n;
			OnNumber(s);
		}
	}

	public void OnPacket(byte[] Data)
	{
		lock (PendingData)
		{
			PendingData.AddRange(Data);
		}

		if (!FlushOnMainThread)
			FlushData();
	}

	void Update()
	{
		if (FlushOnMainThread)
			FlushData();
	}

}
