﻿using System;
using System.Net;
using System.Threading;
using Forge.Utilities;
using Forge.Serialization;

namespace Forge.Networking.Sockets
{
	public abstract class ForgeUDPSocketFacadeBase
	{
		public abstract ISocket ManagedSocket { get; }

		protected INetworkMediator networkMediator;
		public CancellationTokenSource CancellationSource { get; protected set; }
		protected SynchronizationContext synchronizationContext;
		private ConcurrentHashSet<long> _blockedEp = new ConcurrentHashSet<long>();

		public ForgeUDPSocketFacadeBase()
		{
			synchronizationContext = SynchronizationContext.Current;
		}

		public virtual void ShutDown()
		{
			CancellationSource.Cancel();
			ManagedSocket.Close();
		}

		protected void ReadNetwork()
		{
			EndPoint readEp = new IPEndPoint(IPAddress.Parse(CommonSocketBase.LOCAL_IPV4), 0);
			var buffer = new BMSByte();
			buffer.SetArraySize(2048);
			try
			{
				while (true)
				{
					CancellationSource.Token.ThrowIfCancellationRequested();
					buffer.Clear();
					ManagedSocket.Receive(buffer, ref readEp);
					try
					{
						if (!_blockedEp.Contains(readEp.IPKey()))
							ProcessMessageRead(buffer, readEp);
					}
					catch (Exception ex)
                    {
						networkMediator.EngineProxy.Logger.Log($"Unexpected ProcessMessageRead error: {ex.Message}");
					}
				}
			}
			catch (OperationCanceledException)
			{
				networkMediator.EngineProxy.Logger.Log("Cancelling the background network read task");

			}
			catch (Exception ex)
            {
				networkMediator.EngineProxy.Logger.Log($"Cancelling the background network read task. Unexpected: {ex.Message}");
			}
		}

		public void BlockEp(IPEndPoint ep)
		{
			_blockedEp.Add(ep.IPKey());
		}

		protected abstract void ProcessMessageRead(BMSByte buffer, EndPoint sender);
	}
}
