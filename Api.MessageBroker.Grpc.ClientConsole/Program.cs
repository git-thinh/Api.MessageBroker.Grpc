// Copyright 2017, Google Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//     * Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above
// copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the
// distribution.
//     * Neither the name of Google Inc. nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using Grpc.Core;
using static Api.MessageBroker.Grpc.svcMessageService;
using System.Threading;

namespace Api.MessageBroker.Grpc.ClientConsole
{
    class Program
    {
        private const string Host = "localhost";
        private const int Port = 8080;

        private static svcMessageServiceClient _msgService;
        private static AsyncDuplexStreamingCall<mMessage, mMessageFromServer> _call;

        private static void initializeGrpc()
        {
            // Create a channel
            var channel = new Channel(Host + ":" + Port, ChannelCredentials.Insecure);

            // Create a client with the channel
            _msgService = new svcMessageServiceClient(channel);
        }

        static async void connectServer() {
            // Open a connection to the server
            try
            {
                using (_call = _msgService.sendMsg())
                {
                    // Read messages from the response stream
                    while (await _call.ResponseStream.MoveNext(CancellationToken.None))
                    {
                        var serverMessage = _call.ResponseStream.Current;
                        var otherClientMessage = serverMessage.Message;
                        var displayMessage = string.Format("{0}:{1}{2}", otherClientMessage.From, otherClientMessage.Message, Environment.NewLine);
                        //chatTextBox.Text += displayMessage;

                        Console.WriteLine(" -> " + displayMessage);
                    }
                    // Format and display the message
                }
            }
            catch (RpcException)
            {
                _call = null;
                //throw;
                Console.WriteLine("----> Server disconected ...!");
            }
        }

        async static void sendMessage(string _from, string _message)
        {
            // Create a chat message
            var message = new mMessage
            {
                From = _from,
                Message = _message
            };
            // Send the message

            if (_call != null)
            {
                await _call.RequestStream.WriteAsync(message);
            }
        }

        public static void Main(string[] args)
        {
            initializeGrpc();

            connectServer();


            while (true) {
                string data = Console.ReadLine();
                sendMessage("CLIENT", data);
                Thread.Sleep(100);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            _call.Dispose();


            //// Create a channel
            //var channel = new Channel(Host + ":" + Port, ChannelCredentials.Insecure);
            //// Create a client with the channel
            //var client = new GreetingService.GreetingServiceClient(channel);
            //// Create a request
            //var request = new HelloRequest
            //{
            //    Name = "Mete - on C#",
            //    Age = 34,
            //    Sentiment = Sentiment.Happy
            //};
            //// Send the request
            //Console.WriteLine("GreeterClient sending request");
            //var response = client.greeting(request);
            //Console.WriteLine("GreeterClient received response: " + response.Greeting);
            //// Shutdown
            //channel.ShutdownAsync().Wait();
            //Console.WriteLine("Press any key to exit...");
            //Console.ReadKey();
        }
    }
}
