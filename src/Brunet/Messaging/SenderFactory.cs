/*
This program is part of BruNet, a library for the creation of efficient overlay
networks.
Copyright (C) 2007 P. Oscar Boykin <boykin@pobox.com>, Arijit Ganguly <aganguly@acis.ufl.edu> University of Florida

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
using System; 
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
#if BRUNET_NUNIT
using System.Collections.Specialized;
using NUnit.Framework;
#endif

using Brunet.Util;

namespace Brunet.Messaging {

  public delegate ISender SenderFactoryDelegate(object ctx, string uri);

  public class SenderFactoryException: Exception  {
    public SenderFactoryException(string s): base(s) {}
  }
  
  /**
   * Factory class for creating ISender objects from their
   * URI encodings. 
   * AHExactSender - sender:ah?dest=node:[base32 brunet address]&mode=exact
   * example - sender:ah?dest=brunet:node:JOJZG7VO6RFOEZJ6CJJ2WOIJWTXRVRP4&mode=exact
   *
   * AHGreedySender - sender:ah?dest=node:[base32 brunet address]&mode=greedy
   * example - sender:ah?dest=brunet:node:JOJZG7VO6RFOEZJ6CJJ2WOIJWTXRVRP4&mode=exact
   *
   * ForwardingSender: sender:fw?relay=node:[base32 brunet address]&init_mode=[initial routing mode]&dest=[base32 encoded brunet address]&ttl=ttl&mode=path
   * example - sender:fw?relay=brunet:node:JOJZG7VO6RFOEZJ6CJJ2WOIJWTXRVRP4&init_mode=greedy&dest=brunet:node:5FMQW3KKJWOOGVDO6QAQP65AWVZQ4VUQ&ttl=3&mode=path
   */
  public class SenderFactory {
    public static readonly char [] SplitChars = new char[] {'?', '&'};
    public static readonly char [] Delims = new char[] {'='};

    protected readonly static Dictionary<string, SenderFactoryDelegate> _handlers = new Dictionary<string, SenderFactoryDelegate>();

    /** 
     * Register a factory method for parsing sender URIs.
     * @param type type of the sender.
     * @handler factory method for the given type.
     */
    public static void Register(string type, SenderFactoryDelegate handler) {
      lock( _handlers ) {
        _handlers[type] = handler;
      }
    }

    /**
     * Returns an instance of an ISender, given its URI representation.
     * @param ctx context on which the sender is attached. 
     * @param uri URI representation of the sender.
     * @returns an ISender object.
     * @throws SenderFactoryException when URI is invalid or unsupported. 
     */
    public static ISender CreateInstance(object ctx, string uri) {
      int varidx;
      try {
        string type = GetScheme(uri, out varidx);
        return _handlers[type](ctx, uri);
      } catch(Exception
        #if BRUNET_NUNIT
        x) {
        Console.WriteLine(x);
        #else
        ) {
        #endif
        throw new SenderFactoryException("Cannot parse URI: " + uri);         
      }
    }

    /** create a URI string sender:scheme?k1=v1&k2=v2
     * @param scheme the name for this sender
     * @param opts the key-value pairs to encode
     * @return uri
     */
    public static string EncodeUri(string scheme, IDictionary<string, string> opts) {
      List<string> keys = new List<string>(opts.Keys);
      keys.Sort();
      StringWriter sw = new StringWriter();
      sw.Write("sender:{0}", scheme);
      string pattern = "?{0}={1}";
      foreach(string key in keys) {
        sw.Write(pattern, System.Web.HttpUtility.UrlEncode(key), System.Web.HttpUtility.UrlEncode(opts[key])); 
        //For the next time, use a different pattern:
        pattern = "&{0}={1}";
      }
      return sw.ToString();
    }
    /** Decode a URI into a scheme and key-value pairs
     * @param uri the URI to decode
     * @param scheme the for this URI
     * @return key-value pairs encoded
     */
    public static IDictionary<string, string> DecodeUri(string uri, out string scheme) {
      int varidx;
      scheme = GetScheme(uri, out varidx);
      if( varidx > 0 ) {
        string vars = uri.Substring(varidx);
        string[] kvpairs = vars.Split(SplitChars);
        Dictionary<string, string> result = new Dictionary<string, string>(kvpairs.Length);
        foreach(string kvpair in kvpairs) {
          int eq_idx = kvpair.IndexOf('=');
          string key = kvpair.Substring(0, eq_idx);
          string val = kvpair.Substring(eq_idx + 1);
          result.Add(System.Web.HttpUtility.UrlDecode(key), System.Web.HttpUtility.UrlDecode(val));
        }
        return result;
      }
      else {
        return new Dictionary<string,string>();
      }
    }

    public static string GetScheme(string uri, out int varidx) {
      if (!uri.StartsWith("sender:")) {
        throw new SenderFactoryException("Invalid string representation");
      }
      int idx = uri.IndexOf('?');
      if( idx > 0 ) {
        varidx = idx + 1;
        return uri.Substring(7, idx - 7);
      }
      else {
        varidx = -1;
        return uri.Substring(7);
      }
    }
  }
}
