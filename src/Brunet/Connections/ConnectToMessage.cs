/*
This program is part of BruNet, a library for the creation of efficient overlay
networks.
Copyright (C) 2005  University of California
Copyright (C) 2007 P. Oscar Boykin <boykin@pobox.com>  University of Florida

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System.Collections;
using System.Collections.Specialized;
#if BRUNET_NUNIT
using System.Security.Cryptography;
using NUnit.Framework;
using Brunet.Symphony;
using Brunet.Transport;
#endif

namespace Brunet.Connections
{

  /**
   * The ConnectionMessage that is sent out on the network
   * to request connections be made to the sender.
   *
   * When a Node sends out a ConnectToMessage, it puts
   * itself as the target.  This is because that node
   * is requesting that the recipient of the ConnectToMessage
   * connect to the sender (thus the sender is the target).
   *
   * When a node recieves a ConnectToMessage, the CtmRequestHandler
   * processes the message.  ConnectToMessages are sent by
   * Connector objects.
   *
   * This object is immutable
   * 
   * @see CtmRequestHandler
   * @see Connector
   */
  public class ConnectToMessage
  {

    /**
     * @param t connection type
     * @param target the Address of the target node
     * @param token unique token used to associate all connection setup messages
     *              with each other
     */
    public ConnectToMessage(ConnectionType t, NodeInfo target, string token)
    {
      _ct = Connection.ConnectionTypeToString(t);
      _target_ni = target;
      _neighbors = new NodeInfo[0]; //Make sure this isn't null
      _token = token;
    }
    public ConnectToMessage(string contype, NodeInfo target, string token)
    {
      _ct = contype;
      _target_ni = target;
      _neighbors = new NodeInfo[0]; //Make sure this isn't null
      _token = token;
    }
    public ConnectToMessage(string contype, NodeInfo target, NodeInfo[] neighbors, string token)
    {
      _ct = contype;
      _target_ni = target;
      _neighbors = neighbors;
      _token = token;
    }

    public ConnectToMessage(IDictionary ht) {
      _ct = (string)ht["type"];
      _target_ni = NodeInfo.CreateInstance((IDictionary)ht["target"]);
      _token = (string) ht["token"];
      IList neighht = ht["neighbors"] as IList;
      if( neighht != null ) {
        _neighbors = new NodeInfo[ neighht.Count ];
        for(int i = 0; i < neighht.Count; i++) {
          _neighbors[i] = NodeInfo.CreateInstance( (IDictionary)neighht[i] );
        }
      }
    }

    protected string _ct;
    public string ConnectionType { get { return _ct; } }

    protected NodeInfo _target_ni;
    public NodeInfo Target {
      get { return _target_ni; }
    }
    protected NodeInfo[] _neighbors;
    public NodeInfo[] Neighbors { get { return _neighbors; } }
    
    protected string _token;
    public string Token { get { return _token; } }
    
    
    public override bool Equals(object o)
    {
      ConnectToMessage co = o as ConnectToMessage;
      if( co != null ) {
        bool same = true;
	same &= co.ConnectionType == _ct;
	same &= co.Target.Equals( _target_ni );
        same &= co.Token.Equals( _token );
	if( _neighbors == null ) {
          same &= co.Neighbors == null;
	}
	else {
          int n_count = co.Neighbors.Length;
	  for(int i = 0; i < n_count; i++) {
            same &= co.Neighbors[i].Equals( Neighbors[i] );
	  } 
	}
	return same;
      }
      else {
        return false;
      }
    }
    override public int GetHashCode() {
      return Target.GetHashCode();
    }

    public IDictionary ToDictionary() {
      ListDictionary ht = new ListDictionary();
      ht["type"] = _ct;
      ht["target"] = _target_ni.ToDictionary();
      ht["token"] = _token;
      ArrayList neighs = new ArrayList(Neighbors.Length);
      foreach(NodeInfo ni in Neighbors) {
        neighs.Add( ni.ToDictionary() );
      }
      ht["neighbors"] = neighs;
      return ht;
    }
    
  }
//Here are some Unit tests:
#if BRUNET_NUNIT
//Here are some NUnit 2 test fixtures
  [TestFixture]
  public class ConnectToMessageTester {

    public ConnectToMessageTester() { }
    
    public void HTRoundTrip(ConnectToMessage ctm) {
      ConnectToMessage ctm2 = new ConnectToMessage( ctm.ToDictionary() );
      Assert.AreEqual(ctm, ctm2, "CTM HT Roundtrip");
    }
    [Test]
    public void CTMSerializationTest()
    {
      Address a = new DirectionalAddress(DirectionalAddress.Direction.Left);
      TransportAddress ta = TransportAddressFactory.CreateInstance("brunet.tcp://127.0.0.1:5000"); 
      NodeInfo ni = NodeInfo.CreateInstance(a, ta);

      RandomNumberGenerator rng = new RNGCryptoServiceProvider();      
      AHAddress tmp_add = new AHAddress(rng);
      ConnectToMessage ctm1 = new ConnectToMessage(ConnectionType.Unstructured, ni, tmp_add.ToString());
      
      HTRoundTrip(ctm1);

      //Test multiple tas:
      ArrayList tas = new ArrayList();
      tas.Add(ta);
      for(int i = 5001; i < 5010; i++)
        tas.Add(TransportAddressFactory.CreateInstance("brunet.tcp://127.0.0.1:" + i.ToString()));
      NodeInfo ni2 = NodeInfo.CreateInstance(a, tas);

      ConnectToMessage ctm2 = new ConnectToMessage(ConnectionType.Structured, ni2, tmp_add.ToString());
      HTRoundTrip(ctm2);
      //Here is a ConnectTo message with a neighbor list:
      NodeInfo[] neighs = new NodeInfo[5];
      for(int i = 0; i < 5; i++) {
	string ta_tmp = "brunet.tcp://127.0.0.1:" + (i+80).ToString();
        NodeInfo tmp =
		NodeInfo.CreateInstance(new DirectionalAddress(DirectionalAddress.Direction.Left),
	                     TransportAddressFactory.CreateInstance(ta_tmp)
			    );
	neighs[i] = tmp;
      }
      ConnectToMessage ctm3 = new ConnectToMessage("structured", ni, neighs, tmp_add.ToString());
      HTRoundTrip(ctm3);
#if false
      Console.Error.WriteLine( ctm3.ToString() );
      foreach(NodeInfo tni in ctm3a.Neighbors) {
        Console.Error.WriteLine(tni.ToString());
      }
#endif
    }
  }

#endif

}
