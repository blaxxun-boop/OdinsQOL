﻿using System.Collections.Generic;
using BepInEx;

namespace OdinQOL.MapSharing
{
    public class RpcData
    {
        public string Name;
        public object[] Payload;
        public long Target = ZRoutedRpc.Everybody;
    }

    public static class RpcQueue
    {
        private static readonly Queue<RpcData> _rpcQueue = new();
        private static bool _ack = true;

        public static void Enqueue(RpcData rpc)
        {
            _rpcQueue.Enqueue(rpc);
        }

        public static bool SendNextRpc()
        {
            if (_rpcQueue.Count == 0 || !_ack) return false;

            RpcData rpc = _rpcQueue.Dequeue();

            if (rpc.Name.IsNullOrWhiteSpace() ||
                rpc.Payload == null)
                return false;

            ZRoutedRpc.instance.InvokeRoutedRPC(rpc.Target, rpc.Name, rpc.Payload);
            _ack = false;

            return true;
        }

        public static void GotAck()
        {
            _ack = true;
        }
    }
}