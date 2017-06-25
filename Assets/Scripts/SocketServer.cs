﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

public class SocketServer : MonoBehaviour {

	//private string host = "localhost";
	private string host = "192.168.11.6";
	private int port = 22222;

	// State object for reading client data asynchronously
	public class StateObject {
		// Client  socket.
		public Socket workSocket = null;
		// Size of receive buffer.
		public const int BufferSize = 1024;
		// Receive buffer.
		public byte[] buffer = new byte[BufferSize];
		// Received data string.
		public StringBuilder sb = new StringBuilder();  
	}

	List<StateObject> activeConnections = new List<StateObject>();


	// Use this for initialization
	void Start () {
		StartListening();
	}

	public void StartListening() {
		IPAddress ipAddress = IPAddress.Parse(getIPAddress(host));
		IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

		// Create a TCP/IP socket.
		Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		// Bind the socket to the local endpoint and listen for incoming connections.
		try {
			listener.Bind(localEndPoint);
			listener.Listen(10);

			// Start an asynchronous socket to listen for connections.
			listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

		} catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}

	public void AcceptCallback(IAsyncResult ar) {
		// Get the socket that handles the client request.
		Socket listener = (Socket) ar.AsyncState;
		Socket handler = listener.EndAccept(ar);

		// Create the state object.
		StateObject state = new StateObject();
		state.workSocket = handler;
		handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
			new AsyncCallback(ReadCallback), state);

		//確立した接続のオブジェクトをリストに追加
		activeConnections.Add(state);

		//Debug.LogFormat("there is {0} connections", activeConnections.Count);

		//接続待ちを再開しないと次の接続を受け入れなくなる
		listener.BeginAccept(new AsyncCallback(AcceptCallback),listener);
	}

	public void ReadCallback(IAsyncResult ar) {
		String content = String.Empty;

		// Retrieve the state object and the handler socket
		// from the asynchronous state object.
		StateObject state = (StateObject) ar.AsyncState;
		Socket handler = state.workSocket;

		// Read data from the client socket. 
		int bytesRead = handler.EndReceive(ar);

		if (bytesRead > 0) {
			// There  might be more data, so store the data received so far.
			state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

			// Check for end-of-file tag. If it is not there, read 
			// more data.
			content = state.sb.ToString();
			//Debug.Log(content);

			// MSDNのサンプルはEOFを検知して出力をしているけれどもncコマンドはEOFを改行時にLFしか飛ばさないので\nを追加
			if (content.IndexOf("\n") > -1 || content.IndexOf("<EOF>") > -1) {
				// All the data has been read from the 
				// client. Display it on the console.
				//Debug.LogFormat("Read {0} bytes from socket. \n Data : {1}", content.Length, content);

				// Echo the data back to the client.
				//Send(handler, content);

				// Do Action
				var message = action(content);

				foreach (StateObject each in activeConnections) {
					Send(each.workSocket, message);
					//print(message);
				}
				activeConnections.Remove(state);

				/*
				foreach (StateObject each in activeConnections) {
					//string message = string.Format ("You are client No.{0}", i);
					//					Send (each.workSocket, message);
					//eachをactiveConnectionの中から見つけてそのインデックスを取得する方法がこれ
					int num_of_each = activeConnections.FindIndex(delegate(StateObject s) {
						return s == each;
					});
					//state:送信者の番号
					int num_of_from = activeConnections.FindIndex(delegate(StateObject s) {
						return s == state;
					});
					string message = string.Format("you:{0} / from:{1} / data:{2}\n", num_of_each, num_of_from, content);
					Send(each.workSocket, message);
				}
                */

				//clear data in object before next receive
				//StringbuilderクラスはLengthを0にしてクリアする
				state.sb.Length = 0;
			}

			// Not all data received. Get more.
			handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
				new AsyncCallback(ReadCallback), state);
		}
	}

	private string action(string content) {
		string message = "";

		char command = content[0];
		switch (command) {
		case 's':
			GameManager.Instance.GameStart();
			message = "start";
			break;
		case 'r':
			GameManager.Instance.GameReset();
			message = GameManager.Instance.GetResponseMessage(true);
			break;
		case 'a':
			int actionId = (int)(content[1] - '0');
			//Debug.Log("action: " + actionId);
			GameManager.Instance.PlayerAction(actionId);
			message = GameManager.Instance.GetResponseMessage(false);
			break;
		default:
			break;
		}

		return message;
	}

	private void Send(Socket handler, String data) {
		// Convert the string data to byte data using ASCII encoding.
		byte[] byteData = Encoding.ASCII.GetBytes(data);

		// Begin sending the data to the remote device.
		handler.BeginSend(byteData, 0, byteData.Length, 0,
			new AsyncCallback(SendCallback), handler);
	}

	private void SendCallback(IAsyncResult ar) {
		try {
			// Retrieve the socket from the state object.
			Socket handler = (Socket) ar.AsyncState;

			// Complete sending the data to the remote device.
			int bytesSent = handler.EndSend(ar);
			//Debug.LogFormat("Sent {0} bytes to client.", bytesSent);

			//この２つはセットでつかるらしい
			//handler.Shutdown(SocketShutdown.Both);
			//handler.Close();

		} catch (Exception e) {
			Debug.Log(e.ToString());
		}
	}

	private string getIPAddress(string hostName) {
		IPHostEntry host;
		host = Dns.GetHostEntry(hostName);

		foreach (IPAddress ip in host.AddressList)
		{
			if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
			{
				//System.Diagnostics.Debug.WriteLine("LocalIPadress: " + ip);
				Debug.Log("LocalIP address: " + ip);
				return ip.ToString();
			}
		}
		return string.Empty;
	}

	// Update is called once per frame
	void Update () {
		
	}
}
