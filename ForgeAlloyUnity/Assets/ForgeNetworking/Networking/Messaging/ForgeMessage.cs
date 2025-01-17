﻿using Forge.Serialization;

namespace Forge.Networking.Messaging
{
	public abstract class ForgeMessage : IMessage
	{
		public event MessageSent OnMessageSent;
		public IMessageReceiptSignature Receipt { get; set; }
		public abstract IMessageInterpreter Interpreter { get; }
		public abstract void Serialize(BMSByte buffer);
		public abstract void Deserialize(BMSByte buffer);
		public bool IsPooled { get; set; } = false;
		public bool IsBuffered { get; set; } = false;
		public bool IsQueued { get; set; } = false;
		public bool IsSent { get; set; } = false;
		public bool IsSending { get; set; } = false;


		public virtual void Sent()
		{
			IsSent = true;
			if (OnMessageSent == null)
			{
				int code = ForgeMessageCodes.GetCodeFromType(this.GetType());
				if (code > 10 && code < 1000000000)
					throw new System.Exception($"OnMessageSent is Null: {this.GetType()}");
				return;
			}
			OnMessageSent?.Invoke(this);
		}
		public void Unbuffered()
        {
			IsBuffered = false;
			OnMessageSent?.Invoke(this);
		}
		public void Dequeued()
		{
			IsQueued = false;
			Sent();
		}

		public void StartSending()
		{
			IsSending = true;
		}
		public void FinishSending()
		{
			IsSending = false;
			OnMessageSent?.Invoke(this);
		}

		public virtual void Reset()
        {

        }
	}
}
