using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using Pomelo.DotNetClient;
using System.Threading;
using UnityEngine.UI;


public class TestConnection : MonoBehaviour
{
    public Text LogText;

    public InputField IPInput;
    public InputField NameInput;
    public InputField PasswordInput;
    public InputField MessageInput;

    public static Connection _connection = null;

    void Start()
    {
        _connection = new Connection();

        IPInput.text = "localhost";
        NameInput.text = "spark";
        PasswordInput.text = "123456";

        _connection.on("onTick", msg => 
        {
            _onResponseRet(msg);
        });

        //listen on network state changed event
        _connection.on(Connection.DisconnectEvent, msg =>
        {
            Debug.logger.Log("Network error, reason: " + msg.jsonObj["reason"]);
        });

        _connection.on(Connection.ErrorEvent, msg =>
        {
            Debug.logger.Log("Error, reason: " + msg.jsonObj["reason"]);
        });
    }

    //When quit, release resource
    void Update()
    {
        _connection.Update();
    }

    public void Disconnect()
    {
        _connection.Disconnect();
        LogText.text = "";
    }

    //Login the chat application and new PomeloClient.
    public void Connect()
    {
        if(_connection.netWorkState != NetWorkState.DISCONNECTED) return;

        int port = 3080;

        _connection.InitClient(IPInput.text, port, msgObj =>
        {
            //The user data is the handshake user params
            JsonObject user = new JsonObject();
            _connection.connect(user, data =>
            {
                //process handshake call back data
                JsonObject msg = new JsonObject();
                msg["uid"] = NameInput.text;
                msg["pwd"] = PasswordInput.text;
                _connection.request("connector.entryHandler.enter", msg, _onResponseRet);
            });
        });
    }

    void _onResponseRet(Message result)
    {
        Debug.Log(result);
        LogText.text = LogText.text + result.rawString + "\n";
    }

    public void Send()
    {
        JsonObject msg = new JsonObject();
        msg["message"] = MessageInput.text;
        _connection.request("gate.gateHandler.sendMessage", msg, _onResponseRet);

        MessageInput.ActivateInputField();
        MessageInput.Select();
    }

    //When quit, release resource
    void OnApplicationQuit()
    {
        if (_connection != null) _connection.Disconnect();
    }
}