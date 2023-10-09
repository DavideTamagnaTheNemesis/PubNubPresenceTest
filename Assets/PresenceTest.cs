using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PubnubApi;
using PubnubApi.Unity;
using Random = UnityEngine.Random;

public class PresenceTest : MonoBehaviour
{
	private List<string> presenceChannels;
	private Pubnub pubnub;
	private SubscribeCallbackListener listener;

	void Start()
	{
		//Set the Presence Manager don't destroy  so it's persistent across multiple worlds
		DontDestroyOnLoad(gameObject);

		//Initialize PubNub and connect to presence channel
		ConnectToPresence();

		//Load the first world and connect to session chat channels
		StartCoroutine(SimulateDownloadWorld());
	}

	private void ConnectToPresence()
	{
		//Create a random user with unique ID
		string userID = "user" + Random.Range(0, 10000);
		//Demo Keyset
		PNConfiguration config = new PNConfiguration(new UserId(userID));
		config.PublishKey = "pub-c-4d548588-3017-47cf-b1db-b27400470a21";
		config.SubscribeKey = "sub-c-b3a146fc-b19f-11ec-a510-d29fac035801";
		config.SecretKey = "sec-c-NjM3ZjIyMzYtZDIyZS00ZjFjLWIyNGUtYjVmMzMwYzE1MWYy";
		config.LogVerbosity = PNLogVerbosity.BODY;
		pubnub = new Pubnub(config);

		//Subscibe to events
		listener = new SubscribeCallbackListener();
		listener.onStatus += HandleOnStatusCallback;
		listener.onPresence += HandleOnPresence;
		pubnub.AddListener(listener);

		//Subscribe to Presence Channel
		presenceChannels = new List<string> { "presence" };
		pubnub.Subscribe<string>().Channels(presenceChannels).WithPresence().Execute();
	}

	private IEnumerator SimulateDownloadWorld()
	{
		//Simulate Download the world and Session informations (wait for 3 seconds)
		yield return new WaitForSeconds(3);

		//Subscibe to session world channels
		pubnub.Subscribe<string>().Channels(new List<string> { "worldchannel" }).Execute();

		//This can be replicated infinite times just subscibing to another channel after some time
		//yield return new WaitForSeconds(3);
		//pubnub.Subscribe<string>().Channels(new List<string> { "worldchannel2" }).Execute();
	}

	private void HandleOnPresence(Pubnub _pubnub, PNPresenceEventResult _result)
	{
		Debug.Log("EVENT: " + _result.Event + "\tCHANNEL: " + _result.Channel + "\tUSER: " + _result.Uuid);
	}

	private void HandleOnStatusCallback(Pubnub _pn, PNStatus _status)
	{
		if (_status.AffectedChannels != null && _status.AffectedChannels.Count > 0)
		{
			for (int i = 0; i < _status.AffectedChannels.Count; i++)
			{
				string channel = _status.AffectedChannels[i];
				switch (_status.Category)
				{
					case PNStatusCategory.PNConnectedCategory:
						Debug.Log("SUBSCRIBE: " + channel);
						break;
					case PNStatusCategory.PNUnknownCategory:
					case PNStatusCategory.PNAcknowledgmentCategory:
					case PNStatusCategory.PNAccessDeniedCategory:
					case PNStatusCategory.PNTimeoutCategory:
					case PNStatusCategory.PNNetworkIssuesCategory:
					case PNStatusCategory.PNReconnectedCategory:
					case PNStatusCategory.PNDisconnectedCategory:
					case PNStatusCategory.PNUnexpectedDisconnectCategory:
					case PNStatusCategory.PNCancelledCategory:
					case PNStatusCategory.PNBadRequestCategory:
					case PNStatusCategory.PNMalformedFilterExpressionCategory:
					case PNStatusCategory.PNMalformedResponseCategory:
					case PNStatusCategory.PNDecryptionErrorCategory:
					case PNStatusCategory.PNTLSConnectionFailedCategory:
					case PNStatusCategory.PNTLSUntrustedCertificateCategory:
					case PNStatusCategory.PNRequestMessageCountExceededCategory:
						break;
				}
			}
		}
	}

	private void OnDestroy()
	{
		listener.onStatus -= HandleOnStatusCallback;
		listener.onPresence -= HandleOnPresence;

		pubnub.UnsubscribeAll<string>();
		pubnub.UnsubscribeAll<object>();

		pubnub.Destroy();
	}
}
