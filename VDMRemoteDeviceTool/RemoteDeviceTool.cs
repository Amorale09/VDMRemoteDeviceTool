using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Xml.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VDMRemoteDeviceTool
{
	public class TrackerState
	{
		public static int SENSOR_OFFLINE = 0;
		public static int SENSOR_OK = 1;
		public static int SENSOR_ERROR = 2;
	}

	public class ImuType
	{
		public static int IMU_UNKNOWN = 0;
		public static int BNO080 = 3;
		public static int IMU_DEV_RESERVED = 250;
	}

	public class TrackerRotation
	{
		public TrackerRotation() { }
		
		public TrackerRotation(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public float x;
		public float y;
		public float z;
		public float w;
	}

	public class TrackerAcceleration
	{
		public TrackerAcceleration() { }

		public TrackerAcceleration(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public float x;
		public float y;
		public float z;
	}

	public class TrackerPosition
	{
		public TrackerPosition() { }

		public TrackerPosition(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public float x;
		public float y;
		public float z;
	}

	public class TrackerData
	{
		public TrackerData() { }

		public TrackerData(int id, int state, int type, TrackerRotation rotation, TrackerAcceleration acceleration, TrackerPosition position)
		{
			_Id = id;
			_State = state;
			_Type = type;
			_Rotation = rotation;
			_Acceleration = acceleration;
			_Position = position;
		}

		public int _Id { get; set; }
		public int _State { get; set; }
		public int _Type { get; set; }
		public TrackerRotation _Rotation { get; set; }
		public TrackerAcceleration _Acceleration { get; set; }
		public TrackerPosition _Position { get; set; }
	}

	public class RemoteDeviceTool
	{
		public string _HostIp = "192.168.31.164";
		public int _HostPort = 6969;

		private Socket? _UdpSocket;

		private Dictionary<int, TrackerData> _Trackers = new Dictionary<int, TrackerData>();

		private int _PacketNumber = 0;

		private EndPoint _LastRemoteIp;

		private Socket CreateSocket(int sendPort)
		{
			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
			//IPEndPoint localIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
			//socket.Bind(localIP);
			socket.Blocking = false;

			return socket;
		}

		private async void UdpSend(string host_ip, int port, byte[] data)
		{
			var endpoint = new IPEndPoint(IPAddress.Parse(host_ip), port);
			var datas = new ArraySegment<byte>(data);
			await _UdpSocket.SendToAsync(datas, SocketFlags.None, endpoint);
		}

		/*
		private async Task<SocketReceiveFromResult> UdpReceive()
		{
			IPEndPoint ip = new IPEndPoint(IPAddress.Any, 5555);
			byte[] bytes = _UdpSocket.EndReceive(ar, ref ip);
			string message = Encoding.ASCII.GetString(bytes);
			Console.WriteLine("From {0} received: {1} ", ip.Address.ToString(), message);
		}*/

		public void SetTracker(TrackerData data)
		{
			_Trackers[data._Id] = data;
			Console.WriteLine($"added: {data}");
		}

		public List<TrackerData> GetTrackers()
		{
			return _Trackers.Values.ToList();
		}

		public void SetTrackerAcceleration(int id, TrackerAcceleration data)
		{
			_Trackers[id]._Acceleration = data;
		}

		public void SetTrackerRotation(int id, TrackerRotation data)
		{
			_Trackers[id]._Rotation = data;
		}

		public void SetTrackerPosition(int id, TrackerPosition data)
		{
			_Trackers[id]._Position = data;
			Console.WriteLine(_Trackers[id]._Position.x);
		}

		private int GetPacketNumber()
		{
			_PacketNumber += 1;
			return _PacketNumber;
		}

		private async void SendDiscoveryPacket(string addres, int port)
		{
			DatagramPacket pb = new DatagramPacket();
			pb.SendDiscovery();

			await Task.Run(() => { UdpSend(addres, port, pb.buf.ToArray()); });
		}

		private async void SendTrackerInfoPacket(string addres, int port, TrackerData data)
		{
			DatagramPacket pb = new DatagramPacket();
			pb.SendTrackerInfo(GetPacketNumber(), data);

			await Task.Run(() => { UdpSend(addres, port, pb.buf.ToArray()); });
		}

		public async void SendTrackerRotation(string addres, int port, TrackerData data)
		{
			DatagramPacket pb = new DatagramPacket();
			pb.SendTrackerRotation(GetPacketNumber(), 
								   data._Id,
								   data._Rotation.x, 
								   data._Rotation.y,
								   data._Rotation.z,
								   data._Rotation.w);

			await Task.Run(() => { UdpSend(addres, port, pb.buf.ToArray()); });
		}

		public async void SendTrackerAcceleration(string addres, int port, TrackerData data)
		{
			DatagramPacket pb = new DatagramPacket();
			pb.SendTrackerAcceleration(GetPacketNumber(),
									   data._Id,
									   data._Acceleration.x,
									   data._Acceleration.y,
									   data._Acceleration.z);

			await Task.Run(() => { UdpSend(addres, port, pb.buf.ToArray()); });
		}

		private async void SendTrackersData(string addres, int port)
		{
			foreach (var tracker in _Trackers)
			{
				await Task.Run(() => { SendTrackerInfoPacket(addres, port, tracker.Value); });
			}

			foreach (var tracker in _Trackers)
			{
				await Task.Run(() => { SendTrackerRotation(addres, port, tracker.Value); });
			}

			foreach (var trackers in _Trackers)
			{
				//"SendTrackerPosition" in work...
			}

			foreach (var tracker in _Trackers)
			{
				await Task.Run(() => { SendTrackerAcceleration(addres, port, tracker.Value); });
			}
		}

		public async void SendHeartbeatPacket(string addres, int port)
		{
			DatagramPacket pb = new DatagramPacket();
			pb.SendHeartbeat(GetPacketNumber());

			await Task.Run(() => { UdpSend(addres, port, pb.buf.ToArray()); });
		}

		public async void SendFeatureFlag(string addres, int port)
		{
			DatagramPacket pb = new DatagramPacket();
			pb.SendFeatureFlag(GetPacketNumber());

			await Task.Run(() => { UdpSend(addres, port, pb.buf.ToArray()); });
		}
		/*
		private async void HandlePacket(string addres, int port, byte[] recvBuf)
		{
			var newPacket = new DatagramPacket(recvBuf);
			var packetType = newPacket.ReceivePacketType();

			if (packetType == PacketType.PACKET_RECEIVE_HEARTBEAT)
			{
				await Task.Run(() => { SendHeartbeatPacket(addres, port); });
			}
			else if (packetType == PacketType.PACKET_PING_PONG)
			{
				await Task.Run(() => { UdpSend(addres, port, recvBuf); });
			}
			else if (packetType == PacketType.PACKET_SENSOR_INFO)
			{
				;
			} else if(packetType == PacketType.PACKET_FEATURE_FLAGS)
			{
				;
			}

			await Task.Run(() => { SendTrackersData(addres, port); });
			await Task.Run(() => { SendFeatureFlag(addres, port); });
		}*/

		private async void Update()
		{
			await Task.Run(() => { SendTrackersData(_HostIp, _HostPort); });
			}

		public async void Connect(string hostIp, int hostPort)
		{
			_HostIp = hostIp;
			_HostPort = hostPort;

			_UdpSocket = CreateSocket(5555);
			await Task.Run(() => { SendDiscoveryPacket(hostIp, hostPort); });

			await Task.Run(() =>
			{
				while (true)
				{
					Update();
					//Console.WriteLine("send");
					Thread.Sleep(50);
				}
			});
		}
	}
}
