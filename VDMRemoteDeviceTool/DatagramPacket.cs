using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VDMRemoteDeviceTool
{
	public class PacketType
	{
		public static int PACKET_HEARTBEAT = 0;
		public static int PACKET_RECEIVE_HEARTBEAT = 1;
		public static int PACKET_HANDSHAKE = 3;
		public static int PACKET_ACCEL = 4;
		public static int PACKET_PING_PONG = 10;
		public static int PACKET_SENSOR_INFO = 15;
		public static int PACKET_ROTATION_DATA = 17;
		public static int PACKET_FEATURE_FLAGS = 22;
		public static int PACKET_ROTATION_AND_ACCELERATION = 23;
	}

	public class BoardType
	{
		public static int BOARD_UNKNOWN = 0;
		public static int BOARD_SLIMEVR_DEV = 2;
	}

	public class HardwareMcuType
	{
		public static int MCU_UKNOWN = 0;
		public static int MCU_DEV_RESERVED = 250;
	}

	public class DatagramPacket
	{
		public DatagramPacket() { }

		public List<byte> buf = new List<byte>();
		public int offset = 0;

		public DatagramPacket(byte[] buffer)
		{
			buf = buffer.ToList();
		}

		public void SendByte(byte value)
		{
			buf.Add(value);
		}

		public void SendBytes(byte[] value)
		{
			buf.AddRange(value);
		}

		public void SendLong(long value)
		{
			byte[] bytes = BitConverter.GetBytes(value).Reverse().ToArray();
			buf.AddRange(bytes);
		}

		public void SendInt(int value)
		{
			byte[] bytes = BitConverter.GetBytes(value).Reverse().ToArray();
			buf.AddRange(bytes);
		}

		public void SendShortString(string value)
		{
			SendByte(Convert.ToByte(value.Length));
			var data = Encoding.ASCII.GetBytes(value).ToList();
			data.Add(0x00);
			SendBytes(data.ToArray());
		}

		public void SendFloat(float value)
		{
			byte[] bytes = BitConverter.GetBytes(value).Reverse().ToArray();
			buf.AddRange(bytes);
		}

		public int ReceiveInt()
		{
			var data = buf.GetRange(offset, offset + 4);
			data.Reverse();
			var dataByte = data.ToArray();
			var result = BitConverter.ToInt32(dataByte);
			offset += 4;
			return result;
		}

		public int ReceivePacketType()
		{
			return ReceiveInt();
		}

		public void SendPacketType(int packetType)
		{
			SendByte(0);
			SendByte(0);
			SendByte(0);
			SendByte(Convert.ToByte(packetType));
		}

		public void SendDiscovery()
		{
			int FIRMWARE_BUILD_NUMBER = 17;
			string FIRMWARE_VERSION = "0.4.0";

			byte[] MAC = new byte[6];
			for (int i = 0; i < MAC.Length; i++)
			{
				MAC[i] = 120;
			}

			SendPacketType(PacketType.PACKET_HANDSHAKE);
			SendLong(0);
			SendInt(BoardType.BOARD_SLIMEVR_DEV);
			SendInt(ImuType.IMU_DEV_RESERVED);
			SendInt(HardwareMcuType.MCU_DEV_RESERVED);
			SendInt(0);
			SendInt(0);
			SendInt(0);
			SendInt(FIRMWARE_BUILD_NUMBER);
			SendShortString(FIRMWARE_VERSION);
			SendBytes(MAC);
		}

		public void SendTrackerInfo(int packetNumber, TrackerData data)
		{
			SendPacketType(PacketType.PACKET_SENSOR_INFO);
			SendLong(packetNumber);
			SendByte(Convert.ToByte(data._Id));
			SendByte(Convert.ToByte(data._State));
			SendByte(Convert.ToByte(data._Type));
		}

		public void SendTrackerRotation(int packetNumber, int trackerId, float x, float y, float z, float w)
		{
			SendPacketType(PacketType.PACKET_ROTATION_DATA);
			SendLong(packetNumber);
			SendByte(Convert.ToByte(trackerId));
			SendByte(1);
			SendFloat(x);
			SendFloat(y);
			SendFloat(z);
			SendFloat(w);
			SendByte(0);
		}

		public void SendTrackerAcceleration(int packetNumber, int trackerId, float x, float y, float z)
		{
			SendPacketType(PacketType.PACKET_ACCEL);
			SendLong(packetNumber);
			SendFloat(x);
			SendFloat(y);
			SendFloat(z);
			SendByte(Convert.ToByte(trackerId));
		}

		public void SendHeartbeat(int packetNumber)
		{
			SendPacketType(PacketType.PACKET_HEARTBEAT);
			SendLong(packetNumber);
		}

		public void SendFeatureFlag(int packetNumber)
		{
			SendPacketType(PacketType.PACKET_FEATURE_FLAGS);
			SendLong(packetNumber);
			SendByte(0);
		}
	}
}
