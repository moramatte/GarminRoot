using Awos7.WindowsService.LogConfig;
using Infrastructure.Logger.Enterprise;
using Infrastructure.Threading;
using Infrastructure.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Logger
{
    public class UdpTraceListener : CustomTraceListener
	{
		private IAsyncWorker _worker;

		private bool _isInitialized;
		private bool _initFailed;
		private List<IPEndPoint> _clientEndPoints;
		private Socket _socket;

		public string RemoteHosts { get; set; }
		public int HostPort { get; set; }

		public UdpTraceListener(TraceListenerConfig config) : this(config.GetFormatter())
		{
			HostPort = config.Get<int>("Port");
			RemoteHosts = config.Get<string>("RemoteHosts");
			Filter = config.GetFilter();
			Name = Name;
		}

		public override TraceListenerConfig ToConfig()
		{
			return this.CommonConfig() with 
			{ 
				Type = TraceListeners.Udp,
				Values = new EquatableList<ConfigValue>([new("Port", HostPort), new("RemoteHosts", RemoteHosts)])
			};
		}

		public UdpTraceListener(ILogFormatter logFormatter) : base(logFormatter) { }

		public override void Dispose()
		{
			_worker.Cancel();
			base.Dispose();
        }

        private void Init()
		{
			if (_isInitialized)
				return;
			_isInitialized = true;

			try
			{
				_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				_clientEndPoints = new List<IPEndPoint>();

				if (!string.IsNullOrWhiteSpace(RemoteHosts))
				{
					try
					{
						string[] hosts = RemoteHosts.Split(';');
						foreach (var host in hosts)
						{
							string[] hostAndPort = host.Split(':');
							_clientEndPoints.Add(new IPEndPoint(IPAddress.Parse(hostAndPort[0]), int.Parse(hostAndPort[1])));
						}
					}
					catch (Exception)
					{
						//How to log when this fails?
						//Log.Error(this, "Malformed RemoteHost property");
					}
				}

				if (HostPort > 0)
				{
					_worker ??= Create.Worker(ListenerWork);
					_worker.RunWorkerAsync(null);
				}
			}
			catch (Exception)
			{
				_initFailed = true;
				throw;
			}
		}

		async Task ListenerWork(object sender, CancellationToken e)
		{
			try
			{
				UdpClient listener = new UdpClient(HostPort);
				IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, HostPort);
				//Log.Info(this, "The udp listener started at port: {0}", HostPort);

				try
				{
					while (!e.IsCancellationRequested)
					{
						byte[] bytes = listener.Receive(ref endpoint);
						if (!_clientEndPoints.ToArray().Any(ep => ep.Address.Equals(endpoint.Address) && ep.Port == endpoint.Port))
						{
							_clientEndPoints.Add(new IPEndPoint(endpoint.Address, endpoint.Port));
							//Log.Info(this, "{0}:{1} connected");
						}
						else
						{
							//if the logger should listen for anything...
							//ReceivedData.ByteData = bytes;
							//PublishReceivedData();
						}
					}

				}
				catch (Exception)
				{
					//Log.Error(this, "Error receiving from remote host");
					//OnError(new CommunicationChannelException("Error receiving from remote host", exc));
				}
				finally
				{
					listener.Close();
				}
				_clientEndPoints.Clear();
			}
			catch (Exception)
			{
				//Log.Error(this, "The udp listener worker encountered an error: {0}", exc.Message);
			}
		}

		public override void Write(LogEntry data, string message)
		{
			Publish(message);
		}

		private void Publish(string message)
		{
			Init();

			if (_initFailed)
				return;

			byte[] data = Encoding.ASCII.GetBytes(message);
			List<IPEndPoint> clientEndPoints = _clientEndPoints.ToList();
			foreach (var clientEndPoint in clientEndPoints)
			{
				try
				{
					_socket.SendTo(data, clientEndPoint);
				}
				catch (Exception exc)
				{
					Log.Error(GetType(), $"The udp listener worker encountered an error: {exc.Message}");
				}
			}
		}
	}
}
